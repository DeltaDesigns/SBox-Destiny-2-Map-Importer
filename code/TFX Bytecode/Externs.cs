
using Vec4 = System.Numerics.Vector4;

public static class Externs
{

	public static Vec4 GetExternFloat( TfxExtern extern_, byte element )
	{
		switch ( extern_ )
		{
			case TfxExtern.Frame:
				switch ( element * 0x4 )
				{
					case 0x0:
						return new Vec4( RealTime.Now ); // game_time
					case 0x4:
						return new Vec4( RealTime.Now ); // render_time
					case 0xC:
						return new Vec4( 1f ); // Unk
					case 0x10:
						return new Vec4( DestinyAtmosphere.Get().TimeOfDayNormalized ); // Unk
					case 0x14:
						return new Vec4( Time.Delta ); // delta_game_time
					case 0x1C:
						return new Vec4( 1f ); // exposure_scale
					default:
						Log.Error( $"Unsupported element {element * 0x4} (0x{(element * 0x4):X}) for extern {extern_}" );
						return new Vec4( 1f );
				}
			default:
				Log.Error( $"Unsupported extern {extern_}[{element}]" );
				return new Vec4( 1f );
		}
	}

	public static Vec4 GetExternVec4( TfxExtern extern_, byte element )
	{
		switch ( extern_ )
		{
			case TfxExtern.Frame:
				switch ( element )
				{
					case 26:
						return new Vec4( 0f );
					case 27:
						return new Vec4( 1f );
					default:
						Log.Error( $"Unsupported element {element} for extern {extern_}" );
						return new Vec4( 0f );
				}
			case TfxExtern.Atmosphere:
				switch ( element )
				{
					case 7:
						return new Vec4( 1f );
					default:
						Log.Error( $"Unsupported element {element} for extern {extern_}" );
						return new Vec4( 0f );
				}
			default:
				Log.Error( $"Unsupported extern {extern_}[{element}]" );
				return new Vec4( 1f );
		}
	}
}

public enum TfxExtern : byte
{
	None = 0,
	Frame = 1,
	View = 2,
	Deferred = 3,
	DeferredLight = 4,
	DeferredUberLight = 5,
	DeferredShadow = 6,
	Atmosphere = 7,
	RigidModel = 8,
	EditorMesh = 9,
	EditorMeshMaterial = 10,
	EditorDecal = 11,
	EditorTerrain = 12,
	EditorTerrainPatch = 13,
	EditorTerrainDebug = 14,
	SimpleGeometry = 15,
	UiFont = 16,
	CuiView = 17,
	CuiObject = 18,
	CuiBitmap = 19,
	CuiVideo = 20,
	CuiStandard = 21,
	CuiHud = 22,
	CuiScreenspaceBoxes = 23,
	TextureVisualizer = 24,
	Generic = 25,
	Particle = 26,
	ParticleDebug = 27,
	GearDyeVisualizationMode = 28,
	ScreenArea = 29,
	Mlaa = 30,
	Msaa = 31,
	Hdao = 32,
	DownsampleTextureGeneric = 33,
	DownsampleDepth = 34,
	Ssao = 35,
	VolumetricObscurance = 36,
	Postprocess = 37,
	TextureSet = 38,
	Transparent = 39,
	Vignette = 40,
	GlobalLighting = 41,
	ShadowMask = 42,
	ObjectEffect = 43,
	Decal = 44,
	DecalSetTransform = 45,
	DynamicDecal = 46,
	DecoratorWind = 47,
	TextureCameraLighting = 48,
	VolumeFog = 49,
	Fxaa = 50,
	Smaa = 51,
	Letterbox = 52,
	DepthOfField = 53,
	PostprocessInitialDownsample = 54,
	CopyDepth = 55,
	DisplacementMotionBlur = 56,
	DebugShader = 57,
	MinmaxDepth = 58,
	SdsmBiasAndScale = 59,
	SdsmBiasAndScaleTextures = 60,
	ComputeShadowMapData = 61,
	ComputeLocalLightShadowMapData = 62,
	BilateralUpsample = 63,
	HealthOverlay = 64,
	LightProbeDominantLight = 65,
	LightProbeLightInstance = 66,
	Water = 67,
	LensFlare = 68,
	ScreenShader = 69,
	Scaler = 70,
	GammaControl = 71,
	SpeedtreePlacements = 72,
	Reticle = 73,
	Distortion = 74,
	WaterDebug = 75,
	ScreenAreaInput = 76,
	WaterDepthPrepass = 77,
	OverheadVisibilityMap = 78,
	ParticleCompute = 79,
	CubemapFiltering = 80,
	ParticleFastpath = 81,
	VolumetricsPass = 82,
	TemporalReprojection = 83,
	FxaaCompute = 84,
	VbCopyCompute = 85,
	UberDepth = 86,
	GearDye = 87,
	Cubemaps = 88,
	ShadowBlendWithPrevious = 89,
	DebugShadingOutput = 90,
	Ssao3d = 91,
	WaterDisplacement = 92,
	PatternBlending = 93,
	UiHdrTransform = 94,
	PlayerCenteredCascadedGrid = 95,
	SoftDeform = 96,
}
