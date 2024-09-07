using System;

namespace Sandbox;

[Title( "Destiny Final Combine" )]
[Category( "Post Processing" )]
[Icon( "grain" )]
public sealed class DestinyFinalCombine : PostProcess, Component.ExecuteInEditor
{
	IDisposable renderHook;
	private Material DeferredMaterial;
	protected override void OnEnabled()
	{
		renderHook?.Dispose();
		renderHook = null;

		DeferredMaterial = Material.Load( "Shaders/d2_deferred_shading.vmat" );
		if ( renderHook is null )
			renderHook = Camera.AddHookBeforeOverlay( "Destiny Final Combine", 1000, RenderEffect );
	}

	protected override void OnStart()
	{
		base.OnStart();

		Game.ActiveScene.Camera.ZNear = 1;
		//Game.ActiveScene.Camera.ZFar = float.PositiveInfinity;
	}

	protected override void OnDisabled()
	{
		renderHook?.Dispose();
		renderHook = null;
	}

	protected override void OnDestroy()
	{
		renderHook?.Dispose();
		renderHook = null;
	}

	RenderAttributes attributes = new RenderAttributes();

	private Vertex[] screenQuad;
	public void RenderEffect( SceneCamera camera )
	{
		if ( !camera.EnablePostProcessing )
			return;

		Scene.RenderAttributes.Set( "CurrentTime", RealTime.Now );

		//if ( screenQuad == null )
		//{
		//	Vector3 v1 = new Vector3( -0.5f, 0.5f, 0f );   // Top-left
		//	Vector3 v2 = new Vector3( 0.5f, 0.5f, 0f );    // Top-right
		//	Vector3 v3 = new Vector3( -0.5f, -0.5f, 0f );  // Bottom-left
		//	Vector3 v4 = new Vector3( 0.5f, -0.5f, 0f );   // Bottom-right

		//	screenQuad = new Vertex[4];
		//	screenQuad[0].Position = v1;
		//	screenQuad[1].Position = v3;
		//	screenQuad[2].Position = v2;
		//	screenQuad[3].Position = v2;

		//}
		//Graphics.GrabFrameTexture( "ColorBuffer", attributes );
		//Graphics.Draw( screenQuad.AsSpan(), 6, DeferredMaterial, attributes, Graphics.PrimitiveType.TriangleStrip );

		Graphics.GrabFrameTexture( "ColorBuffer", attributes );
		Graphics.Blit( Material.FromShader( "shaders/d2_final_combine.shader" ), attributes );
	}
}
