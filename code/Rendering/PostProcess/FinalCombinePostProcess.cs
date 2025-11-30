using System;

namespace Sandbox;

[Title( "Destiny Final Combine" )]
[Category( "Post Processing" )]
[Icon( "grain" )]
public sealed class DestinyFinalCombine : BasePostProcess, Component.ExecuteInEditor
{
	private Rendering.CommandList commands;
	private Rendering.CommandList commandsTime;

	[Property, Range( 0f, 500f ), MakeDirty]
	public float TimeScale { get; set; } = 1f;

	[Property, Range( 0f, 5f ), MakeDirty]
	public float ExposureScale { get; set; } = 0.15f;

	[Property, Range( -5f, 5f ), MakeDirty]
	public float ExposureIllumRelative { get; set; } = 1f;

	[Property, MakeDirty, Group( "Debug" )]
	public bool DebugBlit { get; set; } = false;
	[Property, Group( "Debug" )]
	public bool DebugText { get; set; }

	protected override void OnEnabled()
	{
		commands = new( "Frame Scope Exposure" );
		commandsTime = new( "Frame Scope Time" );

		OnDirty();
	}

	protected override void OnPreRender()
	{
		base.OnPreRender();

		if ( DebugText )
		{
			Vector2 coord = Screen.Size;
			DebugOverlay.ScreenText( new Vector2( 10.0f, coord.y - 200 ), $"Position: {Camera.GameObject.Parent.WorldPosition:F2}", 16, flags: TextFlag.LeftBottom, color: Color.FromRgb( 0x00ffff ) );
			DebugOverlay.ScreenText( new Vector2( 10.0f, coord.y - 175 ), $"Camera Position: {Camera.WorldPosition:F2}", 16, flags: TextFlag.LeftBottom, color: Color.FromRgb( 0x00ffff ) );
			DebugOverlay.ScreenText( new Vector2( 10.0f, coord.y - 150 ), $"Camera Forward: {Camera.WorldRotation.Forward:F2}", 16, flags: TextFlag.LeftBottom, color: Color.FromRgb( 0x00ffff ) );
			DebugOverlay.ScreenText( new Vector2( 10.0f, coord.y - 125 ), $"Camera Right: {Camera.WorldRotation.Right:F2}", 16, flags: TextFlag.LeftBottom, color: Color.FromRgb( 0x00ffff ) );
			DebugOverlay.ScreenText( new Vector2( 10.0f, coord.y - 100 ), $"Camera Rotation: {Camera.WorldRotation:F2}", 16, flags: TextFlag.LeftBottom, color: Color.FromRgb( 0x00ffff ) );
		}
	}

	protected override void OnDirty()
	{
		base.OnDirty();
	}

	protected override void OnStart()
	{
		base.OnStart();

		Game.ActiveScene.Camera.ZNear = 1;
		Game.ActiveScene.Camera.ZFar = 50000000f;
	}

	public override void Render()
	{
		commands.Reset();
		commandsTime.Reset();

		Game.ActiveScene.RenderAttributes.Set( "CurrentTime", RealTime.Now * TimeScale );
		//commandsTime?.GlobalAttributes.Set( "CurrentTime", RealTime.Now * TimeScale );
		commands?.GlobalAttributes.Set( "ExposureScale", ExposureScale );
		commands?.GlobalAttributes.Set( "ExposureIllumRelative", ExposureIllumRelative );

		InsertCommandList( commands, Rendering.Stage.AfterDepthPrepass, 1, "Frame Scope Exposure" );
		//InsertCommandList( commandsTime, Rendering.Stage.AfterDepthPrepass, int.MinValue, "Frame Scope Time" );

		if ( DebugBlit )
		{
			var blit = BlitMode.WithBackbuffer( Material.FromShader( "pipelines/d2_final_combine.shader" ), Rendering.Stage.AfterPostProcess, int.MaxValue, false );
			Blit( blit, "Debug Blit" );
		}
	}

}
