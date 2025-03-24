using Sandbox;
using System;

public static class Vector2Extensions
{
	/// <summary>
	/// Gets a boundary proximity factor for a Vector2, where:
	/// 0.0 = center of [0,1] space (not near any boundary)
	/// 1.0 = at or beyond any boundary edge
	/// </summary>
	/// <param name="position">The position to check</param>
	/// <returns>Boundary proximity factor (0.0-1.0)</returns>
	public static float GetBoundaryProximityFactor( this Vector2 position )
	{
		// Calculate distance from center (0.5, 0.5)
		float centerX = 0.5f;
		float centerY = 0.5f;

		// Normalize positions to -0.5 to 0.5 range (distance from center)
		float normalizedX = MathF.Abs( position.x - centerX );
		float normalizedY = MathF.Abs( position.y - centerY );

		// Find the maximum normalized coordinate (closest boundary)
		float maxDistance = MathF.Max( normalizedX, normalizedY );

		// Convert to 0-1 range where 0 = center, 1 = boundary
		float proximityFactor = maxDistance * 2f; // *2 because max distance from center is 0.5
		proximityFactor = MathX.Clamp( proximityFactor, 0, 1 );

		return proximityFactor;
	}
}
