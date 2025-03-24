using Sandbox;
using System;

[Title( "Screen Space God Rays" )]
[Category( "Post Processing" )]
[Icon( "light_mode" )]
public sealed class ScreenSpaceGodRays : PostProcess, Component.ExecuteInEditor
{
	public enum Quality
	{
		/// <summary>
		/// 128 base samples (only for screenshots!)
		/// </summary>
		[Icon( "brightness_5" )]
		Ultra = 0,

		/// <summary>
		/// 32 base samples
		/// </summary>
		[Icon( "brightness_4" )]
		High = 1,

		/// <summary>
		/// 24 base samples
		/// </summary>
		[Icon( "brightness_3" )]
		Medium = 2,

		/// <summary>
		/// 16 base samples
		/// </summary>
		[Icon( "brightness_2" )]
		Low = 3,

		/// <summary>
		/// 8 base samples
		/// </summary>
		[Icon( "brightness_1" )]
		UltraLow
	}

	/// <summary>
	/// Determines how many samples we use when blurring.
	/// The number of samples is also affected by the <see cref="Density"/> value, to keep things looking smooth.
	/// </summary>
	[Property, Group( "General" )] public Quality QualityLevel { get; set; } = Quality.Medium;

	/// <summary>
	/// How much should we multiply the output by?
	/// </summary>
	[Property, Group( "General" ), Range( 0, 1 )] public float Intensity { get; set; } = 0.5f;

	/// <summary>
	/// How much exposure should we apply to the initial image?
	/// </summary>
	[Property, Group( "Filtering" ), Range( 0, 1 )] public float Exposure { get; set; } = 1f;

	/// <summary>
	/// What brightness cutoff should we use?
	/// </summary>
	[Property, Group( "Filtering" ), Range( 0, 4 )] public float Threshold { get; set; } = 1.6f;

	/// <summary>
	/// How smooth should the brightness cutoff be?
	/// </summary>
	[Property, Group( "Filtering" ), Range( 0, 4 )] public float Smoothness { get; set; } = 1.5f;

	/// <summary>
	/// How long should the rays be?
	/// </summary>
	[Property, Group( "Rays" ), Range( 0, 1 )] public float Density { get; set; } = 0.6f;

	/// <summary>
	/// How much energy should the rays preserve over distance?
	/// </summary>
	[Property, Group( "Rays" ), Range( 0, 1 )] public float Decay { get; set; } = 0.96f;

	/// <summary>
	/// How much weight should the rays have?
	/// </summary>
	[Property, Group( "Rays" ), Range( 0, 1 )] public float Weight { get; set; } = 1.0f;

	IDisposable renderHook;

	protected override void OnEnabled()
	{
		renderHook = Camera.AddHookBeforeOverlay( "Screen Space God Rays", 2000, RenderEffect );
	}

	protected override void OnDisabled()
	{
		renderHook?.Dispose();
		renderHook = null;
	}

	RenderAttributes attributes = new RenderAttributes();

	private int GetSampleCount()
	{
		var baseSampleCount = QualityLevel switch
		{
			Quality.Ultra => 128,
			Quality.High => 24,
			Quality.Medium => 16,
			Quality.Low => 8,
			Quality.UltraLow => 4,

			_ => 16
		};

		return baseSampleCount * Density.Remap( 0.0f, 1.0f, 1, 8 ).CeilToInt();
	}

	public void RenderEffect( SceneCamera camera )
	{
		if ( !camera.EnablePostProcessing )
			return;

		var sun = Scene.GetComponentInChildren<DirectionalLight>();

		if ( !sun.IsValid() )
			return;

		//
		// Find main light position on screen
		//
		var lightForward = -sun.WorldRotation.Forward;
		var farPosition = camera.Position + lightForward * camera.ZFar;

		camera.ToScreen( farPosition, out var screenPosition );

		var lightPosition = screenPosition / camera.Size;
		attributes.Set( "lightposition", lightPosition );

		//
		// Make it look semi-decent if the light is off-screen
		//
		var strength = 1.0f - lightPosition.GetBoundaryProximityFactor();
		strength = MathF.Pow( strength, 0.5f );
		attributes.Set( "lightintensity", Intensity * strength );

		if ( strength <= 0.0f )
			return;

		//
		// User props
		//
		attributes.Set( "threshold", Threshold );
		attributes.Set( "smoothness", Smoothness );
		attributes.Set( "exposure", Exposure );
		attributes.Set( "density", Density );
		attributes.Set( "weight", Weight );
		attributes.Set( "decay", Decay );

		//
		// Num samples
		//
		attributes.Set( "samplecount", GetSampleCount() );

		Graphics.GrabFrameTexture( "ColorBuffer", attributes );
		Graphics.Blit( Material.FromShader( "shaders/godrays.shader" ), attributes );
	}
}
