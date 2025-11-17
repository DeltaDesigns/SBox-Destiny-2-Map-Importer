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
	public Vector4 AtmosUnk5 { get; set; } = new Vector4( -0.8365f );

	[Property, MakeDirty, Feature( "Atmosphere" )]
	public Vector4 AtmosUnk24 { get; set; } = new Vector4( 0.33713f );

	[Property, MakeDirty, Feature( "Atmosphere" )]
	public Vector4 UnkGodRayDir { get; set; } = new Vector4( 0 );
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

	private Texture _temp;
	private void ConvertAtmoToVolume( in Texture tex, out Texture outTex )
	{
		int sliceWidth = tex.Height;
		int sliceHeight = tex.Height;
		int depth = tex.Width / tex.Height;

		commandListStart.Attributes.Set( "2D_In", tex );

		if ( _temp is null )
			_temp = Texture.CreateVolume( sliceWidth, sliceHeight, depth )
						.WithFormat( ImageFormat.RGBA16161616F )
						.WithDynamicUsage()
						.WithUAVBinding()
						.WithMips( 0 )
						.Finish();

		var cs = new ComputeShader( "Pipelines/d2_convert_to_volume.shader" );
		commandListStart.Attributes.Set( "3D_Out", _temp );
		commandListStart.DispatchCompute( cs, sliceWidth, sliceHeight, depth );

		outTex = _temp;
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
		commandList.GlobalAttributes.Set( "AtmosIntensity", new Vector4( Intensity ) );
		commandList.GlobalAttributes.Set( "AtmosRotation", new Vector4( Rotation ) );
		commandList.GlobalAttributes.Set( "AtmosSunColor", SunColor );
		commandList.GlobalAttributes.Set( "AtmosSunIntensity", SunIntensity );

		commandList.GlobalAttributes.Set( "AtmosSunDir", SunDirectionVector );
		commandList.GlobalAttributes.Set( "AtmosSunDirRight", SunDirectionVector.GetRight() );
		commandList.GlobalAttributes.Set( "AtmosSunDirUp", SunDirectionVector.GetUp() );

		commandList.GlobalAttributes.Set( "AtmosUnk5", AtmosUnk5 );
		commandList.GlobalAttributes.Set( "AtmosUnk24", AtmosUnk24 );

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
		//commandList.Clear( Color.Transparent );

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
		_temp?.Dispose();
		_temp = null;

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
