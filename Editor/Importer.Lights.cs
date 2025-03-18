using System;
using System.IO;
using System.Text.Json;


public partial class DestinyImporter : EditorTool
{
	private static void ImportLights( List<string> mapList )
	{
		var staticMapRoot = scene.CreateObject();
		staticMapRoot.Name = "Lights";

		foreach ( string path in mapList )
		{
			JsonDocument cfg = JsonDocument.Parse( File.ReadAllText( path ) );
			if ( cfg.RootElement.GetProperty( "Lights" ).EnumerateObject().Count() == 0 )
				continue;

			foreach ( var light in cfg.RootElement.GetProperty( "Lights" ).EnumerateObject() )
			{
				int i = 0;
				var staticMapParent = scene.CreateObject();
				staticMapParent.Name = $"{light.Name}";
				staticMapParent.Parent = staticMapRoot;

				foreach ( var transforms in light.Value.EnumerateArray() )
				{
					//Create the transforms first before we create the entity
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

					var obj = scene.CreateObject();
					obj.Name = $"{light.Name}_{i}";
					obj.Parent = staticMapParent;

					obj.WorldPosition = position;
					obj.WorldRotation = quatRot.Angles(); //ToAngles(quatRot);

					Vector3 color = Vector3.One;

					var cfgColor = transforms.GetProperty( "Color" );

					var r = (cfgColor[0].GetSingle()) * _lightIntensityMultiplier;
					var g = (cfgColor[1].GetSingle()) * _lightIntensityMultiplier;
					var b = (cfgColor[2].GetSingle()) * _lightIntensityMultiplier;
					float[] data = { r, g, b };
					float v = data.Max() / 10;

					if ( data.All( v => v >= 0.0f ) )
					{
						//bool needsNormalization = data.Any( v => v > 1.0f );
						float[] floats;
						floats = (float[])data.Clone();
						color.x = Math.Abs( floats[0] );
						color.y = Math.Abs( floats[1] );
						color.z = Math.Abs( floats[2] );
					}

					if ( _overrideLightColor )
						color = new Vector3( _lightColor.r, _lightColor.g, _lightColor.b ) * _lightIntensityMultiplier;

					//if ( _approximateLightIntensity )
					//	lightEntity.SetKeyValue( "Brightness", $"{EstimateLightIntensity( transforms.GetProperty( "Range" ).GetSingle() * 39.37 )}" );

					float range = (transforms.GetProperty( "Range" ).GetSingle() * 39.37f);// * 1.15f;
					float attenuation = 1f;// CalculateAttenuation( color, range, obj.Name ); ;//MathX.Remap( transforms.GetProperty( "Attenuation" ).GetSingle(), 0f, 1f, 0f, 10f, false );

					string type = transforms.GetProperty( "Type" ).GetString();

					//color /= ((Color)color).Luminance;
					var col = new Color( color.x, color.y, color.z ).Darken( 0.75f );

					switch ( type )
					{
						case "Line": // o7 Capsule lights
							var lineLight = obj.Components.GetOrCreate<CapusleLight>();
							var size = transforms.GetProperty( "Size" );
							lineLight.LightColor = col;
							lineLight.Radius = range;
							lineLight.Shadows = false;
							lineLight.Attenuation = attenuation * 39.37f;
							lineLight.Size = new Vector2( size[0].GetSingle(), size[1].GetSingle() );
							break;

						case "Point":
							var pointLight = obj.Components.GetOrCreate<PointLight>();
							pointLight.LightColor = col;
							pointLight.Radius = range;
							pointLight.Shadows = false;
							pointLight.Attenuation = attenuation * 39.37f;

							//if ( type == "Line" )
							//{
							//	pointLight.Attenuation = attenuation * 1.25f;
							//	pointLight.Radius = range * 1.1f;
							//}
							break;
						case "Shadowing":
						case "Spot":
							var fov = transforms.GetProperty( "Size" )[0].GetSingle();
							var spotLight = obj.Components.GetOrCreate<SpotLight>();
							spotLight.LightColor = col;
							spotLight.Radius = range;
							spotLight.Shadows = false;
							spotLight.Attenuation = attenuation;
							spotLight.ConeOuter = fov.RadianToDegree() / 2f; // sbox uses half-angles
							spotLight.ConeInner = (fov / 1.5f).RadianToDegree() / 2f;

							if ( transforms.GetProperty( "Cookie" ).GetString() != "" )
								spotLight.Cookie = Texture.Load( $"textures/{transforms.GetProperty( "Cookie" ).GetString()}.vtex" );

							if ( type == "Shadowing" )
							{
								//spotLight.Radius = range * 2; // idk
								//spotLight.LightColor *= 2;
								//spotLight.Attenuation = 1f;
								spotLight.Shadows = true;
							}

							break;
						default:
							var defaultLight = obj.Components.GetOrCreate<PointLight>();
							defaultLight.LightColor = col;
							defaultLight.Radius = range;
							defaultLight.Shadows = false;
							defaultLight.Attenuation = attenuation;
							break;
					}
					i++;
				}
				if ( !_overrideLightColor )
					TryGetBytecode( staticMapParent );
			}
		}
	}

	public static void TryGetBytecode( GameObject gameObject )
	{
		string fileName = gameObject.Name.Substring( gameObject.Name.Length - 8 );
		if ( !Editor.FileSystem.Mounted.FileExists( $"Shaders/Source2/Lights/{fileName}.bin" ) )
			return;

		var stream = Editor.FileSystem.Mounted.OpenRead( $"Shaders/Source2/Lights/{fileName}.bin" );
		BinaryReader reader = new( stream );

		// Go to bytecode array
		reader.BaseStream.Seek( 0x30, SeekOrigin.Begin );
		var len = reader.ReadInt32();
		byte[] bytecode;
		if ( len > 0 )
		{
			reader.BaseStream.Seek( 4, SeekOrigin.Current );
			var offset = reader.ReadInt32();
			reader.BaseStream.Seek( offset + 0x10 - 0x4, SeekOrigin.Current );
			bytecode = reader.ReadBytes( len );
		}
		else // No bytecode? Time to die :)\
		{
			stream.Dispose();
			reader.Dispose();
			return;
		}

		// Go to bytecode constants
		reader.BaseStream.Position = 0;
		reader.BaseStream.Seek( 0x40, SeekOrigin.Begin );
		len = reader.ReadInt32();
		Vector4[] constants = new Vector4[len];
		if ( len > 0 )
		{
			reader.BaseStream.Seek( 4, SeekOrigin.Current );
			var offset = reader.ReadInt32();

			reader.BaseStream.Seek( offset + 0x10 - 0x4, SeekOrigin.Current );
			for ( int i = 0; i < len; i++ )
			{
				constants[i] = (new Vector4( reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() ));
			}
		}

		var comp = gameObject.Components.GetOrCreate<Bytecode>();
		comp.BytecodeArray = bytecode;
		comp.Constants = constants;
		stream.Dispose();
		reader.Dispose();
	}

	public static float CalculateAttenuation( Color color, float range, string light )
	{
		//1.0f - saturate(distance/range)
		// ((1.0f / (1.0f + 0.1f * range + 0.01f * range * range)) * color.Luminance) * 1000f

		//float magnitude = color.r * color.r + color.g * color.g + color.b * color.b;
		Vector3 vector3 = new Vector3( color.r, color.g, color.b );
		Log.Info( $"{light}: Col {vector3.ToString()} Range {range} Mag {vector3.Length} Mag Sgr {vector3.LengthSquared} Lum {color.Luminance}" );

		float attenuation = ((1.0f / (1.0f + 0.1f * range + 0.01f * range * range)) * vector3.LengthSquared) * 1000f;
		return attenuation;
	}

	static double EstimateLightIntensity( double distance )
	{
		const double Pi = Math.PI;
		double intensity = (Pi * distance) / 1000;
		return intensity * _lightIntensityMultiplier;
	}
}
