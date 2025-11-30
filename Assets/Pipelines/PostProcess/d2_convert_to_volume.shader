MODES
{
	Default();
}

CS
{
	#include "system.fxc"

	Texture2D InputTexture < Attribute( "2D_In" ); >;
	RWTexture3D<float4> OutputTexture < Attribute( "3D_Out" ); >;

	[numthreads( 8, 8, 1 )]
	void MainCs( uint3 vThreadID : SV_DispatchThreadID )
	{
		// DTid.x in [0..127], DTid.y in [0..127], DTid.z in [0..15]
		// Safety guard in case of odd dispatch counts
		if (vThreadID.x >= 128 || vThreadID.y >= 128 || vThreadID.z >= 16)
			return;

		uint u = vThreadID.x;      // x inside slice (0..127)
		uint v = vThreadID.y;      // y inside slice (0..127)
		uint z = vThreadID.z;      // slice index (0..15)

		uint srcX = z * 128 + u; // flatten slice into the 2D atlas X
		uint srcY = v;

		// integer load (x, y, mipLevel)
		float4 color = InputTexture.Load(int3(srcX, srcY, 0));

		OutputTexture[uint3(u, v, z)] = color;
	}	
}