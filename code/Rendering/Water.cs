using Sandbox.Rendering;

namespace Sandbox;

[Title( "Destiny Water" )]
[Category( "Rendering" )]
[Icon( "water" )]
public sealed class DestinyWater : Renderer, Renderer.ExecuteInEditor
{
	public Material Water => Material.FromShader( Shader.Load( "Pipelines/d2_water_uv_generate.shader" ) );

	private CommandList commandList = new CommandList();
	private DestinyWaterRenderer WaterRenderer;
	protected override void OnStart()
	{
		if ( WaterRenderer is null )
			WaterRenderer = new( Scene, this );

		commandList = new( "WaterApply" );
		Game.ActiveScene.Camera.AddCommandList( commandList, Stage.AfterOpaque, int.MaxValue - 1 );
	}


	protected override void OnDisabled()
	{
		commandList?.Reset();
		Game.ActiveScene.Camera.RemoveCommandList( commandList );
		commandList = null;
		WaterRenderer?.Delete();
	}

	protected override void OnDestroy()
	{
		commandList?.Reset();
		commandList?.Reset();
		Game.ActiveScene.Camera.RemoveCommandList( commandList );
		commandList = null;
		WaterRenderer?.Delete();
	}
}
