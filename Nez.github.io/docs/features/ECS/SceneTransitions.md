---
id: SceneTransitions
title: Scene Transitions
---

Every game needs transition effects to have a polished look and feel. With Nez, transitions can be done between Scenes and also within a Scene. There are several built-in transitions and Nez makes it easy to make your own, custom transition effects.



## Using Included Transitions
All of the included transitions can be used intra-Scene (within the same Scene) and between two different Scenes. Some of them have configuration options that control how the transition occurs. Below are a couple examples:

```csharp
// loads up a new Scene with a WindTransition. Note that you provide a `Func<Scene>` to provide the Scene to transition to.
Core.StartSceneTransition( new WindTransition( () => new YourNextScene() ) );


// transitions within the current Scene with a SquaresTransition
var transition = new SquaresTransition();

// for intra-Scene transitions we will probably be interested in knowing when the screen is obscured so we can take action
transition.OnScreenObscured = MyOnScreenObscuredMethod;
Core.StartSceneTransition( transition );


void MyOnScreenObscuredMethod()
{
	// move Camera to new location
	// reset Entities
}
```



## Custom Transitions
The real power of transitions comes when creating your own custom transitions that match your games style. You can use the included transitions as examples to base yours off of. This guide will go over the details, hows and whys of everything as well.

Transitions generally come in two flavors: one part and two part. A one part transition will obscure the screen with a render of the previous Scene, load a new Scene and then transition from the old render to the new Scene's render with an effect (for example, slide the old render off the screen). A two part transition will first perform a transition effect, then load the new Scene and then transition to displaying the new Scene (for example, fade to black, load new Scene, fade to new Scene). Either way, the process is very similar.

- subclass `SceneTransition`
- if you are using an `Effect` load it up in the constructor so it is ready to use
- `Render` is called every frame so that you can control the final render output. You can use the `_isNewSceneLoaded` flag for two part transitions to determine if you are on part one or two.
- override `OnBeginTransition`. This method will compose the bulk of the transition code. It is called in a coroutine so you can yield to control flow.
	- (optional) for two part transitions, you will want to perform your first part (example, fade to black)
	- yield a call to load up the next Scene: `yield return Core.StartCoroutine( LoadNextScene() )`. Note that you should do this even for intra-Scene transitions. Nez will take care of properly setting the `_isNewSceneLoaded` flag even for intra-Scene transitions to make the code for two part transitions the same for both cases.
	- perform your transition (example, fade out the previous Scene render to show the new Scene)
	- call `TransitionComplete` which will end the transition and cleanup the RenderTarget
	- unload any Effects/Textures that you used. Alternatively, you can override `TransitionComplete` and do cleanup there. Just be sure to call base!

The `SceneTransition` class has a handy little helper method that you can use if you are using an Effect for your transition. `TickEffectProgressProperty` lets you yield a call to it in a coroutine and it will set a `_progress` property on your Effect either from 0 - 1 or from 1 - 0. You can use this for both one or two part transitions. See the included transition shaders for examples.

Let's take a look at an example of a transition that uses an Effect. The comments break down exactly what is happening.

```csharp
public class SuperTransition : SceneTransition
{
	/// <summary>
	/// duration for the transition
	/// </summary>
	public float Duration = 1f;

	/// <summary>
	/// ease equation to use for the animation
	/// </summary>
	public EaseType EaseType = EaseType.QuartOut;

	Effect _effect;
	Rectangle _destinationRect;


	/// <summary>
	/// constructor. Note that sceneLoadAction can be null for intra-Scene transitions and everything will still work as expected.
	/// </summary>
	public SuperTransition( Func<Scene> sceneLoadAction ) : base( sceneLoadAction, true )
	{
		// store off the bounds of the scene render so we can display it in the render method
		_destinationRect = previousSceneRender.Bounds;

		// load the Effect
		_effect = Core.content.loadEffect( "TransitionEffect.mgfxo" );
	}


	public override IEnumerator OnBeginTransition()
	{
		// load up the new Scene. If sceneLoadAction is null this will just set the _isNewSceneLoaded flag to true.
		yield return Core.StartCoroutine( LoadNextScene() );

		// use our Effect to the transition to the new Scene. This call will tick the _progress EffectParameter from 0 to 1 with our
		// custom EaseType for extra control.
		yield return Core.StartCoroutine( TickEffectProgressProperty( _effect, duration, easeType ) );

		// call through to let SceneTransition know we are all done and it can clean itself up and stop calling render
		TransitionComplete();

		// cleanup our Effect since we are done with it
		Core.Content.UnloadEffect( _effect );
	}


	public override void Render( Batcher batcher )
	{
		// here, we are just going to render directly to the back buffer using our Effect. We render the previousSceneRender which
		// SceneTransition gets for us. It contains the last render of the previous Scene.
		Core.graphicsDevice.SetRenderTarget( null );
		batcher.Begin( BlendState.NonPremultiplied, Core.DefaultSamplerState, DepthStencilState.None, null, _effect );
		batcher.Draw( previousSceneRender, _destinationRect, Color.White );
		bbatcher.End();
	}
}
```

