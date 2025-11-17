using Sandbox.Rendering;

namespace Sandbox;

[Title( "Destiny Lens Flare" )]
[Category( "Rendering" )]
[Icon( "sun" )]
public sealed class DestinyLensFlare : BasePostProcess, BasePostProcess.ExecuteInEditor
{
	private CommandList commandList = new CommandList();
	private GpuBuffer<Vertex> vertexBuffer;

	[Property]
	public List<Material> LensFlareMaterials { get; set; }

	protected override void OnStart()
	{
		base.OnStart();

		CreateBuffers();
	}

	protected override void OnEnabled()
	{
	}

	public override void Render()
	{
		if ( vertexBuffer is null )
			CreateBuffers();

		commandList.Reset();

		foreach ( var material in LensFlareMaterials )
		{
			if ( material is null )
				continue;
			Log.Info( $"Drawing lens flare with material: {material.Name}" );
			commandList.Draw( vertexBuffer, material );
		}

		InsertCommandList( commandList, Rendering.Stage.BeforePostProcess, 0, "Destiny Lens Flare" );
	}

	private void CreateBuffers()
	{
		// indices
		List<Vertex> vertices =
		[
			new Vertex( new Vector3( 0, 0.5f, 0 ) ) { TexCoord0 = new Vector2(1f, 1f)  }, // 0
			new Vertex( new Vector3( 0, -0.5f, 0 ) ) { TexCoord0 = new Vector2(1f, 0f)  }, // 1
			new Vertex( new Vector3( -0.5f, -0.5f, 0 ) ) { TexCoord0 = new Vector2(0f, 0f)  }, // 2
			new Vertex( new Vector3( -0.5f, -0.5f, 0 ) ) { TexCoord0 = new Vector2(0f, 0f)  }, // 3
			new Vertex( new Vector3( -0.5f, 0.5f, 0 ) ) { TexCoord0 = new Vector2(0f, 1f)  }, // 4
			new Vertex( new Vector3( 0, 0.5f, 0 ) ) { TexCoord0 = new Vector2(1f, 1f)  }, // 5
			new Vertex( new Vector3( 0, -0.5f, 0 ) ) { TexCoord0 = new Vector2(1f, 0f)  }, // 6
			new Vertex( new Vector3( 0, 0.5f, 0 ) ) { TexCoord0 = new Vector2(1f, 1f)  }, // 7
			new Vertex( new Vector3( 0.5f, -0.5f, 0 ) ) { TexCoord0 = new Vector2(0f, 0f)  }, // 8
			new Vertex( new Vector3( 0.5f, 0.5f, 0 ) ) { TexCoord0 = new Vector2(0f, 1f)  }, // 9
			new Vertex( new Vector3( 0.5f, -0.5f, 0 ) ) { TexCoord0 = new Vector2(0f, 0f)  }, // 10
			new Vertex( new Vector3( 0, 0.5f, 0 ) ) { TexCoord0 = new Vector2(1f, 1f)  }, // 11
		];

		int vertexCount = vertices.Count;
		vertexBuffer = new GpuBuffer<Vertex>( vertexCount, GpuBuffer.UsageFlags.Vertex );
		vertexBuffer.SetData( vertices );
	}

	protected override void OnPreRender()
	{
		base.OnPreRender();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected override void OnDisabled()
	{
		base.OnDisabled();
	}
}
