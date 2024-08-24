using System.IO;
using System.Numerics;
using System.Text.Json;


public partial class DestinyImporter : EditorTool
{
	private static void ImportDecorations( List<string> mapList )
	{
		var decoratorRoot = scene.CreateObject();
		decoratorRoot.Name = "Decoration";

		foreach ( string path in mapList )
		{
			JsonDocument cfg = JsonDocument.Parse( File.ReadAllText( path ) );

			if ( cfg.RootElement.GetProperty( "Instances" ).EnumerateObject().Count() == 0 )
				continue;

			foreach ( JsonProperty model in cfg.RootElement.GetProperty( "Instances" ).EnumerateObject() )
			{
				var decoratorParent = scene.CreateObject();
				decoratorParent.Name = $"{model.Name}";
				decoratorParent.Parent = decoratorRoot;

				var decorRender = decoratorParent.Components.GetOrCreate<InstanceRenderer>();
				decorRender.RenderLayer = SceneLayerType.Opaque;
				decorRender.InstanceModel = Model.Load( $"models/Decorators/{model.Name}.vmdl" );

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
				decorRender.Transforms = transforms.ToArray();
			}
		}
	}
}

