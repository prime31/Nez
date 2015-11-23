using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework;
using System.Text.RegularExpressions;


namespace Nez.Console
{
	public partial class DebugConsole
	{
		internal static DebugConsole instance;

		/// <summary>
		/// bind any custom Actions you would like to function keys
		/// </summary>
		public Action[] functionKeyActions;

		const float UNDERSCORE_TIME = 0.5f;
		const float REPEAT_DELAY = 0.5f;
		const float REPEAT_EVERY = 1 / 30f;
		const float OPACITY = 0.65f;

		// render constants
		Vector2 FONT_SCALE;
		const float FONT_SIZE = 14;
		const int HORIZONTAL_PADDING = 10;
		const int BOTTOM_MARGIN = 50;
		const int LINE_HEIGHT = 20;

		bool enabled = true;
		bool isOpen;
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
			functionKeyActions = new Action[12];

			var scale = FONT_SIZE / Graphics.instance.spriteFont.MeasureString( " " ).Y;
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

			// Split the string if you overlow horizontally
			var maxWidth = Core.graphicsDevice.PresentationParameters.BackBufferWidth - 40;
			var screenHeight = Core.graphicsDevice.PresentationParameters.BackBufferHeight;

			while( Graphics.instance.spriteFont.MeasureString( str ).X * FONT_SCALE.X > maxWidth )
			{
				var split = -1;
				for( int i = 0; i < str.Length; i++ )
				{
					if( str[i] == ' ' )
					{
						if( Graphics.instance.spriteFont.MeasureString( str.Substring( 0, i ) ).X * FONT_SCALE.X <= maxWidth )
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

			for( int i = 0; i < functionKeyActions.Length; i++ )
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
						if( _currentState[Keys.LeftShift] == KeyState.Down || _currentState[Keys.RightShift] == KeyState.Down )
							_currentText += key.ToString();
						else
							_currentText += key.ToString().ToLower();
					}
				break;

				case( Keys.D1 ):
					if( _currentState[Keys.LeftShift] == KeyState.Down || _currentState[Keys.RightShift] == KeyState.Down )
						_currentText += '!';
					else
						_currentText += '1';
				break;
				case( Keys.D2 ):
					if( _currentState[Keys.LeftShift] == KeyState.Down || _currentState[Keys.RightShift] == KeyState.Down )
						_currentText += '@';
					else
						_currentText += '2';
				break;
				case( Keys.D3 ):
					if( _currentState[Keys.LeftShift] == KeyState.Down || _currentState[Keys.RightShift] == KeyState.Down )
						_currentText += '#';
					else
						_currentText += '3';
				break;
				case( Keys.D4 ):
					if( _currentState[Keys.LeftShift] == KeyState.Down || _currentState[Keys.RightShift] == KeyState.Down )
						_currentText += '$';
					else
						_currentText += '4';
				break;
				case( Keys.D5 ):
					if( _currentState[Keys.LeftShift] == KeyState.Down || _currentState[Keys.RightShift] == KeyState.Down )
						_currentText += '%';
					else
						_currentText += '5';
				break;
				case( Keys.D6 ):
					if( _currentState[Keys.LeftShift] == KeyState.Down || _currentState[Keys.RightShift] == KeyState.Down )
						_currentText += '^';
					else
						_currentText += '6';
				break;
				case( Keys.D7 ):
					if( _currentState[Keys.LeftShift] == KeyState.Down || _currentState[Keys.RightShift] == KeyState.Down )
						_currentText += '&';
					else
						_currentText += '7';
				break;
				case( Keys.D8 ):
					if( _currentState[Keys.LeftShift] == KeyState.Down || _currentState[Keys.RightShift] == KeyState.Down )
						_currentText += '*';
					else
						_currentText += '8';
				break;
				case( Keys.D9 ):
					if( _currentState[Keys.LeftShift] == KeyState.Down || _currentState[Keys.RightShift] == KeyState.Down )
						_currentText += '(';
					else
						_currentText += '9';
				break;
				case( Keys.D0 ):
					if( _currentState[Keys.LeftShift] == KeyState.Down || _currentState[Keys.RightShift] == KeyState.Down )
						_currentText += ')';
					else
						_currentText += '0';
				break;
				case( Keys.OemComma):
					if( _currentState[Keys.LeftShift] == KeyState.Down || _currentState[Keys.RightShift] == KeyState.Down )
						_currentText += '<';
					else
						_currentText += ',';
				break;
				case Keys.OemPeriod:
					if( _currentState[Keys.LeftShift] == KeyState.Down || _currentState[Keys.RightShift] == KeyState.Down )
						_currentText += '>';
					else
						_currentText += '.';
				break;
				case Keys.OemQuestion:
					if( _currentState[Keys.LeftShift] == KeyState.Down || _currentState[Keys.RightShift] == KeyState.Down )
						_currentText += '?';
					else
						_currentText += '/';
				break;
				case Keys.OemSemicolon:
					if( _currentState[Keys.LeftShift] == KeyState.Down || _currentState[Keys.RightShift] == KeyState.Down )
						_currentText += ':';
					else
						_currentText += ';';
				break;
				case Keys.OemQuotes:
					if( _currentState[Keys.LeftShift] == KeyState.Down || _currentState[Keys.RightShift] == KeyState.Down )
						_currentText += '"';
					else
						_currentText += '\'';
				break;
				case Keys.OemBackslash:
					if( _currentState[Keys.LeftShift] == KeyState.Down || _currentState[Keys.RightShift] == KeyState.Down )
						_currentText += '|';
					else
						_currentText += '\\';
				break;
				case Keys.OemOpenBrackets:
					if( _currentState[Keys.LeftShift] == KeyState.Down || _currentState[Keys.RightShift] == KeyState.Down )
						_currentText += '{';
					else
						_currentText += '[';
				break;
				case Keys.OemCloseBrackets:
					if( _currentState[Keys.LeftShift] == KeyState.Down || _currentState[Keys.RightShift] == KeyState.Down )
						_currentText += '}';
					else
						_currentText += ']';
				break;
				case Keys.OemMinus:
					if( _currentState[Keys.LeftShift] == KeyState.Down || _currentState[Keys.RightShift] == KeyState.Down )
						_currentText += '_';
					else
						_currentText += '-';
				break;
				case Keys.OemPlus:
					if( _currentState[Keys.LeftShift] == KeyState.Down || _currentState[Keys.RightShift] == KeyState.Down )
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
					if( _currentState[Keys.LeftShift] == KeyState.Down || _currentState[Keys.RightShift] == KeyState.Down )
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
			_drawCommands.Insert( 0, ">" + _currentText );
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

			Graphics.instance.spriteBatch.Begin();

			Graphics.instance.drawRect( HORIZONTAL_PADDING, screenHeight - BOTTOM_MARGIN, workingWidth, 40, Color.Black * OPACITY );
			if( _underscore )
				Graphics.instance.spriteBatch.DrawString( Graphics.instance.spriteFont, ">" + _currentText + "_", new Vector2( 20, screenHeight - 42 ), Color.White );
			else
				Graphics.instance.spriteBatch.DrawString( Graphics.instance.spriteFont, ">" + _currentText, new Vector2( 20, screenHeight - 42 ), Color.White );

			if( _drawCommands.Count > 0 )
			{
				var height = 10 + ( LINE_HEIGHT * _drawCommands.Count );
				Graphics.instance.drawRect( HORIZONTAL_PADDING, screenHeight - height - 60, workingWidth, height, Color.Black * OPACITY );
				for( int i = 0; i < _drawCommands.Count; i++ )
				{
					var position = new Vector2( 20, screenHeight - 92 - ( LINE_HEIGHT * i ) );
					var color = _drawCommands[i].IndexOf( ">" ) == 0 ? Color.Yellow : Color.White;
					Graphics.instance.spriteBatch.DrawString( Graphics.instance.spriteFont, _drawCommands[i], position, color, 0, Vector2.Zero, FONT_SCALE, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0 );
				}
			}

			Graphics.instance.spriteBatch.End();
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
			if( functionKeyActions[num] != null )
				functionKeyActions[num]();
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

			// Maintain the sorted command list
			foreach( var command in _commands )
				_sorted.Add( command.Key );
			_sorted.Sort();
		}


		void processMethod( MethodInfo method )
		{
			Command attr = null;
			{
				var attrs = method.GetCustomAttributes( typeof( Command ), false );
				if( attrs.Length > 0 )
					attr = attrs[0] as Command;
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

