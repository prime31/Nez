namespace Nez.Tools.Atlases
{
	class MainClass
	{
		static string inputFolder = "";
		static string outputFilename = "adventurer";


		public static void Main( string[] args )
		{
			System.Console.WriteLine( $"CurDir {System.Environment.CurrentDirectory}" );
			var arg = $"-image:{outputFilename}.png -map:{outputFilename}.atlas -r -fps:7 {inputFolder}";
			var arguments = arg.Split( new char[] { ' ' } );
			var res = Console.Program.Main(arguments);
			System.Console.WriteLine( $"result {res}" );
		}
	}
}
