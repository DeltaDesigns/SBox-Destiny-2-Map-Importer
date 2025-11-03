using System;

public static class Helpers
{
	private static Texture _solidBlackTexture;
	public static Texture SolidBlackTexture
	{
		get
		{
			if ( _solidBlackTexture == null )
			{
				_solidBlackTexture = CreateFilledTexture( Color.Black, 1, 1 );
			}
			return _solidBlackTexture;
		}
	}

	private static Texture _transparentTexture;
	public static Texture TransparentTexture
	{
		get
		{
			if ( _transparentTexture == null )
			{
				_transparentTexture = CreateFilledTexture( Color32.FromRgba( 0x00000000 ) );
			}
			return _transparentTexture;
		}
	}


	public static void Create3DTexture( in Texture tex, out Texture outTex, ImageFormat format = ImageFormat.RGBA8888 )
	{
		//Log.Info( $"{tex.Width}x{tex.Height}x{tex.Width / tex.Height}" );
		int sliceWidth = tex.Height;
		int sliceHeight = tex.Height;
		int depth = tex.Width / tex.Height; // The number of slices
											//Log.Warning( $"{sliceWidth}x{sliceWidth}x{depth}" );

		// Create the 3D texture
		Texture3DBuilder texture3D = Texture.CreateVolume( sliceWidth, sliceHeight, depth )
			.WithName( $"{tex.ResourceName}_3D" )
			.WithFormat( format )
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
					int pixel2DIndex = z * sliceWidth + y * sliceWidth * depth + x;
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

	public static Texture CreateTransparentTexture3D( int width, int height, int depth )
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

	public static Texture CreateFilledTexture( Color32 color, int width = 1, int height = 1 )
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

	public static Color32 ApplyDithering( Color32 color )
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

