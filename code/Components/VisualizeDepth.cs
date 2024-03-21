using System;

namespace Sandbox;

[Title( "Visualize Depth Buffer" )]
[Category( "UI" )]
[Icon( "desktop_windows" )]
public class VisualizeDepth : Component
{
	[Property, Range( 0.0f, 1f )] public float Opacity { get; set; } = 1f;

	[Property] public Shader Shader { get; set; }

	IDisposable renderHook;

	protected override void OnEnabled()
	{
		var cam = Components.Get<CameraComponent>();
		renderHook = cam.AddHookAfterUI( "visualize_depth_buffer", 0, RenderEffect );
	}

	protected override void OnFixedUpdate()
	{

	}

	protected override void OnDestroy()
	{

	}

	void RenderEffect( SceneCamera camera )
	{
		var material = Material.FromShader( Shader );

		using var rt = RenderTarget.GetTemporary( 1, ImageFormat.Default, ImageFormat.None );
		Graphics.RenderTarget = rt;
		Graphics.Attributes.SetCombo( "D_WORLDPANEL", 0 );
		Graphics.Viewport = new Rect( 0, new Vector2( rt.Width, rt.Height ) );
		Graphics.Clear();

		Graphics.RenderTarget = null;
		Graphics.Blit( material, Graphics.Attributes );
	}
}
