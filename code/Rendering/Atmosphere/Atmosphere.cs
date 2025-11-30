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

	[Property, MakeDirty, Feature( "Atmosphere" )]
	public bool UseAtmosphericFog { get; set; } = false;

	#region Unk Extern Values
	[Property, MakeDirty, Feature( "Atmosphere" ), Group( "Unk Extern Values" )]
	public Vector4 AtmosUnk180 { get; set; } = new Vector4( 0.378f, 0.429f, 0.45f, 0f );
	[Property, MakeDirty, Feature( "Atmosphere" ), Group( "Unk Extern Values" )]
	public Vector4 AtmosUnk1D0 { get; set; } = new Vector4( 0 );
	[Property, MakeDirty, Feature( "Atmosphere" ), Group( "Unk Extern Values" )]
	public Vector4 AtmosUnk210 { get; set; } = new Vector4( 0 );

	[Property, MakeDirty, Feature( "Atmosphere" ), Group( "Unk Extern Values" )]
	public float AtmosUnk74 { get; set; } = 0f;
	[Property, MakeDirty, Feature( "Atmosphere" ), Group( "Unk Extern Values" )]
	public float AtmosUnk78 { get; set; } = 0f;
	[Property, MakeDirty, Feature( "Atmosphere" ), Group( "Unk Extern Values" )]
	public float AtmosUnk150 { get; set; } = -0.85f;
	[Property, MakeDirty, Feature( "Atmosphere" ), Group( "Unk Extern Values" )]
	public float AtmosUnk154 { get; set; } = 1.329f;
	[Property, MakeDirty, Feature( "Atmosphere" ), Group( "Unk Extern Values" )]
	public float AtmosFogIntensity { get; set; } = 0.9f;
	[Property, MakeDirty, Feature( "Atmosphere" ), Group( "Unk Extern Values" )]
	public float AtmosUnk164 { get; set; } = 0.1f;
	[Property, MakeDirty, Feature( "Atmosphere" ), Group( "Unk Extern Values" )]
	public float AtmosUnk168 { get; set; } = 12f;
	[Property, MakeDirty, Feature( "Atmosphere" ), Group( "Unk Extern Values" )]
	public float AtmosUnk16C { get; set; } = 20012f;
	[Property, MakeDirty, Feature( "Atmosphere" ), Group( "Unk Extern Values" )]
	public float AtmosUnk170 { get; set; } = 0.03f;
	[Property, MakeDirty, Feature( "Atmosphere" ), Group( "Unk Extern Values" )]
	public float AtmosUnk190 { get; set; } = 1f;
	[Property, MakeDirty, Feature( "Atmosphere" ), Group( "Unk Extern Values" )]
	public float AtmosUnk194 { get; set; } = 0.109f;
	[Property, MakeDirty, Feature( "Atmosphere" ), Group( "Unk Extern Values" )]
	public float AtmosUnk198 { get; set; } = 5.939f;

	[Property, MakeDirty, Feature( "Atmosphere" ), Group( "Unk Extern Values" )]
	public float AtmosUnk1BC { get; set; } = 0.1f;
	[Property, MakeDirty, Feature( "Atmosphere" ), Group( "Unk Extern Values" )]
	public float AtmosUnk1C0 { get; set; } = 0.55f;
	[Property, MakeDirty, Feature( "Atmosphere" ), Group( "Unk Extern Values" )]
	public float AtmosUnk1C4 { get; set; } = 0f;
	[Property, MakeDirty, Feature( "Atmosphere" ), Group( "Unk Extern Values" )]
	public float AtmosUnk1E0 { get; set; } = -0.85f;
	[Property, MakeDirty, Feature( "Atmosphere" ), Group( "Unk Extern Values" )]
	public float AtmosUnk1E8 { get; set; } = 0.2f;
	[Property, MakeDirty, Feature( "Atmosphere" ), Group( "Unk Extern Values" )]
	public float AtmosUnk1EC { get; set; } = 0f;
	[Property, MakeDirty, Feature( "Atmosphere" ), Group( "Unk Extern Values" )]
	public float AtmosUnk1F8 { get; set; } = 0f;
	[Property, MakeDirty, Feature( "Atmosphere" ), Group( "Unk Extern Values" )]
	public float AtmosUnk1FC { get; set; } = 0f;
	[Property, MakeDirty, Feature( "Atmosphere" ), Group( "Unk Extern Values" )]
	public float AtmosUnk208 { get; set; } = 0f;
	#endregion

	#endregion

	#region Day Cycle
	[Property, Range( 0, 3600 ), Change( "OnTimeOfDayChanged" ), Feature( "Day Cycle" )]
	public float TimeOfDay { get; set; } = 1800f;

	[Property, Range( 0, 1f ), MakeDirty, Feature( "Day Cycle" )]
	public float TimeOfDayNormalized { get; set; } = 0.5f;

	//[Property, Range( -1, 1f ), MakeDirty, Feature( "Day Cycle" )]
	//public float UnkTimeValue { get; set; } = 0.5f;

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

	public Material SkyHemisphereColor => Material.FromShader( Shader.Load( "Pipelines/Atmosphere/d2_full_hemisphere_sky_color_generate.shader" ) );
	public Material SkyHemisphereScatter => Material.FromShader( Shader.Load( "Pipelines/Atmosphere/d2_sky_hemisphere_seed_inscattering.shader" ) );
	public Material SkyHemisphereBlur => Material.FromShader( Shader.Load( "Pipelines/Atmosphere/d2_sky_hemisphere_spherical_blur.shader" ) );
	public Material SkyNear => Material.FromShader( Shader.Load( "Pipelines/Atmosphere/d2_sky_lookup_generate_near.shader" ) );
	public Material SkyFar => Material.FromShader( Shader.Load( "Pipelines/Atmosphere/d2_sky_lookup_generate_far.shader" ) );
	public Material Sky => Material.FromShader( Shader.Load( "Pipelines/Atmosphere/d2_sky.shader" ) );
	public Material AtmosDepthApply => Material.FromShader( Shader.Load( "Pipelines/Atmosphere/d2_atmo_depth_apply.shader" ) );

	public Material AtmosDensityLookup => Material.FromShader( Shader.Load( "Pipelines/Atmosphere/d2_atmo_depth_angle_density_lookup_generate.shader" ) );

	private CommandList commandList = new CommandList();
	private CommandList commandListStart = new CommandList();
	//private GlobalChannelsController _globalChannels => GlobalChannelsController.Get();

	private GlobalChannelsController _globalChannels;
	private GlobalChannelsController GlobalChannels
	{
		get
		{
			if ( _globalChannels == null )
				_globalChannels = GlobalChannelsController.Get();

			return _globalChannels;
		}
		set
		{
			_globalChannels = value;
		}
	}

	private static DestinyAtmosphere _instance;
	public static DestinyAtmosphere Get()
	{
		if ( _instance is null )
			Game.ActiveScene.Components.TryGet<DestinyAtmosphere>( out _instance, FindMode.InDescendants );

		return _instance;
	}

	protected override void OnEnabled()
	{
		_instance = this;
		base.OnEnabled();
	}

	protected override void OnStart()
	{
		commandListStart = new( "AtmosphereApplyStart" );
		commandList = new( "AtmosphereApply" );

		OnTimeOfDayChanged( TimeOfDay, TimeOfDay );
		OnSunAngleChanged( SunDirection, SunDirection );
		//if ( Game.ActiveScene.Camera == null ) return;

		if ( Texture0_3D is null && Texture0 is not null )
			//Helpers.Create3DTexture( Texture0, out Texture0_3D );
			ConvertAtmoToVolume( Texture0, out Texture0_3D );

		if ( Texture1_3D is null && Texture1 is not null )
			//Helpers.Create3DTexture( Texture1, out Texture1_3D );
			ConvertAtmoToVolume( Texture1, out Texture1_3D );

		ApplyStartingAttributes();

		Game.ActiveScene.Camera.AddCommandList( commandListStart, Stage.AfterSkybox, 0 );
		Game.ActiveScene.Camera.AddCommandList( commandList, Stage.AfterSkybox, 4 );
	}

	private void ConvertAtmoToVolume( in Texture tex, out Texture outTex )
	{
		int sliceWidth = tex.Height;
		int sliceHeight = tex.Height;
		int depth = tex.Width / tex.Height;

		commandListStart.Attributes.Set( "2D_In", tex );

		//if ( _temp is null )
		outTex = Texture.CreateVolume( sliceWidth, sliceHeight, depth )
					.WithName( $"{tex.ResourceName}_3D" )
					.WithFormat( ImageFormat.RGBA16161616F )
					.WithUAVBinding()
					.WithMips( 0 )
					.Finish();

		var cs = new ComputeShader( "Pipelines/d2_convert_to_volume.shader" );
		commandListStart.Attributes.Set( "3D_Out", outTex );
		commandListStart.DispatchCompute( cs, sliceWidth, sliceHeight, depth );
		commandListStart.ResourceBarrierTransition( outTex, ResourceState.PixelShaderResource );

		Log.Info( $"Converted atmosphere texture {tex.ResourceName} to 3D volume texture {outTex.ResourceName}" );
	}

	private void ApplyStartingAttributes()
	{
		// Only need applied on start, I think?
		commandListStart.GlobalAttributes.Set( "AtmosTexture0", Texture0_3D ?? Helpers.CreateTransparentTexture3D( 1, 1, 6 ) );
		commandListStart.GlobalAttributes.Set( "AtmosTexture1", Texture1_3D ?? Helpers.CreateTransparentTexture3D( 1, 1, 6 ) );
		commandListStart.GlobalAttributes.Set( "AtmosDensityLookup", Texture3 ); // TODO-ish

		// commandListStart.GlobalAttributes.Set( "AtmosTexture3", Helpers.CreateFilledTexture( new Color( 1, 1, 0 ) ) ); // TODO

		if ( Game.ActiveScene.Camera.Components.Get<DestinyLightShafts>() is null )
			commandListStart.GlobalAttributes.Set( "RadialBlur12", Helpers.CreateFilledTexture( new Color( 1, 0, 0 ) ) ); // TODO: Depth thing for fake god rays

		if ( !UseDayCycle && DayCycleRotations.Count > 0 )
			// sun_track_direction
			GlobalChannels?.SetGlobalChannel( 102, DayCycleRotations[DayCycleRotations.Count / 2] );
	}

	protected override void OnDirty()
	{
		base.OnDirty();

		SetAttributes();
		RenderAtmosphereNew();
	}

	public void SetAttributes()
	{
		if ( commandList is null )
			return;

		Rotation = GlobalChannels?.Get( "sky_snapshot_rotation" )?.x / 360f ?? Rotation;
		Intensity = GlobalChannels?.Get( "sky_snapshot_intensity" )?.x ?? Intensity;

		SunColor = GlobalChannels?.Get( "sun_glow_color" ) ?? SunColor;
		SunIntensity = GlobalChannels?.Get( "sun_glow_intensity" )?.x ?? SunIntensity;
		SunDirectionVector = GlobalChannels?.Get( "sun_track_direction" ) ?? SunDirectionVector;

		commandList.Reset();
		commandList.GlobalAttributes.Set( "AtmosTimeOfDay", new Vector4( TimeOfDayNormalized ) );
		commandList.GlobalAttributes.Set( "FrameTimeOfDay", new Vector4( TimeOfDayNormalized ) );
		//commandList.GlobalAttributes.Set( "FrameTimeOfDay", new Vector4( ComputeUnkTimeValue( SunDirectionVector.z ) ) );
		//Log.Info( ComputeUnkTimeValue( SunDirectionVector.z ) );

		commandList.GlobalAttributes.Set( "AtmosIntensity", new Vector4( Intensity ) );
		commandList.GlobalAttributes.Set( "AtmosRotation", new Vector4( Rotation ) );
		commandList.GlobalAttributes.Set( "AtmosSunColor", SunColor );
		commandList.GlobalAttributes.Set( "AtmosSunIntensity", SunIntensity );

		commandList.GlobalAttributes.Set( "AtmosSunDir", SunDirectionVector );
		commandList.GlobalAttributes.Set( "AtmosSunDirRight", SunDirectionVector.GetRight() );
		commandList.GlobalAttributes.Set( "AtmosSunDirUp", SunDirectionVector.GetUp() );

		ApplyUnknownExternValues();

		if ( AffectSceneSun )
		{
			SunComponent = Components.GetOrCreate<DirectionalLight>();
			SunComponent.WorldRotation = SunDirection + new Angles( 180, 0, 0 );

			var sunColor = (GlobalChannels?.Get( "sun_color" ) ?? SunColor) * SunIntensity;
			SunComponent.LightColor = sunColor.WithW( 1 );

			var skyColor = ((GlobalChannels?.Get( "up_ambient_color" )) ?? Color.Transparent);
			SunComponent.SkyColor = skyColor;
		}
	}

	public void RenderAtmosphereNew()
	{
		if ( commandList is null )
			return;

		Vector2 atmosRTsize = ScreenSize / 3;
		commandList.GlobalAttributes.Set( "AtmosRTDimensions", new Vector4( atmosRTsize.x, atmosRTsize.y, 1 / atmosRTsize.x, 1 / atmosRTsize.y ) );

		// full_hemisphere_sky_color_generate
		var hemiSkyColor = commandList.GetRenderTarget( "HemiSkyColor", 512, 512, ImageFormat.RGBA16161616F, numMips: 10 );
		commandList.SetRenderTarget( hemiSkyColor );
		commandList.Blit( SkyHemisphereColor );
		commandList.GlobalAttributes.Set( "AtmosHemisphere", hemiSkyColor.ColorTexture );

		commandList.ClearRenderTarget();
		commandList.ReleaseRenderTarget( hemiSkyColor );

		//---------------------------------------------------------------------------

		var hemiScatter = commandList.GetRenderTarget( "HemiScatter", 512, 512, ImageFormat.RG1616F );
		var hemiBlur = commandList.GetRenderTarget( "HemiBlur", 512, 512, ImageFormat.RG1616F );

		// sky_hemisphere_seed_inscattering
		commandList.SetRenderTarget( hemiScatter );
		commandList.Blit( SkyHemisphereScatter );
		commandList.GlobalAttributes.Set( "AtmosHemisphereScatter", hemiScatter.ColorTexture );

		//------------------------------------------

		// sky_hemisphere_spherical_blur
		commandList.SetRenderTarget( hemiBlur );
		commandList.Blit( SkyHemisphereBlur );
		commandList.GlobalAttributes.Set( "AtmosHemisphereBlur", hemiBlur.ColorTexture );

		// Dispose both *after* use
		commandList.ClearRenderTarget();  // Unbind before disposing
		commandList.ReleaseRenderTarget( hemiScatter );
		commandList.ReleaseRenderTarget( hemiBlur );

		//---------------------------------------------------------------------------

		var rtFar = commandList.GetRenderTarget( "RTFar", (int)ScreenSize.x / 3, (int)ScreenSize.y / 3, ImageFormat.RGBA16161616F );
		var rtNear = commandList.GetRenderTarget( "RTNear", (int)ScreenSize.x / 3, (int)ScreenSize.y / 3, ImageFormat.RGBA16161616F );

		// sky_lookup_generate_far
		commandList.SetRenderTarget( rtFar );
		commandList.Blit( SkyFar );
		commandList.GlobalAttributes.Set( "AtmosFar", rtFar.ColorTexture );

		//------------------------------------------

		//sky_lookup_generate_near
		commandList.SetRenderTarget( rtNear );
		commandList.Blit( SkyNear );
		commandList.GlobalAttributes.Set( "AtmosNear", rtNear.ColorTexture );

		// Dispose both *after* use
		commandList.ClearRenderTarget();
		commandList.ReleaseRenderTarget( rtFar );
		commandList.ReleaseRenderTarget( rtNear );

		//------------------------------------------

		// atmo_depth_angle_density_lookup_generate
		var density = commandList.GetRenderTarget( "Density", 512, 512, ImageFormat.RGBA16161616F );
		commandList.SetRenderTarget( density );
		commandList.Blit( AtmosDensityLookup );
		commandList.GlobalAttributes.Set( "AtmosDensity", density.ColorTexture );

		commandList.ClearRenderTarget();
		commandList.ReleaseRenderTarget( density );

		commandList.Blit( Sky );

		if ( UseAtmosphericFog )
		{
			commandList.Attributes.GrabFrameTexture( "ColorBuffer" );
			commandList.Blit( AtmosDepthApply );
		}
		commandList.Clear( Color.Transparent );
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
		if ( UseDayCycle )
		{
			TimeOfDay += Time.Delta * (3600f / DayLength);

			if ( TimeOfDay >= 3600f )
				TimeOfDay -= 3600f;
		}

		// Use mapped TimeOfDay to get sun direction from DayCycleRotations
		if ( DayCycleRotations != null && DayCycleRotations.Count > 0 )
		{
			float tod_half = Math.Max( 0, TimeOfDay / 2f );

			int fromIndex = Math.Clamp( tod_half.FloorToInt(), 0, DayCycleRotations.Count - 1 );
			int toIndex = Math.Clamp( tod_half.CeilToInt(), 0, DayCycleRotations.Count - 1 );

			Vector4 from = DayCycleRotations[fromIndex];
			Vector4 to = DayCycleRotations[toIndex];
			float t = tod_half - tod_half.FloorToInt();
			Vector4 lerpedRotation = Vector4.Lerp( from, to, t );

			// sun_track_direction
			GlobalChannels?.SetGlobalChannel( 102, lerpedRotation );
		}

		float distance_to_night = Math.Abs( TimeOfDay / 1800.0f - 1.0f );
		GlobalChannels.MiscValues[0] = new Vector4( (1f - distance_to_night) * 0.725f );
	}

	private void ApplyUnknownExternValues()
	{

		commandList.GlobalAttributes.Set( "AtmosUnk1D0", AtmosUnk1D0 ); // sky_color_override?
		commandList.GlobalAttributes.Set( "AtmosUnk210", AtmosUnk210 );

		commandList.GlobalAttributes.Set( "AtmosUnk74", AtmosUnk74 );
		commandList.GlobalAttributes.Set( "AtmosUnk78", AtmosUnk78 );
		commandList.GlobalAttributes.Set( "AtmosUnk150", AtmosUnk150 );
		commandList.GlobalAttributes.Set( "AtmosUnk154", AtmosUnk154 );

		AtmosFogIntensity = GlobalChannels.Get( 26 ).x; // Unsure
		commandList.GlobalAttributes.Set( "AtmosFogIntensity", AtmosFogIntensity );

		AtmosUnk164 = GlobalChannels.Get( 15 ).x;
		commandList.GlobalAttributes.Set( "AtmosUnk164", AtmosUnk164 ); // Fog density? Unsure

		AtmosUnk168 = GlobalChannels.Get( 16 ).x;
		commandList.GlobalAttributes.Set( "AtmosUnk168", AtmosUnk168 );

		AtmosUnk16C = GlobalChannels.Get( 17 ).x;
		commandList.GlobalAttributes.Set( "AtmosUnk16C", AtmosUnk16C );

		AtmosUnk170 = GlobalChannels.Get( 19 ).x;
		commandList.GlobalAttributes.Set( "AtmosUnk170", AtmosUnk170 ); // fog_height_falloff

		AtmosUnk180 = GlobalChannels.Get( 20 );
		commandList.GlobalAttributes.Set( "AtmosUnk180", AtmosUnk180 ); // fog_decay_color

		AtmosUnk190 = GlobalChannels.Get( 21 ).x;
		commandList.GlobalAttributes.Set( "AtmosUnk190", AtmosUnk190 ); // fog_decay_scale

		commandList.GlobalAttributes.Set( "AtmosUnk194", AtmosUnk194 );
		commandList.GlobalAttributes.Set( "AtmosUnk198", AtmosUnk198 );

		AtmosUnk1BC = GlobalChannels.Get( 36 ).x;
		commandList.GlobalAttributes.Set( "AtmosUnk1BC", AtmosUnk1BC );

		AtmosUnk1C0 = GlobalChannels.Get( 35 ).x;
		commandList.GlobalAttributes.Set( "AtmosUnk1C0", AtmosUnk1C0 ); // Unsure

		commandList.GlobalAttributes.Set( "AtmosUnk1C4", AtmosUnk1C4 );
		commandList.GlobalAttributes.Set( "AtmosUnk1E0", AtmosUnk1E0 );

		AtmosUnk1E8 = GlobalChannels.Get( 38 ).x;
		commandList.GlobalAttributes.Set( "AtmosUnk1E8", AtmosUnk1E8 );

		commandList.GlobalAttributes.Set( "AtmosUnk1EC", AtmosUnk1EC );
		commandList.GlobalAttributes.Set( "AtmosUnk1F8", AtmosUnk1F8 );
		commandList.GlobalAttributes.Set( "AtmosUnk1FC", AtmosUnk1FC );
		commandList.GlobalAttributes.Set( "AtmosUnk208", AtmosUnk208 );
	}

	public static float ComputeUnkTimeValue( float sunDirZ )
	{
		// Cubic coefficients from curve fitting
		float a = -0.4147f;
		float b = 0.7202f;
		float c = 0.0328f;
		float d = 0.35f;

		// Compute cubic polynomial
		return a * sunDirZ * sunDirZ * sunDirZ
			 + b * sunDirZ * sunDirZ
			 + c * sunDirZ
			 + d;

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
		TimeOfDayNormalized = newValue / 3600f; // Normalize to 0-1 range based on DayLength
	}

	protected override void OnPreRender()
	{
		if ( ScreenSize != Game.ActiveScene.Camera.ScreenRect.Size )
		{
			ScreenSize = Game.ActiveScene.Camera.ScreenRect.Size;
			Vector2 atmosRTsize = ScreenSize / 3;
			commandList.Attributes.Set( "AtmosRTDimensions", new Vector4( atmosRTsize.x, atmosRTsize.y, 1 / atmosRTsize.x, 1 / atmosRTsize.y ) );
			Log.Info( $"AtmosphereRenderer2: Screen size updated to {ScreenSize}" );
			OnDirty();
		}
	}

	protected override void OnDisabled()
	{
		base.OnDisabled();
		Cleanup();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Cleanup();
	}

	private void Cleanup()
	{
		//_temp?.Dispose();
		//_temp = null;

		Texture0_3D?.Dispose();
		Texture0_3D = null;

		Texture1_3D?.Dispose();
		Texture1_3D = null;

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
		_instance = null;
	}
}
