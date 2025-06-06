namespace Sandbox;

[Title( "Destiny Decal" )]
[Category( "Rendering" )]
[Icon( "patch" )]
public sealed class DestinyDecal : Renderer, Renderer.ExecuteInEditor
{
	[Property]
	public Material Material { get; set; }
	[Property]
	public ModelRenderer MdlRenderer { get; set; }
	private Model DecalCube => CreateCube();

	[Property]
	public Vector3 DecalPosition { get; set; }
	[Property]
	public Rotation DecalRotation { get; set; }
	[Property]
	public Vector3 DecalScale { get; set; }

	private DecalSceneObject decalSceneObject;
	protected override void OnEnabled()
	{
		//WorldPosition = DecalPosition;
		//WorldRotation = DecalRotation;
		//WorldScale = DecalScale;

		//decalSceneObject = new( Scene.SceneWorld, DecalCube, new()
		//{
		//	Position = DecalPosition,
		//	Rotation = DecalRotation,
		//	Scale = DecalScale
		//} );

		MdlRenderer = Components.GetOrCreate<ModelRenderer>();
		MdlRenderer.Model = DecalCube;
		MdlRenderer.MaterialOverride = Material;

		var angle = WorldRotation * new Rotation( 0, 0, 0, -1 );
		MdlRenderer.SceneObject.Batchable = false;
		MdlRenderer.SceneObject.Attributes.Set( "ObjectPosition", WorldPosition );
		MdlRenderer.SceneObject.Attributes.Set( "ObjectRotation", new Vector4( angle.x, angle.y, angle.z, angle.w ) );
		MdlRenderer.SceneObject.Attributes.Set( "ObjectScale", WorldScale );
	}

	RenderAttributes attributes = new RenderAttributes();
	protected override void OnPreRender()
	{
		base.OnPreRender();
		var angle = WorldRotation * new Rotation( 0, 0, 0, -1 );
		MdlRenderer.SceneObject.Batchable = false;
		MdlRenderer.SceneObject.Attributes.Set( "ObjectPosition", WorldPosition );
		MdlRenderer.SceneObject.Attributes.Set( "ObjectRotation", new Vector4( angle.x, angle.y, angle.z, angle.w ) );
		MdlRenderer.SceneObject.Attributes.Set( "ObjectScale", WorldScale );
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		decalSceneObject?.Delete();
		decalSceneObject = null;
	}

	protected override void OnDisabled()
	{
		base.OnDisabled();
		decalSceneObject?.Delete();
		decalSceneObject = null;
	}

	private Model CreateCube()
	{
		ModelBuilder modelBuilder = new ModelBuilder();
		Mesh mesh = new Mesh( Material );

		VertexBuffer vertexBuffer = new VertexBuffer();
		vertexBuffer.Init( false );
		vertexBuffer.AddTriangle(
			new Vertex( (new Vector3( -0.5f, -0.5f, -0.5f ) + new Vector3( 0, 0, 0 )) * 39.37f ),
			new Vertex( (new Vector3( -0.5f, -0.5f, -0.5f ) + new Vector3( 0, 0, 1 )) * 39.37f ),
			new Vertex( (new Vector3( -0.5f, -0.5f, -0.5f ) + new Vector3( 0, 1, 1 )) * 39.37f ) );

		vertexBuffer.AddTriangle(
			new Vertex( (new Vector3( -0.5f, -0.5f, -0.5f ) + new Vector3( 0, 0, 0 )) * 39.37f ),
			new Vertex( (new Vector3( -0.5f, -0.5f, -0.5f ) + new Vector3( 0, 1, 1 )) * 39.37f ),
			new Vertex( (new Vector3( -0.5f, -0.5f, -0.5f ) + new Vector3( 0, 1, 0 )) * 39.37f ) );

		vertexBuffer.AddTriangle(
			new Vertex( (new Vector3( -0.5f, -0.5f, -0.5f ) + new Vector3( 1, 0, 0 )) * 39.37f ),
			new Vertex( (new Vector3( -0.5f, -0.5f, -0.5f ) + new Vector3( 1, 1, 0 )) * 39.37f ),
			new Vertex( (new Vector3( -0.5f, -0.5f, -0.5f ) + new Vector3( 1, 1, 1 )) * 39.37f ) );

		vertexBuffer.AddTriangle(
			new Vertex( (new Vector3( -0.5f, -0.5f, -0.5f ) + new Vector3( 1, 0, 0 )) * 39.37f ),
			new Vertex( (new Vector3( -0.5f, -0.5f, -0.5f ) + new Vector3( 1, 1, 1 )) * 39.37f ),
			new Vertex( (new Vector3( -0.5f, -0.5f, -0.5f ) + new Vector3( 1, 0, 1 )) * 39.37f ) );

		vertexBuffer.AddTriangle(
			new Vertex( (new Vector3( -0.5f, -0.5f, -0.5f ) + new Vector3( 0, 0, 0 )) * 39.37f ),
			new Vertex( (new Vector3( -0.5f, -0.5f, -0.5f ) + new Vector3( 1, 0, 0 )) * 39.37f ),
			new Vertex( (new Vector3( -0.5f, -0.5f, -0.5f ) + new Vector3( 1, 0, 1 )) * 39.37f ) );

		vertexBuffer.AddTriangle(
			new Vertex( (new Vector3( -0.5f, -0.5f, -0.5f ) + new Vector3( 0, 0, 0 )) * 39.37f ),
			new Vertex( (new Vector3( -0.5f, -0.5f, -0.5f ) + new Vector3( 1, 0, 1 )) * 39.37f ),
			new Vertex( (new Vector3( -0.5f, -0.5f, -0.5f ) + new Vector3( 0, 0, 1 )) * 39.37f ) );

		vertexBuffer.AddTriangle(
			new Vertex( (new Vector3( -0.5f, -0.5f, -0.5f ) + new Vector3( 0, 1, 0 )) * 39.37f ),
			new Vertex( (new Vector3( -0.5f, -0.5f, -0.5f ) + new Vector3( 0, 1, 1 )) * 39.37f ),
			new Vertex( (new Vector3( -0.5f, -0.5f, -0.5f ) + new Vector3( 1, 1, 1 )) * 39.37f ) );

		vertexBuffer.AddTriangle(
			new Vertex( (new Vector3( -0.5f, -0.5f, -0.5f ) + new Vector3( 0, 1, 0 )) * 39.37f ),
			new Vertex( (new Vector3( -0.5f, -0.5f, -0.5f ) + new Vector3( 1, 1, 1 )) * 39.37f ),
			new Vertex( (new Vector3( -0.5f, -0.5f, -0.5f ) + new Vector3( 1, 1, 0 )) * 39.37f ) );

		vertexBuffer.AddTriangle(
			new Vertex( (new Vector3( -0.5f, -0.5f, -0.5f ) + new Vector3( 0, 0, 0 )) * 39.37f ),
			new Vertex( (new Vector3( -0.5f, -0.5f, -0.5f ) + new Vector3( 0, 1, 0 )) * 39.37f ),
			new Vertex( (new Vector3( -0.5f, -0.5f, -0.5f ) + new Vector3( 1, 1, 0 )) * 39.37f ) );

		vertexBuffer.AddTriangle(
			new Vertex( (new Vector3( -0.5f, -0.5f, -0.5f ) + new Vector3( 0, 0, 0 )) * 39.37f ),
			new Vertex( (new Vector3( -0.5f, -0.5f, -0.5f ) + new Vector3( 1, 1, 0 )) * 39.37f ),
			new Vertex( (new Vector3( -0.5f, -0.5f, -0.5f ) + new Vector3( 1, 0, 0 )) * 39.37f ) );

		vertexBuffer.AddTriangle(
			new Vertex( (new Vector3( -0.5f, -0.5f, -0.5f ) + new Vector3( 0, 0, 1 )) * 39.37f ),
			new Vertex( (new Vector3( -0.5f, -0.5f, -0.5f ) + new Vector3( 1, 0, 1 )) * 39.37f ),
			new Vertex( (new Vector3( -0.5f, -0.5f, -0.5f ) + new Vector3( 1, 1, 1 )) * 39.37f ) );

		vertexBuffer.AddTriangle(
			new Vertex( (new Vector3( -0.5f, -0.5f, -0.5f ) + new Vector3( 0, 0, 1 )) * 39.37f ),
			new Vertex( (new Vector3( -0.5f, -0.5f, -0.5f ) + new Vector3( 1, 1, 1 )) * 39.37f ),
			new Vertex( (new Vector3( -0.5f, -0.5f, -0.5f ) + new Vector3( 0, 1, 1 )) * 39.37f ) );

		mesh.PrimitiveType = MeshPrimitiveType.Triangles;
		mesh.CreateBuffers( vertexBuffer );
		modelBuilder.AddMesh( mesh );

		return modelBuilder.Create();
	}
}
