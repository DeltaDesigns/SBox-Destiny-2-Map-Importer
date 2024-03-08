using Sandbox;

public sealed class TerrainDyemap : Component, Component.ExecuteInEditor
{
	[Property] public Texture DyemapTexture { get; set; }
	[Property] public Color Color { get; set; }

	protected override void OnStart()
	{
		var mdl = Components.Get<ModelRenderer>();
		mdl.SceneObject.Batchable = false;
		mdl.SceneObject.Attributes.Set( "TerrainDyemap", DyemapTexture );
	}
}
