using static Nez.Tools.Atlases.SpriteAtlasPacker;

namespace Nez.Tools.Atlases.Console
{
	public static class Program
	{
		public static int Main(string[] args)
		{
			var arguments = ProgramArguments.Parse(args);
			if (arguments == null || (arguments != null && arguments.input == null))
				return (int)FailCode.FailedParsingArguments;

			return PackSprites(arguments.ToConfig());
		}
	}
}
