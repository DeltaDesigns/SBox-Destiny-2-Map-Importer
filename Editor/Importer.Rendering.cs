using System;
using System.IO;
using System.Text.Json;

public partial class DestinyImporter : EditorTool
{
	public static void ImportAtmosphere( string path )
	{
		Log.Info( path );
		if ( !File.Exists( $"{path}/Rendering/Atmosphere.json" ) )
			return;

		JsonDocument cfg = JsonDocument.Parse( File.ReadAllText( $"{path}/Rendering/Atmosphere.json" ) );
		var entry = cfg.RootElement;

		var atmosphere = scene.CreateObject();
		atmosphere.Name = "Atmosphere";

		var atmosComp = atmosphere.Components.GetOrCreate<DestinyAtmosphere>();

		SetTexture( entry, "Lookup0", tex => atmosComp.Texture0 = tex );
		SetTexture( entry, "Lookup1", tex => atmosComp.Texture1 = tex );
	}

	private static void SetTexture( JsonElement entry, string lookupKey, Action<Texture?> setTexture )
	{
		if ( entry.GetProperty( lookupKey ).ValueKind != JsonValueKind.Null )
		{
			var hashProperty = entry.GetProperty( lookupKey ).GetProperty( "Hash" ).GetProperty( "Hash32" );
			Log.Info( hashProperty );
			string hash = ReverseBytes( hashProperty.GetUInt32() ).ToString( "X" );
			Log.Info( hash );

			setTexture( hash != "" ? Texture.Load( $"Textures/Atmosphere/{hash}.vtex" ) : null );
		}
	}

	[Menu( "Editor", "Importer Debug/Export AtmosphereNear Texture" )]
	public static void DebugAtmosTexture()
	{
		var atmosTex = Game.ActiveScene.RenderAttributes.GetTexture( "AtmosHemisphere" );
		Pixmap.FromTexture( atmosTex, false ).SavePng( @$"C:\Users\Michael\Desktop\test.png" );
	}
}

