using System.IO;
using System.Numerics;
using System.Text.Json;

public partial class DestinyImporter : EditorTool
{
	private static void ImportSkyObjects( List<string> mapList )
	{
		var skyRoot = scene.CreateObject();
		skyRoot.Name = "Sky Objects";

		foreach ( string path in mapList )
		{
			JsonDocument cfg = JsonDocument.Parse( File.ReadAllText( path ) );

			if ( cfg.RootElement.GetProperty( "Instances" ).EnumerateObject().Count() == 0 )
				continue;

			foreach ( JsonProperty model in cfg.RootElement.GetProperty( "Instances" ).EnumerateObject() )
			{
				var mdl = Model.Load( $"models/SkyEntities/{model.Name}.vmdl" );
				if ( IsValidModel( mdl ) )
					continue;

				var skyParent = scene.CreateObject();
				skyParent.Name = $"{model.Name}";
				skyParent.Parent = skyRoot;

				var skyRender = skyParent.Components.GetOrCreate<InstanceRenderer>();
				skyRender.ObjectType = InstanceRenderer.FeatureType.Sky;
				skyRender.InstanceModel = mdl;

				List<Transform> transforms = new List<Transform>();
				foreach ( JsonElement instance in model.Value.EnumerateArray() )
				{
					Vector3 position = new Vector3(
						instance.GetProperty( "Translation" )[0].GetSingle() * 39.37f,
						instance.GetProperty( "Translation" )[1].GetSingle() * 39.37f,
						instance.GetProperty( "Translation" )[2].GetSingle() * 39.37f );

					Quaternion quatRot = new Quaternion
					{
						X = instance.GetProperty( "Rotation" )[0].GetSingle(),
						Y = instance.GetProperty( "Rotation" )[1].GetSingle(),
						Z = instance.GetProperty( "Rotation" )[2].GetSingle(),
						W = instance.GetProperty( "Rotation" )[3].GetSingle()
					};

					Vector3 scale = new Vector3(
						instance.GetProperty( "Scale" )[0].GetSingle(),
						instance.GetProperty( "Scale" )[1].GetSingle(),
						instance.GetProperty( "Scale" )[2].GetSingle() );

					transforms.Add( new Transform()
					{
						Position = position,
						Rotation = quatRot,
						Scale = scale
					} );
				}
				skyRender.Transforms = transforms.ToArray();
			}
		}
	}
}

