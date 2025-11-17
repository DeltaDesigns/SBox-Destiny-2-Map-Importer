namespace Sandbox;

[Title( "Destiny Color Grading" )]
[Category( "Post Processing" )]
[Icon( "grain" )]
public sealed class DestinyColorGrading : BasePostProcess<DestinyColorGrading>, Component.ExecuteInEditor
{
	private Rendering.CommandList commands;

	private Material material => Material.FromShader( Shader.Load( "Pipelines/d2_color_grading.shader" ) );
	private Texture TempLUT;
	private Texture TempVignette => Texture.Load( $"Pipelines/Textures/vingette_temp.vtex" );

	[Property, MakeDirty]
	public Texture LUT2D { get; set; }
	public Texture LUT3D { get; set; }

	[Property, MakeDirty]
	public float Brightness { get; set; } = 0.9968f;

	[Property, MakeDirty]
	public float ChromaticAberration { get; set; } = 0.05f;

	[Property, MakeDirty]
	public Vector2 Distortion { get; set; } = new( 0.02f, 0.50f );

	[Property, MakeDirty]
	public Vector4 Unknown { get; set; } = new( 0.03125f, -5.00f, 14.00f, 2.50f );

	private GlobalChannelsController _globalChannels;
	private GlobalChannelsController GlobalChannels
	{
		get
		{
			if ( _globalChannels == null )
				_globalChannels = GlobalChannelsController.Get();

			return _globalChannels;
		}
		set
		{
			_globalChannels = value;
		}
	}

	protected override void OnEnabled()
	{
		commands = new( "Destiny Color Grading" );

		//Helpers.Create3DTexture( Texture.Load( $"Pipelines/Textures/lut_temp.vtex" ), out TempLUT, ImageFormat.RGBA8888 );
	}

	protected override void OnStart()
	{
		base.OnStart();

		if ( GlobalChannels is not null )
			LUT2D = GlobalChannels.LUT;
	}

	protected override void OnPreRender()
	{
		base.OnPreRender();
	}

	private void ProcessLUT()
	{
		commands.Reset();
		commands.Attributes.Set( "LUT2D", LUT2D );

		var lut_2d_processed = commands.GetRenderTarget( "lut_2d_processed", 1024, 32, ImageFormat.RGBA16161616F );
		commands.SetRenderTarget( lut_2d_processed );
		commands.Clear( Color.Transparent );
		commands.Blit( Material.FromShader( Shader.Load( "Pipelines/d2_color_grading_fill_using_tint_map_plus_matrix_hdr.shader" ) ) );
		commands.Attributes.Set( "LUT2D_Processed", lut_2d_processed.ColorTexture );
		commands.ClearRenderTarget();
		commands.ReleaseRenderTarget( lut_2d_processed );

		if ( LUT3D is null )
			LUT3D = Texture.CreateVolume( 32, 32, 32 )
				.WithName( $"LUT3D" )
				.WithFormat( ImageFormat.RGBA1010102 )
				.WithUAVBinding()
				.WithMips( 0 )
				.Finish();

		var cs = new ComputeShader( "Pipelines/d2_color_grading_convert_to_volume_texture_hdr.shader" );
		commands.Attributes.Set( "LUT3D", LUT3D );
		commands.DispatchCompute( cs, 32, 32, 32 );

		InsertCommandList( commands, Rendering.Stage.AfterPostProcess, 0, "Color Grading Process LUT" );
	}

	protected override void OnDirty()
	{
		base.OnDirty();
	}


	public override void Render()
	{
		if ( LUT2D is not null )
		{
			ProcessLUT();
		}

		Attributes.Set( "ColorGradingUnk2", Unknown );
		Attributes.Set( "ColorGradingBrightness", Brightness );
		Attributes.Set( "ColorGradingChromaticAberration", ChromaticAberration );
		Attributes.Set( "ColorGradingDistortion", Distortion );

		Attributes.Set( "Unk1", Helpers.SolidBlackTexture );
		Attributes.Set( "Unk2", Helpers.SolidBlackTexture );
		Attributes.Set( "Unk4", Helpers.TransparentTexture );

		Attributes.Set( "Vignette", TempVignette );

		Attributes.Set( "ColorLUT", LUT3D ?? TempLUT );

		var blit = BlitMode.WithBackbuffer( material, Rendering.Stage.AfterPostProcess, 1, false );
		Blit( blit, "Color Grading" );
	}

	//protected override void OnDisabled()
	//{
	//	Camera.RemoveCommandList( commands );
	//	commands = null;
	//}
}
