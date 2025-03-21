namespace Sandbox;

[Title( "Destiny Final Combine" )]
[Category( "Post Processing" )]
[Icon( "grain" )]
public sealed class DestinyFinalCombine : PostProcess, Component.ExecuteInEditor
{
	private Rendering.CommandList commands;
	[Property, Range( 0f, 500f )] public float TimeScale { get; set; } = 1f;

	protected override void OnEnabled()
	{
		commands = new( "Destiny Final Combine" );

		commands.GrabFrameTexture( "ColorBuffer" );
		commands.Blit( Material.FromShader( "pipelines/d2_final_combine.shader" ) );
		Camera.AddCommandList( commands, Rendering.Stage.AfterUI );
	}

	protected override void OnPreRender()
	{
		base.OnPreRender();
		Scene.RenderAttributes.Set( "CurrentTime", RealTime.Now * TimeScale );
	}

	protected override void OnStart()
	{
		base.OnStart();

		Game.ActiveScene.Camera.ZNear = 1;
		Game.ActiveScene.Camera.ZFar = 50000000f;//float.PositiveInfinity;
	}

	protected override void OnDisabled()
	{
		Camera.RemoveCommandList( commands );
		commands = null;
	}
}
