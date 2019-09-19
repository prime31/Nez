Nez Persistence: Binary
==========

## Features

* Ultra high performance, minimal allocation binary format
* Built in support for primitive types, nullables, arrays, lists and more
* Easily add support for any types you require
* Handy key/value storage for lightweight preferences (`KeyValueDataStore`)
* Unit tested.


## Usage: FileDataStore

The API is namespaced under `Nez.Persistence.Binary` and the primary class is `FileDataStore`. There are only a few methods you need to know, listed below. The `FileDataStore` can save any class that implements `IPersistable`. By default, it stores the data in a very effiecient binary format. You can optionally store data in text format for debugging by passing in `FileFormat.Text` to the constructor. The constructor also takes in an optional `persistantDataPath` that will be used for saving files. If no `persistantDataPath` is provided `Utils.GetStorageRoot()` will be used.

It is recommended to create a single `FileDataStore` and store it in the `GameServiceContainer` for easy access.

```csharp
namespace Nez.Persistence
{
	public class FileDataStore
	{
		public FileDataStore( string persistentDataPath, FileFormat fileFormat = FileFormat.Binary )
		public void Save( string filename, IPersistable persistable )
		public void Load( string filename, IPersistable persistable )
		public void Clear()
	}
}
```

Custom classes that need to be serialized must implement `IPersistable`. When they want to be saved they should pass a filename and themself to the `FileDataStore`. Loading works the same way. When either `Save` or `Load` is called the relevant `IPersistable` methods will be called on the class so that it can save/load itself. An example implementation is below.

```csharp
public class PersistableExample : IPersistable
{
	public List<string> strings = new List<string>();
	public int anInt;
	public bool? nullableBool;

	void IPersistable.Persist( IPersistableWriter writer )
	{
		// write out your data. Most types can be written out of the box
		writer.Write( strings );
		writer.Write( anInt );
		writer.Write( nullableBool );
	}

	void IPersistable.Recover( IPersistableReader reader )
	{
		// read your data IN THE SAME ORDER YOUR WROTE IT!
		reader.ReadStringListInto( strings );
		anInt = reader.ReadInt();
		nullableBool = reader.ReadOptionalBool();
	}
}

// saving the object
var myObj = new PersistableExample();
var dataStore = Core.services.GetOrAddService<FileDataStore>();
dataStore.Save( "the-filename.bin", myObj );

// loading the object. If the file doesn't exist, the Recover method will not be called
dataStore.Load( "the-filename.bin", myObj );
```


## Usage: KeyValueDataStore

The `KeyValueDataStore` is for storing small bits of data. It contains a default instance accessible via the `Default` ivar. Be sure to call `Load` before using it the first time, preferably at application init. Calling `Flush` at application shutdown will save the data.

You can also create your own instances via the constructor passing in the filename of your choosing. You have the option of saving/loading from any `FileDataStore` by passing the `FileDataStore` to `Load` or `Flush`.

Useage example:

```csharp
// fetch the FileDataStore from your service container
var fileDataStore = Core.services.GetOrAddService<FileDataStore>();

// load data
KeyValueDataStore.Load( fileDataStore );


// setting data
KeyValueDataStore.Default.Set( "the-key", true ); // values can be of type string, bool, int or float.

// getting data
var data = KeyValueDataStore.Default.GetBool( "the-key" );
var data = KeyValueDataStore.Default.GetBool( "the-key", false ); // specifying a default value if the key isnt present

// checking for existence of a key
if( KeyValueDataStore.Default.ContainsBoolKey( "the-key" ) )
	// do something
	
// deleting a keys data
KeyValueDataStore.Default.DeleteBoolKey( "the-key" );


// saving data
KeyValueDataStore.Flush( fileDataStore );
```
