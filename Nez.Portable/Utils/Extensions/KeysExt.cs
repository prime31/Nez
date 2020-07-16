using Microsoft.Xna.Framework.Input;


namespace Nez
{
	public static class KeysExt
	{
		public static char? GetChar(this Keys key)
		{
			var isShiftDown = InputUtils.IsShiftDown();

			if (key == Keys.A)
				return isShiftDown ? 'A' : 'a';

			if (key == Keys.B)
				return isShiftDown ? 'B' : 'b';

			if (key == Keys.C)
				return isShiftDown ? 'C' : 'c';

			if (key == Keys.D)
				return isShiftDown ? 'D' : 'd';

			if (key == Keys.E)
				return isShiftDown ? 'E' : 'e';

			if (key == Keys.F)
				return isShiftDown ? 'F' : 'f';

			if (key == Keys.G)
				return isShiftDown ? 'G' : 'g';

			if (key == Keys.H)
				return isShiftDown ? 'H' : 'h';

			if (key == Keys.I)
				return isShiftDown ? 'I' : 'i';

			if (key == Keys.J)
				return isShiftDown ? 'J' : 'j';

			if (key == Keys.K)
				return isShiftDown ? 'K' : 'k';

			if (key == Keys.L)
				return isShiftDown ? 'L' : 'l';

			if (key == Keys.M)
				return isShiftDown ? 'M' : 'm';

			if (key == Keys.N)
				return isShiftDown ? 'N' : 'n';

			if (key == Keys.O)
				return isShiftDown ? 'O' : 'o';

			if (key == Keys.P)
				return isShiftDown ? 'P' : 'p';

			if (key == Keys.Q)
				return isShiftDown ? 'Q' : 'q';

			if (key == Keys.R)
				return isShiftDown ? 'R' : 'r';

			if (key == Keys.S)
				return isShiftDown ? 'S' : 's';

			if (key == Keys.T)
				return isShiftDown ? 'T' : 't';

			if (key == Keys.U)
				return isShiftDown ? 'U' : 'u';

			if (key == Keys.V)
				return isShiftDown ? 'V' : 'v';

			if (key == Keys.W)
				return isShiftDown ? 'W' : 'w';

			if (key == Keys.X)
				return isShiftDown ? 'X' : 'x';

			if (key == Keys.Y)
				return isShiftDown ? 'Y' : 'y';

			if (key == Keys.Z)
				return isShiftDown ? 'Z' : 'z';


			if ((key == Keys.D0 && !isShiftDown) || key == Keys.NumPad0)
				return '0';

			if ((key == Keys.D1 && !isShiftDown) || key == Keys.NumPad1)
				return '1';

			if ((key == Keys.D2 && !isShiftDown) || key == Keys.NumPad2)
				return '2';

			if ((key == Keys.D3 && !isShiftDown) || key == Keys.NumPad3)
				return '3';

			if ((key == Keys.D4 && !isShiftDown) || key == Keys.NumPad4)
				return '4';

			if ((key == Keys.D5 && !isShiftDown) || key == Keys.NumPad5)
				return '5';

			if ((key == Keys.D6 && !isShiftDown) || key == Keys.NumPad6)
				return '6';

			if ((key == Keys.D7 && !isShiftDown) || key == Keys.NumPad7)
				return '7';

			if ((key == Keys.D8 && !isShiftDown) || key == Keys.NumPad8)
				return '8';

			if ((key == Keys.D9 && !isShiftDown) || key == Keys.NumPad9)
				return '9';


			if (key == Keys.D0 && isShiftDown)
				return ')';

			if (key == Keys.D1 && isShiftDown)
				return '!';

			if (key == Keys.D2 && isShiftDown)
				return '@';

			if (key == Keys.D3 && isShiftDown)
				return '#';

			if (key == Keys.D4 && isShiftDown)
				return '$';

			if (key == Keys.D5 && isShiftDown)
				return '%';

			if (key == Keys.D6 && isShiftDown)
				return '^';

			if (key == Keys.D7 && isShiftDown)
				return '&';

			if (key == Keys.D8 && isShiftDown)
				return '*';

			if (key == Keys.D9 && isShiftDown)
				return '(';


			if (key == Keys.Space)
				return ' ';

			if (key == Keys.Tab)
				return '\t';

			if (key == Keys.Enter)
				return (char) 13;

			if (key == Keys.Back)
				return (char) 8;


			if (key == Keys.Add)
				return '+';

			if (key == Keys.Decimal)
				return '.';

			if (key == Keys.Divide)
				return '/';

			if (key == Keys.Multiply)
				return '*';

			if (key == Keys.OemBackslash)
				return '\\';

			if (key == Keys.OemComma && !isShiftDown)
				return ',';

			if (key == Keys.OemComma && isShiftDown)
				return '<';

			if (key == Keys.OemOpenBrackets && !isShiftDown)
				return '[';

			if (key == Keys.OemOpenBrackets && isShiftDown)
				return '{';

			if (key == Keys.OemCloseBrackets && !isShiftDown)
				return ']';

			if (key == Keys.OemCloseBrackets && isShiftDown)
				return '}';

			if (key == Keys.OemPeriod && !isShiftDown)
				return '.';

			if (key == Keys.OemPeriod && isShiftDown)
				return '>';

			if (key == Keys.OemPipe && !isShiftDown)
				return '\\';

			if (key == Keys.OemPipe && isShiftDown)
				return '|';

			if (key == Keys.OemPlus && !isShiftDown)
				return '=';

			if (key == Keys.OemPlus && isShiftDown)
				return '+';

			if (key == Keys.OemMinus && !isShiftDown)
				return '-';

			if (key == Keys.OemMinus && isShiftDown)
				return '_';

			if (key == Keys.OemQuestion && !isShiftDown)
				return '/';

			if (key == Keys.OemQuestion && isShiftDown)
				return '?';

			if (key == Keys.OemQuotes && !isShiftDown)
				return '\'';

			if (key == Keys.OemQuotes && isShiftDown)
				return '"';

			if (key == Keys.OemSemicolon && !isShiftDown)
				return ';';

			if (key == Keys.OemSemicolon && isShiftDown)
				return ':';

			if (key == Keys.OemTilde && !isShiftDown)
				return '`';

			if (key == Keys.OemTilde && isShiftDown)
				return '~';

			if (key == Keys.Subtract)
				return '-';

			return null;
		}
	}
}