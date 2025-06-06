using System.IO;
using System.Text.Json;


public partial class DestinyImporter : EditorTool
{
	private static void ImportDecals( string path )
	{
		var staticMapRoot = scene.CreateObject();
		staticMapRoot.Name = "Decals";

		JsonDocument cfg = JsonDocument.Parse( File.ReadAllText( $"{path}/Rendering/Decals.json" ) );
		foreach ( var decal in cfg.RootElement.EnumerateObject() )
		{
			int i = 0;
			var staticMapParent = scene.CreateObject();
			staticMapParent.Name = $"{decal.Name}";
			staticMapParent.Parent = staticMapRoot;

			foreach ( var transforms in decal.Value.GetProperty( "Instances" ).EnumerateArray() )
			{
				//Create the transforms first before we create the entity
				Vector3 position = new Vector3(
					transforms.GetProperty( "Translation" )[0].GetSingle() * 39.37f,
					transforms.GetProperty( "Translation" )[1].GetSingle() * 39.37f,
					transforms.GetProperty( "Translation" )[2].GetSingle() * 39.37f );

				Rotation quatRot = new Rotation
				{
					x = transforms.GetProperty( "Rotation" )[0].GetSingle(),
					y = transforms.GetProperty( "Rotation" )[1].GetSingle(),
					z = transforms.GetProperty( "Rotation" )[2].GetSingle(),
					w = transforms.GetProperty( "Rotation" )[3].GetSingle()
				};

				// sizes for the different types
				Vector3 scale = new Vector3(
					transforms.GetProperty( "Scale" )[0].GetSingle(),
					transforms.GetProperty( "Scale" )[1].GetSingle(),
					transforms.GetProperty( "Scale" )[2].GetSingle() );

				var obj = scene.CreateObject();
				obj.Name = $"{decal.Name}_{i}";
				obj.Parent = staticMapParent;

				obj.WorldPosition = position;
				obj.WorldRotation = quatRot; //ToAngles(quatRot);
				obj.WorldScale = scale;

				var decalComp = obj.Components.Create<DestinyDecal>( false );
				decalComp.Material = Material.Load( $"Shaders/Source2/Materials/{decal.Name}.vmat" );
				decalComp.DecalPosition = position;
				decalComp.DecalRotation = quatRot;
				decalComp.DecalScale = scale;
				decalComp.Enabled = true;
			}
		}
	}
}
