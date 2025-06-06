namespace Sandbox;

[Title( "Destiny Color Grading" )]
[Category( "Post Processing" )]
[Icon( "grain" )]
public sealed class DestinyColorGrading : PostProcess, Component.ExecuteInEditor
{
	private Rendering.CommandList commands;

	private Material material => Material.FromShader( Shader.Load( "Pipelines/d2_color_grading.shader" ) );
	private Texture TempLUT;
	private Texture TempVignette => Texture.Load( $"Pipelines/Textures/vingette_temp.vtex" );

	[Property, MakeDirty]
	public float Brightness { get; set; } = 0.9968f;

	[Property, MakeDirty]
	public float ChromaticAberration { get; set; } = 0.05f;

	[Property, MakeDirty]
	public Vector2 Distortion { get; set; } = new( 0.02f, 0.50f );

	[Property, MakeDirty]
	public Vector4 Unknown { get; set; } = new( 0.03125f, -5.00f, 14.00f, 2.50f );

	protected override void OnEnabled()
	{
		commands = new( "Destiny Color Grading" );

		Helpers.Create3DTexture( Texture.Load( $"Pipelines/Textures/lut_temp.vtex" ), out TempLUT, ImageFormat.RGBA8888 );

		OnDirty();
		Camera.AddCommandList( commands, Rendering.Stage.AfterPostProcess );
	}

	protected override void OnPreRender()
	{
		base.OnPreRender();
	}


	protected override void OnDirty()
	{
		base.OnDirty();

		if ( commands is not null )
		{
			commands.Reset();
			SetCommands();
		}
	}

	public void SetCommands()
	{
		commands.Attributes.Set( "ColorGradingUnk2", Unknown );
		commands.Attributes.Set( "ColorGradingBrightness", Brightness );
		commands.Attributes.Set( "ColorGradingChromaticAberration", ChromaticAberration );
		commands.Attributes.Set( "ColorGradingDistortion", Distortion );

		commands.Attributes.Set( "Unk1", Helpers.CreateFilledTexture( Color32.Black ) );
		commands.Attributes.Set( "Unk2", Helpers.CreateFilledTexture( Color32.Black ) );
		commands.Attributes.Set( "Unk4", Helpers.CreateFilledTexture( Color32.FromRgba( 0x00000000 ) ) );

		commands.Attributes.Set( "Vignette", TempVignette );
		commands.Attributes.Set( "ColorLUT", TempLUT );
		commands.Attributes.GrabFrameTexture( "Framebuffer" );
		commands.Blit( material );
	}

	protected override void OnStart()
	{
		base.OnStart();

		//Game.ActiveScene.Camera.ZNear = 1;
		//Game.ActiveScene.Camera.ZFar = 50000000f;//float.PositiveInfinity;
	}

	protected override void OnDisabled()
	{
		Camera.RemoveCommandList( commands );
		commands = null;
	}
}
