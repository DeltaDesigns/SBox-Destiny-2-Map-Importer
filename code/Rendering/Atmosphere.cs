using System;

namespace Sandbox;

[Title( "Destiny Atmosphere" )]
[Category( "Rendering" )]
[Icon( "cloud" )]
public sealed class DestinyAtmosphere : Component, Component.ExecuteInEditor
{
	[Property] public Texture Texture0 { get; set; }
	[Property] public Texture Texture1 { get; set; }
	[Property] public Texture Texture2 { get; set; }
	[Property] public Shader GenerateSkyNear { get; set; }
	[Property] public Shader GenerateSkyFar { get; set; }

	[Property, Range( -1, 1 )]
	public float TimeOfDay { get; set; } = 0.5f;
	[Property, Range( 0, 1 )]
	public float Intensity { get; set; } = 0.75f;
	[Property, Range( 0, 1 )]
	public float Rotation { get; set; } = 0f;

	private Texture Texture0_3D;
	private Texture Texture1_3D;
	private Material SkyNear;
	private Material SkyFar;
	private Vertex[] screenQuad;

	IDisposable renderHook;
	RenderAttributes attributes = new RenderAttributes();

	protected override void OnStart()
	{
		//if ( Game.ActiveScene.Camera == null ) return;
		SkyNear = Material.FromShader( GenerateSkyNear != null ? GenerateSkyNear : Shader.Load( "Shaders/d2_sky_lookup_generate_near.shader" ) );
		SkyFar = Material.FromShader( GenerateSkyFar != null ? GenerateSkyFar : Shader.Load( "Shaders/d2_sky_lookup_generate_far.shader" ) );

		if ( Texture0_3D is null && Texture0 is not null )
			Create3DTexture( Texture0, out Texture0_3D );
		if ( Texture1_3D is null && Texture1 is not null )
			Create3DTexture( Texture1, out Texture1_3D );

		attributes.Set( "AtmosTexture0", Texture0_3D );
		attributes.Set( "AtmosTexture1", Texture1_3D );
		attributes.Set( "AtmosTexture2", Texture2 );
		attributes.Set( "AtmosTexture3", Texture.Transparent );

		//GenerateSkyNear = Shader.Load( $"Shaders/d2_sky_lookup_generate_near.shader" );
		if ( renderHook is null )
			renderHook = Game.ActiveScene.Camera.AddHookAfterTransparent( "Destiny Atmosphere", -1000, RenderAtmosphere );
	}

	protected override void OnValidate()
	{
		base.OnValidate();
		Scene.RenderAttributes.Set( "AtmosTimeOfDay", new Vector4( TimeOfDay ) );
		Scene.RenderAttributes.Set( "AtmosIntensity", new Vector4( Intensity ) );
		Scene.RenderAttributes.Set( "AtmosRotation", new Vector4( Rotation ) );
	}

	protected override void OnDisabled()
	{
		renderHook?.Dispose();
		renderHook = null;
	}

	protected override void OnDestroy()
	{
		renderHook?.Dispose();
		renderHook = null;
	}

	public void RenderAtmosphere( SceneCamera camera )
	{
		//if ( Game.ActiveScene.Camera == null ) return;
		using var rt = RenderTarget.GetTemporary( 1, ImageFormat.RGBA8888_LINEAR );

		if ( screenQuad == null )
		{
			Vector3 v1 = new Vector3( -0.5f, 0.5f, 0f );   // Top-left
			Vector3 v2 = new Vector3( 0.5f, 0.5f, 0f );    // Top-right
			Vector3 v3 = new Vector3( -0.5f, -0.5f, 0f );  // Bottom-left
			Vector3 v4 = new Vector3( 0.5f, -0.5f, 0f );   // Bottom-right

			screenQuad = new Vertex[6];
			screenQuad[0].Position = v1;
			screenQuad[1].Position = v3;
			screenQuad[2].Position = v2;
			screenQuad[3].Position = v2;
			screenQuad[4].Position = v3;
			screenQuad[5].Position = v4;
		}

		// Far
		Graphics.RenderTarget = rt;
		Graphics.Clear();
		Graphics.Draw( screenQuad.AsSpan(), 6, SkyFar, attributes, Graphics.PrimitiveType.TriangleStrip );
		Scene.RenderAttributes.Set( "AtmosFar", rt.ColorTarget );
		Graphics.RenderTarget = null;

		// Near
		Graphics.RenderTarget = rt;
		Graphics.Clear();
		Graphics.Draw( screenQuad.AsSpan(), 6, SkyNear, attributes, Graphics.PrimitiveType.TriangleStrip );

		Scene.RenderAttributes.Set( "AtmosNear", rt.ColorTarget );
		Graphics.RenderTarget = null;
	}

	public void Create3DTexture( in Texture tex, out Texture outTex )
	{
		//Log.Info( $"{tex.Width}x{tex.Height}x{tex.Width / tex.Height}" );
		int sliceWidth = tex.Height;
		int sliceHeight = tex.Height;
		int depth = tex.Width / tex.Height; // The number of slices

		// Create the 3D texture
		Texture texture3D = Texture.CreateVolume( sliceWidth, sliceHeight, depth )
			.WithName( $"{tex.ResourceName}_3D" )
			.WithFormat( ImageFormat.RGBA8888 )
			.WithStaticUsage()
			.WithMips( 0 )
			.Finish();

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
		texture3D.Update3D( byteArray );
		outTex = texture3D;
		//texture3D.Dispose();
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
