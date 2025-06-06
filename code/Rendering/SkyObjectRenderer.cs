using Sandbox.Rendering;

[Title( "Destiny Sky Objects" )]
[Category( "Rendering" )]
[Icon( "cloud" )]
public sealed class DestinySkyObjects : Renderer, Renderer.ExecuteInEditor
{
	//[Property, WideMode]
	//public Model Model { get; set; }

	//[Property, WideMode]
	//public int RenderOrder { get; set; }

	[Property, WideMode]
	public List<SkyObject> Instances { get; set; }

	private CommandList commands { get; set; }

	protected override void OnStart()
	{
		commands = new CommandList( "Destiny Sky Objects" );

		foreach ( var instance in Instances.OrderBy( x => x.Order ).ToList() )
		{
			commands.DrawModel( instance.Model, instance.Transform );
		}

		Game.ActiveScene.Camera?.AddCommandList( commands, Stage.AfterSkybox, 4 );
	}

	protected override void OnDisabled()
	{
		if ( commands is not null )
		{
			commands.Reset();
			Game.ActiveScene.Camera?.RemoveCommandList( commands );
			commands = null;
		}
	}

	protected override void OnDestroy()
	{
		if ( commands is not null )
		{
			commands.Reset();
			Game.ActiveScene.Camera?.RemoveCommandList( commands );
			commands = null;
		}
	}

	public struct SkyObject
	{
		public Model Model { get; set; }
		public Transform Transform { get; set; }
		public float Order { get; set; }

		public override string ToString()
		{
			return $"{Model.Name}";
		}
	}

}

