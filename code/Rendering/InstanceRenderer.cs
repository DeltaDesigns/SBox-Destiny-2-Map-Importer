using System;

[Title( "Destiny Instance Renderer" )]
[Category( "Rendering" )]
[Icon( "nature" )]
public sealed class InstanceRenderer : Component, Component.ExecuteInEditor
{
	[Property] public Model InstanceModel;
	[Property, Hide] public Transform[] Transforms;
	[Property] public SceneLayerType RenderLayer;

	private IDisposable renderHook;
	protected override void OnStart()
	{
		renderHook?.Dispose();
		if ( RenderLayer == SceneLayerType.Translucent )
			renderHook = Game.ActiveScene.Camera.AddHookAfterTransparent( "TransparentRenderer", 0, RenderInstances );
		else
			renderHook = Game.ActiveScene.Camera.AddHookAfterOpaque( "OpaqueRenderer", 0, RenderInstances );
	}

	protected override void OnDisabled()
	{
		renderHook?.Dispose();
		renderHook = null;
	}

	private void RenderInstances( SceneCamera c )
	{
		Graphics.DrawModelInstanced( InstanceModel, Transforms, Scene.RenderAttributes );
	}
}
