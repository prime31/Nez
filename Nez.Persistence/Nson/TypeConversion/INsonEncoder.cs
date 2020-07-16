namespace Nez.Persistence
{
	public interface INsonEncoder
	{
		/// <summary>
		/// writes a key/value pair. <paramref name="value"/> can any NSON encodeable value.
		/// </summary>
		/// <param name="key">Key.</param>
		/// <param name="value">Value.</param>
		void EncodeKeyValuePair(string key, object value);
	}
}
