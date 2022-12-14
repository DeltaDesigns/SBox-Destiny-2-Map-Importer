using Sandbox;
using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json.Nodes;
using Editor;
using Editor.MapEditor;
using Editor.MapDoc;
using System.Collections.Generic;

public class D2MapHammerImporter : NoticeWidget //window
{
	
	public D2MapHammerImporter()
	{
		
	}

	[Menu( "Hammer", "Importer/Import D2 Map", "info" )]
	public static void HammerImporter()
	{
		List<string> mapList = new List<string>();
		
		var map = Hammer.ActiveMap;
		if ( !map.IsValid() ) return;

		//open a file dialog to select the cfg file
		var fd = new FileDialog( null );
		fd.SetNameFilter("*.cfg");

		fd.Title = "Select D2 Map(s) (Info.cfg)";
		fd.SetFindExistingFiles();

		if ( fd.Execute() )
		{
			mapList = fd.SelectedFiles;
		}

		
		foreach( string path in mapList)
		{
			JsonNode cfg = JsonNode.Parse( File.ReadAllText( path ) );
			
			// Reads each instance (models) and its transforms (position, rotation, scale)
			foreach ( var model in (JsonObject)cfg["Instances"] )
			{
				MapEntity asset;
				MapInstance asset_instance = null;
				MapEntity previous_model = null;
				int i = 0;

				foreach ( var instance in (JsonArray)model.Value )
				{
					//Create the transforms first before we create the entity
					Vector3 position = new Vector3( (float)instance["Translation"][0] * 39.37f, (float)instance["Translation"][1] * 39.37f, (float)instance["Translation"][2] * 39.37f );

					Quaternion quatRot = new Quaternion
					{
						X = (float)instance["Rotation"][0],
						Y = (float)instance["Rotation"][1],
						Z = (float)instance["Rotation"][2],
						W = (float)instance["Rotation"][3]
					};

					if ( previous_model == null )
					{
						asset = new MapEntity( map );

						asset.ClassName = "prop_static";
						asset.Name = model.Key + " " + i;
						asset.SetKeyValue( "model", $"models/{model.Key}.vmdl" );
						//asset.SetKeyValue( "Disable Mesh Merging", $"true" ); not working for some reason?

						//asset.Position = position;
						//asset.Angles = ToAngles( quatRot );
						asset.Scale = new Vector3( (float)instance["Scale"] );
						
						asset_instance = new MapInstance()
						{
							Target = asset,
							Position = position,
							Angles = ToAngles( quatRot )
						};
						previous_model = asset;
					}
					else
					{
						if ( previous_model.Scale == (float)instance["Scale"] )
						{
							asset_instance.Copy();
							asset_instance.Angles = ToAngles( quatRot );
							asset_instance.Position = position;
						}
						else
						{
							asset = new MapEntity( map );

							asset.ClassName = "prop_static";
							asset.Name = model.Key + " " + i;
							asset.SetKeyValue( "model", $"models/{model.Key}.vmdl" );
							//asset.SetKeyValue( "Disable Mesh Merging", $"true" ); not working for some reason?

							//asset.Position = position;
							//asset.Angles = ToAngles( quatRot );
							asset.Scale = new Vector3( (float)instance["Scale"] );

							asset_instance = new MapInstance()
							{
								Target = asset,
								Position = position,
								Angles = ToAngles( quatRot )
							};
							previous_model = asset;
						}
					}
					i++;
				}
			}
		}
	}

	//Todo: Properly instance models for better performance hopefully
	//[Menu( "Hammer", "Importer/Instance Test", "info" )]
	//public static void InstanceTest()
	//{
	//	var map = Hammer.ActiveMap;
	//	if ( !map.IsValid() ) return;

	//	MapNode target = Selection.All.First();

	//	MapInstance instance = new MapInstance()
	//	{
	//		Position = target.Position + Vector3.Up * 128.0f,
	//		Target = target,
	//		Scale = target.Scale,
	//		Angles = target.Angles
	//	};
	//}


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
}
