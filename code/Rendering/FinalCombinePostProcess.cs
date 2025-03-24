namespace Sandbox;

[Title( "Destiny Final Combine" )]
[Category( "Post Processing" )]
[Icon( "grain" )]
public sealed class DestinyFinalCombine : PostProcess, Component.ExecuteInEditor
{
	private Rendering.CommandList commandsPP;
	private Rendering.CommandList commands;

	[Property, Range( 0f, 500f ), MakeDirty]
	public float TimeScale { get; set; } = 1f;

	[Property, MakeDirty]
	public bool DebugBlit { get; set; } = false;

	//private Tonemapping test;

	protected override void OnEnabled()
	{
		commandsPP = new( "Destiny Final Combine" );
		commands = new( "Destiny Final Combine 2" );

		//test = Components.Get<Tonemapping>( FindMode.InSelf );

		Camera.AddCommandList( commands, Rendering.Stage.AfterDepthPrepass );
		Camera.AddCommandList( commandsPP, Rendering.Stage.BeforePostProcess );
	}

	protected override void OnPreRender()
	{
		base.OnPreRender();
		//Scene.RenderAttributes.Set( "CurrentTime", RealTime.Now * TimeScale );
		commands?.SetGlobal( "CurrentTime", RealTime.Now * TimeScale );
	}

	protected override void OnDirty()
	{
		base.OnDirty();

		if ( commandsPP is not null )
		{
			commandsPP.Reset();
			SetCommands();
		}
		if ( commands is not null )
		{
			commands.Reset();
		}
	}

	public void SetCommands()
	{
		if ( DebugBlit )
		{
			commandsPP.GrabFrameTexture( "ColorBuffer" );
			commandsPP.Blit( Material.FromShader( "pipelines/d2_final_combine.shader" ) );
		}
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
		Camera.RemoveCommandList( commandsPP );
		commands = null;
		commandsPP = null;
	}
}
