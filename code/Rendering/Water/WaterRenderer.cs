using System;
using System.Numerics;

public class DestinyWaterRenderer : SceneCustomObject
{
	private Scene scene;
	private DestinyWater Water;
	private Vector2 ScreenSize;

	public Dictionary<string, List<Transform>> Models;

	RenderAttributes RenderAttributes = new();
	private RenderAttributes GlobalAttributes => scene.RenderAttributes;

	public DestinyWaterRenderer( Scene world, DestinyWater water ) : base( world.SceneWorld )
	{
		scene = world;
		Water = water;
		Flags.WantsFrameBufferCopy = true;
		//Flags.IsTranslucent = true;

		Models = new();
		foreach ( var comps in scene.GetAllObjects( true ).Where( x => x.Name == "Water Decals" ) )
		{
			foreach ( var instances in comps.Children )
			{
				if ( !Models.ContainsKey( instances.Name ) )
					Models.TryAdd( instances.Name, new() );

				foreach ( var instance in instances.Children )
				{
					Models[instances.Name].Add( instance.WorldTransform );
				}
			}
		}

		//foreach ( (string model, List<Transform> transforms) in Models )
		//{
		//	Log.Info( $"{model}: {transforms.Count}" );
		//	foreach ( Transform t in transforms )
		//	{
		//		Log.Info( $"-{t.ToString()}" );
		//	}
		//}
	}

	public override void RenderSceneObject()
	{
		base.RenderSceneObject();

		//if ( Graphics.LayerType != SceneLayerType.Translucent )
		//	return;

		if ( ScreenSize != Graphics.Viewport.Size )
		{
			ScreenSize = Graphics.Viewport.Size;
			Vector2 RTsize = ScreenSize / 8;
			RenderAttributes.Set( "Water_Healing_UV_RT_Dimensions", new Vector4( RTsize.x, RTsize.y, 1 / RTsize.x, 1 / RTsize.y ) );
		}

		using var water_reflection = RenderTarget.GetTemporary( (int)(ScreenSize.x / 8), (int)(ScreenSize.y / 8), ImageFormat.RGBA16161616F );
		using var water_uv_healing = RenderTarget.GetTemporary( (int)(ScreenSize.x / 8), (int)(ScreenSize.y / 8), ImageFormat.RGBA16161616F );

		// water reflection mesh
		Graphics.RenderTarget = water_reflection;
		Graphics.Clear();
		foreach ( (string model, List<Transform> transforms) in Models )
		{
			Graphics.DrawModelInstanced( Model.Load( $"Models/WaterDecals/{model}_Reflection.vmdl" ), transforms.ToArray().AsSpan() );
		}
		GlobalAttributes.Set( "WaterReflectionUV", water_reflection.ColorTarget );

		//------------------------------------------

		// water_reflection_uv_healing
		Graphics.RenderTarget = water_uv_healing;
		Graphics.Blit( Water.WaterUVHealing, RenderAttributes );
		GlobalAttributes.Set( "WaterReflectionUVHealed", water_uv_healing.ColorTarget );
		Graphics.RenderTarget = null;

		water_reflection?.Dispose();
		water_uv_healing?.Dispose();

		//------------------------------------------

		// water_reflection_resolve
		using var water_resolve = RenderTarget.GetTemporary( (int)(ScreenSize.x / 4), (int)(ScreenSize.y / 4), ImageFormat.RGBA16161616F );
		using var water_healing = RenderTarget.GetTemporary( (int)(ScreenSize.x / 4), (int)(ScreenSize.y / 4), ImageFormat.RGBA16161616F );

		Graphics.GrabFrameTexture( "FrameBuffer", RenderAttributes );
		RenderAttributes.Set( "Framebuffer_RT_Dimensions", new Vector4( (ScreenSize.x), (ScreenSize.y), (1 / (ScreenSize.x)), (1 / (ScreenSize.y)) ) );
		RenderAttributes.Set( "Water_Resolve_RT_Dimensions", new Vector4( (ScreenSize.x / 4), (ScreenSize.y / 4), (1 / (ScreenSize.x / 4)), (1 / (ScreenSize.y / 4)) ) );

		Graphics.RenderTarget = water_resolve;
		Graphics.Blit( Water.WaterReflectionResolve, RenderAttributes );
		GlobalAttributes.Set( "WaterReflectionResolved", water_resolve.ColorTarget );
		Graphics.RenderTarget = null;

		//------------------------------------------

		// water_reflection_healing
		Graphics.RenderTarget = water_healing;
		Graphics.Blit( Water.WaterReflectionHealing, RenderAttributes );
		GlobalAttributes.Set( "WaterReflection", water_healing.ColorTarget );
		Graphics.RenderTarget = null;

		water_healing?.Dispose();
		water_resolve?.Dispose();
	}

	public Matrix4x4 ProjectionMatrix( Vector2 ScreenSize )
	{
		return Matrix4x4.CreatePerspective( ScreenSize.x, ScreenSize.y, scene.Camera.ZNear, scene.Camera.ZFar );
	}

	public Matrix4x4 ViewMatrix( Vector2 ScreenSize )
	{
		return Matrix4x4.CreateLookAt( scene.Camera.WorldPosition, scene.Camera.WorldPosition + scene.Camera.WorldRotation.Forward, Vector3.Up );
	}
}
