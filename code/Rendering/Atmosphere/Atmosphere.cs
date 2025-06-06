using Sandbox.Rendering;
using System;

namespace Sandbox;

[Title( "Destiny Atmosphere" )]
[Category( "Rendering" )]
[Icon( "cloud" )]
public sealed class DestinyAtmosphere : Renderer, Renderer.ExecuteInEditor
{
	// -- Atmosphere properties ---

	[Property] public Texture Texture0 { get; set; }
	[Property] public Texture Texture1 { get; set; }
	[Property] public Texture Texture3 { get; set; } //= Texture.Load( $"Pipelines/Textures/depth_angle_lookup_temp.vtex" );

	#region Atmosphere
	[Property, Range( 0, 1 ), MakeDirty, Feature( "Atmosphere" )]
	public float Intensity { get; set; } = 0.75f;

	[Property, Range( 0, 1 ), MakeDirty, Feature( "Atmosphere" )]
	public float Rotation { get; set; } = 0f;
	#endregion

	#region Day Cycle
	[Property, Range( 0, 3600 ), Change( "OnTimeOfDayChanged" ), Feature( "Day Cycle" )]
	public float TimeOfDay { get; set; } = 1800f;

	[Property, Range( 0, 1f ), MakeDirty, Feature( "Day Cycle" )]
	public float TimeOfDayNormalized { get; set; } = 0.5f;

	[Property, Range( 1, 3600 ), MakeDirty, Feature( "Day Cycle" ), Title( "Day Length (seconds)" )]
	public int DayLength { get; set; } = 3600;

	[Property, MakeDirty, Feature( "Day Cycle" )]
	public bool UseDayCycle { get; set; } = false;

	[Property, MakeDirty, Feature( "Day Cycle" ), Hide]
	public List<Vector4> DayCycleRotations { get; set; }
	#endregion

	#region Sun
	[Property, MakeDirty, Feature( "Sun" )]
	public bool AffectSceneSun { get; set; } = false;

	[Property, MakeDirty, Feature( "Sun" ), HideIf( "AffectSceneSun", false )]
	public DirectionalLight SunComponent { get; set; }

	[Property, MakeDirty, Feature( "Sun" )]
	public Color SunColor { get; set; } = Color.FromRgb( 0xE5F7FF );

	[Property, MakeDirty, Feature( "Sun" ), Range( 0, 1 )]
	public float SunIntensity { get; set; } = 0.05923f;

	[Property, MakeDirty, Change( "OnSunAngleChanged" ), Feature( "Sun" )]
	public Angles SunDirection { get; set; } = new Angles( 300f, 0f, 0f );

	private Vector3 _sunDirVector;
	private bool isUpdating = false; // Prevents infinite loop

	[Property, MakeDirty, Change( "OnSunDirChanged" ), ReadOnly, Feature( "Sun" )]
	public Vector3 SunDirectionVector
	{
		get => _sunDirVector;
		set
		{
			_sunDirVector = value;
		}
	}

	#endregion

	//------------------------------

	private Vector2 ScreenSize;

	private Texture Texture0_3D;
	private Texture Texture1_3D;

	public Material SkyHemisphereColor => Material.FromShader( Shader.Load( "Pipelines/d2_full_hemisphere_sky_color_generate.shader" ) );
	public Material SkyHemisphereScatter => Material.FromShader( Shader.Load( "Pipelines/d2_sky_hemisphere_seed_inscattering.shader" ) );
	public Material SkyHemisphereBlur => Material.FromShader( Shader.Load( "Pipelines/d2_sky_hemisphere_spherical_blur.shader" ) );
	public Material SkyNear => Material.FromShader( Shader.Load( "Pipelines/d2_sky_lookup_generate_near.shader" ) );
	public Material SkyFar => Material.FromShader( Shader.Load( "Pipelines/d2_sky_lookup_generate_far.shader" ) );
	public Material Sky => Material.FromShader( Shader.Load( "Pipelines/d2_sky.shader" ) );

	public Material AtmosDensityLookup => Material.FromShader( Shader.Load( "Pipelines/d2_atmo_depth_angle_density_lookup_generate.shader" ) );

	private CommandList commandList = new CommandList();
	private CommandList commandListStart = new CommandList();
	private GlobalChannelsController _globalChannels => GlobalChannelsController.Get();

	protected override void OnStart()
	{
		OnSunAngleChanged( SunDirection, SunDirection );
		//if ( Game.ActiveScene.Camera == null ) return;

		if ( Texture0_3D is null && Texture0 is not null )
			Helpers.Create3DTexture( Texture0, out Texture0_3D );
		if ( Texture1_3D is null && Texture1 is not null )
			Helpers.Create3DTexture( Texture1, out Texture1_3D );

		commandListStart = new( "AtmosphereApplyStart" );
		ApplyStartingAttributes();

		commandList = new( "AtmosphereApply" );
		OnDirty();

		Game.ActiveScene.Camera.AddCommandList( commandListStart, Stage.AfterSkybox, 0 );
		Game.ActiveScene.Camera.AddCommandList( commandList, Stage.AfterSkybox, 2 );

		//if ( AtmosphereRenderer is null )
		//	AtmosphereRenderer = new( Scene, this );
	}

	private void ApplyStartingAttributes()
	{
		// Only need applied on start, I think?
		commandListStart.GlobalAttributes.Set( "AtmosTexture0", Texture0_3D ?? Helpers.CreateTransparentTexture3D( 1, 1, 6 ) );
		commandListStart.GlobalAttributes.Set( "AtmosTexture1", Texture1_3D ?? Helpers.CreateTransparentTexture3D( 1, 1, 6 ) );
		commandListStart.GlobalAttributes.Set( "AtmosTexture2", Helpers.CreateFilledTexture( new Color( 1, 0, 0 ) ) ); // TODO: Depth thing for fake god rays
		commandListStart.GlobalAttributes.Set( "AtmosTexture3", Helpers.CreateFilledTexture( new Color( 1, 1, 0 ) ) ); // TODO
		commandListStart.GlobalAttributes.Set( "AtmosDensityLookup", Texture3 ); // TODO-ish

		if ( !UseDayCycle && DayCycleRotations.Count > 0 )
			_globalChannels?.Set( "sun_track_direction", DayCycleRotations[DayCycleRotations.Count / 2] );
	}

	protected override void OnDirty()
	{
		base.OnDirty();

		if ( commandList is null )
			return;

		Rotation = _globalChannels?.Get( "sky_snapshot_rotation" )?.x / 360f ?? Rotation;
		Intensity = _globalChannels?.Get( "sky_snapshot_intensity" )?.x ?? Intensity;

		SunColor = _globalChannels?.Get( "sun_glow_color" ) ?? SunColor;
		SunIntensity = _globalChannels?.Get( "sun_glow_intensity" )?.x ?? SunIntensity;
		SunDirectionVector = _globalChannels?.Get( "sun_track_direction" ) ?? SunDirectionVector;

		commandList.Reset();
		commandList.GlobalAttributes.Set( "AtmosTimeOfDay", new Vector4( TimeOfDayNormalized ) );
		commandList.GlobalAttributes.Set( "FrameTimeOfDay", new Vector4( TimeOfDayNormalized ) );
		commandList.GlobalAttributes.Set( "AtmosIntensity", new Vector4( Intensity ) );
		commandList.GlobalAttributes.Set( "AtmosRotation", new Vector4( Rotation ) );
		commandList.GlobalAttributes.Set( "AtmosSunColor", SunColor );
		commandList.GlobalAttributes.Set( "AtmosSunIntensity", SunIntensity );
		commandList.GlobalAttributes.Set( "AtmosSunDir", SunDirectionVector );

		if ( AffectSceneSun )
		{
			SunComponent = Components.GetOrCreate<DirectionalLight>();
			SunComponent.WorldRotation = SunDirection + new Angles( 180, 0, 0 );
			SunComponent.LightColor =
				((_globalChannels?.Get( "sun_color" ) * (_globalChannels?.Get( "sun_intensity" ) / 100f)) ?? SunColor)
				* (1 - Math.Abs( TimeOfDayNormalized - 0.5f ) * 2); // 0 is morning, 0.5 is noon, 1 is night

			SunComponent.SkyColor = ((_globalChannels?.Get( "skybox_up_ambient_color" ) * _globalChannels?.Get( "skybox_up_ambient_intensity" )) ?? Color.Transparent);
		}

		RenderAtmosphereNew();
	}

	public void RenderAtmosphereNew()
	{
		if ( commandList is null )
			return;

		if ( ScreenSize != Game.ActiveScene.Camera.ScreenRect.Size )
		{
			ScreenSize = Game.ActiveScene.Camera.ScreenRect.Size;
			Vector2 atmosRTsize = ScreenSize / 3;
			commandList.GlobalAttributes.Set( "AtmosRTDimensions", new Vector4( atmosRTsize.x, atmosRTsize.y, 1 / atmosRTsize.x, 1 / atmosRTsize.y ) );
		}

		// full_hemisphere_sky_color_generate
		var hemiSkyColor = commandList.GetRenderTarget( "HemiSkyColor", 512, 512, ImageFormat.RGBA16161616F, numMips: 10 );
		commandList.SetRenderTarget( hemiSkyColor );
		commandList.Clear( Color.Transparent );
		commandList.Blit( SkyHemisphereColor );
		commandList.GlobalAttributes.Set( "AtmosHemisphere", hemiSkyColor.ColorTexture );
		commandList.ClearRenderTarget();
		commandList.ReleaseRenderTarget( hemiSkyColor );

		//---------------------------------------------------------------------------

		var hemiScatter = commandList.GetRenderTarget( "HemiScatter", 512, 512, ImageFormat.RG1616F );
		var hemiBlur = commandList.GetRenderTarget( "HemiBlur", 512, 512, ImageFormat.RG1616F );

		// sky_hemisphere_seed_inscattering
		commandList.SetRenderTarget( hemiScatter );
		commandList.Clear( Color.Transparent );
		commandList.Blit( SkyHemisphereScatter );
		commandList.Attributes.Set( "AtmosHemisphereScatter", hemiScatter.ColorTexture );

		// sky_hemisphere_spherical_blur
		commandList.SetRenderTarget( hemiBlur );
		commandList.Clear( Color.Transparent );
		commandList.Blit( SkyHemisphereBlur );
		commandList.Attributes.Set( "AtmosHemisphereBlur", hemiBlur.ColorTexture );
		commandList.ClearRenderTarget();  // Unbind before disposing

		// Dispose both *after* use
		commandList.ReleaseRenderTarget( hemiScatter );
		commandList.ReleaseRenderTarget( hemiBlur );

		//---------------------------------------------------------------------------

		var rtFar = commandList.GetRenderTarget( "RTFar", (int)ScreenSize.x / 3, (int)ScreenSize.y / 3, ImageFormat.RGBA16161616F );
		var rtNear = commandList.GetRenderTarget( "RTNear", (int)ScreenSize.x / 3, (int)ScreenSize.y / 3, ImageFormat.RGBA16161616F );

		// sky_lookup_generate_far
		commandList.SetRenderTarget( rtFar );
		commandList.Clear( Color.Transparent );
		commandList.Blit( SkyFar );
		commandList.GlobalAttributes.Set( "AtmosFar", rtFar.ColorTexture );

		//sky_lookup_generate_near
		commandList.SetRenderTarget( rtNear );
		commandList.Clear( Color.Transparent );
		commandList.Blit( SkyNear );
		commandList.GlobalAttributes.Set( "AtmosNear", rtNear.ColorTexture );
		commandList.ClearRenderTarget();

		// Dispose both *after* use
		commandList.ReleaseRenderTarget( rtFar );
		commandList.ReleaseRenderTarget( rtNear );


		// atmo_depth_angle_density_lookup_generate
		var density = commandList.GetRenderTarget( "Density", 512, 512, ImageFormat.RGBA16161616F );
		commandList.SetRenderTarget( density );
		commandList.Clear( Color.Transparent );
		commandList.Blit( AtmosDensityLookup );
		commandList.GlobalAttributes.Set( "AtmosDensity", density.ColorTexture );
		commandList.ClearRenderTarget();
		commandList.ReleaseRenderTarget( density );


		//var rt = commandList.GetRenderTarget( "SkyApplyRT", ImageFormat.RGBA1010102 );
		commandList.Blit( Sky );
		//commandList.ReleaseRenderTarget( rt );
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
		if ( UseDayCycle )
		{
			TimeOfDay += Time.Delta;

			if ( TimeOfDay > DayLength )
				TimeOfDay -= DayLength;

			// Use mapped TimeOfDay to get sun directon from DayCycleRotations
			if ( DayCycleRotations != null && DayCycleRotations.Count > 0 )
			{
				float tod_half = TimeOfDay / 2f;

				Vector4 from = DayCycleRotations[tod_half.FloorToInt()];
				Vector4 to = DayCycleRotations[tod_half.CeilToInt()];
				float t = tod_half - tod_half.FloorToInt();
				Vector4 lerpedRotation = Vector4.Lerp( from, to, t );

				_globalChannels?.Set( "sun_track_direction", lerpedRotation );
			}
		}
	}

	private void OnSunDirChanged( Vector3 oldValue, Vector3 newValue )
	{
		if ( isUpdating ) return;
		isUpdating = true;

		SunDirection = newValue.EulerAngles; // Convert vector back to angles

		isUpdating = false;
	}

	private void OnSunAngleChanged( Angles oldValue, Angles newValue )
	{
		if ( isUpdating ) return;
		isUpdating = true;

		SunDirectionVector = Angles.AngleVector( newValue ); // Convert angles to vector

		isUpdating = false;
	}

	private void OnTimeOfDayChanged( float oldValue, float newValue )
	{
		TimeOfDayNormalized = newValue / DayLength; // Normalize to 0-1 range based on DayLength
	}

	protected override void OnPreRender()
	{
		if ( ScreenSize != Game.ActiveScene.Camera.ScreenRect.Size )
		{
			ScreenSize = Game.ActiveScene.Camera.ScreenRect.Size;
			Vector2 atmosRTsize = ScreenSize / 3;
			commandList.Attributes.Set( "AtmosRTDimensions", new Vector4( atmosRTsize.x, atmosRTsize.y, 1 / atmosRTsize.x, 1 / atmosRTsize.y ) );
			Log.Info( $"AtmosphereRenderer2: Screen size updated to {ScreenSize}" );
		}
	}

	protected override void OnDisabled()
	{
		if ( commandList is not null )
		{
			commandList.Reset();
			Game.ActiveScene.Camera?.RemoveCommandList( commandList );
			commandList = null;
		}

		if ( commandListStart is not null )
		{
			commandListStart.Reset();
			Game.ActiveScene.Camera?.RemoveCommandList( commandListStart );
			commandListStart = null;
		}
	}

	protected override void OnDestroy()
	{
		if ( commandList is not null )
		{
			commandList.Reset();
			Game.ActiveScene.Camera?.RemoveCommandList( commandList );
			commandList = null;
		}

		if ( commandListStart is not null )
		{
			commandListStart.Reset();
			Game.ActiveScene.Camera?.RemoveCommandList( commandListStart );
			commandListStart = null;
		}
	}
}
