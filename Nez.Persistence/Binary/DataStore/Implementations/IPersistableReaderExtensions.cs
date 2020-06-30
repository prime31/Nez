using System.Collections.Generic;


namespace Nez.Persistence.Binary
{
	public static class IPersistableReaderExtensions
	{
		public static bool? ReadOptionalBool(this IPersistableReader self)
		{
			var hasValue = self.ReadBool();
			if (!hasValue)
				return null;

			return self.ReadBool();
		}

		public static void ReadStringListInto(this IPersistableReader self, List<string> list, bool clearList = true)
		{
			if (clearList)
				list.Clear();

			var cnt = self.ReadInt();
			for (var i = 0; i < cnt; i++)
				list.Add(self.ReadString());
		}

		public static void ReadIntListInto(this IPersistableReader self, List<int> list, bool clearList = true)
		{
			if (clearList)
				list.Clear();

			var cnt = self.ReadInt();
			for (var i = 0; i < cnt; i++)
				list.Add(self.ReadInt());
		}

		public static void ReadFloatListInto(this IPersistableReader self, List<float> list, bool clearList = true)
		{
			if (clearList)
				list.Clear();

			var cnt = self.ReadInt();
			for (var i = 0; i < cnt; i++)
				list.Add(self.ReadFloat());
		}

		public static string[] ReadStringArray(this IPersistableReader self)
		{
			var cnt = self.ReadInt();
			var arr = new string[cnt];

			for (var i = 0; i < cnt; i++)
				arr[i] = self.ReadString();
			return arr;
		}

		public static int[] ReadIntArray(this IPersistableReader self)
		{
			var cnt = self.ReadInt();
			var arr = new int[cnt];

			for (var i = 0; i < cnt; i++)
				arr[i] = self.ReadInt();
			return arr;
		}

		public static float[] ReadFloatArray(this IPersistableReader self)
		{
			var cnt = self.ReadInt();
			var arr = new float[cnt];

			for (var i = 0; i < cnt; i++)
				arr[i] = self.ReadFloat();
			return arr;
		}
		
		public static T[] ReadPersistableArray<T>(this IPersistableReader self) where T : IPersistable, new()
		{
			var cnt = self.ReadInt();
			var arr = new T[cnt];

			for (var i = 0; i < cnt; i++)
			{
				var persisted = new T();
				self.ReadPersistableInto(persisted);
				arr[i] = persisted;
			}
				
			return arr;
		}

		public static void ReadPersistableInto(this IPersistableReader self, IPersistable persistable)
		{
			persistable.Recover(self);
		}
	}
}