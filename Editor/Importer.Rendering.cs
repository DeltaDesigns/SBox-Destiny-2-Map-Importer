using System;
using System.IO;
using System.Text.Json;

public partial class DestinyImporter : EditorTool
{
	public static void ImportAtmosphere( string path )
	{
		if ( !File.Exists( $"{path}/Rendering/Atmosphere.json" ) )
			return;

		JsonDocument cfg = JsonDocument.Parse( File.ReadAllText( $"{path}/Rendering/Atmosphere.json" ) );
		var entry = cfg.RootElement;

		var atmosphere = scene.CreateObject();
		atmosphere.Name = "Atmosphere";

		var renderingRoot = scene.Directory.FindByName( "Rendering" ).FirstOrDefault();
		if ( renderingRoot == null )
		{
			renderingRoot = scene.CreateObject();
			renderingRoot.Name = "Rendering";
		}

		atmosphere.Parent = renderingRoot;

		var atmosComp = atmosphere.Components.GetOrCreate<DestinyAtmosphere>();

		SetTexture( entry, "Texture0", tex => atmosComp.Texture0 = tex );
		SetTexture( entry, "Texture1", tex => atmosComp.Texture1 = tex );
		SetTexture( entry, "Texture4", tex => atmosComp.Texture3 = tex );


		if ( entry.GetProperty( "DayCycle" ).ValueKind != JsonValueKind.Null )
		{
			var rotations = entry.GetProperty( "DayCycle" ).GetProperty( "Rotations" ).EnumerateArray().ToList();

			List<Vector4> rotationList = new();
			foreach ( var rotation in rotations )
			{
				var temp = new Vector4(
					rotation.GetProperty( "X" ).GetSingle(),
					rotation.GetProperty( "Y" ).GetSingle(),
					rotation.GetProperty( "Z" ).GetSingle(),
					rotation.GetProperty( "W" ).GetSingle() );
				rotationList.Add( temp );
			}
			atmosComp.DayCycleRotations = rotationList;
			atmosComp.UseDayCycle = true;
			atmosComp.AffectSceneSun = true;
			//if ( rotations.Count > 2 )
			//{
			//	var temp = rotations[(int)rotations.Count / 2];
			//	comp.SetGlobalChannel( 102, new Vector4( temp.GetProperty( "X" ).GetSingle(), temp.GetProperty( "Y" ).GetSingle(), temp.GetProperty( "Z" ).GetSingle(), temp.GetProperty( "W" ).GetSingle() ) );
			//}
		}
	}

	public static void ImportLensFlares( string path )
	{
		if ( !File.Exists( $"{path}/Rendering/LensFlares.json" ) )
			return;

		JsonDocument cfg = JsonDocument.Parse( File.ReadAllText( $"{path}/Rendering/LensFlares.json" ) );
		var entry = cfg.RootElement;

		var lensFlaresRoot = scene.CreateObject();
		lensFlaresRoot.Name = "Lens Flares";

		var renderingRoot = scene.Directory.FindByName( "Rendering" ).FirstOrDefault();
		if ( renderingRoot == null )
		{
			renderingRoot = scene.CreateObject();
			renderingRoot.Name = "Rendering";
		}

		lensFlaresRoot.Parent = renderingRoot;

		foreach ( var lensFlare in cfg.RootElement.EnumerateObject() )
		{
			int i = 0;
			var lensFlareParent = scene.CreateObject();
			lensFlareParent.Name = $"{lensFlare.Name}";
			lensFlareParent.Parent = lensFlaresRoot;

			foreach ( var transforms in lensFlare.Value.GetProperty( "Instances" ).EnumerateArray() )
			{
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
				obj.Name = $"{lensFlare.Name}_{i}";
				obj.Parent = lensFlareParent;

				obj.WorldPosition = position;
				obj.WorldRotation = quatRot.Angles();

				var comp = obj.Components.GetOrCreate<DestinyLensFlare>();
				comp.LensFlareMaterials = new();
				foreach ( var materialEntry in lensFlare.Value.GetProperty( "Materials" ).EnumerateArray() )
				{
					string matHash = materialEntry.GetString();
					var material = Material.Load( "materials/dev/reflectivity_30.vmat" );// Material.Load( $"Shaders/Source2/Materials/{matHash}.vmat" );
					comp.LensFlareMaterials.Add( material );
				}

				i++;
			}

		}
	}

	private static void SetTexture( JsonElement entry, string lookupKey, Action<Texture?> setTexture )
	{
		if ( entry.GetProperty( lookupKey ).ValueKind != JsonValueKind.Null )
		{
			string hash = entry.GetProperty( lookupKey ).GetString();
			setTexture( hash != "" ? Texture.Load( $"Textures/Atmosphere/{hash}.vtex" ) : null );
		}
	}

	[Menu( "Editor", "Importer Debug/Export AtmosphereNear Texture" )]
	public static void DebugAtmosTexture()
	{
		var atmosTex = Game.ActiveScene.RenderAttributes.GetTexture( "ColorLUT" );
		Pixmap.FromTexture( atmosTex, false ).SavePng( @$"C:\Users\Michael\Desktop\test.png" );
	}

	//----------------------------

	public static void SetGlobalChannels( string path )
	{
		Log.Info( path );
		if ( !File.Exists( $"{path}/Rendering/GlobalChannels.json" ) )
			return;

		var globalChannels = scene.CreateObject();
		globalChannels.Name = "Global Channels";

		var renderingRoot = scene.Directory.FindByName( "Rendering" ).FirstOrDefault();
		if ( renderingRoot == null )
		{
			renderingRoot = scene.CreateObject();
			renderingRoot.Name = "Rendering";
		}
		globalChannels.Parent = renderingRoot;
		var comp = globalChannels.AddComponent<GlobalChannelsController>();

		JsonDocument cfg = JsonDocument.Parse( File.ReadAllText( $"{path}/Rendering/GlobalChannels.json" ) );
		var entry = cfg.RootElement;

		foreach ( var channel in entry.EnumerateObject() )
		{
			// dumb but oh well
			if ( channel.Name == "LUT" )
			{
				var tex = Texture.Load( $"Textures/LUT/{channel.Value.GetProperty( "Name" )}.vtex" );
				if ( tex != null )
					comp.LUT = tex;
			}

			var bytecodeProp = channel.Value.GetProperty( "Bytecode" );
			if ( bytecodeProp.ValueKind == JsonValueKind.Null )
				continue;

			List<byte> bytecode = new();
			List<Vector4> constants = new();
			bytecode = channel.Value.GetProperty( "Bytecode" ).EnumerateArray().Select( x => x.GetByte() ).ToList();
			constants = new();
			foreach ( var constant in channel.Value.GetProperty( "Constants" ).EnumerateArray() )
			{
				constants.Add( new Vector4( constant.GetProperty( "X" ).GetSingle(), constant.GetProperty( "Y" ).GetSingle(), constant.GetProperty( "Z" ).GetSingle(), constant.GetProperty( "W" ).GetSingle() ) );
			}


			var name = channel.Value.GetProperty( "Name" ).GetString();
			var index = channel.Value.GetProperty( "Index" ).GetInt32();

			var globalChannel = scene.CreateObject();
			globalChannel.Parent = globalChannels;
			globalChannel.Name = $"Global Channel {index} : {name}";

			var global = globalChannel.Components.GetOrCreate<GlobalChannel>();
			global.ChannelName = name;
			global.ChannelIndex = index;
			global.Bytecode = bytecode.ToList();
			global.Constants = constants;
			global.Value = constants.First();
		}
		comp.Fill();
	}
}

