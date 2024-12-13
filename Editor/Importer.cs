using System.IO;
using System.Numerics;
using System.Text.Json;

[EditorTool]
[Title( "Import Destiny Map" )]
[Icon( "local_cafe" )]
[Alias( "destiny_importer" )]
[Group( "Destiny Tools" )]
[Order( 0 )]
public partial class DestinyImporter : EditorTool
{
	// General
	public static bool _importObjects = true;
	public static bool _instanceObjects = true;
	public static bool _importCubemaps = true;

	//Lights
	public static bool _importLights = true;
	public static bool _approximateLightIntensity = false;
	public static bool _overrideLightColor = false;
	public static bool _overrideLightIntensity = false;
	public static float _lightIntensityMultiplier = 1.0f;
	public static Color _lightColor = Color.Gray;

	//Misc
	public static bool _overrideTerrainMats = false;
	public static bool _overrideAllMats = false;

	private NavigationView View { get; set; }
	private BaseWindow Window { get; set; }
	public static Scene scene { get; private set; }

	public override void OnEnabled()
	{
		AllowGameObjectSelection = false;

		scene = Scene;
		Window = new();

		Window.WindowTitle = "Import Options";
		Window.SetWindowIcon( "grid_view" );

		Window.Size = new Vector2( 550, 450 );
		View = new NavigationView( Window );
		Window.Layout = Layout.Column();
		Window.Layout.Add( View );

		CreateUI();
		Window.Show();
	}

	public void CreateUI()
	{
		var general = new NavigationView.Option( "General", "settings" );
		general.CreatePage = () =>
		{
			var scroll = new ScrollArea( null );
			scroll.Canvas = new Widget( scroll );
			scroll.Canvas.Layout = Layout.Column();

			var top = scroll.Canvas.Layout.AddColumn();
			var body = scroll.Canvas.Layout;
			body.Margin = 16;
			body.Spacing = 12;

			var title = new Label.Title( "General Settings" );
			title.Alignment = TextFlag.CenterHorizontally;
			top.Add( title );
			top.AddSeparator( true );

			Checkbox importObjects = body.Add( new Checkbox( "Import Map Objects" ), 2 );
			importObjects.Value = _importObjects;
			importObjects.Clicked = () => _importObjects = importObjects.Value;
			body.Add( new Label.Small( "Uncheck if you just want to import things like cubemaps and lights" ) );
			body.AddSeparator( true );

			Checkbox createInstances = body.Add( new Checkbox( "Instance Map Objects" ), 2 );
			createInstances.Value = _instanceObjects;
			createInstances.Clicked = () => _instanceObjects = createInstances.Value;
			body.Add( new Label.Small( "Use Instancing For Sky and Decoration Objects (Recommended)" ) );
			body.AddSeparator( true );

			Checkbox importCubemaps = body.Add( new Checkbox( "Import Cubemaps" ), 2 );
			importCubemaps.Value = _importCubemaps;
			importCubemaps.Clicked = () => _importCubemaps = importCubemaps.Value;
			body.Add( new Label.Small( "Imports cubemaps. May need manually adjusted" ) );

			body.AddSpacingCell( 32 );
			body.AddStretchCell();
			return scroll;
		};

		var lights = new NavigationView.Option( "Lights", "lightbulb" );
		lights.CreatePage = () =>
		{
			var scroll = new ScrollArea( null );
			scroll.Canvas = new Widget( scroll );
			scroll.Canvas.Layout = Layout.Column();
			scroll.Canvas.UpdatesEnabled = true;

			var top = scroll.Canvas.Layout.AddColumn();
			var body = scroll.Canvas.Layout;
			body.Margin = 16;
			body.Spacing = 12;

			var title = new Label.Title( "Light Settings" );
			title.Alignment = TextFlag.CenterHorizontally;
			top.Add( title );
			top.AddSeparator( true );

			Checkbox importLights = body.Add( new Checkbox( "Import Lights" ), 2 );
			importLights.Value = _importLights;
			importLights.Clicked = () => _importLights = importLights.Value;
			body.Add( new Label.Small( "Imports lights. May need manual adjustments." ) );
			body.AddSeparator( true );

			//--------------------------

			Checkbox approxIntensity = body.Add( new Checkbox( "Approximate Light Brightness" ), 2 );
			approxIntensity.Value = _approximateLightIntensity;

			FloatProperty lightMultiplier = body.Add( new FloatProperty( null ) );
			lightMultiplier.Visible = _approximateLightIntensity;
			lightMultiplier.Value = _lightIntensityMultiplier;
			lightMultiplier.OnChildValuesChanged += ( body ) => _lightIntensityMultiplier = lightMultiplier.Value;

			approxIntensity.Clicked = () =>
			{
				_approximateLightIntensity = approxIntensity.Value;
				lightMultiplier.Visible = _approximateLightIntensity;
			};
			body.Add( new Label.Small( "Approximate light brightness from light range with optional multiplier. May give mixed results" ) );
			body.AddSeparator( true );

			//--------------------------

			Checkbox overrideLightColor = body.Add( new Checkbox( "Override Light Color" ), 2 );
			overrideLightColor.Value = _overrideLightColor;

			ColorProperty lightColor = body.Add( new ColorProperty( null ) );
			lightColor.Visible = _overrideLightColor;
			lightColor.Value = _lightColor;
			lightColor.OnChildValuesChanged += ( body ) => _lightColor = lightColor.Value;

			overrideLightColor.Clicked = () =>
			{
				_overrideLightColor = overrideLightColor.Value;
				lightColor.Visible = _overrideLightColor;
			};
			body.Add( new Label.Small( "Override light color" ) );
			body.AddSeparator( true );

			body.AddSpacingCell( 32 );
			body.AddStretchCell();
			return scroll;
		};

		var misc = new NavigationView.Option( "Misc", "miscellaneous_services" );
		misc.CreatePage = () =>
		{
			var scroll = new ScrollArea( null );
			scroll.Canvas = new Widget( scroll );
			scroll.Canvas.Layout = Layout.Column();

			var top = scroll.Canvas.Layout.AddColumn();
			var body = scroll.Canvas.Layout;
			body.Margin = 16;
			body.Spacing = 12;

			var title = new Label.Title( "Miscellaneous" );
			title.Alignment = TextFlag.CenterHorizontally;
			top.Add( title );
			top.AddSeparator( true );

			Checkbox overrideAllMats = body.Add( new Checkbox( "Override All Materials" ), 2 );
			overrideAllMats.Value = _overrideAllMats;
			overrideAllMats.Clicked = () => _overrideAllMats = overrideAllMats.Value;
			body.Add( new Label.Small( "Force All Objects To Use Generic Dev Texture, with a random color :)" ) );
			body.AddSeparator( true );

			Checkbox overrideTerrain = body.Add( new Checkbox( "Override Terrain Materials" ), 2 );
			overrideTerrain.Value = _overrideTerrainMats;
			overrideTerrain.Clicked = () => _overrideTerrainMats = overrideTerrain.Value;
			body.Add( new Label.Small( "Force Terrain Objects To Use Generic Dev Texture" ) );

			body.AddSpacingCell( 32 );
			body.AddStretchCell();
			return scroll;
		};

		View.MenuContents.Spacing = 8;
		var top = View.MenuTop.AddRow();
		top.Add( new Label.Subtitle( "Options" )
		{
			Alignment = TextFlag.CenterHorizontally,
		} );

		View.AddPage( general );
		View.AddPage( lights );
		View.AddPage( misc );

		var files = View.MenuBottom.Add( new Button.Primary( "Select Files", "info" ) );
		files.Clicked = () => ImportCFG();
	}

	public void ImportCFG()
	{
		List<string> mapList = new List<string>();
		string basePath;

		var map = scene;
		if ( !scene.Active )
		{
			//D2MapImporterPopup.Popup( "D2 Map Importer", "You need to have an active map! (File->New)", Color.Red, 2 );
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
			basePath = Path.GetDirectoryName( mapList[0] );
		}
		else
			return;

		ImportAtmosphere( basePath );
		//return;

		if ( _importLights )
			ImportLights( mapList ); //Import lights, WIP

		if ( _importCubemaps )
			ImportCubemaps( mapList ); //Import cubemaps

		if ( !_importObjects )
			return;

		ImportTerrain( mapList.Where( x => x.GetFilenameSafe().Contains( "Terrain_info" ) ).ToList() );
		if ( _instanceObjects )
		{
			ImportDecorations( mapList.Where( x => x.GetFilenameSafe().Contains( "Decorators_info" ) ).ToList() );
			ImportSkyObjects( mapList.Where( x => x.GetFilenameSafe().Contains( "SkyEnts_info" ) ).ToList() );
		}

		// Statics / Normal Entities
		var staticMapRoot = scene.CreateObject();
		staticMapRoot.Name = "Static Map";
		foreach ( string path in mapList )
		{
			JsonDocument cfg = JsonDocument.Parse( File.ReadAllText( path ) );
			if ( cfg.RootElement.GetProperty( "Instances" ).EnumerateObject().Count() == 0 || cfg.RootElement.GetProperty( "MeshName" ).GetRawText().Contains( "Terrain" ) )
				continue;

			string fileName = Path.GetFileNameWithoutExtension( path );
			fileName = fileName.Substring( 0, fileName.Length - 5 ); //removes "_info" from the name

			ImportType type = ImportType.Static;
			switch ( true )
			{
				case true when fileName.Contains( "Entities" ):
					type = ImportType.Entity;
					break;
				case true when fileName.Contains( "Terrain" ):
					continue;
				case true when fileName.Contains( "SkyEnts" ):
				case true when fileName.Contains( "Decorators" ):
					if ( _instanceObjects )
						continue;  // Skip import
					else
						type = fileName.Contains( "SkyEnts" ) ? ImportType.Sky : ImportType.Decorator;
					break;
				default:
					break;
			}

			var group = scene.CreateObject();
			group.Name = fileName;
			group.Parent = staticMapRoot;
			group.NetworkMode = NetworkMode.Never;

			foreach ( JsonProperty model in cfg.RootElement.GetProperty( "Instances" ).EnumerateObject() )
			{
				string modelName = GetModelPath( type, model.Name );
				var static_mdl = Model.Load( modelName );
				if ( IsValidModel( static_mdl ) )
					continue;

				int i = 0;
				var staticMapParent = scene.CreateObject();
				staticMapParent.Name = $"{model.Name}";
				staticMapParent.Parent = group;
				staticMapParent.NetworkMode = NetworkMode.Never;

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
					staticMapPart.WorldRotation = ToAngles( quatRot );
					staticMapPart.WorldScale = scale;
					staticMapPart.NetworkMode = NetworkMode.Never;

					var mdl = staticMapPart.Components.GetOrCreate<ModelRenderer>();
					mdl.Model = static_mdl;

					if ( _overrideAllMats && type != ImportType.Sky )
					{
						mdl.MaterialOverride = Material.Load( "materials/dev/reflectivity_50.vmat" );
						mdl.Tint = Color.Random;
					}

					if ( type == ImportType.Static || type == ImportType.Entity )
					{
						// Until map collisions are properly figured out, we're just gonna use the model itself as the collider....
						var col = staticMapPart.Components.GetOrCreate<ModelCollider>();
						col.Static = true;
						col.Model = mdl.Model;
					}
					if ( type == ImportType.Decorator || type == ImportType.Sky )
					{
						mdl.RenderType = ModelRenderer.ShadowRenderType.Off;
					}
					i++;
				}
			}
		}
	}

	public override void OnDisabled()
	{

	}

	public override void OnUpdate()
	{

	}
}
