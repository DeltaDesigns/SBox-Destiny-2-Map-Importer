public class DestinyWaterRenderer : SceneCustomObject
{
	private Scene scene;
	private DestinyWater Water;
	private Vector2 ScreenSize;
	public DestinyWaterRenderer( Scene world, DestinyWater water ) : base( world.SceneWorld )
	{
		scene = world;
		Water = water;
		Flags.IsTranslucent = true;
	}

	public override void RenderSceneObject()
	{
		base.RenderSceneObject();

		if ( Graphics.LayerType != SceneLayerType.Translucent )
			return;

		// full_hemisphere_sky_color_generate
		//using var hemiSkyColor = RenderTarget.GetTemporary( 64, 64, ImageFormat.RGBA16161616F );
		using var water = RenderTarget.GetTemporary( 512, 512, ImageFormat.RGBA16161616F );
		Graphics.RenderTarget = water;
		Graphics.Clear();
		Graphics.Blit( Water.Water );
		scene.RenderAttributes.Set( "WaterTest", water.ColorTarget );
		Graphics.RenderTarget = null;
		water?.Dispose();
	}
}
