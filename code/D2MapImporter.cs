using Sandbox;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Tools;
using Tools.MapDoc;
using Tools.MapEditor;

public class D2MapHammerImporter : NoticeWidget //window
{
	
	public D2MapHammerImporter()
	{
		
	}

	[Menu( "Hammer", "Importer/Import D2 Map", "info" )]
	public static void HammerImporter()
	{
		var path = "";
		var map = Hammer.ActiveMap;
		if ( !map.IsValid() ) return;

		//open a file dialog to select the cfg file
		var fd = new FileDialog( null );

		fd.Title = "Select a D2 Map (Info.cfg)";
		fd.SetFindFile();

		if ( fd.Execute() )
		{
			path = fd.SelectedFile;

			Log.Info( path );
		}
		
		//If no file was selected, return
		if ( path == "" || !path.EndsWith( ".cfg" ) )
		{
			return;
		}


		JsonObject cfg = (JsonObject)JsonNode.Parse( File.ReadAllText( path ) );
		//JsonObject cfg = (JsonObject)JsonNode.Parse( FileSystem.Mounted.ReadAllText( "code/test.cfg" ) );

		//Reads each instance (models) and its transforms (position, rotation, scale)
		foreach ( var model in (JsonObject)cfg["Instances"] )
		{
			int i = 0;
			foreach ( var instance in (JsonArray)model.Value )
			{
				i++;
				MapEntity asset = new MapEntity( map );

				asset.ClassName = "prop_static";
				asset.Name = model.Key + " " + i;
				asset.SetKeyValue( "model", $"models/{model.Key}.vmdl" );

				var position = new Vector3( (float)instance["Translation"][0] * 39.37f, (float)instance["Translation"][1] * 39.37f, (float)instance["Translation"][2] * 39.37f );

				Quaternion quatRot = new Quaternion
				{
					X = (float)instance["Rotation"][0],
					Y = (float)instance["Rotation"][1],
					Z = (float)instance["Rotation"][2],
					W = (float)instance["Rotation"][3]
				};
				
				asset.Position = position;
				asset.Angles = ToAngles( quatRot );
				asset.Scale = new Vector3( (float)instance["Scale"] );
			}
		}
	}

	//Todo: Properly instance models for better performance hopefully
	[Menu( "Hammer", "Importer/Instance Test", "info" )]
	public static void InstanceTest()
	{
		var map = Hammer.ActiveMap;
		if ( !map.IsValid() ) return;

		MapNode target = Selection.All.First().Copy();
		target.Scale = new Vector3( 1.5f );
		
		MapInstance instance = new MapInstance()
		{
			Position = target.Position + Vector3.Up * 128.0f,
			Target = target,
			Scale = target.Scale * 2.0f,
			Angles = target.Angles
		};
	}

	//Just testing some stuff
	[Menu( "Hammer", "Importer/Select Test", "info" )]
	public static void SelectTest()
	{
		var map = Hammer.ActiveMap;
		if ( !map.IsValid() ) return;

		//object, scale
		Dictionary<MapNode, float> modelList = new Dictionary<MapNode, float>();
		
		MapNode firstModel = null;
		
		//Select models with the same scale
		foreach (var model in map.World.Children)
		{
			if ( !model.ToString().Contains( "prop_static" ) )
			{
				continue;
			}
			
			firstModel = model;
			
			Log.Info( $"Model: {model.ToString()} Scale: {model.Scale}" );

			foreach ( var model2 in map.World.Children )
			{
				if ( !model2.ToString().Contains( "prop_static" ) )
				{
					continue;
				}

				if (model2.ToString() == firstModel.ToString() )
				{
					if ( model2.Scale == firstModel.Scale )
					{
						Selection.Add( model2 );
					}
				}
			}
			//foreach ( var selected in Selection.All )
			//{
			//	map.DeleteNode( selected );
			//}
			
			
			//modelList.Add( model, model.Scale.x );


		}
	}

	//Converts a Quaternion to Euler Angles + some fuckery to fix certain rotations
	private static Angles ToAngles( Quaternion q, string model = "" )
	{
		float SINGULARITY_THRESHOLD = 0.4999995f;
		float SingularityTest = q.Z * q.X - q.W * q.Y;
		
		float num = 2f * q.W * q.W + 2f * q.X * q.X - 1f;
		float num2 = 2f * q.X * q.Y + 2f * q.W * q.Z;
		float num3 = 2f * q.X * q.Z - 2f * q.W * q.Y;
		float num4 = 2f * q.Y * q.Z + 2f * q.W * q.X;
		float num5 = 2f * q.W * q.W + 2f * q.Z * q.Z - 1f;
		Angles result = default( Angles );

		
		if ( SingularityTest < -SINGULARITY_THRESHOLD)
		{
			result.pitch = 90f;
			result.yaw = MathF.Atan2( q.W, q.X ).RadianToDegree()-90;
			result.roll = MathF.Atan2( q.Y, q.Z ).RadianToDegree()-90;
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
	

	//
	// A dock is one of those tabby floaty windows, like the console and the addon manager.
	//
	//[Dock( "Editor", "My Example Dock", "snippet_folder" )]
	//public class MyExampleDock : Widget
	//{
	//	Color color;

	//	public MyExampleDock( Widget parent ) : base( parent )
	//	{
	//		// Layout top to bottom
	//		SetLayout( LayoutMode.TopToBottom );

	//		var button = new Button( "Change Color", "color_lens" );
	//		button.Clicked = () =>
	//		{
	//			color = Color.Random;
	//			Update();
	//		};

	//		// Fill the top
	//		Layout.AddStretchCell();

	//		// Add a new layout cell to the bottom
	//		var bottomRow = Layout.Add( LayoutMode.LeftToRight );
	//		bottomRow.Margin = 16;
	//		bottomRow.AddStretchCell();
	//		bottomRow.Add( button );
	//	}

	//	protected override void OnPaint()
	//	{
	//		base.OnPaint();

	//		Paint.ClearPen();
	//		Paint.SetBrush( color );
	//		Paint.DrawRect( LocalRect );
	//	}
	//}
}
