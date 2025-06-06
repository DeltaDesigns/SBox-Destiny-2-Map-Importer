using System.IO;
using System.Text.Json;

public partial class DestinyImporter : EditorTool
{
	private static void ImportTerrain( List<string> mapList )
	{
		if ( mapList.Count == 0 )
			return;

		var terrainRoot = scene.CreateObject();
		terrainRoot.Name = "Terrain";

		foreach ( string path in mapList )
		{
			JsonDocument cfg = JsonDocument.Parse( File.ReadAllText( path ) );

			if ( cfg.RootElement.GetProperty( "TerrainDyemaps" ).EnumerateObject().Count() == 0 )
				continue;

			foreach ( JsonProperty model in cfg.RootElement.GetProperty( "TerrainDyemaps" ).EnumerateObject() )
			{
				int i = 0;
				var terrainPartParent = scene.CreateObject();
				terrainPartParent.Name = $"{model.Name}";
				terrainPartParent.Parent = terrainRoot;

				foreach ( JsonElement terrain in model.Value.EnumerateArray().Reverse() )
				{
					var terrainPart = scene.CreateObject();
					terrainPart.Name = $"{model.Name}_{i}";
					terrainPart.Parent = terrainPartParent;

					var mdl = terrainPart.Components.GetOrCreate<ModelRenderer>();
					mdl.Model = Model.Load( $"models/Terrain/{model.Name}_{i}.vmdl" );

					if ( !_overrideTerrainMats )
					{
						var dyemap = terrainPart.Components.GetOrCreate<TerrainDyemap>();
						dyemap.DyemapTexture = Texture.Load( $"textures/{terrain.GetString()}.vtex" );
					}
					else
					{
						mdl.MaterialOverride = Material.Load( "materials/dev/reflectivity_50.vmat" );
					}

					// Until map collisions are properly figured out, we're just gonna use the terrain itself as the collider....
					var col = terrainPart.Components.GetOrCreate<ModelCollider>();
					col.Static = true;
					col.Model = mdl.Model;

					i++;
				}
			}
		}
	}
}

