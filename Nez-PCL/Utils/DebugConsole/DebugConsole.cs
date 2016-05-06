using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework;
using System.Text.RegularExpressions;
using Nez.BitmapFonts;
using Microsoft.Xna.Framework.Graphics;


namespace Nez.Console
{
	public partial class DebugConsole
	{
		internal static DebugConsole instance;

		/// <summary>
		/// bind any custom Actions you would like to function keys
		/// </summary>
		Action[] _functionKeyActions;

		const float UNDERSCORE_TIME = 0.5f;
		const float REPEAT_DELAY = 0.5f;
		const float REPEAT_EVERY = 1 / 30f;
		const float OPACITY = 0.65f;

		// render constants
		Vector2 FONT_SCALE;
		const float FONT_LINE_HEIGHT = 11;
		const int HORIZONTAL_PADDING = 10;
		const int BOTTOM_MARGIN = 30;
		const int BOTTOM_CONSOLE_HEIGHT = 20;
		const int LINE_HEIGHT = 14;

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

			var scale = FONT_LINE_HEIGHT / Graphics.instance.bitmapFont.lineHeight;
			FONT_SCALE = new Vector2( scale, scale );

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


		public void log( object obj )
		{
			var str = obj.ToString();

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

			while( Graphics.instance.bitmapFont.measureString( str ).X * FONT_SCALE.X > maxWidth )
			{
				var split = -1;
				for( var i = 0; i < str.Length; i++ )
				{
					if( str[i] == ' ' )
					{
						if( Graphics.instance.bitmapFont.measureString( str.Substring( 0, i ) ).X * FONT_SCALE.X <= maxWidth )
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
			else if( Input.isKeyPressed( Keys.OemTilde, Keys.Oem8 ) )
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

			if( key != Keys.OemTilde && key != Keys.Oem8 && key != Keys.Enter && _repeatKey != key )
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
					isOpen = _canOpen = false;
				break;
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
			if( !isOpen )
				return;
			
			var screenWidth = Core.graphicsDevice.PresentationParameters.BackBufferWidth;
			var screenHeight = Core.graphicsDevice.PresentationParameters.BackBufferHeight;
			var workingWidth = screenWidth - 2 * HORIZONTAL_PADDING;

			Graphics.instance.batcher.begin();

			Graphics.instance.batcher.drawRect( HORIZONTAL_PADDING, screenHeight - BOTTOM_MARGIN, workingWidth, BOTTOM_CONSOLE_HEIGHT, Color.Black * OPACITY );
			var commandLineString = "> " + _currentText;
			if( _underscore )
				commandLineString += "_";
			
			Graphics.instance.batcher.drawString( Graphics.instance.bitmapFont, commandLineString, new Vector2( 20, screenHeight - BOTTOM_CONSOLE_HEIGHT - FONT_LINE_HEIGHT * 0.35f ), Color.White );

			if( _drawCommands.Count > 0 )
			{
				var height = LINE_HEIGHT * _drawCommands.Count + 15;
				var topOfHistoryRect = screenHeight - height - BOTTOM_CONSOLE_HEIGHT - 20;
				Graphics.instance.batcher.drawRect( HORIZONTAL_PADDING, topOfHistoryRect, workingWidth, height, Color.Black * OPACITY );
				for( int i = 0; i < _drawCommands.Count; i++ )
				{
					var position = new Vector2( 20, topOfHistoryRect + height - 20 - LINE_HEIGHT * i );
					var color = _drawCommands[i].IndexOf( ">" ) == 0 ? Color.Yellow : Color.White;
					Graphics.instance.batcher.drawString( Graphics.instance.bitmapFont, _drawCommands[i], position, color, 0, Vector2.Zero, FONT_SCALE, SpriteEffects.None, 0 );
				}
			}

			Graphics.instance.batcher.end();
		}

		#endregion


		#region Execute

		public void executeCommand( string command, string[] args )
		{
			if( _commands.ContainsKey( command ) )
				_commands[command].action( args );
			else
				log( "Command '" + command + "' not found! Type 'help' for list of commands" );
		}


		public void executeFunctionKeyAction( int num )
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
			// Check executing assembly for Commands
			foreach( var type in Assembly.GetExecutingAssembly().GetTypes() )
				foreach( var method in type.GetMethods( BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic ) )
					processMethod( method );

			// Check the calling assembly for Commands
			foreach( var type in Assembly.GetCallingAssembly().GetTypes() )
				foreach( var method in type.GetMethods( BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic ) )
					processMethod( method );

			try
			{
				var currentdomain = typeof( string ).Assembly.GetType( "System.AppDomain" ).GetProperty( "CurrentDomain" ).GetGetMethod().Invoke( null, new object[] { } );
				var getassemblies = currentdomain.GetType().GetMethod( "GetAssemblies", new Type[]{ } );
				var assemblies = getassemblies.Invoke( currentdomain, new object[]{ } ) as Assembly[];

				foreach( var assembly in assemblies )
					foreach( var type in assembly.GetTypes() )
						foreach( var method in type.GetMethods( BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic ) )
							processMethod( method );
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


		void processMethod( MethodInfo method )
		{
			CommandAttribute attr = null;
			{
				var attrs = method.GetCustomAttributes( typeof( CommandAttribute ), false );
				if( attrs.Length > 0 )
					attr = attrs[0] as CommandAttribute;
			}

			if( attr != null )
			{
				if( !method.IsStatic )
					throw new Exception( method.DeclaringType.Name + "." + method.Name + " is marked as a command, but is not static" );
				else
				{
					CommandInfo info = new CommandInfo();
					info.help = attr.help;  

					var parameters = method.GetParameters();
					var defaults = new object[parameters.Length];                 
					string[] usage = new string[parameters.Length];

					for( int i = 0; i < parameters.Length; i++ )
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
							defaults[i] = null;
						else if( p.DefaultValue != null )
						{
							defaults[i] = p.DefaultValue;
							if( p.ParameterType == typeof( string ) )
								usage[i] += "=\"" + p.DefaultValue.ToString() + "\"";
							else
								usage[i] += "=" + p.DefaultValue.ToString();
						}
						else
							defaults[i] = null;
					}

					if( usage.Length == 0 )
						info.usage = "";
					else
						info.usage = "[" + string.Join( " ", usage ) + "]";

					info.action = (args ) =>
					{
						if( parameters.Length == 0 )
							method.Invoke( null, null );
						else
						{
							object[] param = (object[])defaults.Clone();

							for( int i = 0; i < param.Length && i < args.Length; i++ )
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

