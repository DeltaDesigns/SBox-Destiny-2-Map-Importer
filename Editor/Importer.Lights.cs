using System;
using System.IO;
using System.Text.Json;


public partial class DestinyImporter : EditorTool
{
	private static void ImportLights( string path )
	{
		var staticMapRoot = scene.CreateObject();
		staticMapRoot.Name = "Lights";

		JsonDocument cfg = JsonDocument.Parse( File.ReadAllText( $"{path}/Rendering/Lights.json" ) );
		foreach ( var light in cfg.RootElement.EnumerateObject() )
		{
			int i = 0;
			var staticMapParent = scene.CreateObject();
			staticMapParent.Name = $"{light.Name}";
			staticMapParent.Parent = staticMapRoot;

			foreach ( var transforms in light.Value.GetProperty( "Instances" ).EnumerateArray() )
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

				// sizes for the different types
				Vector3 scale = new Vector3(
					transforms.GetProperty( "Scale" )[0].GetSingle(),
					transforms.GetProperty( "Scale" )[1].GetSingle(),
					transforms.GetProperty( "Scale" )[2].GetSingle() );

				var obj = scene.CreateObject();
				obj.Name = $"{light.Name}_{i}";
				obj.Parent = staticMapParent;

				obj.WorldPosition = position;
				obj.WorldRotation = quatRot.Angles(); //ToAngles(quatRot);

				Vector3 color = Vector3.One;

				var cfgColor = light.Value.GetProperty( "Color" );

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

				float range = scale.y; // Default, for point
				float attenuation = 1f; // CalculateAttenuation( color, range, obj.Name );

				string type = light.Value.GetProperty( "Type" ).GetString();
				var col = new Color( color.x, color.y, color.z ).Darken( 0.75f );

				switch ( type )
				{
					case "Line": // TODO: Use actual capsule when fully added
						var lineLight = obj.Components.GetOrCreate<PointLight>();
						lineLight.LightColor = col;
						lineLight.Radius = scale.z * 39.37f;
						lineLight.Shadows = false;
						lineLight.Attenuation = attenuation * 39.37f;
						lineLight.FogMode = Light.FogInfluence.Disabled;
						//lineLight.Size = new Vector2( scale.x, scale.y );
						break;

					case "Point":
						var pointLight = obj.Components.GetOrCreate<PointLight>();
						pointLight.LightColor = col;
						pointLight.Radius = range * 39.37f;
						pointLight.Shadows = false;
						pointLight.Attenuation = attenuation * 39.37f;
						pointLight.FogMode = Light.FogInfluence.Disabled;
						break;

					case "Shadowing":
					case "Spot":
						var fov = scale.x;
						var spotLight = obj.Components.GetOrCreate<SpotLight>();
						spotLight.LightColor = col;
						spotLight.Radius = scale.y * 39.37f;
						spotLight.Shadows = false;
						spotLight.Attenuation = attenuation;
						spotLight.ConeOuter = fov.RadianToDegree() / 2f; // sbox uses half-angles
						spotLight.ConeInner = (fov / 1.5f).RadianToDegree() / 2f;
						spotLight.FogMode = Light.FogInfluence.Disabled;

						if ( light.Value.GetProperty( "Cookie" ).GetString() != "" )
							spotLight.Cookie = Texture.Load( $"textures/{light.Value.GetProperty( "Cookie" ).GetString()}.vtex" );

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
						defaultLight.Radius = range * 39.37f;
						defaultLight.Shadows = false;
						defaultLight.Attenuation = attenuation;
						defaultLight.FogMode = Light.FogInfluence.Disabled;
						break;
				}
				i++;
			}
			if ( !_overrideLightColor )
			{
				var bytecode = light.Value.GetProperty( "Bytecode" ).EnumerateArray().Select( x => x.GetByte() ).ToArray();
				List<Vector4> constants = new();
				foreach ( var constant in light.Value.GetProperty( "BytecodeConstants" ).EnumerateArray() )
				{
					constants.Add( new Vector4( constant.GetProperty( "X" ).GetSingle(), constant.GetProperty( "Y" ).GetSingle(), constant.GetProperty( "Z" ).GetSingle(), constant.GetProperty( "W" ).GetSingle() ) );
				}

				TryGetBytecode( staticMapParent, bytecode, constants.ToArray() );
			}
		}

	}

	public static void TryGetBytecode( GameObject gameObject, byte[] bytecode, Vector4[] constants )
	{
		if ( bytecode.Length == 0 )
			return;

		var comp = gameObject.Components.GetOrCreate<LightBytecode>();
		comp.BytecodeArray = bytecode;
		comp.Constants = constants;
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
