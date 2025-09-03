using Sandbox.Rendering;
using System;

namespace Sandbox;

[Title( "Destiny Water" )]
[Category( "Rendering" )]
[Icon( "water" )]
public sealed class DestinyWater : Renderer, Renderer.ExecuteInEditor
{
	public Material WaterUVHealing => Material.FromShader( Shader.Load( "Pipelines/d2_water_reflection_uv_healing.shader" ) );
	public Material WaterReflectionResolve => Material.FromShader( Shader.Load( "Pipelines/d2_water_reflection_resolve.shader" ) );
	public Material WaterReflectionHealing => Material.FromShader( Shader.Load( "Pipelines/d2_water_reflection_healing.shader" ) );

	private Vector2 ScreenSize;

	private CommandList Commands = new CommandList();
	protected override void OnStart()
	{
		Commands = new( "D2WaterApply" );
		CreateWaterReflections();

		Game.ActiveScene.Camera.AddCommandList( Commands, Stage.AfterOpaque, int.MaxValue - 1 );
	}

	protected override void OnDirty()
	{
		if ( Commands is null )
			return;

		Commands.Reset();
		CreateWaterReflections();
	}

	private void CreateWaterReflections()
	{
		if ( Commands is null )
			return;

		if ( ScreenSize != Game.ActiveScene.Camera.ScreenRect.Size )
		{
			ScreenSize = Game.ActiveScene.Camera.ScreenRect.Size;
			Vector2 atmosRTsize = ScreenSize / 3;
			Commands.Attributes.Set( "Water_Healing_UV_RT_Dimensions", new Vector4( atmosRTsize.x, atmosRTsize.y, 1 / atmosRTsize.x, 1 / atmosRTsize.y ) );
		}

		var water_reflection = Commands.GetRenderTarget( "water_reflection", (int)(ScreenSize.x / 8), (int)(ScreenSize.y / 8), ImageFormat.RGBA16161616F );
		var water_uv_healing = Commands.GetRenderTarget( "water_uv_healing", (int)(ScreenSize.x / 8), (int)(ScreenSize.y / 8), ImageFormat.RGBA16161616F );

		// water reflection mesh
		Commands.SetRenderTarget( water_reflection );
		Commands.Clear( Color.Transparent );
		foreach ( var comps in Scene.GetAllObjects( true ).Where( x => x.Name == "Water Decals" ) )
		{
			foreach ( var instances in comps.Children )
			{
				Commands.DrawModelInstanced( Model.Load( $"Models/WaterDecals/{instances.Name}_Reflection.vmdl" ), instances.Children.Select( x => x.WorldTransform ).ToArray().AsSpan() );
			}
		}
		Commands.GlobalAttributes.Set( "WaterReflectionUV", water_reflection.ColorTexture );

		//------------------------------------------

		// water_reflection_uv_healing
		Commands.SetRenderTarget( water_uv_healing );
		Commands.Blit( WaterUVHealing );
		Commands.GlobalAttributes.Set( "WaterReflectionUVHealed", water_uv_healing.ColorTexture );
		Commands.ClearRenderTarget();

		Commands.ReleaseRenderTarget( water_reflection );
		Commands.ReleaseRenderTarget( water_uv_healing );

		//------------------------------------------

		// water_reflection_resolve
		var water_resolve = Commands.GetRenderTarget( "water_resolve", (int)(ScreenSize.x / 4), (int)(ScreenSize.y / 4), ImageFormat.RGBA16161616F );
		var water_healing = Commands.GetRenderTarget( "water_healing", (int)(ScreenSize.x / 4), (int)(ScreenSize.y / 4), ImageFormat.RGBA16161616F );

		Commands.Attributes.GrabFrameTexture( "FrameBuffer" );
		Commands.Attributes.Set( "Framebuffer_RT_Dimensions", new Vector4( (ScreenSize.x), (ScreenSize.y), (1 / (ScreenSize.x)), (1 / (ScreenSize.y)) ) );
		Commands.Attributes.Set( "Water_Resolve_RT_Dimensions", new Vector4( (ScreenSize.x / 4), (ScreenSize.y / 4), (1 / (ScreenSize.x / 4)), (1 / (ScreenSize.y / 4)) ) );

		Commands.SetRenderTarget( water_resolve );
		Commands.Blit( WaterReflectionResolve );
		Commands.GlobalAttributes.Set( "WaterReflectionResolved", water_resolve.ColorTexture );
		Commands.ClearRenderTarget();

		//------------------------------------------

		// water_reflection_healing
		Commands.SetRenderTarget( water_healing );
		Commands.Blit( WaterReflectionHealing );
		Commands.GlobalAttributes.Set( "WaterReflection", water_healing.ColorTexture );
		Commands.ClearRenderTarget();

		Commands.ReleaseRenderTarget( water_resolve );
		Commands.ReleaseRenderTarget( water_healing );
	}

	protected override void OnPreRender()
	{
		if ( ScreenSize != Game.ActiveScene.Camera.ScreenRect.Size )
		{
			ScreenSize = Game.ActiveScene.Camera.ScreenRect.Size;
			Vector2 atmosRTsize = ScreenSize / 3;
			Commands.Attributes.Set( "Water_Healing_UV_RT_Dimensions", new Vector4( atmosRTsize.x, atmosRTsize.y, 1 / atmosRTsize.x, 1 / atmosRTsize.y ) );
		}
	}

	protected override void OnDisabled()
	{
		if ( Commands is not null )
		{
			Commands.Reset();
			Game.ActiveScene.Camera?.RemoveCommandList( Commands );
			Commands = null;
		}
	}

	protected override void OnDestroy()
	{
		if ( Commands is not null )
		{
			Commands.Reset();
			Game.ActiveScene.Camera?.RemoveCommandList( Commands );
			Commands = null;
		}
	}
}
