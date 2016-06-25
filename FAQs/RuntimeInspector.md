Runtime Inspector
==========
Nez includes some really handy runtime Entity inspection facilities. You can access the inspector by opening the debug console (via the tilde key) and then using the `inspect` command. Out of the box the inspector can inspect the following types: int, float, string, bool and Transform.


## Exposing Properties and Fields in the Inspector
By default, the inspector will display any public properties/fields that are of a supported type. The inspector can also display private fields/properties by just adding the `Inspectable` attribute:

```csharp
[Inspectable]
string myPrivateField;
```

Int and float fields/properties can optionally be displayed with a slider by adding the `Range` attribute. Note that you do not have to add both the `Inspectable` and `Range` attributes for private fields/properties. Just the `Range` attribute is enough to let the inspector know you want it displayed.

```csharp
[Range( 0.1f, 100 )]
float groundAccel;

// the third, optional parameter lets you specify the sliders step value
[Range( 0.1f, 100, 5 )]
float airAccel;
```



## Extending the Inspector
You can display any custom types in the inspector as well by writing your own custom inspectors. You can do this by adding the `CustomInspector` attribute on the class that you want to make a custom inspector for (YourClass in the example below). The attribute takes in a single parameter which is the Type of the `Inspector` subclass that manages the UI for the class (YourClassInspector in the example). Note that the `Inspector` subclass is wrapped in C&#35;if/C&#35;endif so that it is only compiled into debug builds.

The `Inspector` class provides several helpers to assist with making custom inspectors. It will cache access to the getter/setter for the field/property for easy access. It wraps access to the getter/setter via the `getValue` and `setValue` methods which are generic and take care of casting for you. If you want to add your own custom attributes on the field/property they are accessible via the `getFieldOrPropertyAttribute` generic method.


```csharp
[CustomInspector( typeof( YourClassInspector ) )]
public class YourClass
{
	bool _isBlue { get; set; }
	float _friction;
	// the rest of your class
}


C&#35;if DEBUG
public class YourClassInspector : Inspector
{
	// this is where you setup your UI and add it to the table
	public override void initialize( Table table, Skin skin )
	{}
	
	// this is where you update the UI
	public override void update()
	{}
}
C&#35;endif
```