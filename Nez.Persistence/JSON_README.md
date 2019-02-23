Nez Persistence: JSON
==========

## Description

Forked and hacked from the excellent [TinyJSON](https://github.com/pbhogan/TinyJSON) by Patrick Hogan [twitter](http://twitter.com/pbhogan)


## Features

* Transmogrify objects into JSON and back again.
* Uses reflection to dump and load object graphs automagically.
* Supports object graphs with references preserved.
* Supports primitives, classes, structs, enums, lists, dictionaries and arrays.
* Supports single dimensional arrays, multidimensional arrays and jagged arrays.
* Parsed data uses proxy variants that can be implicitly cast to primitive types for cleaner code, or directly encoded back to JSON.
* Numeric types are handled without fuss.
* Polymorphic classes supported with a type hint encoded into the JSON.
* Supports optionally pretty printing JSON output.
* Supports optionally encode properties and private fields (via the SerializedAttribute on them).
* Supports decoding fields and properties from aliased names.
* Unit tested.


## Usage

The API is namespaced under `Nez.Persistence` and the primary class is `Json`. There are really only a few methods you need to know:

```csharp
namespace Nez.Persistence
{
	public static class Json
	{
		public static string Encode( object obj, JsonSettings options = null )
		public static Variant Decode( string json )
		public static T Decode<T>( string json )
	}
}
```

`Encode()` will take a C# object, list, dictionary or primitive value type and turn it into JSON.

```csharp
var data = new List<int>() { { 0 }, { 1 }, { 2 } };
Console.WriteLine( Json.Encode( data ) ); // output: [1,2,3]
```


`Decode()` will load a string of JSON, returns `null` if invalid or a `Variant` proxy object if successful. The proxy allows for implicit casts and can convert between various C# numeric value types.

```csharp
var data = Json.Decode( "{\"foo\": 1, \"bar\": 2.34}" );
int i = data["foo"];
float f = data["bar"];
```

`Decode<T>()` will load a string of JSON, returns `null` if invalid or a an object of type `T` if successful.

```csharp
var obj = Json.Decode<SomeClass>( json );
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

	[NonSerialized]
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

var testClassJson = Json.Encode( testClass );
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

And then you can rehydrate your object from the JSON using `Decode<T>`:

```
var obj = Json.Decode<TestClass>( testClassJson );
```


You can also use `Decode` or `DecodeInto` to reconstruct JSON data back into an object if you opted to load a `Variant`:

```csharp
TestClass testClass;
Json.DecodeInto( JSON.Load( testClassJson ), out testClass );
```

Finally, you'll notice that `TestClass` has the methods `BeforeEncode()` and `AfterDecode()` which have the `BeforeEncode` and `AfterDecode` attributes. These methods will be called *before* the object starts being serialized and *after* the object has been fully deserialized. This is useful when some further preparation or initialization logic is required.

By default, public fields and properties are encoded, not private fields. You can tag any field or property to be included with the `SerializedAttribute` attribute, or force a public field to be excluded with the `NonSerialized` attribute.


## Decode Aliases

Fields and properties can be decoded from aliases using the `DecodeAlias` attribute. While decoding, if no matching data is found in the JSON for a given field or property, its aliases will also be searched for.

```csharp
class TestClass
{
	[Serialized] // note that properties are opt-in! You must tell Json you want them serialized.
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

Now, let's create an `Entity` and populate it with a `Component` and a `Sprite` then serialize it to JSON. Note the `JsonSettings` object. It lets you opt in to the features. Once really neat option is the `TypeNameHandling.Auto`. Json will figure out on the fly if it needs to inject the objects type into the JSON or not. You'll see the results in the JSON below.

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
var json = Json.Encode( entity, settings );
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

Several options are currently available for JSON encoding, and can be passed in as a second parameter to `JSON.Encode()`.

* `PrettyPrint` will output nicely formatted JSON to make it more readable.
* `PreserveReferencesHandling` will add extra metadata into the JSON so an object graph with circular references can be rebuilt
* `TypeNameHandling` lets you specify when type names will be injected into the JSON
* `EnforceHeirarchyOrderEnabled` will ensure fields and properties are encoded in class heirarchy order, from the root base class on down, but comes at a slight performance cost.


## Using Variants

For most use cases you can just assign, cast or make your object graph using the API outlined above, but at times you may need to work with the intermediate proxy objects to, say, dig through and iterate over a collection. To do this, cast the `Variant` to the appropriate subclass (likely either `ProxyArray` or `ProxyObject`) and you're good to go:

```csharp
var list = Json.Decode( "[1,2,3]" );
foreach( var item in list as ProxyArray )
{
	int number = item;
	Console.WriteLine( number );
}

var dict = Json.Decode( "{\"x\":1,\"y\":2}" );
foreach( var pair in dict as ProxyObject )
{
	float value = pair.Value;
	Console.WriteLine( pair.Key + " = " + value );
}
```

The non-collection `Variant` subclasses are `ProxyBoolean`, `ProxyNumber` and `ProxyString`. A variant can also be `null`. Any `Variant` object can be directly encoded to JSON by calling its `ToJson()` method or passing it to `Json.Encode()`.

Variant's can also be turned back into strongly typed objects via the `VariantConverter.Decode<T>` method.


## Advanced: JsonTypeConverter for custom encoding/decoding

Json lets you fully take over the encoding to JSON and the conversion back to a strongly typed object. You can do this by creating an `JsonTypeConverter<T>` and implementing the abstract methods. Any time Json comes accross an object of Type `T` it will pass it off to your `JsonObjectConverter`. Note that you can turn down the job by overriding `CanRead` or `CanWrite`.

The `WriteJson` method will be passed a `IJsonEncoder` which can be used to write the JSON for your object. The `ConvertToObject` method will be passed an `IObjectConverter` which can be used to convert the raw data back into your object.

You can add fields, remove fields or set any data on the object that you want when overriding these methods.


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
		encoder.EncodeValue( true );
	}

	public override Doodle ConvertToObject( IObjectConverter converter, Type objectType, Doodle existingValue, ProxyObject data )
	{
		var doodle = new Doodle();
		foreach( var bits in data )
		{
			// do something with data
			Debug.log( $"field name: {bits.Key}, value: {bits.Value}" );
		}

		return doodle;
	}
}

```

To use a `JsonTypeConverter` you just have to tell Json about it by sticking it in your JsonSettings object or using one of the convenience methods:

```csharp
var doodle = new Doodle();

var settings = new JsonSettings { TypeConverters = new JsonTypeConverter[] { new DoodleJsonConverter() } };
var json = Json.Encode( doodle, settings );
var newDoodle = Json.Decode<Doodle>( json, settings );
```

Released under the [MIT License](http://www.opensource.org/licenses/mit-license.php).

