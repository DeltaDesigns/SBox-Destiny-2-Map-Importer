using Sandbox.Rendering;
using System;

namespace Sandbox;

[Title( "Destiny Atmosphere" )]
[Category( "Rendering" )]
[Icon( "cloud" )]
public sealed class DestinyAtmosphere : Renderer, Renderer.ExecuteInEditor
{
	// -- Atmosphere properties ---

	[Property] public Texture Texture0 { get; set; }
	[Property] public Texture Texture1 { get; set; }
	[Property] public Texture Texture3 { get; set; } = Texture.Load( $"Pipelines/Textures/depth_angle_lookup_temp.vtex" );

	[Property, Range( -1, 1 ), MakeDirty, Category( "Atmosphere" )]
	public float TimeOfDay { get; set; } = 0.5f;
	[Property, Range( 0, 1 ), MakeDirty, Category( "Atmosphere" )]
	public float Intensity { get; set; } = 0.75f;
	[Property, Range( 0, 1 ), MakeDirty, Category( "Atmosphere" )]
	public float Rotation { get; set; } = 0f;

	//--- Sun properties ---

	[Property, MakeDirty, Category( "Sun" )]
	public bool AffectSceneSun { get; set; } = false;

	[Property, MakeDirty, Category( "Sun" ), HideIf( "AffectSceneSun", false )]
	public DirectionalLight SunComponent { get; set; }

	[Property, MakeDirty, Category( "Sun" )]
	public Color SunColor { get; set; } = Color.White;

	[Property, MakeDirty, Category( "Sun" ), Range( 0, 1 )]
	public float SunIntensity { get; set; } = 0.05923f;

	[Property, MakeDirty, Change( "OnSunAngleChanged" ), Category( "Sun" )]
	public Angles SunDirection { get; set; } = new Angles( 300f, 0f, 0f );

	private Vector3 _sunDirVector;
	private bool isUpdating = false; // Prevents infinite loop

	[Property, MakeDirty, Change( "OnSunDirChanged" ), ReadOnly, Category( "Sun" )]
	public Vector3 SunDirectionVector
	{
		get => _sunDirVector;
		set
		{
			_sunDirVector = value;
		}
	}

	//------------------------------

	private Texture Texture0_3D;
	private Texture Texture1_3D;

	public Material SkyHemisphereColor => Material.FromShader( Shader.Load( "Pipelines/d2_full_hemisphere_sky_color_generate.shader" ) );
	public Material SkyHemisphereScatter => Material.FromShader( Shader.Load( "Pipelines/d2_sky_hemisphere_seed_inscattering.shader" ) );
	public Material SkyHemisphereBlur => Material.FromShader( Shader.Load( "Pipelines/d2_sky_hemisphere_spherical_blur.shader" ) );
	public Material SkyNear => Material.FromShader( Shader.Load( "Pipelines/d2_sky_lookup_generate_near.shader" ) );
	public Material SkyFar => Material.FromShader( Shader.Load( "Pipelines/d2_sky_lookup_generate_far.shader" ) );
	public Material Sky => Material.FromShader( Shader.Load( "Pipelines/d2_sky.shader" ) );

	private CommandList commandList = new CommandList();
	private CommandList commandListStart = new CommandList();
	private DestinyAtmosphereRenderer AtmosphereRenderer;
	protected override void OnStart()
	{
		//if ( Game.ActiveScene.Camera == null ) return;

		//SkyNear = Material.FromShader( Shader.Load( "Pipelines/d2_sky_lookup_generate_near.shader" ) );
		//SkyFar = Material.FromShader( Shader.Load( "Pipelines/d2_sky_lookup_generate_far.shader" ) );
		//Sky = Material.FromShader( Shader.Load( "Pipelines/d2_sky.shader" ) );

		if ( Texture0_3D is null && Texture0 is not null )
			Create3DTexture( Texture0, out Texture0_3D );
		if ( Texture1_3D is null && Texture1 is not null )
			Create3DTexture( Texture1, out Texture1_3D );

		// Only need applied on start, I think?
		commandListStart = new( "AtmosphereApplyStart" );
		commandListStart.SetGlobal( "AtmosTexture0", Texture0_3D ?? CreateTransparentTexture3D( 1, 1, 6 ) );
		commandListStart.SetGlobal( "AtmosTexture1", Texture1_3D ?? CreateTransparentTexture3D( 1, 1, 6 ) );
		commandListStart.SetGlobal( "AtmosTexture2", CreateFilledTexture( new Color( 1, 0, 0 ) ) ); // TODO: Depth thing for fake god rays
		commandListStart.SetGlobal( "AtmosTexture3", CreateFilledTexture( new Color( 1, 1, 0 ) ) ); // TODO
		commandListStart.SetGlobal( "AtmosDensity", Texture3 ); // TODO

		commandList = new( "AtmosphereApply" );
		OnDirty();
		Game.ActiveScene.Camera.AddCommandList( commandListStart, Stage.AfterOpaque, int.MaxValue - 1 );
		Game.ActiveScene.Camera.AddCommandList( commandList, Stage.AfterOpaque, int.MaxValue );

		if ( AtmosphereRenderer is null )
			AtmosphereRenderer = new( Scene, this );
	}

	protected override void OnDirty()
	{
		base.OnDirty();

		if ( commandList is null )
			return;

		commandList.Reset();
		commandList.SetGlobal( "AtmosTimeOfDay", new Vector4( TimeOfDay ) );
		commandList.SetGlobal( "AtmosIntensity", new Vector4( Intensity ) );
		commandList.SetGlobal( "AtmosRotation", new Vector4( Rotation ) );
		commandList.SetGlobal( "AtmosSunColor", SunColor );
		commandList.SetGlobal( "AtmosSunIntensity", SunIntensity );
		commandList.SetGlobal( "AtmosSunDir", SunDirectionVector );

		if ( AffectSceneSun && SunComponent is not null )
		{
			SunComponent.WorldRotation = SunDirection + new Angles( 180, 0, 0 );
			SunComponent.LightColor = SunColor.Lighten( 1.25f ) * (SunIntensity * 10);
		}

		RenderAtmosphereNew();
	}

	public CommandList RenderAtmosphereNew()
	{
		var rt = commandList.GetRenderTarget( "SkyApplyRT", ImageFormat.RGBA1010102 );

		commandList.Blit( Sky );
		commandList.ReleaseRenderTarget( rt );

		return commandList;
	}

	private void OnSunDirChanged( Vector3 oldValue, Vector3 newValue )
	{
		if ( isUpdating ) return;
		isUpdating = true;

		SunDirection = newValue.EulerAngles; // Convert vector back to angles

		isUpdating = false;
	}

	private void OnSunAngleChanged( Angles oldValue, Angles newValue )
	{
		if ( isUpdating ) return;
		isUpdating = true;

		SunDirectionVector = Angles.AngleVector( newValue ); // Convert angles to vector

		isUpdating = false;
	}

	protected override void OnDisabled()
	{
		commandList?.Reset();
		Game.ActiveScene.Camera.RemoveCommandList( commandList );
		commandList = null;

		commandListStart?.Reset();
		Game.ActiveScene.Camera.RemoveCommandList( commandListStart );
		commandListStart = null;

		AtmosphereRenderer?.Delete();
		//AtmosphereRenderer = null;
	}

	protected override void OnDestroy()
	{
		commandList?.Reset();
		Game.ActiveScene.Camera.RemoveCommandList( commandList );
		commandList = null;

		commandListStart?.Reset();
		Game.ActiveScene.Camera.RemoveCommandList( commandListStart );
		commandListStart = null;

		AtmosphereRenderer?.Delete();
		//AtmosphereRenderer = null;
	}

	public void Create3DTexture( in Texture tex, out Texture outTex )
	{
		//Log.Info( $"{tex.Width}x{tex.Height}x{tex.Width / tex.Height}" );
		int sliceWidth = tex.Height;
		int sliceHeight = tex.Height;
		int depth = tex.Width / tex.Height; // The number of slices

		// Create the 3D texture
		Texture3DBuilder texture3D = Texture.CreateVolume( sliceWidth, sliceHeight, depth )
			.WithName( $"{tex.ResourceName}_3D" )
			.WithFormat( ImageFormat.RGBA8888 )
			.WithStaticUsage()
			.WithMips( 0 );

		// Get the pixel colors from the 2D texture
		Color32[] pixels2D = tex.GetPixels();

		// Create an array to store the colors for the 3D texture
		Color32[] pixels3D = new Color32[sliceWidth * sliceHeight * depth];

		// Populate the 3D texture's color array
		for ( int z = 0; z < depth; z++ )
		{
			for ( int y = 0; y < sliceHeight; y++ )
			{
				for ( int x = 0; x < sliceWidth; x++ )
				{
					int pixel2DIndex = z * sliceWidth + y * sliceWidth * 16 + x;
					int pixel3DIndex = x + (y * sliceWidth) + (z * sliceWidth * sliceHeight);

					pixels3D[pixel3DIndex] = pixels2D[pixel2DIndex];
				}
			}
		}

		int numColors = pixels3D.Length;
		byte[] byteArray = ConvertColorArrayToByteArray( pixels3D ); // 4 bytes per color (RGBA)

		// Apply the color data to the 3D texture
		texture3D.WithData( byteArray );
		outTex = texture3D.Finish();
		//texture3D.Dispose();
	}

	private Texture CreateTransparentTexture3D( int width, int height, int depth )
	{
		Texture3DBuilder transparentTexture = Texture.CreateVolume( width, height, depth )
			.WithName( $"TextureTransparent_3D" )
			.WithFormat( ImageFormat.RGBA8888 )
			.WithStaticUsage()
			.WithMips( 0 );

		Color32 transparentColor = new Color( 0, 0, 0, 0 ); // Fully transparent
		Color32[] transparentColors = new Color32[width * height * depth];

		// Fill the texture with transparent color
		for ( int i = 0; i < transparentColors.Length; i++ )
		{
			transparentColors[i] = transparentColor;
		}

		transparentTexture.WithData( ConvertColorArrayToByteArray( transparentColors ) );

		return transparentTexture.Finish();
	}

	private Texture CreateFilledTexture( Color32 color, int width = 1, int height = 1 )
	{
		Texture2DBuilder transparentTexture = Texture.Create( width, height )
			.WithFormat( ImageFormat.RGBA8888 )
			.WithStaticUsage()
			.WithMips( 0 );

		Color32[] transparentColors = new Color32[width * height];

		// Fill the texture with transparent color
		for ( int i = 0; i < transparentColors.Length; i++ )
		{
			transparentColors[i] = color;
		}

		transparentTexture.WithData( ConvertColorArrayToByteArray( transparentColors ) );

		return transparentTexture.Finish();
	}

	public static byte[] ConvertColorArrayToByteArray( Color32[] colors )
	{
		int bytePerPixel = 4; // RGBA8888 has 4 bytes per pixel
		byte[] byteArray = new byte[colors.Length * bytePerPixel];

		for ( int i = 0; i < colors.Length; i++ )
		{
			int byteIndex = i * bytePerPixel;

			// Apply dithering directly to the byte values
			//Color32 ditheredColor = ApplyDithering( colors[i] );

			// Store in byte array
			byteArray[byteIndex + 0] = colors[i].r;
			byteArray[byteIndex + 1] = colors[i].g;
			byteArray[byteIndex + 2] = colors[i].b;
			byteArray[byteIndex + 3] = colors[i].a;
		}

		return byteArray;
	}

	private static Color32 ApplyDithering( Color32 color )
	{
		// Simple dithering using random noise
		int ditherStrength = 0; // Strength of dithering (can be adjusted as needed)
		int rNoise = (Game.Random.Next( -ditherStrength, ditherStrength + 1 ));
		int gNoise = (Game.Random.Next( -ditherStrength, ditherStrength + 1 ));
		int bNoise = (Game.Random.Next( -ditherStrength, ditherStrength + 1 ));
		int aNoise = (Game.Random.Next( -ditherStrength, ditherStrength + 1 ));

		byte r = (byte)Math.Clamp( color.r + rNoise, 0, 255 );
		byte g = (byte)Math.Clamp( color.g + gNoise, 0, 255 );
		byte b = (byte)Math.Clamp( color.b + bNoise, 0, 255 );
		byte a = (byte)Math.Clamp( color.a + aNoise, 0, 255 );

		return new Color32( r, g, b, a );
	}
}
