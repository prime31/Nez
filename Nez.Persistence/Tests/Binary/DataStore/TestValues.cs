using System.Collections.Generic;
using System.Linq;

namespace Nez.Persistence.Binary.Tests
{
	public static class TestValues
	{
		public static string aString = "stringy";
		public static int aInt = 6789;
		public static uint aUint = 84953;
		public static double aDouble = 23432462.34;
		public static float aFloat = 3262.34f;
		public static bool aBool = true;

		public static bool? aNullableBool = null;
		public static int[] aIntArray = { 1, 2, 3, 4, 9, 8, 7, 6 };
		public static float[] aFloatArray = { 23.2f, 24.3f, 32.1f, 6843.567f };
		public static string[] aStringArray = { "one", "two", "three", "four", "five" };
		public static List<int> aIntList = new List<int>( aIntArray );
		public static List<float> aFloatList = new List<float>( aFloatArray );
		public static List<string> aStringList = new List<string>( aStringArray );

	}

	public class PersistableExample : IPersistable
	{
		public List<string> strings = new List<string>();
		public int anInt;
		public bool? nullableBool;

		void IPersistable.Persist( IPersistableWriter writer )
		{
			writer.Write( strings );
			writer.Write( anInt );
			writer.Write( nullableBool );
		}

		void IPersistable.Recover( IPersistableReader reader )
		{
			reader.ReadStringListInto( strings );
			anInt = reader.ReadInt();
			nullableBool = reader.ReadOptionalBool();
		}

		public override bool Equals( object obj )
		{
			var other = obj as PersistableExample;
			return anInt == other.anInt && nullableBool == other.nullableBool && strings.SequenceEqual( other.strings );
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}