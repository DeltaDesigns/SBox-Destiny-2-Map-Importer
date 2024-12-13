using System;

[Title( "Destiny Instance Renderer" )]
[Category( "Rendering" )]
[Icon( "nature" )]
public sealed class InstanceRenderer : Component, Component.ExecuteInEditor
{
	[Property] public Model InstanceModel;
	[Property, Hide] public Transform[] Transforms;
	[Property] public FeatureType ObjectType;

	private IDisposable renderHook;
	private DecoratorSceneObject DecorSceneObj;
	private SkySceneObject SkySceneObj;

	protected override void OnStart()
	{
		renderHook?.Dispose();
		renderHook = null;
		RenderInstances();
		//if ( RenderLayer == SceneLayerType.Translucent )
		//	renderHook = Game.ActiveScene.Camera.AddHookAfterTransparent( "TransparentRenderer", 0, RenderInstancesTransparent );
		//else
		//renderHook = Game.ActiveScene.Camera.AddHookAfterOpaque( "OpaqueRenderer", 0, RenderInstancesTransparent );
	}

	protected override void OnDisabled()
	{
		renderHook?.Dispose();
		renderHook = null;

		DecorSceneObj?.Delete();
		DecorSceneObj = null;

		SkySceneObj?.Delete();
		SkySceneObj = null;
	}

	//protected override void OnDestroy()
	//{
	//	base.OnDestroy();
	//	DecorSceneObj?.Delete();
	//	DecorSceneObj = null;

	//	SkySceneObj?.Delete();
	//	SkySceneObj = null;
	//}

	private void RenderInstancesTransparent( SceneCamera c )
	{
		Graphics.DrawModelInstanced( InstanceModel, Transforms, Scene.RenderAttributes );
	}

	private void RenderInstances()
	{
		switch ( ObjectType )
		{
			case FeatureType.Sky:
				SkySceneObj = new( Scene.SceneWorld, InstanceModel, Transforms );
				//renderHook = Game.ActiveScene.Camera.AddHookAfterTransparent( "TransparentRenderer", 1, RenderInstancesTransparent );
				break;
			case FeatureType.Decorator:
				DecorSceneObj = new( Scene.SceneWorld, InstanceModel, Transforms );
				break;
		}
	}

	public enum FeatureType
	{
		None = 0,
		Static = 1,
		Entity = 2,
		Decorator = 3,
		Sky = 4,
	}
}
