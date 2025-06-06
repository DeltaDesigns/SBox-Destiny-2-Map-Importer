public class DecalSceneObject : SceneCustomObject
{
	public Model Cube { get; set; }
	public Transform Transform { get; set; }

	public DecalSceneObject( SceneWorld world, Model model, Transform transform ) : base( world )
	{
		Cube = model;
		Transform = transform;
		Batchable = true;

		Flags.CastShadows = false;
		Flags.NeedsLightProbe = true;
		Flags.NeedsEnvironmentMap = true;
		Flags.IsOpaque = true;
		Flags.SkyBoxLayer = false;
		Flags.WantsFrameBufferCopy = true;

		var angle = transform.Rotation * new Rotation( 0, 0, 0, -1 );
		Attributes.Set( "ObjectPosition", transform.Position );
		Attributes.Set( "ObjectRotation", new Vector4( angle.x, angle.y, angle.z, angle.w ) );
		Attributes.Set( "ObjectScale", transform.Scale );

	}

	RenderAttributes attributes { get; set; } = new();
	public override void RenderSceneObject()
	{
		if ( Graphics.LayerType == SceneLayerType.Translucent )
			Graphics.DrawModel( Cube, Transform, Attributes );
	}
}

