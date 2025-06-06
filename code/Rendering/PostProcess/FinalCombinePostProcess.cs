using System;
using System.Numerics;

namespace Sandbox;

[Title( "Destiny Final Combine" )]
[Category( "Post Processing" )]
[Icon( "grain" )]
public sealed class DestinyFinalCombine : PostProcess, Component.ExecuteInEditor
{
	private Rendering.CommandList commandsPP;
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

	//private Tonemapping test;

	protected override void OnEnabled()
	{
		commandsPP = new( "Destiny Final Combine" );
		commands = new( "Destiny Final Combine 2" );

		commandsTime = new( "Destiny Time CL" );

		//test = Components.Get<Tonemapping>( FindMode.InSelf );

		OnDirty();
		Camera.AddCommandList( commandsTime, Rendering.Stage.AfterDepthPrepass );
		Camera.AddCommandList( commands, Rendering.Stage.AfterDepthPrepass );
		Camera.AddCommandList( commandsPP, Rendering.Stage.BeforePostProcess );
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

	[Button( "test" ), Group( "Debug" )]
	public void Test()
	{
		var rot = Camera.WorldRotation;
		var pos = Camera.WorldPosition;

		//Log.Info( $"Screen Size: {Camera.ScreenRect.Size}, ZNear {Camera.ZNear}, ZFar {Camera.ZFar} ({Camera.ZFar / (Camera.ZNear - Camera.ZFar)}) ({(Camera.ZFar / (Camera.ZNear - Camera.ZFar)) * Camera.ZNear})" );
		//Log.Info( $"M1 ({perspective.M11},{perspective.M12},{perspective.M13},{perspective.M14})" );
		//Log.Info( $"M2 ({perspective.M21},{perspective.M22},{perspective.M23},{perspective.M24})" );
		//Log.Info( $"M3 ({perspective.M31},{perspective.M32},{perspective.M33},{perspective.M34})" );
		//Log.Info( $"M4 ({perspective.M41},{perspective.M42},{perspective.M43},{perspective.M44})" );
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
			commandsPP.Attributes.GrabFrameTexture( "ColorBuffer" );
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
