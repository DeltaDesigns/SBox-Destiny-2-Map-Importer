using System.IO;
using System.Numerics;
using System.Text.Json;


public partial class DestinyImporter : EditorTool
{
	private static void ImportCubemaps( List<string> mapList )
	{
		var staticMapRoot = scene.CreateObject();
		staticMapRoot.Name = "Cubemaps";

		foreach ( string path in mapList )
		{
			JsonDocument cfg = JsonDocument.Parse( File.ReadAllText( path ) );
			if ( cfg.RootElement.GetProperty( "Cubemaps" ).EnumerateObject().Count() == 0 )
				continue;

			foreach ( var model in cfg.RootElement.GetProperty( "Cubemaps" ).EnumerateObject() )
			{
				int i = 0;
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
					staticMapPart.Parent = staticMapRoot;

					staticMapPart.WorldPosition = position;
					staticMapPart.WorldRotation = ToAngles( quatRot );

					var cubemap = staticMapPart.Components.GetOrCreate<EnvmapProbe>();
					cubemap.Bounds = new BBox
					{
						Mins = -scale * 39.37f,
						Maxs = scale * 39.37f
					};

					cubemap.Projection = SceneCubemap.ProjectionMode.Box;
					if ( instance.GetProperty( "Texture" ).GetString() != string.Empty )
						cubemap.Texture = Texture.Load( $"textures/{instance.GetProperty( "Texture" )}.vtex" );
					else
					{
						cubemap.RenderDynamically = true;
						cubemap.UpdateStrategy = EnvmapProbe.CubemapDynamicUpdate.OnEnabled;
					}

					cubemap.TintColor = new Color( 0xFF020202 );
					cubemap.Feathering = 16f;

					i++;
				}
			}
		}
	}
}

