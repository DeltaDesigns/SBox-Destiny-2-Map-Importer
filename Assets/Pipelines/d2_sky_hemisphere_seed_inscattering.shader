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
	uint v0 : SV_VertexID < Semantic( VertexID ); >;
    float3 vPositionOs : POSITION < Semantic( PosXyz ); >;
    float2 vTexCoord : TEXCOORD0 < Semantic( LowPrecisionUv ); >;
};

struct PixelInput
{
    float4 o0 : SV_Position;
	float4 o1 : TEXCOORD0; // Custom semantic to pass screen position
};

VS
{
 #define cmp -
    PixelInput MainVs( VertexInput i )
    {
        PixelInput o;
        float4 r0,r1;
		
		float3 vCameraPos = g_vCameraPositionWs/39.37;
		float4 cb12[10] = {
			g_matWorldToProjection,
			float4(cross(g_vCameraUpDirWs, -g_vCameraDirWs),1),
			float4(g_vCameraUpDirWs,1),
			float4(-g_vCameraDirWs,1),
			float4(vCameraPos,1),
			float4(g_vViewportSize, g_vInvViewportSize),
			float4(1,0,0,0)
		};
		
		float4 cb0[4] = {
			g_matProjectionToView
		};
        //o.vPositionPs = float4( i.vPositionOs.xy, 0.0f, 1.0f );
        //o.vTexCoord = i.vTexCoord;
		
		r0.x = (uint)i.v0.x;
		//r0.xyz = r0.xxx * float3(0.25,0.5,0.5) + float3(0.125,0.25,0.25);
		r0.xyz = r0.xxx * float3(-0.25,0.5,0.5) + float3(-0.125,0.25,0.25);
		r0.xyz = frac(r0.xyz);
		r0.xyz = cmp(r0.xyz >= float3(0.5,0.5,0.5));
		r1.yzw = r0.xyz ? float3(1,1,-1) : float3(-1,-1,1);
		r1.x = r0.x ? 2.000000 : 0;
		r0.xyzw = float4(0,1,0,1) + r1.xwxw;
		o.o0.xy = r1.yz;
		o.o1.xyzw = float4(0.5,0.5,0.5,0.5) * r0.xyzw;
		o.o0.z = cb12[9].x;
		o.o0.w = 1;
			
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

	Texture2D g_t4 < Attribute( "AtmosHemisphere" ); SrgbRead(true); >;
	SamplerState s1_s < Filter(MIN_MAG_LINEAR_MIP_POINT); AddressU(CLAMP); AddressV(CLAMP); AddressW(CLAMP); ComparisonFunc(NEVER); MaxAniso(1); >;

	float4 cb0_26 < Default4(-0.30372, -0.59835f, 0.74144f, 0.0f ); Attribute( "AtmosSunDir"); >;
	float4 cb0_47 < Default4(0.92537f, 0.0f, 0.37906f, 0.37906f ); Attribute( "AtmosSunDirRight"); >;
	float4 cb0_48 < Default4(-0.22681f, 0.80123f, 0.5537f, 0.5537f ); Attribute( "AtmosSunDirUp"); >;
	float4 cb0_49 < Default4(-0.30372, -0.59835f, 0.74144f, 0.74144f ); Attribute( "AtmosSunDir"); >;

    float4 MainPs( PixelInput i ) : SV_Target0
    {
		float4 o0,r0,r1,r2,r3;

		float4 v1 = i.o1; 
		
		float4 cb0_46 = float4(0.25, 0.25, 0.25, 0.25); // Postprocess[192 (0xC0)]
		//float4 cb0_47 = float4(0.92537, 0.00, 0.37906, 0.37906); // Postprocess[208 (0xD0)]
		//float4 cb0_48 = float4(-0.22681, 0.80123, 0.5537, 0.5537); // Postprocess[224 (0xE0)]
		
		r0.xy = float2(-0.5,-0.5) + v1.xy;
		r0.xy = r0.xy + r0.xy;
		r0.x = -r0.x * r0.x + 1;
		r0.x = -r0.y * r0.y + r0.x;
		r0.x = max(0, r0.x);
		r0.z = sqrt(r0.x);
		r0.xy = v1.xy * float2(2,2) + float2(-1,-1);
		r0.w = dot(r0.xyz, r0.xyz);
		r0.w = rsqrt(r0.w);
		r0.xyw = r0.xyz * r0.www;
		r1.x = -2 * r0.w;
		r0.xyw = r0.xyw * -r1.xxx + float3(0,0,-1);
		r1.x = dot(cb0_47.xyz, cb0_26.xyz);
		r1.y = dot(cb0_48.xyz, cb0_26.xyz);
		r1.z = dot(cb0_49.xyz, cb0_26.xyz);
		r1.xyz = r1.xyz + -r0.xyw;
		r1.xyz = r1.xyz * cb0_46.xxx + r0.xyw;
		r1.w = dot(r1.xyz, r1.xyz);
		r1.w = rsqrt(r1.w);
		r1.xyz = r1.xyz * r1.www + -r0.xyw;
		r2.xy = float2(0,0.5);
		[loop] while (true) {
		r1.w = cmp(r2.y >= 8);
		if (r1.w != 0) break;
		r1.w = 0.125 * r2.y;
		r3.xyz = r1.www * r1.xyz + r0.xyw;
		r1.w = dot(r3.xyz, r3.xyz);
		r1.w = rsqrt(r1.w);
		r3.xy = r3.xy * r1.ww;
		r3.w = r3.z * r1.w + 1;
		r1.w = dot(r3.xyw, r3.xyw);
		r1.w = rsqrt(r1.w);
		r2.zw = r3.xy * r1.ww;
		r2.zw = r2.zw * float2(0.5,0.5) + float2(0.5,0.5);
		r1.w = g_t4.Sample(s1_s, r2.zw).w;
		r1.w = r2.x + -r1.w;
		r2.x = 1 + r1.w;
		r2.y = 1 + r2.y;
		}
		r0.x = 2.5 * r0.z;
		r0.x = min(1, r0.x);
		r0.y = r2.x * 0.125 + -1;
		o0.xyzw = r0.xxxx * r0.yyyy + float4(1,1,1,1);

		return float4(o0.xyz, o0.w);
    }
}