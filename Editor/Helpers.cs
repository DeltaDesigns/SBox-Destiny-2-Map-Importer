using System;
using System.Numerics;

public partial class DestinyImporter : EditorTool
{
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

	//[Menu("Hammer", "D2 Map Importer/Help", "info")]
	//private static void OpenHelp()
	//{
	//	Process.Start(new ProcessStartInfo { FileName = "https://github.com/DeltaDesigns/SBox-Destiny-2-Map-Importer", UseShellExecute = true });
	//}

	public static string GetModelPath( ImportType type, string model )
	{
		switch ( type )
		{
			case ImportType.Static:
				return $"models/Statics/{model}.vmdl";
			case ImportType.Terrain:
				return $"models/Terrain/{model}.vmdl";
			case ImportType.Sky:
				return $"models/SkyEntities/{model}.vmdl";
			case ImportType.Water:
			case ImportType.Entity:
				return $"models/Entities/{model}.vmdl";
			case ImportType.Decorator:
				return $"models/Decorators/{model}.vmdl";
			default:
				return $"models/{model}.vmdl";
		}
	}

	public static bool IsValidModel( Model mdl )
	{
		return mdl == null ||
			mdl.IsError ||
			mdl.MeshCount == 0 ||
			!Editor.FileSystem.Content.FileExists( $"{mdl.ResourcePath.Split( ".vmdl" )[0]}.fbx" );
	}

	public enum ImportType
	{
		Static,
		Entity,
		Sky,
		Water,
		Terrain,
		Decorator
	}
}
