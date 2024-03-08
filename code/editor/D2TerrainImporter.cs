using Sandbox;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Editor;

/// <summary>
/// Import D2 terrain from a cfg file
/// </summary>
[EditorTool]
[Title( "Import D2 Terrain" )]
[Icon( "landscape" )]
[Alias( "d2_terrain" )]
[Group( "D2 Tools" )]
[Order( 0 )]
public class D2TerrainImporter : EditorTool
{
	public D2TerrainImportSettings ImportSettings { get; private set; } = new();
	public static Scene scene { get; private set; }
	public static D2TerrainImporter Current { get; set; }

	public override void OnEnabled()
	{
		Current = this;
		AllowGameObjectSelection = false;

		var brushSettings = new ImportSettingsWidgetWindow( SceneOverlay, EditorUtility.GetSerializedObject( ImportSettings ) );
		AddOverlay( brushSettings, TextFlag.RightBottom, 10 );

		scene = Scene;
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
	}

	public void ImportTerrain()
	{
		List<string> mapList = new List<string>();

		var map = scene;
		if ( !scene.Active )
		{
			D2MapImporterPopup.Popup( "D2 Map Importer", "You need to have an active map! (File->New)", Color.Red, 2 );
			return;
		}

		//open a file dialog to select cfg files
		var fd = new FileDialog( null );
		fd.SetNameFilter( "*.cfg" );
		fd.Title = "Select D2 Map(s) (Info.cfg)";
		fd.SetFindExistingFiles();

		if ( fd.Execute() )
		{
			mapList = fd.SelectedFiles;
		}
		else
			return;

		var terrainRoot = scene.CreateObject();
		terrainRoot.Name = "Terrain";

		//scene.GetAllObjects(true).Count( x => x.Name == "Terrain" )

		foreach ( string path in mapList )
		{
			JsonDocument cfg = JsonDocument.Parse( File.ReadAllText( path ) );

			if ( cfg.RootElement.GetProperty( "TerrainDyemaps" ).EnumerateObject().Count() == 0 )
			{
				Log.Info( $"D2 Terrain Importer: {Path.GetFileNameWithoutExtension( path )} contains no terrain, skipping" );
				continue;
			}

			foreach ( JsonProperty model in cfg.RootElement.GetProperty( "TerrainDyemaps" ).EnumerateObject() )
			{
				string modelName = $"models/Terrain/{model}.vmdl";
				int i = 0;

				var terrainPartParent = scene.CreateObject();
				terrainPartParent.Name = $"{model.Name}";
				terrainPartParent.Parent = terrainRoot;

				foreach ( JsonElement terrain in model.Value.EnumerateArray().Reverse() )
				{
					Log.Info( $"{model.Name}_{i} => {terrain.GetString()}" );

					var terrainPart = scene.CreateObject();
					terrainPart.Name = $"{model.Name}_{i}";
					terrainPart.Parent = terrainPartParent;

					var mdl = terrainPart.Components.GetOrCreate<ModelRenderer>();
					mdl.Model = Model.Load( $"models/Terrain/{model.Name}_{i}.vmdl" );

					if ( !ImportSettings.OverrideMaterials )
					{
						var dyemap = terrainPart.Components.GetOrCreate<TerrainDyemap>();
						dyemap.DyemapTexture = Texture.Load( FileSystem.Content, $"textures/{terrain.GetString()}.vtex" );
					}
					else
					{
						mdl.MaterialOverride = Material.Load( "materials/dev/reflectivity_50.vmat" );
					}


					i++;
				}
			}
		}
	}
}

public class D2TerrainImportSettings
{
	[Property] public bool OverrideMaterials { get; set; } = false;
}

public class ImportSettingsWidgetWindow : WidgetWindow
{
	public ImportSettingsWidgetWindow( Widget parent, SerializedObject so ) : base( parent, "Import Settings" )
	{
		Layout = Layout.Row();
		Layout.Margin = 8;
		MaximumWidth = 300.0f;

		var cs = new ControlSheet();
		cs.AddRow( so.GetProperty( nameof( D2TerrainImportSettings.OverrideMaterials ) ) );

		cs.SetMinimumColumnWidth( 0, 50 );
		cs.Margin = new Sandbox.UI.Margin( 8, 0, 4, 0 );

		var l = Layout.Row();
		l.Add( cs );

		Layout.Add( l );

		var files = l.Add( new Button.Primary( "Select Files", "info" ) );
		files.Clicked = () => D2TerrainImporter.Current.ImportTerrain();
	}
}
