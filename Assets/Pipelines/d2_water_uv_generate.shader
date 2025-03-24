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
    RenderState( DepthEnable, true );

    Texture2D g_t0 < Attribute( "AtmosFar" ); SrgbRead(true); >;
	float4 AtmosSunColor < Attribute( "AtmosSunColor" ); Default4(1.0f, 1.0f, 1.0f, 1.0f ); >;
	SamplerState s1_s < Filter(MIN_MAG_LINEAR_MIP_POINT); AddressU(CLAMP); AddressV(CLAMP); AddressW(CLAMP); ComparisonFunc(NEVER); MaxAniso(1); >;

	float4x4 TargetPixelToProjective(float2 size)
	{
		return float4x4(
			2.0f / size.x,  0.0f,          0.0f, 	0.0f,
			0.0f,          -2.0f / size.y, 0.0f, 	0.0f,
			0.0f,           0.0f,          1.0f,	0.0f,
			-1.0f,          1.0f,          0.0f, 	1.0f
		);
	}

    float4 MainPs( PixelInput i ) : SV_Target0
    {
		float3 vCameraPos = g_vCameraPositionWs/39.37;
		float4 cb12[15] = {
			transpose(g_matWorldToProjection),
			float4(cross(g_vCameraUpDirWs, -g_vCameraDirWs),0),
			float4(g_vCameraUpDirWs,0),
			float4(-g_vCameraDirWs,0),
			float4(vCameraPos,1),
			g_matProjectionToView * TargetPixelToProjective( g_vViewportSize ),
			float4(g_vViewportSize, g_vInvViewportSize),
			float4(1,0,0,0),
			float4(vCameraPos,1)
		};
		float4 o0,r0;
		float4 v0 = i.vPositionSs;
		float4 v1 = i.vPositionSs;
		float4 v2 = i.vPositionSs;//i.vPositionWithOffsetWs.xyz + g_vCameraPositionWs.xyz / 39.37f;
		float4 v3 = i.vPositionSs;
		
		r0.xy = float2(0.5,0.5) * v1.xy;
		r0.xy = r0.xy / v1.ww;
		r0.xy = float2(0.5,0.5) + r0.xy;
		r0.z = 1 + -r0.y;
		r0.xy = r0.xz + r0.xz;
		o0.xy = saturate(-v0.xy * cb12[12].zw + r0.xy);
		r0.x = -cb12[7].z + v3.z;
		r0.yzw = -cb12[7].xyz + v2.xyz;
		r0.x = r0.x / r0.w;
		r0.xyz = -r0.xxx * r0.yzw;
		r0.x = dot(r0.xyz, cb12[6].xyz);
		o0.z = 0.00200000009 * abs(r0.x);
		o0.w = 1;

		return o0;
    }
}