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
	#define CUSTOM_TEXTURE_FILTERING

	RenderState( DepthWriteEnable, false );
    RenderState( DepthEnable, false );
	
	SamplerState s1_s < Filter(MIN_MAG_MIP_POINT); AddressU(CLAMP); AddressV(CLAMP); AddressW(CLAMP); ComparisonFunc(NEVER); MaxAniso(1); >;

	float4 rt < Default4(240.00, 135.00, 0.00417, 0.00741); Attribute( "RTDimensions"); >;
	float4 rt2 < Default4(240.00, 135.00, 0.00417, 0.00741); Attribute( "RTDimensions2"); >;

	float shaftDistance < Default1(500000); Attribute("LightShaftDistance");>;

    float4 MainPs( PixelInput i ) : SV_Target0
    {
		float2 v2 = i.vPositionSs;
		float4 o0,r0,r1;
		float4 Unk57 = float4( 100, 0, -2.23092, -2.23092);


		r0.xy = float4(g_vViewportSize, g_vInvViewportSize).xy * i.vTexCoord.xy;
		r1.x = saturate(Depth::GetLinear(r0.xy) - shaftDistance);
		r1.x = min(1, r1.x);
		r1.x = max(0, r1.x);
		o0.xyz = float3(r1.x,0,0);
		return o0;
		
		// game method, practically no difference
		r0.xy = rt.zw * v2.xy;
		r0.xy = g_tDepthChain.Sample(s1_s, r0.xy).xy;
		r0.xy = saturate(r0.xy * Unk57.xx + Unk57.yy);
		r0.x = r0.x + r0.y;
		r1.xw = rt2.zw;
		r1.yz = float2(0,0);
		r1.xyzw = v2.xyxy * rt.zwzw + r1.xyzw;
		r0.yz = g_tDepthChain.Sample(s1_s, r1.xy).xy;
		r1.xy = g_tDepthChain.Sample(s1_s, r1.zw).xy;
		r1.xy = saturate(r1.xy * Unk57.xx + Unk57.yy);
		r0.w = r1.x + r1.y;
		r0.yz = saturate(r0.yz * Unk57.xx + Unk57.yy);
		r0.y = r0.y + r0.z;
		r0.x = r0.x + r0.y;
		r0.yz = v2.xy * rt.zw + rt2.zw;
		r0.yz = g_tDepthChain.Sample(s1_s, r0.yz).xy;
		r0.yz = saturate(r0.yz * Unk57.xx + Unk57.yy);
		r0.y = r0.y + r0.z;
		r0.x = r0.x + r0.y;
		r0.x = r0.x + r0.w;
		o0.xyz = float3(0.125,0.125,0.125) * r0.xxx;
		o0.w = 1;

		return 1-o0;
    }
}