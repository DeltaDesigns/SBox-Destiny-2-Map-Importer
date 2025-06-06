using System.IO;
using System.Numerics;
using System.Text.Json;


public partial class DestinyImporter : EditorTool
{
	private static void ImportCubemaps( string path )
	{
		var staticMapRoot = scene.CreateObject();
		staticMapRoot.Name = "Cubemaps";

		JsonDocument cfg = JsonDocument.Parse( File.ReadAllText( $"{path}/Rendering/Cubemaps.json" ) );
		foreach ( var entry in cfg.RootElement.EnumerateObject() )
		{
			var transform = entry.Value.GetProperty( "Transform" );
			Vector3 position = new Vector3(
				transform.GetProperty( "Translation" )[0].GetSingle() * 39.37f,
				transform.GetProperty( "Translation" )[1].GetSingle() * 39.37f,
				transform.GetProperty( "Translation" )[2].GetSingle() * 39.37f );

			Quaternion quatRot = new Quaternion
			{
				X = transform.GetProperty( "Rotation" )[0].GetSingle(),
				Y = transform.GetProperty( "Rotation" )[1].GetSingle(),
				Z = transform.GetProperty( "Rotation" )[2].GetSingle(),
				W = transform.GetProperty( "Rotation" )[3].GetSingle()
			};

			Vector3 scale = new Vector3(
				transform.GetProperty( "Scale" )[0].GetSingle(),
				transform.GetProperty( "Scale" )[1].GetSingle(),
				transform.GetProperty( "Scale" )[2].GetSingle() );

			var staticMapPart = scene.CreateObject();
			staticMapPart.Name = $"{entry.Name}";
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
			if ( entry.Value.GetProperty( "CubemapTexture" ).GetString() != string.Empty )
				cubemap.Texture = Texture.Load( $"textures/cubemaps/{entry.Value.GetProperty( "CubemapTexture" ).GetString()}.vtex" );
			else
			{
				cubemap.RenderDynamically = true;
				cubemap.UpdateStrategy = EnvmapProbe.CubemapDynamicUpdate.OnEnabled;
			}

			cubemap.TintColor = new Color( 0xFF020202 );
			cubemap.Feathering = 16f;



		}
	}
}

