[Title( "Destiny Instance Renderer" )]
[Category( "Rendering" )]
[Icon( "nature" )]
public sealed class InstanceRenderer : Component, Component.ExecuteInEditor
{
	[Property] public Model InstanceModel;
	[Property, Hide] public Transform[] Transforms;
	[Property] public FeatureType ObjectType;

	private DecoratorSceneObject DecorSceneObj;

	protected override void OnStart()
	{
		RenderInstances();
	}

	protected override void OnDisabled()
	{
		DecorSceneObj?.Delete();
		DecorSceneObj = null;
	}

	private void RenderInstances()
	{
		switch ( ObjectType )
		{
			case FeatureType.Decorator:
				DecorSceneObj = new( Scene.SceneWorld, InstanceModel, Transforms );
				break;
		}
	}

	public enum FeatureType
	{
		None = 0,
		Static = 1,
		Entity = 2,
		Decorator = 3,
		Sky = 4,
	}
}
