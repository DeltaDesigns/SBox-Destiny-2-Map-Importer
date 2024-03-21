using System;
using System.IO;
using System.Numerics;
using System.Text.Json;

namespace Editor;

/// <summary>
/// Import D2 terrain from a cfg file
/// </summary>
[EditorTool]
[Title( "Import D2 Statics" )]
[Icon( "local_cafe" )]
[Alias( "d2_static" )]
[Group( "D2 Tools" )]
[Order( 0 )]
public class D2StaticImporter : EditorTool
{
	public D2StaticImportSettings ImportSettings { get; private set; } = new();
	public static Scene scene { get; private set; }
	public static D2StaticImporter Current { get; set; }

	public D2StaticImporter()
	{
		Current = this;
	}

	public override void OnEnabled()
	{
		AllowGameObjectSelection = false;

		var brushSettings = new StaticImportSettingsWidgetWindow( SceneOverlay, EditorUtility.GetSerializedObject( ImportSettings ) );
		AddOverlay( brushSettings, TextFlag.RightBottom, 10 );

		scene = Scene;
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
	}

	public void ImportStatics()
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

		var staticMapRoot = scene.CreateObject();
		staticMapRoot.Name = "Static Map";

		//scene.GetAllObjects(true).Count( x => x.Name == "Terrain" )

		foreach ( string path in mapList )
		{
			JsonDocument cfg = JsonDocument.Parse( File.ReadAllText( path ) );

			if ( cfg.RootElement.GetProperty( "Instances" ).EnumerateObject().Count() == 0 )
			{
				Log.Info( $"D2 Terrain Importer: {Path.GetFileNameWithoutExtension( path )} contains no instances, skipping" );
				continue;
			}

			foreach ( JsonProperty model in cfg.RootElement.GetProperty( "Instances" ).EnumerateObject() )
			{
				string modelName = $"models/Statics/{model}.vmdl";
				int i = 0;

				var staticMapParent = scene.CreateObject();
				staticMapParent.Name = $"{model.Name}";
				staticMapParent.Parent = staticMapRoot;

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

					staticMapPart.Transform.Position = position;
					staticMapPart.Transform.Rotation = ToAngles( quatRot );
					staticMapPart.Transform.Scale = scale;

					var mdl = staticMapPart.Components.GetOrCreate<ModelRenderer>();
					mdl.Model = Model.Load( $"models/Statics/{model.Name}.vmdl" );

					if ( ImportSettings.OverrideMaterials )
					{
						mdl.MaterialOverride = Material.Load( "materials/dev/reflectivity_50.vmat" );
					}

					// Until map collisions are properly figured out, we're just gonna use the model itself as the collider....
					var col = staticMapPart.Components.GetOrCreate<ModelCollider>();
					col.Static = true;
					col.Model = mdl.Model;

					i++;
				}
			}
		}
	}

	//Converts a Quaternion to Euler Angles + some fuckery to fix certain rotations
	private static Angles ToAngles( Quaternion q )
	{
		if ( q == Quaternion.Zero )
			return Angles.Zero;

		float SINGULARITY_THRESHOLD = 0.4999995f;
		float SingularityTest = q.Z * q.X - q.W * q.Y;

		float num = 2f * q.W * q.W + 2f * q.X * q.X - 1f;
		float num2 = 2f * q.X * q.Y + 2f * q.W * q.Z;
		float num3 = 2f * q.X * q.Z - 2f * q.W * q.Y;
		float num4 = 2f * q.Y * q.Z + 2f * q.W * q.X;
		float num5 = 2f * q.W * q.W + 2f * q.Z * q.Z - 1f;
		Angles result = default;

		if ( SingularityTest < -SINGULARITY_THRESHOLD )
		{
			result.pitch = 90f;
			result.yaw = MathF.Atan2( q.W, q.X ).RadianToDegree() - 90;
			result.roll = MathF.Atan2( q.Y, q.Z ).RadianToDegree() - 90;
		}
		else if ( SingularityTest > SINGULARITY_THRESHOLD )
		{
			result.pitch = -90f;
			result.yaw = -MathF.Atan2( q.W, q.X ).RadianToDegree() + 90;
			result.roll = MathF.Atan2( q.Y, q.Z ).RadianToDegree() + 90;
		}
		else
		{
			result.pitch = MathF.Asin( 0 - num3 ).RadianToDegree();
			result.yaw = MathF.Atan2( num2, num ).RadianToDegree();
			result.roll = MathF.Atan2( num4, num5 ).RadianToDegree();
		}

		return new Angles( result.pitch, result.yaw, result.roll );
	}
}

public class D2StaticImportSettings
{
	[Property] public bool OverrideMaterials { get; set; } = false;
}

public class StaticImportSettingsWidgetWindow : WidgetWindow
{
	public StaticImportSettingsWidgetWindow( Widget parent, SerializedObject so ) : base( parent, "Import Settings" )
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
		files.Clicked = () => D2StaticImporter.Current.ImportStatics();
	}
}
