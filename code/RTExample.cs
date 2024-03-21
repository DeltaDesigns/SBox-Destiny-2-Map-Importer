public sealed class RTExample : Component
{
	/// <summary>
	/// The Camera that will be shown
	/// </summary>
	[Property] public CameraComponent Camera { get; set; }

	public ModelRenderer model;
	private Texture tex { get; set; }

	protected override void OnStart()
	{
		if ( Camera == null )
			Camera = Game.ActiveScene.Camera;

		model = Components.Get<ModelRenderer>();

		// Sets up the texture that the camera will render to
		tex = Texture.CreateRenderTarget()
			.WithSize( Camera.ScreenRect.Width.CeilToInt(), Camera.ScreenRect.Height.CeilToInt() )
			.Create();
	}

	// Setting the texture in fixed updates since it doesnt eat as much fps as OnPreRender() would
	protected override void OnFixedUpdate()
	{
		Camera.RenderToTexture( tex );
		model.SceneObject.Attributes.Set( "RenderTargetExample", tex );
	}
}
