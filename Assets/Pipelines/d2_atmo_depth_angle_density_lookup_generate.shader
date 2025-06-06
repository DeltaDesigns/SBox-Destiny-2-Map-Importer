MODES
{
    VrForward();
}

FEATURES
{
}
COMMON
{
    #include "postprocess/shared.hlsl"
}

struct VertexInput
{
    float3 vPositionOs : POSITION < Semantic( PosXyz ); >;
    float2 vTexCoord : TEXCOORD0 < Semantic( LowPrecisionUv ); >;
};

struct PixelInput
{
    float2 vTexCoord : TEXCOORD0;

	#if ( PROGRAM == VFX_PROGRAM_VS )
		float4 vPositionPs		: SV_Position;
	#endif

	#if ( ( PROGRAM == VFX_PROGRAM_PS ) )
		float4 vPositionSs		: SV_Position;
	#endif
};

VS
{
    PixelInput MainVs( VertexInput i )
    {
        PixelInput o;
        
        o.vPositionPs = float4( i.vPositionOs.xy, 0.0f, 1.0f );
        o.vTexCoord = i.vTexCoord;
        return o;
    }
}
PS
{
	#include "postprocess/common.hlsl"
	#define CUSTOM_TEXTURE_FILTERING
    #define cmp -
	
	RenderState( DepthWriteEnable, false );
    RenderState( DepthEnable, false );

    Texture2D g_t0 < Attribute( "AtmosDensityLookup" ); SrgbRead(true); >;
	SamplerState s1_s < Filter(MIN_MAG_LINEAR_MIP_POINT); AddressU(WRAP); AddressV(CLAMP); AddressW(WRAP); ComparisonFunc(NEVER); MaxAniso(1); >;

    float4 MainPs( PixelInput i ) : SV_Target0
    {
		float4 o0,r0,r1,r2;
		
		float4 cb0[45] =
		{
			float4(0.00, 0.00, 0.00, 0.00),       
			float4(0.00, 0.00, 0.00, 0.00),       
			float4(0.00, 0.00, 0.00, 0.00),       
			float4(0.00, 0.00, 0.00, 0.00),       
			float4(0.00, 0.00, 0.00, 0.00),       
			float4(0.00, 0.00, 0.00, 0.00),       
			float4(0.00, 0.00, 0.00, 0.00),       
			float4(0.00, 0.00, 0.00, 0.00),       
			float4(0.00, 0.00, 0.00, 0.00),       
			float4(0.00, 0.00, 0.00, 0.00),       
			float4(0.00, 0.00, 0.00, 0.00),       
			float4(0.00, 0.00, 0.00, 0.00),       
			float4(0.00, 0.00, 0.00, 0.00),       
			float4(0.00, 0.00, 0.00, 0.00),       
			float4(0.00, 0.00, 0.00, 0.00),       
			float4(0.00, 0.00, 0.00, 0.00),       
			float4(0.00, 0.00, 0.00, 0.00),       
			float4(0.00, 0.00, 0.00, 0.00),       
			float4(0.00, 0.00, 0.00, 0.00),       
			float4(0.00, 0.00, 0.00, 0.00),       
			float4(0.00, 0.00, 0.00, 0.00),       
			float4(0.00, 0.00, 0.00, 0.00),       
			float4(0.00, 0.00, 0.00, 0.00),       
			float4(0.00, 0.00, 0.00, 0.00),       
			float4(0.00, 0.00, 0.00, 0.00),       
			float4(0.00, 0.00, 0.00, 0.00),       
			float4(0.00, 0.00, 0.00, 0.00),       
			float4(0.00, 0.00, 0.00, 0.00),       
			float4(0.00, 0.00, 0.00, 0.00),       
			float4(0.00, 0.00, 0.00, 0.00),       
			float4(0.00, 0.00, 0.00, 0.00),       
			float4(0.00, 0.00, 0.00, 0.00),       
			float4(0.00, 0.00, 0.00, 0.00),       
			float4(512.00, 512.00, 0.00195, 0.00195),
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(0.00, 0.00, 0.00, 0.00),         
			float4(1.50751, 0.00, 0.00, 0.00 ),     
			float4(2.88539, 0.00014, 0.00, 0.00  ), 
			float4(3.01502, 0.00, 0.00, 0.00  ),    
			float4(0.00, 0.00002, 0.4973, -0.00054 ),
			float4(0.0999, 0.0999, 0.0999, 0.30),
		};
		
		float4 v2 = i.vPositionSs; 
		
		r0.xy = float2(-0.5,-0.5) + v2.xy;
		r0.xy = cb0[33].zw * r0.xy;
		r0.y = r0.y * 2 + -1;
		r0.x = r0.x * r0.x;
		r0.xz = float2(50000,50.0000038) * r0.xx;
		r0.w = r0.y * r0.z;
		r1.xy = cb0[40].xy / r0.yy;
		r1.zw = cb0[41].xy * r0.ww;
		r0.y = cmp(abs(r0.w) < 9.99999975e-006);
		r1.zw = max(float2(-120,-120), r1.zw);
		r1.zw = exp2(-r1.zw);
		r1.zw = float2(1,1) + -r1.zw;
		r1.xy = r1.xy * r1.zw;
		r0.zw = cb0[42].xy * r0.zz;
		r1.zw = cb0[43].xy * r0.xx + cb0[43].zw;
		r2.xyz = g_t0.Sample(s1_s, r1.zw).xyz;
		r0.xy = r0.yy ? r0.zw : r1.xy;
		r0.x = r0.x + r0.y;
		r0.y = dot(r2.xyz, float3(0.300000012,0.589999974,0.109999999));
		r1.xyz = r2.xyz * r0.xxx;
		r0.x = r0.y * r0.x;
		o0.w = exp2(-r0.x);
		r0.xyz = cb0[44].xyz * -r1.xyz;
		o0.xyz = exp2(r0.xyz);
		return o0;
    }
}