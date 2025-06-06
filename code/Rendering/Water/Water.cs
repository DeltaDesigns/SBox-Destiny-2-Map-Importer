namespace Sandbox;

[Title( "Destiny Water" )]
[Category( "Rendering" )]
[Icon( "water" )]
public sealed class DestinyWater : Renderer, Renderer.ExecuteInEditor
{
	public Material WaterUVHealing => Material.FromShader( Shader.Load( "Pipelines/d2_water_reflection_uv_healing.shader" ) );
	public Material WaterReflectionResolve => Material.FromShader( Shader.Load( "Pipelines/d2_water_reflection_resolve.shader" ) );
	public Material WaterReflectionHealing => Material.FromShader( Shader.Load( "Pipelines/d2_water_reflection_healing.shader" ) );

	//private CommandList commandList = new CommandList();
	private DestinyWaterRenderer WaterRenderer;
	protected override void OnStart()
	{
		if ( WaterRenderer is null )
			WaterRenderer = new( Scene, this );

		//commandList = new( "WaterApply" );
		//Game.ActiveScene.Camera.AddCommandList( commandList, Stage.AfterOpaque, int.MaxValue - 1 );
	}


	protected override void OnDisabled()
	{
		//commandList?.Reset();
		//Game.ActiveScene.Camera.RemoveCommandList( commandList );
		//commandList = null;
		WaterRenderer?.Delete();
	}

	protected override void OnDestroy()
	{
		//if ( commandList is not null )
		//{
		//	commandList.Reset();
		//	Game.ActiveScene.Camera?.RemoveCommandList( commandList );
		//	commandList = null;
		//}
		WaterRenderer?.Delete();
	}
}
