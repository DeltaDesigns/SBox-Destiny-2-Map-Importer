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

			if ( cfg.RootElement.GetProperty( "Atmosphere" ).EnumerateObject().Count() != 0 )
				ImportAtmosphere( cfg.RootElement.GetProperty( "Atmosphere" ) );

			if ( cfg.RootElement.GetProperty( "Instances" ).EnumerateObject().Count() == 0 )
				continue;

			foreach ( JsonProperty model in cfg.RootElement.GetProperty( "Instances" ).EnumerateObject() )
			{
				var skyParent = scene.CreateObject();
				skyParent.Name = $"{model.Name}";
				skyParent.Parent = skyRoot;

				var skyRender = skyParent.Components.GetOrCreate<InstanceRenderer>();
				skyRender.RenderLayer = SceneLayerType.Translucent;
				skyRender.InstanceModel = Model.Load( $"models/SkyEntities/{model.Name}.vmdl" );

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

	public static void ImportAtmosphere( JsonElement entry )
	{
		var atmosphere = scene.CreateObject();
		atmosphere.Name = "Atmosphere";

		var atmosComp = atmosphere.Components.GetOrCreate<DestinyAtmosphere>();

		string tex0 = entry.GetProperty( "Texture0" ).GetString();
		atmosComp.Texture0 = tex0 != "" ? Texture.Load( $"Textures/Atmosphere/{tex0}.vtex" ) : Texture.Transparent;

		string tex1 = entry.GetProperty( "Texture1" ).GetString();
		atmosComp.Texture1 = tex1 != "" ? Texture.Load( $"Textures/Atmosphere/{tex1}.vtex" ) : atmosComp.Texture0;

		string tex2 = entry.GetProperty( "Texture2" ).GetString();
		atmosComp.Texture2 = tex2 != "" ? Texture.Load( $"Textures/Atmosphere/{tex2}.vtex" ) : Texture.Transparent;

		atmosComp.GenerateSkyNear = Shader.Load( "Shaders/d2_sky_lookup_generate_near.shader" );
		atmosComp.GenerateSkyFar = Shader.Load( "Shaders/d2_sky_lookup_generate_far.shader" );
	}

	[Menu( "Editor", "Importer Debug/Test" )]
	public static void OpenMyMenu()
	{
		var atmosTex = Game.ActiveScene.RenderAttributes.GetTexture( "AtmosNear" );
		Pixmap.FromTexture( atmosTex, false ).SavePng( @$"C:\Users\Michael\Desktop\test.png" );
	}
}

