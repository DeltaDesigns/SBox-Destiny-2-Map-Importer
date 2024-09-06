public class DecoratorSceneObject : SceneCustomObject
{
	public Model InstanceModel { get; set; }
	public Transform[] Transforms { get; set; }
	public DecoratorSceneObject( SceneWorld world, Model model, Transform[] transforms ) : base( world )
	{
		//RenderLayer = SceneRenderLayer.Default;
		InstanceModel = model;
		Transforms = transforms;
		Batchable = true;

		Flags.CastShadows = false;
		Flags.NeedsLightProbe = false;
		Flags.NeedsEnvironmentMap = false;
		Flags.IsOpaque = true;
		Flags.SkyBoxLayer = false;
		Flags.WantsFrameBufferCopy = false;
	}

	public override void RenderSceneObject()
	{
		if ( Graphics.LayerType >= SceneLayerType.DepthPrepass )
			Graphics.DrawModelInstanced( InstanceModel, Transforms );
	}
}

