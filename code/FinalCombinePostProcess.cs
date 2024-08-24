using System;

namespace Sandbox;

[Title( "Destiny Final Combine" )]
[Category( "Post Processing" )]
[Icon( "grain" )]
public sealed class DestinyFinalCombine : PostProcess, Component.ExecuteInEditor
{
	IDisposable renderHook;
	protected override void OnEnabled()
	{
		renderHook = Camera.AddHookBeforeOverlay( "Destiny Final Combine", 1000, RenderEffect );
	}

	protected override void OnDisabled()
	{
		renderHook?.Dispose();
		renderHook = null;
	}

	RenderAttributes attributes = new RenderAttributes();

	public void RenderEffect( SceneCamera camera )
	{
		if ( !camera.EnablePostProcessing )
			return;

		Scene.RenderAttributes.Set( "CurrentTime", RealTime.Now );

		// Pass the FrameBuffer to the shader
		Graphics.GrabFrameTexture( "ColorBuffer", attributes );

		// Blit a quad across the entire screen with our custom shader
		Graphics.Blit( Material.FromShader( "shaders/d2_final_combine.shader" ), attributes );
	}
}
