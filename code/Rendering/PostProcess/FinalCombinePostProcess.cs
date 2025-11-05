using System;

namespace Sandbox;

[Title( "Destiny Final Combine" )]
[Category( "Post Processing" )]
[Icon( "grain" )]
public sealed class DestinyFinalCombine : BasePostProcess, Component.ExecuteInEditor
{
	private Rendering.CommandList commands;
	private Rendering.CommandList commandsLightShafts;
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

	#region Light Shafts
	public Material DepthZFar => Material.FromShader( Shader.Load( "Pipelines/d2_depth_zfar.shader" ) );
	public Material RadialBlur8 => Material.FromShader( Shader.Load( "Pipelines/d2_radial_blur_8.shader" ) );
	public Material RadialBlur12 => Material.FromShader( Shader.Load( "Pipelines/d2_radial_blur_12.shader" ) );

	[Property, Feature( "Light Shafts" )]
	public LightShaftMode LightShaftQuality { get; set; } = LightShaftMode.Medium;

	[Property, Feature( "Light Shafts" ), MakeDirty, Range( 0f, 2000000f )]
	public float LightShaftDistance { get; set; } = 500000f;

	#region Shaft Steps
	// radial_blur_8 steps
	private Vector4[] _high8Steps = new Vector4[]
	{
		new Vector4( 1f,1f,1f,1f ),
		new Vector4( 1f,1f,1f,1f ),
		new Vector4( 1f,1f,1f,1f ),
		new Vector4( 1f,1f,1f,1f ),
		new Vector4( 0.0625f, 0.00f, 0.00f, 0.00f )
	};

	private Vector4[] _medium8Steps = new Vector4[]
	{
		new Vector4( 1f,1f,1f,1f ),
		new Vector4( 1f,1f,1f,1f ),
		new Vector4( 0f,0f,0f,0f ),
		new Vector4( 0f,0f,0f,0f ),
		new Vector4( 0.125f, 0.00f, 0.00f, 0.00f )
	};

	// radial_blur_12 steps
	private Vector4[] _high12Steps = new Vector4[]
	{
		new Vector4( 0.92045f, 0.84737f, 0.78076f, 0.72063f ),
		new Vector4( 0.6633f, 0.61064f, 0.56264f, 0.51931f ),
		new Vector4( 0.478f, 0.44005f, 0.40546f, 0.37423f ),
		new Vector4( 0.34446f, 0.31711f, 0.29219f, 0.26968f ),
		new Vector4( 0.11701f, 0.00f, 0.00f, 0.00f )
	};

	private Vector4[] _medium12Steps = new Vector4[]
	{
		new Vector4( 0.84737f, 0.72063f, 0.61491f, 0.52224f ),
		new Vector4( 0.44253f, 0.37634f, 0.32113f, 0.27273f ),
		new Vector4( 0f,0f,0f,0f ),
		new Vector4( 0f,0f,0f,0f ),
		new Vector4( 0.24284f, 0.00f, 0.00f, 0.00f )
	};
	#endregion

	#endregion



	private DestinyAtmosphere Atmosphere => DestinyAtmosphere.Get();

	protected override void OnEnabled()
	{
		commands = new( "Frame Scope Exposure" );
		commandsLightShafts = new( "Light Shaft Radial Blur" );
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
		commandsLightShafts.Reset();

		commandsTime?.GlobalAttributes.Set( "CurrentTime", RealTime.Now * TimeScale );
		commands?.GlobalAttributes.Set( "ExposureScale", ExposureScale );
		commands?.GlobalAttributes.Set( "ExposureIllumRelative", ExposureIllumRelative );

		InsertCommandList( commands, Rendering.Stage.AfterDepthPrepass, 1, "Frame Scope Exposure" );
		InsertCommandList( commandsTime, Rendering.Stage.AfterDepthPrepass, 0, "Frame Scope Time" );

		if ( LightShaftQuality != LightShaftMode.Off )
		{
			RenderLightShafts();
		}
		else
		{
			commandsLightShafts.GlobalAttributes.Set( "RadialBlur12", Helpers.SolidRedTexture );
		}

		InsertCommandList( commandsLightShafts, Rendering.Stage.AfterSkybox, 1, "Light Shaft Radial Blur" );

		if ( DebugBlit )
		{
			var blit = BlitMode.WithBackbuffer( Material.FromShader( "pipelines/d2_final_combine.shader" ), Rendering.Stage.AfterPostProcess, int.MaxValue, false );
			Blit( blit, "Debug Blit" );
		}
	}

	public void RenderLightShafts()
	{
		Vector2 rtSize2 = Camera.ScreenRect.Size / 2;
		Vector2 rtSize4 = Camera.ScreenRect.Size / 4;
		Vector2 rtSize8 = Camera.ScreenRect.Size / 8;
		//Log.Info( rtSize );

		var farPosition = Camera.WorldPosition + Atmosphere.SunDirectionVector;
		var screenPosition = Camera.PointToScreenNormal( farPosition, out bool isBehind );

		commandsLightShafts.Attributes.Set( "LightShaftDistance", LightShaftDistance );
		commandsLightShafts.Attributes.Set( "LightShaftDir", new Vector4( screenPosition.x, screenPosition.y, isBehind ? -1 : 1, 1 ) );

		var depthZFar = commandsLightShafts.GetRenderTarget( "DepthZFar", (int)rtSize4.x, (int)rtSize4.y, ImageFormat.RGBA16161616F );
		var radial_blur_8 = commandsLightShafts.GetRenderTarget( "RadialBlur8", (int)rtSize8.x, (int)rtSize8.y, ImageFormat.RGBA16161616F );
		var radial_blur_12 = commandsLightShafts.GetRenderTarget( "RadialBlur12", (int)rtSize8.x, (int)rtSize8.y, ImageFormat.RGBA16161616F );

		commandsLightShafts.Attributes.Set( "RTDimensions", new Vector4( rtSize4.x, rtSize4.y, 1 / rtSize4.x, 1 / rtSize4.y ) );
		commandsLightShafts.Attributes.Set( "RTDimensions2", new Vector4( rtSize2.x, rtSize2.y, 1 / rtSize2.x, 1 / rtSize2.y ) );

		// depth_zfar
		commandsLightShafts.SetRenderTarget( depthZFar );
		commandsLightShafts.Blit( DepthZFar );
		commandsLightShafts.Attributes.Set( "DepthZFar", depthZFar.ColorTexture );

		commandsLightShafts.Attributes.Set( "RTDimensions", new Vector4( rtSize8.x, rtSize8.y, 1 / rtSize8.x, 1 / rtSize8.y ) );

		SetQualitySteps( true, LightShaftQuality );

		// radial_blur_8
		commandsLightShafts.SetRenderTarget( radial_blur_8 );
		commandsLightShafts.Blit( RadialBlur8 );
		commandsLightShafts.Attributes.Set( "RadialBlur8", radial_blur_8.ColorTexture );

		SetQualitySteps( false, LightShaftQuality );

		// radial_blur_12
		commandsLightShafts.SetRenderTarget( radial_blur_12 );
		commandsLightShafts.Blit( RadialBlur12 );
		commandsLightShafts.GlobalAttributes.Set( "RadialBlur12", radial_blur_12.ColorTexture );

		commandsLightShafts.ClearRenderTarget();
		commandsLightShafts.ReleaseRenderTarget( depthZFar );
		commandsLightShafts.ReleaseRenderTarget( radial_blur_8 );
		commandsLightShafts.ReleaseRenderTarget( radial_blur_12 );
	}

	private Vector4[] GetQualitySteps( bool radial8, LightShaftMode mode )
	{
		if ( radial8 )
		{
			return mode == LightShaftMode.High ? _high8Steps : _medium8Steps;
		}
		else // 12
		{
			return mode == LightShaftMode.High ? _high12Steps : _medium12Steps;
		}
	}

	private void SetQualitySteps( bool radial8, LightShaftMode mode )
	{
		var steps = GetQualitySteps( radial8, mode );
		for ( int i = 0; i < 5; i++ )
		{
			commandsLightShafts.Attributes.Set( $"QualitySteps{i + 1}", steps[i] );
		}
	}

	public enum LightShaftMode
	{
		Off,
		Medium,
		High
	}
}
