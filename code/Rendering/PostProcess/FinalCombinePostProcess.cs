using System;
using System.Numerics;

namespace Sandbox;

[Title( "Destiny Final Combine" )]
[Category( "Post Processing" )]
[Icon( "grain" )]
public sealed class DestinyFinalCombine : PostProcess, Component.ExecuteInEditor
{
	private Rendering.CommandList commands;
	private Rendering.CommandList commandsDebug;
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

	//private Tonemapping test;

	protected override void OnEnabled()
	{
		commandsDebug = new( "Destiny Debug" );
		commands = new( "Destiny Final Combine" );

		commandsTime = new( "Destiny Time CL" );

		OnDirty();
		Camera.AddCommandList( commandsTime, Rendering.Stage.AfterDepthPrepass );
		Camera.AddCommandList( commands, Rendering.Stage.AfterDepthPrepass );
		Camera.AddCommandList( commandsDebug, Rendering.Stage.AfterPostProcess );
	}

	protected override void OnPreRender()
	{
		base.OnPreRender();
		commandsTime?.GlobalAttributes.Set( "CurrentTime", RealTime.Now * TimeScale );


		if ( DebugText )
		{
			Vector2 coord = Screen.Size;
			DebugOverlay.ScreenText( new Vector2( 10.0f, coord.y - 200 ), $"Position: {Camera.GameObject.Parent.WorldPosition:F2}", 16, flags: TextFlag.LeftBottom, color: Color.FromRgb( 0x00ffff ) );
			DebugOverlay.ScreenText( new Vector2( 10.0f, coord.y - 175 ), $"Camera Position: {Camera.WorldPosition:F2}", 16, flags: TextFlag.LeftBottom, color: Color.FromRgb( 0x00ffff ) );
			DebugOverlay.ScreenText( new Vector2( 10.0f, coord.y - 150 ), $"Camera Forward: {Camera.WorldRotation.Forward:F2}", 16, flags: TextFlag.LeftBottom, color: Color.FromRgb( 0x00ffff ) );
			DebugOverlay.ScreenText( new Vector2( 10.0f, coord.y - 125 ), $"Camera Right: {Camera.WorldRotation.Right:F2}", 16, flags: TextFlag.LeftBottom, color: Color.FromRgb( 0x00ffff ) );
			DebugOverlay.ScreenText( new Vector2( 10.0f, coord.y - 100 ), $"Camera Rotation: {Camera.WorldRotation:F2}", 16, flags: TextFlag.LeftBottom, color: Color.FromRgb( 0x00ffff ) );
		}

		var perspective = ProjectionMatrix( Camera.ScreenRect.Size );
		commands?.GlobalAttributes.Set( "WorldToProj", perspective );
	}

	public Matrix4x4 ProjectionMatrix( Vector2 ScreenSize )
	{
		var world_to_camera = Matrix4x4.CreateLookAtLeftHanded( Camera.WorldPosition / 39.37f, (Camera.WorldPosition / 39.37f) + Camera.WorldRotation.Forward, Camera.WorldRotation.Up );
		var camera_to_projective = Matrix4x4.CreatePerspectiveFieldOfViewLeftHanded( Camera.FieldOfView.DegreeToRadian(), ScreenSize.x / ScreenSize.y, Camera.ZNear, Camera.ZFar );

		return (camera_to_projective * world_to_camera);
	}

	public Matrix4x4 ViewMatrix( Vector2 ScreenSize )
	{
		return Matrix4x4.CreateLookAt( Camera.WorldPosition, Camera.WorldPosition + Camera.WorldRotation.Forward, Vector3.Up );
	}

	protected override void OnDirty()
	{
		base.OnDirty();

		if ( commandsDebug is not null )
		{
			commandsDebug.Reset();
			SetCommands();
		}
		if ( commands is not null )
		{
			commands.Reset();
			SetCommands();
		}
	}

	public void SetCommands()
	{
		//commands?.GlobalAttributes.Set( "FrameTimeOfDay", 0.5f );
		commands?.GlobalAttributes.Set( "ExposureScale", ExposureScale );
		commands?.GlobalAttributes.Set( "ExposureIllumRelative", ExposureIllumRelative );

		if ( DebugBlit )
		{
			commandsDebug.Attributes.GrabFrameTexture( "ColorBuffer" );
			commandsDebug.Blit( Material.FromShader( "pipelines/d2_final_combine.shader" ) );
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
		Camera.RemoveCommandList( commandsDebug );
		commands = null;
		commandsDebug = null;
	}
}
