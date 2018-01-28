using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using System.Reflection;
using Microsoft.Xna.Framework;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using Nez.IEnumerableExtensions;


namespace Nez.Console
{
	public partial class DebugConsole
	{
		public static DebugConsole instance;

		/// <summary>
		/// controls the scale of the console
		/// </summary>
		public static float renderScale = 1f;

		/// <summary>
		/// bind any custom Actions you would like to function keys
		/// </summary>
		Action[] _functionKeyActions;

		const float UNDERSCORE_TIME = 0.5f;
		const float REPEAT_DELAY = 0.5f;
		const float REPEAT_EVERY = 1 / 30f;
		const float OPACITY = 0.65f;

		// render constants
		const int LINE_HEIGHT = 10;
		const int TEXT_PADDING_X = 5;
		const int TEXT_PADDING_Y = 4;

		/// <summary>
		/// separation of the command entry and history boxes
		/// </summary>
		const int COMMAND_HISTORY_PADDING = 10;

		/// <summary>
		/// global padding on the left/right of the console
		/// </summary>
		const int HORIZONTAL_PADDING = 10;

		bool enabled = true;
		internal bool isOpen;
		Dictionary<string,CommandInfo> _commands;
		List<string> _sorted;

		KeyboardState _oldState;
		KeyboardState _currentState;
		string _currentText = "";
		List<string> _drawCommands;
		bool _underscore;
		float _underscoreCounter;
		List<string> _commandHistory;
		int _seekIndex = -1;
		int _tabIndex = -1;
		string _tabSearch;
		float _repeatCounter = 0;
		Keys? _repeatKey = null;
		bool _canOpen;
		public static Keys consoleKey = Keys.OemTilde;
		#if DEBUG
		internal RuntimeInspector _runtimeInspector;
		#endif


		static DebugConsole()
		{
			instance = new DebugConsole();
		}


		public DebugConsole()
		{
			_commandHistory = new List<string>();
			_drawCommands = new List<string>();
			_commands = new Dictionary<string,CommandInfo>();
			_sorted = new List<string>();
			_functionKeyActions = new Action[12];

			buildCommandsList();
		}


		public void log( Exception e )
		{
			log( e.Message );

			var str = e.StackTrace;
			var parts = str.Split( new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries );
			foreach( var line in parts )
			{
				var lineWithoutPath = Regex.Replace( line, @"in\s\/.*?\/.*?(\w+\.cs)", "$1" );
				log( lineWithoutPath );
			}
		}


		public void log( string format, params object[] args )
		{
			log( string.Format( format, args ) );
		}


		public void log( object obj )
		{
			log( obj.ToString() );
		}


		public void log( string str )
		{
			// split up multi-line logs and log each line seperately
			var parts = str.Split( new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries );
			if( parts.Length > 1 )
			{
				foreach( var line in parts )
					log( line );
				return;
			}

			// Split the string if you overlow horizontally
			var maxWidth = Core.graphicsDevice.PresentationParameters.BackBufferWidth - 40;
			var screenHeight = Core.graphicsDevice.PresentationParameters.BackBufferHeight;

			while( Graphics.instance.bitmapFont.measureString( str ).X * renderScale > maxWidth )
			{
				var split = -1;
				for( var i = 0; i < str.Length; i++ )
				{
					if( str[i] == ' ' )
					{
						if( Graphics.instance.bitmapFont.measureString( str.Substring( 0, i ) ).X * renderScale <= maxWidth )
							split = i;
						else
							break;
					}
				}

				if( split == -1 )
					break;

				_drawCommands.Insert( 0, str.Substring( 0, split ) );
				str = str.Substring( split + 1 );
			}

			_drawCommands.Insert( 0, str );

			// Don't overflow top of window
			var maxCommands = ( screenHeight - 100 ) / 30;
			while( _drawCommands.Count > maxCommands )
				_drawCommands.RemoveAt( _drawCommands.Count - 1 );
		}


		#region Updating and Rendering

		internal void update()
		{
			if( isOpen )
				updateOpen();
			else if( enabled )
				updateClosed();
		}


		void updateClosed()
		{
			if( !_canOpen )
			{
				_canOpen = true;
			}
			else if( Input.isKeyPressed( consoleKey, Keys.Oem8 ) )
			{
				isOpen = true;
				_currentState = Keyboard.GetState();
			}

			for( var i = 0; i < _functionKeyActions.Length; i++ )
				if( Input.isKeyPressed( (Keys)( Keys.F1 + i ) ) )
					executeFunctionKeyAction( i );
		}


		void updateOpen()
		{
			_oldState = _currentState;
			_currentState = Keyboard.GetState();

			_underscoreCounter += Time.deltaTime;
			while( _underscoreCounter >= UNDERSCORE_TIME )
			{
				_underscoreCounter -= UNDERSCORE_TIME;
				_underscore = !_underscore;
			}

			if( _repeatKey.HasValue )
			{
				if( _currentState[_repeatKey.Value] == KeyState.Down )
				{
					_repeatCounter += Time.deltaTime;

					while( _repeatCounter >= REPEAT_DELAY )
					{
						handleKey( _repeatKey.Value );
						_repeatCounter -= REPEAT_EVERY;
					}
				}
				else
					_repeatKey = null;
			}

			foreach( var key in _currentState.GetPressedKeys() )
			{
				if( _oldState[key] == KeyState.Up )
				{
					handleKey( key );
					break;
				}
			}
		}


		void handleKey( Keys key )
		{
			if( key != Keys.Tab && key != Keys.LeftShift && key != Keys.RightShift && key != Keys.RightAlt && key != Keys.LeftAlt && key != Keys.RightControl && key != Keys.LeftControl )
				_tabIndex = -1;

			if( key != consoleKey && key != Keys.Oem8 && key != Keys.Enter && _repeatKey != key )
			{
				_repeatKey = key;
				_repeatCounter = 0;
			}

			switch( key )
			{
				default:
					if( key.ToString().Length == 1 )
					{
						if( InputUtils.isShiftDown() )
							_currentText += key.ToString();
						else
							_currentText += key.ToString().ToLower();
					}
				break;

				case( Keys.D1 ):
					if( InputUtils.isShiftDown() )
						_currentText += '!';
					else
						_currentText += '1';
				break;
				case( Keys.D2 ):
					if( InputUtils.isShiftDown() )
						_currentText += '@';
					else
						_currentText += '2';
				break;
				case( Keys.D3 ):
					if( InputUtils.isShiftDown() )
						_currentText += '#';
					else
						_currentText += '3';
				break;
				case( Keys.D4 ):
					if( InputUtils.isShiftDown() )
						_currentText += '$';
					else
						_currentText += '4';
				break;
				case( Keys.D5 ):
					if( InputUtils.isShiftDown() )
						_currentText += '%';
					else
						_currentText += '5';
				break;
				case( Keys.D6 ):
					if( InputUtils.isShiftDown() )
						_currentText += '^';
					else
						_currentText += '6';
				break;
				case( Keys.D7 ):
					if( InputUtils.isShiftDown() )
						_currentText += '&';
					else
						_currentText += '7';
				break;
				case( Keys.D8 ):
					if( InputUtils.isShiftDown() )
						_currentText += '*';
					else
						_currentText += '8';
				break;
				case( Keys.D9 ):
					if( InputUtils.isShiftDown() )
						_currentText += '(';
					else
						_currentText += '9';
				break;
				case( Keys.D0 ):
					if( InputUtils.isShiftDown() )
						_currentText += ')';
					else
						_currentText += '0';
				break;
				case( Keys.OemComma):
					if( InputUtils.isShiftDown() )
						_currentText += '<';
					else
						_currentText += ',';
				break;
				case Keys.OemPeriod:
					if( InputUtils.isShiftDown() )
						_currentText += '>';
					else
						_currentText += '.';
				break;
				case Keys.OemQuestion:
					if( InputUtils.isShiftDown() )
						_currentText += '?';
					else
						_currentText += '/';
				break;
				case Keys.OemSemicolon:
					if( InputUtils.isShiftDown() )
						_currentText += ':';
					else
						_currentText += ';';
				break;
				case Keys.OemQuotes:
					if( InputUtils.isShiftDown() )
						_currentText += '"';
					else
						_currentText += '\'';
				break;
				case Keys.OemBackslash:
					if( InputUtils.isShiftDown() )
						_currentText += '|';
					else
						_currentText += '\\';
				break;
				case Keys.OemOpenBrackets:
					if( InputUtils.isShiftDown() )
						_currentText += '{';
					else
						_currentText += '[';
				break;
				case Keys.OemCloseBrackets:
					if( InputUtils.isShiftDown() )
						_currentText += '}';
					else
						_currentText += ']';
				break;
				case Keys.OemMinus:
					if( InputUtils.isShiftDown() )
						_currentText += '_';
					else
						_currentText += '-';
				break;
				case Keys.OemPlus:
					if( InputUtils.isShiftDown() )
						_currentText += '+';
					else
						_currentText += '=';
				break;

				case Keys.Space:
					_currentText += " ";
				break;
				case Keys.Back:
					if( _currentText.Length > 0 )
						_currentText = _currentText.Substring( 0, _currentText.Length - 1 );
				break;
				case Keys.Delete:
					_currentText = "";
				break;

				case Keys.Up:
					if( _seekIndex < _commandHistory.Count - 1 )
					{
						_seekIndex++;
						_currentText = string.Join( " ", _commandHistory[_seekIndex] );
					}
				break;
				case Keys.Down:
					if( _seekIndex > -1 )
					{
						_seekIndex--;
						if( _seekIndex == -1 )
							_currentText = "";
						else
							_currentText = string.Join( " ", _commandHistory[_seekIndex] );
					}
				break;

				case Keys.Tab:
					if( InputUtils.isShiftDown() )
					{
						if( _tabIndex == -1 )
						{
							_tabSearch = _currentText;
							findLastTab();
						}
						else
						{
							_tabIndex--;
							if( _tabIndex < 0 || ( _tabSearch != "" && _sorted[_tabIndex].IndexOf( _tabSearch ) != 0 ) )
								findLastTab();
						}
					}
					else
					{
						if( _tabIndex == -1 )
						{
							_tabSearch = _currentText;
							findFirstTab();
						}
						else
						{
							_tabIndex++;
							if( _tabIndex >= _sorted.Count || ( _tabSearch != "" && _sorted[_tabIndex].IndexOf( _tabSearch ) != 0 ) )
								findFirstTab();
						}
					}
					if( _tabIndex != -1 )
						_currentText = _sorted[_tabIndex];
				break;

				case Keys.F1:
				case Keys.F2:
				case Keys.F3:
				case Keys.F4:
				case Keys.F5:
				case Keys.F6:
				case Keys.F7:
				case Keys.F8:
				case Keys.F9:
				case Keys.F10:
				case Keys.F11:
				case Keys.F12:
					executeFunctionKeyAction( (int)( key - Keys.F1 ) );
				break;

				case Keys.Enter:
					if( _currentText.Length > 0 )
						enterCommand();
				break;

				case Keys.Oem8:
				case Keys.OemTilde:
				break;
			}

			if( key == consoleKey )
			{
				isOpen = _canOpen = false;
			}
		}


		void enterCommand()
		{
			var data = _currentText.Split( new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries );
			if( _commandHistory.Count == 0 || _commandHistory[0] != _currentText )
				_commandHistory.Insert( 0, _currentText );
			_drawCommands.Insert( 0, "> " + _currentText );
			_currentText = "";
			_seekIndex = -1;

			string[] args = new string[data.Length - 1];
			for( int i = 1; i < data.Length; i++ )
				args[i - 1] = data[i];
			executeCommand( data[0].ToLower(), args );
		}


		void findFirstTab()
		{
			for( int i = 0; i < _sorted.Count; i++ )
			{
				if( _tabSearch == "" || _sorted[i].IndexOf( _tabSearch ) == 0 )
				{
					_tabIndex = i;
					break;
				}
			}
		}


		void findLastTab()
		{
			for( int i = 0; i < _sorted.Count; i++ )
				if( _tabSearch == "" || _sorted[i].IndexOf( _tabSearch ) == 0 )
					_tabIndex = i;
		}


		internal void render()
		{
			#if DEBUG
			if( _runtimeInspector != null )
			{
				_runtimeInspector.update();
				_runtimeInspector.render();
			}
			#endif

			if( !isOpen )
				return;

			var screenWidth = Screen.width;
			var screenHeight = Screen.height;
			var workingWidth = screenWidth - 2 * HORIZONTAL_PADDING;

			Graphics.instance.batcher.begin();

			// setup the rect that encompases the command entry section
			var commandEntryRect = RectangleExt.fromFloats( HORIZONTAL_PADDING, screenHeight - LINE_HEIGHT * renderScale, workingWidth, LINE_HEIGHT * renderScale );

			// take into account text padding. move our location up a bit and expand the Rect to accommodate
			commandEntryRect.Location -= new Point( 0, TEXT_PADDING_Y * 2 );
			commandEntryRect.Height += TEXT_PADDING_Y * 2;

			Graphics.instance.batcher.drawRect( commandEntryRect, Color.Black * OPACITY );
			var commandLineString = "> " + _currentText;
			if( _underscore )
				commandLineString += "_";

			var commandTextPosition = commandEntryRect.Location.ToVector2() + new Vector2( TEXT_PADDING_X, TEXT_PADDING_Y );
			Graphics.instance.batcher.drawString( Graphics.instance.bitmapFont, commandLineString, commandTextPosition, Color.White, 0, Vector2.Zero, new Vector2( renderScale ), SpriteEffects.None, 0 );

			if( _drawCommands.Count > 0 )
			{
				// start with the total height of the text then add in padding. We have an extra padding because we pad each line and the top/bottom
				var height = LINE_HEIGHT * renderScale * _drawCommands.Count;
				height += ( _drawCommands.Count + 1 ) * TEXT_PADDING_Y;

				var topOfHistoryRect = commandEntryRect.Y - height - COMMAND_HISTORY_PADDING;
				Graphics.instance.batcher.drawRect( HORIZONTAL_PADDING, topOfHistoryRect, workingWidth, height, Color.Black * OPACITY );

				var yPosFirstLine = topOfHistoryRect + height - TEXT_PADDING_Y - LINE_HEIGHT * renderScale;
				for( var i = 0; i < _drawCommands.Count; i++ )
				{
					var yPosCurrentLineAddition = ( i * LINE_HEIGHT * renderScale ) + ( i * TEXT_PADDING_Y );
					var position = new Vector2( HORIZONTAL_PADDING + TEXT_PADDING_X, yPosFirstLine - yPosCurrentLineAddition );
					var color = _drawCommands[i].IndexOf( ">" ) == 0 ? Color.Yellow : Color.White;
					Graphics.instance.batcher.drawString( Graphics.instance.bitmapFont, _drawCommands[i], position, color, 0, Vector2.Zero, new Vector2( renderScale ), SpriteEffects.None, 0 );
				}
			}

			Graphics.instance.batcher.end();
		}

#endregion


		#region Execute

		void executeCommand( string command, string[] args )
		{
			if( _commands.ContainsKey( command ) )
				_commands[command].action( args );
			else
				log( "Command '" + command + "' not found! Type 'help' for list of commands" );
		}


		void executeFunctionKeyAction( int num )
		{
			if( _functionKeyActions[num] != null )
				_functionKeyActions[num]();
		}


		public static void bindActionToFunctionKey( int functionKey, Action action )
		{
			instance._functionKeyActions[functionKey - 1] = action;
		}

		#endregion


		#region Parse Commands

		void buildCommandsList()
		{
			// this will get us the Nez assembly
			processAssembly( typeof( DebugConsole ).GetTypeInfo().Assembly );

			// this will get us the current executables assembly in 99.9% of cases
			// for now we will let the next section handle loading this. If it doesnt work out we'll uncomment this
			//processAssembly( Core._instance.GetType().GetTypeInfo().Assembly );

			try
			{
				// this is a nasty hack that lets us get at all the assemblies. It is only allowed to exist because this will never get
				// hit in a release build.
				var appDomainType = typeof( string ).GetTypeInfo().Assembly.GetType( "System.AppDomain" );
				var domain = appDomainType.GetRuntimeProperty( "CurrentDomain" ).GetMethod.Invoke( null, new object[]{} );
				var assembliesMethod = ReflectionUtils.getMethodInfo( domain, "GetAssemblies" );
				// not sure about arguments, detect in runtime
				var methodCallParams = assembliesMethod.GetParameters().Length == 0 ? new object[] { } : new object[] { false };
				var assemblies = assembliesMethod.Invoke( domain, methodCallParams ) as Assembly[];

				var ignoredAssemblies = new string[] { "mscorlib", "MonoMac", "MonoGame.Framework", "Mono.Security", "System", "OpenTK", "ObjCImplementations", "Nez" };
				foreach( var assembly in assemblies )
				{
					var name = assembly.GetName().Name;
					if( name.StartsWith( "System." ) || ignoredAssemblies.contains( name ) )
						continue;

					processAssembly( assembly );
				}
			}
			catch( Exception e )
			{
				Debug.log( "DebugConsole pooped itself trying to get all the loaded assemblies. {0}", e );
			}


			// Maintain the sorted command list
			foreach( var command in _commands )
				_sorted.Add( command.Key );
			_sorted.Sort();
		}


		void processAssembly( Assembly assembly )
		{
			foreach( var type in assembly.DefinedTypes )
			{
				foreach( var method in type.DeclaredMethods )
				{
					CommandAttribute attr = null;
					var attrs = method.GetCustomAttributes( typeof( CommandAttribute ), false )
						.Where( a => a is CommandAttribute );
					if( attrs.count() > 0 )
						attr = attrs.First() as CommandAttribute;

					if( attr != null )
						processMethod( method, attr );
				}
			}
		}


		void processMethod( MethodInfo method, CommandAttribute attr )
		{
			if( !method.IsStatic )
			{
				throw new Exception( method.DeclaringType.Name + "." + method.Name + " is marked as a command, but is not static" );
			}
			else
			{
				var info = new CommandInfo();
				info.help = attr.help;  

				var parameters = method.GetParameters();
				var defaults = new object[parameters.Length];                 
				var usage = new string[parameters.Length];

				for( var i = 0; i < parameters.Length; i++ )
				{                       
					var p = parameters[i];
					usage[i] = p.Name + ":";

					if( p.ParameterType == typeof( string ) )
						usage[i] += "string";
					else if( p.ParameterType == typeof( int ) )
						usage[i] += "int";
					else if( p.ParameterType == typeof( float ) )
						usage[i] += "float";
					else if( p.ParameterType == typeof( bool ) )
						usage[i] += "bool";
					else
						throw new Exception( method.DeclaringType.Name + "." + method.Name + " is marked as a command, but has an invalid parameter type. Allowed types are: string, int, float, and bool" );

					// no System.DBNull in PCL so we fake it
					if( p.DefaultValue.GetType().FullName == "System.DBNull" )
					{
						defaults[i] = null;
					}
					else if( p.DefaultValue != null )
					{
						defaults[i] = p.DefaultValue;
						if( p.ParameterType == typeof( string ) )
							usage[i] += "=\"" + p.DefaultValue.ToString() + "\"";
						else
							usage[i] += "=" + p.DefaultValue.ToString();
					}
					else
					{
						defaults[i] = null;
					}
				}

				if( usage.Length == 0 )
					info.usage = "";
				else
					info.usage = "[" + string.Join( " ", usage ) + "]";

				info.action = args =>
				{
					if( parameters.Length == 0 )
					{
						method.Invoke( null, null );
					}
					else
					{
						var param = (object[])defaults.Clone();

						for( var i = 0; i < param.Length && i < args.Length; i++ )
						{
							if( parameters[i].ParameterType == typeof( string ) )
								param[i] = argString( args[i] );
							else if( parameters[i].ParameterType == typeof( int ) )
								param[i] = argInt( args[i] );
							else if( parameters[i].ParameterType == typeof( float ) )
								param[i] = argFloat( args[i] );
							else if( parameters[i].ParameterType == typeof( bool ) )
								param[i] = argBool( args[i] );
						}

						try
						{
							method.Invoke( null, param );
						}
						catch( Exception e )
						{
							log( e );
						}
					}
				};

				_commands[attr.name] = info;
			}
		}


		struct CommandInfo
		{
			public Action<string[]> action;
			public string help;
			public string usage;
		}


		#region Parsing Arguments

		static string argString( string arg )
		{
			if( arg == null )
				return "";
			else
				return arg;
		}


		static bool argBool( string arg )
		{
			if( arg != null )
				return !( arg == "0" || arg.ToLower() == "false" || arg.ToLower() == "f" );
			else
				return false;
		}


		static int argInt( string arg )
		{
			try
			{
				return Convert.ToInt32( arg );
			}
			catch
			{
				return 0;
			}
		}


		static float argFloat( string arg )
		{
			try
			{
				return Convert.ToSingle( arg );
			}
			catch
			{
				return 0;
			}
		}

		#endregion

		#endregion

	}
}

