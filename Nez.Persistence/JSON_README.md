Nez Persistence: JSON
==========

## Description

A little JSON library that does big things.


## Features

* Transmogrify objects into JSON and back again.
* Uses reflection to dump and load object graphs automagically.
* Supports object graphs with references preserved.
* Supports primitives, classes, structs, enums, lists, dictionaries and arrays.
* Supports single dimensional arrays, multidimensional arrays and jagged arrays.
* Polymorphic classes supported with a type hint encoded into the JSON automatically.
* Supports optionally pretty printing JSON output.
* Supports optionally encoding properties and private fields (via the `JsonIncludeAttribute` on them).
* Supports optionally excluding public fields (via the `JsonExcludeAttribute` on them)
* Supports decoding fields and properties from aliased names.
* Unit tested.


## Usage

The API is namespaced under `Nez.Persistence` and the primary class is `Json`. There are really only a few methods you need to know:

```csharp
namespace Nez.Persistence
{
	public static class Json
	{
		public static string ToJson( object obj, JsonSettings options = null )
		public static object FromJson( string json, JsonSettings settings = null )
		public static void FromJsonOverwrite( string json, object item )
	}
}
```

`ToJson()` will take a C# object, list, dictionary or primitive value type and turn it into JSON.

```csharp
var data = new List<int>() { { 0 }, { 1 }, { 2 } };
Console.WriteLine( Json.ToJson( data ) ); // output: [1,2,3]
```


`FromJson()` will load a string of JSON, returns `null` if invalid or an object if successful.

```csharp
var data = Json.FromJson( "{\"foo\": 1, \"bar\": 2.34}" ) as IDictionary;
var i = Convert.ToInt32( data["foo"] );
var f = Convert.ToSingle( data["bar"] );
```

`FromJson<T>()` will load a string of JSON, returns `null` if invalid or a an object of type `T` if successful.

```csharp
var obj = Json.FromJson<SomeClass>( json );
```


Json can also handle classes, structs, enums and nested objects. Given these definitions:

```csharp
enum TestEnum
{
	Thing1,
	Thing2,
	Thing3
}


struct TestStruct
{
	public int x;
	public int y;
}


class TestClass
{
	public string name;
	public TestEnum type;
	public List<TestStruct> data = new List<TestStruct>();

	[JsonExclude]
	public int _ignored;

	[BeforeEncode]
	public void BeforeEncode()
	{
		Console.WriteLine( "BeforeEncode callback fired!" );
	}

	[AfterDecode]
	public void AfterDecode()
	{
		Console.WriteLine( "AfterDecode callback fired!" );
	}
}
```

The following code:

```csharp
var testClass = new TestClass();
testClass.name = "Rumpelstiltskin Jones";
testClass.type = TestEnum.Thing2;
testClass.data.Add( new TestStruct() { x = 1, y = 2 } );
testClass.data.Add( new TestStruct() { x = 3, y = 4 } );
testClass.data.Add( new TestStruct() { x = 5, y = 6 } );

var testClassJson = Json.ToJson( testClass, prettyPrint: true );
Console.WriteLine( testClassJson );
```

Will output (if pretty printed):

```json
{
	"name": "Rumpelstiltskin Jones",
	"type": "Thing2",
	"data": [
		{
			"x": 1,
			"y": 2
		},
		{
			"x": 3,
			"y": 4
		},
		{
			"x": 5,
			"y": 6
		}
	]
}
```

And then you can rehydrate your object from the JSON using `FromJson<T>`:

```csharp
var obj = Json.FromJson<TestClass>( testClassJson );
```


You can also use `FromJsonOverwrite` to reconstruct partial or full JSON data back into an existing object. It will overwrite any properties/fields on the object with the data in JSON:

```csharp
var testClass = new TestClass();
Json.FromJsonOverwrite( json, testClass );
```

Finally, you'll notice that `TestClass` has the methods `BeforeEncode()` and `AfterDecode()` which have the `BeforeEncode` and `AfterDecode` attributes. These methods will be called *before* the object starts being serialized and *after* the object has been fully deserialized. This is useful when some further preparation or initialization logic is required.

By default, public fields and properties are encoded, not private fields. You can tag any field or property to be included with the `Serialized` attribute, or force a public field to be excluded with the `NonSerialized` attribute.


## Decode Aliases

Fields and properties can be decoded from aliases using the `DecodeAlias` attribute. While decoding, if no matching data is found in the JSON for a given field or property, its aliases will also be searched for.

```csharp
class TestClass
{
	[JsonInclude] // note that properties are opt-in! You must tell Json you want them serialized.
	public int index { get; set; }
	
	[DecodeAlias("anotherName")]
	public string name; // decode from "name" or "anotherName"

	[DecodeAlias("anotherNumber", "yetAnotherNumber")]
	public int number; // decode from "number", "anotherNumber", or "yetAnotherNumber"
}
```


## Type Hinting

When decoding polymorphic types, Json has no way of knowing which subclass to instantiate unless a type hint is included. So, optionally Json will add a key named `@type` to each encoded object with the fully qualified type of the object. You can request this by passing a `JsonSettings` object to the `Json.Decode` method:

```csharp
var settings = new JsonSettings()
{
	TypeNameHandling = TypeNameHandling.Auto
};
```


## Reference Handling and Polymorphic Types. The Reason this Code Exists

Json has the ability to detect and restore references (opt in). For example, if a List contains two of the same Entity objects Json has the ability to maintain the references when deserializing and rebuild the same List.

Json can also optionally deal with Polymorphic types. Lets say you have a `Component` class and then a bunch of subclasses of `Component`. If you store these all in a `List<Component>`, most JSON libs will give you back a List full of just `Component` objects even if your list had subclasses in it. Json remedies that issue.
	
Let's look at a simple example of both reference handling and dealing with polymorphic types. Here are our classes for the demonstration:

```csharp
class Entity
{
	public List<Component> components;
}

class Component
{
	public Entity entity;
}

class Sprite : Component
{}
```

Now, let's create an `Entity` and populate it with a `Component` and a `Sprite` then serialize it to JSON. Note the `JsonSettings` object. It lets you opt in to the features. One really neat option is the `TypeNameHandling.Auto`. Json will figure out on the fly if it needs to inject the objects type into the JSON or not. You'll see the results in the JSON below.

```csharp
var entity = new Entity
{
	components = new List<Component> { new Component(), new Sprite() }
};

var settings = new JsonSettings
{
	PrettyPrint = true,
	TypeNameHandling = TypeNameHandling.Auto,
	PreserveReferencesHandling = true
};

// or for convenience since this is used often: var settings = JsonSettings.HandlesReferences
var json = Json.ToJson( entity, settings );
```

And the resulting JSON. What we have in there is some extra metadata Json can use when it decodes this JSON into an Entity again. Each object gets a unique @id field and any references to existing objects get replaced by a @ref field. Magic!

```json
{
	"@id": "1",
	"components": [
		{
			"entity": null
		},
		{
			"@id": "3",
			"@type": "TestTypeHintAndReferences+Sprite",
			"entity": {
				"@ref": "1"
			}
		}
	]
}
```


## Encode Options

Several options are currently available for JSON encoding, and can be passed in as a second parameter to `Json.ToJson()`.

* `PrettyPrint` will output nicely formatted JSON to make it more readable.
* `PreserveReferencesHandling` will add extra metadata into the JSON so an object graph with circular references can be rebuilt
* `TypeNameHandling` lets you specify when type names will be injected into the JSON
* `TypeConverters` lets you augment the encoding/decoding of the object. More on this later.


## Using Generics

For most use cases you can just assign, cast or make your object graph using the API outlined above, but at times you may need to work with the intermediate objects to dig through and iterate over a collection. To do this, just omit the type when calling `FromJson`. You will get back either a primitive, a `List<object>` or a `Dictionary<string, object>`:

```csharp
var list = Json.Decode( "[1,2,3]" );
foreach( var item in list as IList )
{
	var number = item;
	Console.WriteLine( number );
	// note that if you want to strongly type the number `Convert.ChangeType`/`Convert.ToInt32` and friends work best
}

var dict = Json.Decode( "{\"x\":1,\"y\":2}" );
foreach( var pair in dict as IDictionary )
{
	var value = pair.Value;
	Console.WriteLine( pair.Key + " = " + value );
}
```


## Advanced: JsonTypeConverter for custom encoding/decoding

Json lets you add some custom data to the JSON and then fetch it for any strongly typed object. You can also fully take over encoding to JSON writing whatever you want for any particular object. You can do this by creating a `JsonTypeConverter<T>` and implementing the abstract methods. Any time Json comes accross an object of Type `T` it will pass it off to your `JsonObjectConverter`.

The `JsonTypeConverter` indicates what it wants to do via the three properties `CanRead` (defaults to true), `CanWrite` (defaults to true) and `WantsExclusiveWrite` (defaults to false).

If `CanWrite` is true, the `WriteJson` method will be passed an `IJsonEncoder` which can be used to write custom JSON for your object. It will be called *before* the encoder encodes the object's fields and properties. If you do not want the encoder to write any data at all you can override `WantsExclusiveWrite` returning `true` (see second example below).

When encoding the JSON back to an object, if `CanRead` is true, the `OnFoundCustomData` method will be passed any key/value pairs that do not have corresponding fields/properties.


```csharp
class Doodle
{
	public int x;
	public int y;
	public int z;
}

class DoodleJsonConverter : JsonTypeConverter<Doodle>
{
	public override void WriteJson( IJsonEncoder encoder, Doodle value )
	{
		// EncodeKeyValuePair can take any primitive, list, array, dictionary or object
		encoder.EncodeKeyValuePair( "key-that-isnt-on-object", true );
		encoder.EncodeKeyValuePair( "another_key", "with a value" );
	}

	public override void OnFoundCustomData( Doodle instance, string key, object value )
	{
		Debug.log( $"field name: {key}, value: {value}" );
	}
}

```

To use a `JsonTypeConverter` you just have to tell Json about it by sticking it in your JsonSettings object:

```csharp
var doodle = new Doodle();

// convert to JSON. The WriteJson method will be called
var settings = new JsonSettings { TypeConverters = new JsonTypeConverter[] { new DoodleJsonConverter() } };
var json = Json.ToJson( doodle, settings );

// rehydrate the JSON. The OnFoundCustomData will be called twice given the demo code above.
var newDoodle = Json.Decode<Doodle>( json, settings );
```

This example `JsonTypeConverter` fully takes over JSON encoding by returning `true` for `WantsExclusiveWrite`. The only thing the encoder will write in this case is id/reference data for reference tracking if enabled.

```csharp
class WantsExclusiveWriteConverter : JsonTypeConverter<Doodle>
{
	public override bool WantsExclusiveWrite => true;

	public override void WriteJson( IJsonEncoder encoder, Doodle value )
	{
		encoder.EncodeKeyValuePair( "key-that-isnt-on-object", true );
		encoder.EncodeKeyValuePair( "another_key", "with a value" );
		encoder.EncodeKeyValuePair( "string_array", new string[] { "first", "second" } );
	}

	public override void OnFoundCustomData( Doodle instance, string key, object value )
	{}
}


// using the converter
var doodle = new Doodle();

// Convert to JSON. The WriteJson method will be called.
// Since WantsExclusiveWrite is true, no other data will be present in the JSON string.
var settings = new JsonSettings { TypeConverters = new JsonTypeConverter[] { new WantsExclusiveWriteConverter() } };
var json = Json.ToJson( doodle, settings );

// rehydrate the JSON. The OnFoundCustomData will be called three times given the demo code above.
var newDoodle = Json.Decode<Doodle>( json, settings );
```

The resulting json would be the following. Notice that none of the normal `Doodle` data is present:

`{"key-that-isnt-on-object":true,"another_key":"with a value","string_array":["first","second"]}`


## Advanced: JsonObjectFactory to override object creation

Creating a `JsonObjectFactory` lets you override object creation entirely. This is extremely useful when you have an object that needs a specific constructor called or some object-specific setup. You can write any data you want and then override how the object gets instantiated and populated.

Continuing to use our Doodle class above, we will create a `JsonObjectFactory` that overrides object creation:


```csharp
class ObjectFactoryConverter : JsonObjectFactory<Doodle>
{
	public override Doodle Create( Type objectType, IDictionary objectData )
	{
		var doodle = new Doodle();

		doodle.x = Convert.ToInt32( objectData["x"] );
		doodle.y = Convert.ToInt32( objectData["y"] );
		doodle.z = Convert.ToInt32( objectData["z"] );

		return doodle;
	}
}
```

`JsonObjectFactory` is actually a subclass of `JsonTypeConverter` so usage is identical. You just add your `JsonObjectFactory` to the `TypeConverters` on the `JsonSettings` object and pass it in to `FromJson`:

```csharp
var settings = new JsonSettings { TypeConverters = new JsonTypeConverter[] { new ObjectFactoryConverter() } };
var newDoodle = Json.FromJson<Doodle>( json, settings );
```


## Meta

Forked and hacked from the excellent [TinyJSON](https://github.com/pbhogan/TinyJSON) by Patrick Hogan [twitter](http://twitter.com/pbhogan)

Released under the [MIT License](http://www.opensource.org/licenses/mit-license.php).

