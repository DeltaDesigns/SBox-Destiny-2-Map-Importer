using System.IO;
using System.Numerics;
using System.Text.Json;

public partial class DestinyImporter : EditorTool
{
	private static void ImportWaterDecals( List<string> mapList )
	{
		if ( mapList.Count == 0 )
			return;

		var waterRoot = scene.CreateObject();
		waterRoot.Name = "Water Decals";

		foreach ( string path in mapList )
		{
			JsonDocument cfg = JsonDocument.Parse( File.ReadAllText( path ) );

			if ( cfg.RootElement.GetProperty( "Instances" ).EnumerateObject().Count() == 0 )
				continue;

			foreach ( JsonProperty model in cfg.RootElement.GetProperty( "Instances" ).EnumerateObject() )
			{
				if ( model.Name.Contains( "_Reflection" ) )
					continue;

				var static_mdl = Model.Load( $"Models/WaterDecals/{model.Name}.vmdl" );
				if ( IsInvalidModel( static_mdl ) )
					continue;

				int i = 0;
				var staticMapParent = scene.CreateObject();
				staticMapParent.Name = $"{model.Name}";
				staticMapParent.Parent = waterRoot;

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

					var staticMapPart = scene.CreateObject();
					staticMapPart.Name = $"{model.Name}_{i}";
					staticMapPart.Parent = staticMapParent;

					staticMapPart.WorldPosition = position;
					staticMapPart.WorldRotation = quatRot;
					staticMapPart.WorldScale = scale;

					var mdl = staticMapPart.Components.GetOrCreate<ModelRenderer>();
					mdl.Model = static_mdl;
					i++;
				}
			}
		}
	}
}

