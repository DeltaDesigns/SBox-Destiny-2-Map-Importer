public class DestinyAtmosphereRenderer : SceneCustomObject
{
	private Scene scene;
	private DestinyAtmosphere Atmosphere;
	public DestinyAtmosphereRenderer( Scene world, DestinyAtmosphere atmosphere ) : base( world.SceneWorld )
	{
		scene = world;
		Atmosphere = atmosphere;
		Flags.IsOpaque = true; // Need so we can access opaque, depth prepass, and translucent render layers
	}

	public override void RenderSceneObject()
	{
		base.RenderSceneObject();

		if ( Graphics.LayerType != SceneLayerType.Opaque )
			return;
		//Log.Info( Graphics.LayerType );

		using var rtFar = RenderTarget.GetTemporary( (int)Graphics.Viewport.Size.x / 2, (int)Graphics.Viewport.Size.y / 2, ImageFormat.RGBA16161616F );
		using var rtNear = RenderTarget.GetTemporary( (int)Graphics.Viewport.Size.x / 2, (int)Graphics.Viewport.Size.y / 2, ImageFormat.RGBA16161616F );

		// Render AtmosFar
		Graphics.RenderTarget = rtFar;
		Graphics.Clear();
		Graphics.Blit( Atmosphere.SkyFar );
		scene.RenderAttributes.Set( "AtmosFar", rtFar.ColorTarget );
		Graphics.RenderTarget = null;

		// Render AtmosNear
		Graphics.RenderTarget = rtNear;
		Graphics.Clear();
		Graphics.Blit( Atmosphere.SkyNear );
		scene.RenderAttributes.Set( "AtmosNear", rtNear.ColorTarget );
		Graphics.RenderTarget = null;

		// Release resources at the end of frame
		rtFar?.Dispose();
		rtNear?.Dispose();
	}
}
