## Getting Started: The Basics

From this point on it is assumed that you have a standard MonoGame project setup and that you have at least skimmed over the Nez Systems from the README. Nez is a [PCL](http://www.hanselman.com/blog/CrossPlatformPortableClassLibrariesWithNETAreHappening.aspx) so the platform of your project doesnt matter. You can either add the Nez project to your solution or just add the Nez DLL as a reference.

Nez needs to have the DefaultContent/NezDefaultBMFont.xnb file placed in your projects Content/nez folder. It sets up the font as the default for all text in it's debug console and for your own use. If you intend to use any of the built in Effects or PostProcessors you should also copy the DefaultContent/effects folder contents into your projects Content/nez/effects folder. Be sure to set the Build Action to Content so they get copied into your compiled game.

Your standard Game class (which is created for you by the MonoGame templates) needs to subclass Nez.Core. All parameters passed to the Core constructor are optional.

```cs
public class Game1 : Core
{
    public Game1() : base( width: 1280, height: 768, isFullScreen: false, enableEntitySystems: false )
    {}
}
```



## Creating Your First Scene

Scenes are the root object that you add your Entities to. They can be thought of as levels in your game, menus, etc. All further examples in this guide will just add to the Initialize method in the Game1 class. Lets add to the code above and create our first Scene.

```cs
public class Game1 : Core
{
    public Game1() : base( width: 1280, height: 768, isFullScreen: false, enableEntitySystems: false )
    {}
    
    
    protected override void Initialize()
    {
		base.Initialize();
		Window.AllowUserResizing = true;
		
		// create our Scene with the DefaultRenderer and a clear color of CornflowerBlue
		var myScene = Scenes.createWithDefaultRenderer( Color.CornflowerBlue );
		
		// set the scene so Nez can take over
		scene = myScene;
	}
}
```


## Adding Some Entities

Now that we have our Scene up and running we need to add Some Entities to it. Entities house your Components which is where most of your game logic lives.

```cs
protected override void Initialize()
{
	base.Initialize();
	Window.AllowUserResizing = true;
		
	// create our Scene with the DefaultRenderer and a clear color of CornflowerBlue
	var myScene = Scenes.createWithDefaultRenderer( Color.CornflowerBlue );

    // setup our Scene by adding some Entities
    var entityOne = scene.createEntity( "entity-one" );
    var entityTwo = scene.createEntity( "entity-two" );
		
	// set the scene so Nez can take over
	scene = myScene;
}
```

Now we have two Entities in our Scene but nothing has really changed. An Entity by itself doesn't really do anything. Our Entities need Components added to them so they can become alive. Lets load up a Texture and add it to each Entity so that we have something to look at.

```cs
protected override void Initialize()
{
	base.Initialize();
	Window.AllowUserResizing = true;
		
	// create our Scene with the DefaultRenderer and a clear color of CornflowerBlue
	var myScene = Scenes.createWithDefaultRenderer( Color.CornflowerBlue );

    // load a Texure. Note that the Texture is loaded via the scene.contentManager class. This works just like the standard MonoGame Content class
    // with the big difference being that it is tied to a Scene. When the Scene is unloaded so too is all the content loaded via scene.contentManager.
    var texture = scene.contentManager.Load<Texture2D>( "images/someTexture" );

    // setup our Scene by adding some Entities
    var entityOne = scene.createEntity( "entity-one" );
    entityOne.addComponent( new Sprite( texture ) );
    
    var entityTwo = scene.createEntity( "entity-two" );
    entityTwo.addComponent( new Sprite( texture ) );
    
    // move entityTwo to a new location so it isn't overlapping entityOne
    entityTwo.transform.position = new Vector2( 200, 200 );
		
	// set the scene so Nez can take over
	scene = myScene;
}
```

Assuming you have added a png/xnb file to your project in the Content/images folder when you run your game you should now see the two Entities. We used the built in Sprite class to display the Texture.

It should be noted that the Scene class can also be subclassed so that you can keep your code organized. When subclassing, Scene creation can be done via `Scene.create<T>`, `Scene.createWithDefaultRenderer<T>` or simplying newing an instance of your Scene subclass. The `initialize` method will be called on your Scene so that you can set things up.

That's it for this tutorial. We now know how to create and setup a Scene, create Entities and add Components to them.