using System.Collections.Generic;


namespace Nez.Persistence.Binary
{
	/// <summary>
	/// simple storage of some basic types. This is designed for small amounts of data such as preferences.
	/// </summary>
	public class KeyValueDataStore : IPersistable
	{
		/// <summary>
		/// default instance of the data store for easy access
		/// </summary>
		public static KeyValueDataStore Default = new KeyValueDataStore(kDefaultFileName);

		const string kDefaultFileName = "KeyValueData.bin";

		public bool IsDirty => _isDirty;

		bool _isDirty;
		string _filename;
		Dictionary<string, bool> _boolDict = new Dictionary<string, bool>();
		Dictionary<string, int> _intDict = new Dictionary<string, int>();
		Dictionary<string, float> _floatDict = new Dictionary<string, float>();
		Dictionary<string, string> _stringDict = new Dictionary<string, string>();


		/// <summary>
		/// creates a KeyValueDataStore that will persist and load from <paramref name="filename"/>. Note that you must
		/// manually call Load to do the initial data loading.
		/// </summary>
		/// <param name="filename">Filename.</param>
		public KeyValueDataStore(string filename)
		{
			_filename = filename;
		}


		public void DeleteAll()
		{
			_boolDict.Clear();
			_intDict.Clear();
			_floatDict.Clear();
			_stringDict.Clear();
		}

		/// <summary>
		/// flushes the data to disk
		/// </summary>
		/// <param name="dataStore">Data store.</param>
		public void Flush(FileDataStore dataStore)
		{
			dataStore.Save(_filename, this);
		}

		/// <summary>
		/// restores the data from disk if it is available
		/// </summary>
		/// <param name="dataStore">Data store.</param>
		public void Load(FileDataStore dataStore)
		{
			dataStore.Load(_filename, this);
		}

		#region IPersistable

		void IPersistable.Persist(IPersistableWriter writer)
		{
			if (!_isDirty)
				return;

			var cnt = _boolDict.Count;
			writer.Write(cnt);
			foreach (var kv in _boolDict)
			{
				writer.Write(kv.Key);
				writer.Write(kv.Value);
			}

			cnt = _intDict.Count;
			writer.Write(cnt);
			foreach (var kv in _intDict)
			{
				writer.Write(kv.Key);
				writer.Write(kv.Value);
			}

			cnt = _floatDict.Count;
			writer.Write(cnt);
			foreach (var kv in _floatDict)
			{
				writer.Write(kv.Key);
				writer.Write(kv.Value);
			}

			cnt = _stringDict.Count;
			writer.Write(cnt);
			foreach (var kv in _stringDict)
			{
				writer.Write(kv.Key);
				writer.Write(kv.Value);
			}

			_isDirty = false;
		}

		void IPersistable.Recover(IPersistableReader reader)
		{
			DeleteAll();

			var cnt = reader.ReadInt();
			for (var i = 0; i < cnt; i++)
			{
				var key = reader.ReadString();
				_boolDict[key] = reader.ReadBool();
			}

			cnt = reader.ReadInt();
			for (var i = 0; i < cnt; i++)
			{
				var key = reader.ReadString();
				_intDict[key] = reader.ReadInt();
			}

			cnt = reader.ReadInt();
			for (var i = 0; i < cnt; i++)
			{
				var key = reader.ReadString();
				_floatDict[key] = reader.ReadFloat();
			}

			cnt = reader.ReadInt();
			for (var i = 0; i < cnt; i++)
			{
				var key = reader.ReadString();
				_stringDict[key] = reader.ReadString();
			}
		}

		#endregion

		#region Setters

		public void Set(string key, bool value)
		{
			_isDirty = true;
			_boolDict[key] = value;
		}

		public void Set(string key, int value)
		{
			_isDirty = true;
			_intDict[key] = value;
		}

		public void Set(string key, float value)
		{
			_isDirty = true;
			_floatDict[key] = value;
		}

		public void Set(string key, string value)
		{
			_isDirty = true;
			_stringDict[key] = value;
		}

		#endregion

		#region Getters

		public bool GetBool(string key, bool defaultValue = default(bool))
		{
			if (_boolDict.TryGetValue(key, out var val))
				return val;

			return defaultValue;
		}

		public int GetInt(string key, int defaultValue = default(int))
		{
			if (_intDict.TryGetValue(key, out var val))
				return val;

			return defaultValue;
		}

		public float GetFloat(string key, float defaultValue = default(float))
		{
			if (_floatDict.TryGetValue(key, out var val))
				return val;

			return defaultValue;
		}

		public string GetString(string key, string defaultValue = null)
		{
			if (_stringDict.TryGetValue(key, out var val))
				return val;

			return defaultValue;
		}

		#endregion

		#region Checkers

		public bool ContainsBoolKey(string key) => _boolDict.ContainsKey(key);

		public bool ContainsIntKey(string key) => _intDict.ContainsKey(key);

		public bool ContainsFloatKey(string key) => _floatDict.ContainsKey(key);

		public bool ContainsStringKey(string key) => _stringDict.ContainsKey(key);

		#endregion

		#region Deleters

		public void DeleteBoolKey(string key)
		{
			if (_boolDict.ContainsKey(key))
			{
				_isDirty = true;
				_boolDict.Remove(key);
			}
		}

		public void DeleteIntKey(string key)
		{
			if (_intDict.ContainsKey(key))
			{
				_isDirty = true;
				_intDict.Remove(key);
			}
		}

		public void DeleteFloatKey(string key)
		{
			if (_floatDict.ContainsKey(key))
			{
				_isDirty = true;
				_floatDict.Remove(key);
			}
		}

		public void DeleteStringKey(string key)
		{
			if (_stringDict.ContainsKey(key))
			{
				_isDirty = true;
				_stringDict.Remove(key);
			}
		}

		#endregion
	}
}