public class DestinyAtmosphereRenderer : SceneCustomObject
{
	private Scene scene;
	private DestinyAtmosphere Atmosphere;
	private Vector2 ScreenSize;
	private RenderAttributes GlobalAttributes => scene.RenderAttributes;

	public DestinyAtmosphereRenderer( Scene world, DestinyAtmosphere atmosphere ) : base( world.SceneWorld )
	{
		scene = world;
		Atmosphere = atmosphere;
		Flags.IsOpaque = true; // Need so we can access opaque, depth prepass, and translucent render layers

		//if( Graphics.Viewport is not null)
		//ScreenSize = Graphics.Viewport.Size;
		//Vector2 size = ScreenSize / 3;
		//scene.RenderAttributes.Set( "AtmosRTDimensions", new Vector4( (int)size.x, (int)size.y, (int)1 / size.x, (int)1 / size.y ) );
	}

	RenderAttributes RenderAttributes = new();
	public override void RenderSceneObject()
	{
		base.RenderSceneObject();

		if ( Graphics.LayerType != SceneLayerType.Opaque )
			return;

		if ( ScreenSize != Graphics.Viewport.Size )
		{
			ScreenSize = Graphics.Viewport.Size;
			Vector2 atmosRTsize = ScreenSize / 3;
			RenderAttributes.Set( "AtmosRTDimensions", new Vector4( (int)atmosRTsize.x, (int)atmosRTsize.y, (int)1 / atmosRTsize.x, (int)1 / atmosRTsize.y ) );
		}

		// full_hemisphere_sky_color_generate
		//using var hemiSkyColor = RenderTarget.GetTemporary( 64, 64, ImageFormat.RGBA16161616F );
		using var hemiSkyColor = RenderTarget.GetTemporary( 512, 512, ImageFormat.RGBA16161616F, numMips: 10 );
		Graphics.RenderTarget = hemiSkyColor;
		Graphics.Clear();
		Graphics.Blit( Atmosphere.SkyHemisphereColor );
		GlobalAttributes.Set( "AtmosHemisphere", hemiSkyColor.ColorTarget );
		Graphics.RenderTarget = null;
		hemiSkyColor?.Dispose();

		//---------------------------------------------------------------------------

		using var hemiScatter = RenderTarget.GetTemporary( 512, 512, ImageFormat.RG1616F );
		using var hemiBlur = RenderTarget.GetTemporary( 512, 512, ImageFormat.RG1616F );

		// sky_hemisphere_seed_inscattering
		Graphics.RenderTarget = hemiScatter;
		Graphics.Clear();
		Graphics.Blit( Atmosphere.SkyHemisphereScatter );
		RenderAttributes.Set( "AtmosHemisphereScatter", hemiScatter.ColorTarget );

		// sky_hemisphere_spherical_blur
		Graphics.RenderTarget = hemiBlur;
		Graphics.Clear();
		Graphics.Blit( Atmosphere.SkyHemisphereBlur, RenderAttributes );
		RenderAttributes.Set( "AtmosHemisphereBlur", hemiBlur.ColorTarget );

		Graphics.RenderTarget = null;  // Unbind before disposing

		// Dispose both *after* use
		hemiScatter?.Dispose();
		hemiBlur?.Dispose();

		//---------------------------------------------------------------------------

		using var rtFar = RenderTarget.GetTemporary( (int)Graphics.Viewport.Size.x / 3, (int)Graphics.Viewport.Size.y / 3, ImageFormat.RGBA16161616F );
		using var rtNear = RenderTarget.GetTemporary( (int)Graphics.Viewport.Size.x / 3, (int)Graphics.Viewport.Size.y / 3, ImageFormat.RGBA16161616F );

		// sky_lookup_generate_far
		Graphics.RenderTarget = rtFar;
		Graphics.Clear();
		Graphics.Blit( Atmosphere.SkyFar, RenderAttributes );
		GlobalAttributes.Set( "AtmosFar", rtFar.ColorTarget );

		//sky_lookup_generate_near
		Graphics.RenderTarget = rtNear;
		Graphics.Clear();
		Graphics.Blit( Atmosphere.SkyNear, RenderAttributes );
		GlobalAttributes.Set( "AtmosNear", rtNear.ColorTarget );

		Graphics.RenderTarget = null;  // Unbind before disposing

		// Dispose both *after* use
		rtFar?.Dispose();
		rtNear?.Dispose();
	}
}
