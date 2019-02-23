namespace Nez.Persistance
{
	public enum PreserveReferencesHandling
	{
		/// <summary>
		/// Do not preserve references when serializing types
		/// </summary>
		None,
		/// <summary>
		/// Preserve references when serializing
		/// </summary>
		All
	}

	public enum TypeNameHandling
	{
		/// <summary>
		/// Do not include the .NET type name when serializing types
		/// </summary>
		None,
		/// <summary>
		/// Include the .NET type name when serializing into a JSON object structure
		/// </summary>
		Objects,
		/// <summary>
		/// Include the .NET type name when serializing into a JSON array structure
		/// </summary>
		Arrays,
		/// <summary>
		/// Always include the .NET type name when serializing
		/// </summary>
		All,
		/// <summary>
		/// Include the .NET type name when the type of the object being serialized is not the same as its declared type
		/// </summary>
		Auto
	}

	public class JsonSettings
	{
		public PreserveReferencesHandling PreserveReferencesHandling = PreserveReferencesHandling.None;
		public TypeNameHandling TypeNameHandling = TypeNameHandling.None;
		public bool PrettyPrint = false;
		public bool EnforceHeirarchyOrderEnabled = false;
	}

}
