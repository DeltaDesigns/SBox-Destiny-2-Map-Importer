public class SkySceneObject : SceneCustomObject
{
	public Model InstanceModel { get; set; }
	public Transform[] Transforms { get; set; }

	public SkySceneObject( SceneWorld world, Model model, Transform[] transforms ) : base( world )
	{
		//RenderLayer = SceneRenderLayer.Default;
		InstanceModel = model;
		Transforms = transforms;
		Batchable = true;

		Flags.CastShadows = false;
		Flags.NeedsLightProbe = false;
		Flags.NeedsEnvironmentMap = false;
		Flags.IsOpaque = false;
		Flags.IsTranslucent = true;
		Flags.SkyBoxLayer = true;
		Flags.WantsFrameBufferCopy = true;
		Tags.Append( "skybox" );
	}

	RenderAttributes attributes = new RenderAttributes();
	public override void RenderSceneObject()
	{
		if ( Graphics.LayerType != SceneLayerType.Translucent )
			return;

		Graphics.DrawModelInstanced( InstanceModel, Transforms, Graphics.Attributes );
	}
}

