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
	#include "common/classes/_classes.hlsl"
	
	Texture2D g_tBlitDebug < Attribute( "RadialBlur12" ); SrgbRead( false ); >;
    Texture2D g_tColorBuffer < Attribute( "ColorBuffer" ); SrgbRead( true ); >;
    SamplerState s1_s < Filter(MIN_MAG_MIP_POINT); AddressU(CLAMP); AddressV(CLAMP); AddressW(CLAMP); ComparisonFunc(NEVER); MaxAniso(1); >;

	int NearTextureIndex < Attribute( "TestIndex" ); >;

    float4 MainPs( PixelInput i ) : SV_Target0
    {

		float4 v0 = i.vPositionSs;
		float2 screenUV = float4(g_vViewportSize, g_vInvViewportSize).zw * v0.xy;
		float4 o0,r0,r1,r2;
		float4 DepthConstants = float4(1.0f / g_flFarPlane, (g_flFarPlane - g_flNearPlane) / (g_flFarPlane * g_flNearPlane),0,0);

		
		r0.xy = float4(g_vViewportSize, g_vInvViewportSize).zw * v0.xy;

		float4 test = Bindless::GetTexture2DMS(NearTextureIndex)[v0.xy];
		float4 test2 = Depth::GetNormalized(v0.xy);
		test2.x = test2.x * DepthConstants.y*39.37008f + DepthConstants.x*39.37f;
		test2.x = 1 / test2.x;
		
		float4 test3 = g_tBlitDebug.Sample(s1_s, float3(r0.xy, 0));
		//float4 color = g_tColorBuffer.Sample(s1_s, test3.xy);

		//r0.xyz = pow(color.xyz, 1.25);
		//r1.xyz = r0.xyz * float3(1.04874694,1.04874694,1.04874694) + float3(3.13439703,3.13439703,3.13439703);
		//r1.xyz = r1.xyz * r0.xyz;
		//r2.xyz = r0.xyz * float3(0.990440011,0.990440011,0.990440011) + float3(3.24044991,3.24044991,3.24044991);
		//r0.xyz = r0.xyz * r2.xyz + float3(0.651790023,0.651790023,0.651790023);
		//o0.xyz = saturate(r1.xyz / r0.xyz);
		//o0.w = 1;
		float testt = normalize(1-Depth::Get(r0.xy * float2(-0.5,-0.5) + float2(0.5,0.5)) * 50000);
		return float4(test3.xyz, 1);
    }
}