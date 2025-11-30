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
	
	float4 GlobalChannel43 < Attribute("GlobalChannel43"); Default4(0, 0, 0, 0); >;
	float4 GlobalChannel40 < Attribute("GlobalChannel40"); Default4(0, 0, 0, 0); >;
}

struct VertexInput
{
	uint v0 : SV_VertexID < Semantic( VertexID ); >;
    float3 vPositionOs : POSITION < Semantic( PosXyz ); >;
    float2 vTexCoord : TEXCOORD0 < Semantic( LowPrecisionUv ); >;
};

struct PixelInput
{
    float4 o0 : TEXCOORD0;
	float4 o1 : TEXCOORD1; // Custom semantic to pass screen position
    float4 o2 : SV_Position;
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
			float4(g_vCameraDirWs,1),
			float4(vCameraPos,1),
			float4(g_vViewportSize, g_vInvViewportSize),
			float4(0,0,0,0)
		};
		
		float4 cb0[4] = {
			g_matProjectionToView
		};
		
		r0.x = (uint)i.v0.x;
		r0.xy = r0.xx * float2(-0.25,0.5) + float2(-0.125,0.25);
		r0.xy = frac(r0.xy);
		r0.xy = cmp(r0.xy >= float2(0.5,0.5));
		r0.xy = r0.xy ? float2(1,1) : 0;
		r0.xy = r0.xy * float2(2,2) + float2(-1,-1);
		r1.xyzw = cb0[1].xyzw * r0.yyyy;
		r1.xyzw = cb0[0].xyzw * r0.xxxx + r1.xyzw;
		o.o2.xy = r0.xy;
		r0.xyzw = cb0[2].xyzw * cb12[9].xxxx + r1.xyzw;
		r0.xyzw = cb0[3].xyzw + r0.xyzw;
		o.o0.xyzw = r0.xyzw;
		r1.xyzw = cb12[5].xyzw * r0.yyyy;
		r1.xyzw = cb12[4].xyzw * r0.xxxx + r1.xyzw;
		r1.xyzw = cb12[6].xyzw * r0.zzzz + r1.xyzw;
		o.o1.xyzw = cb12[7].xyzw * r0.wwww + r1.xyzw;
		o.o2.z = cb12[9].x;
		o.o2.w = 1;
		
        //o.o1 = float4( i.vPositionOs.xy, 1.0f, 1.0f );
        //o.o2 = i.vPositionOs.xyzz; // Compute screen space position
        //o.uv = i.vTexCoord;
        return o;
    }
}

PS
{
    RenderState( DepthWriteEnable, false );
    RenderState( DepthEnable, false );
	
	#include "postprocess/common.hlsl"
	#include "common/classes/_classes.hlsl"
	#define CUSTOM_TEXTURE_FILTERING
    #define cmp -
	
    Texture3D g_t0 < Attribute( "AtmosTexture0" ); SrgbRead(true); >;
	Texture3D g_t1 < Attribute( "AtmosTexture1" ); SrgbRead(true); >;
	Texture2D g_t2 < Attribute( "RadialBlur12" ); SrgbRead(true); >;
	Texture2D g_t3 < Attribute( "AtmosHemisphereBlur" ); SrgbRead(true); >;
    
	SamplerState s1_s < Filter(MIN_MAG_LINEAR_MIP_POINT); AddressU(CLAMP); AddressV(CLAMP); AddressW(CLAMP); ComparisonFunc(NEVER); MaxAniso(1); >;
	SamplerState s2_s < Filter(MIN_MAG_LINEAR_MIP_POINT); AddressU(WRAP); AddressV(CLAMP); AddressW(WRAP); ComparisonFunc(NEVER); MaxAniso(1); >;

	float AtmosRotation < Attribute( "AtmosRotation" ); Default1( 0 ); >;
	float AtmosIntensity < Attribute( "AtmosIntensity" ); Default1( 1 ); >;
	float AtmosUnk1BC < Attribute( "AtmosUnk1BC" ); Default1( 0.5 ); >;
	float AtmosTimeOfDay < Attribute( "AtmosTimeOfDay" ); Default1( 0.5 ); >;
	float4 AtmosSunDir < Attribute( "AtmosSunDir" ); Default4( -0.30372, -0.59835, 0.74144, 0.0 ); >;
	float4 AtmosRTDimensions < Attribute( "AtmosRTDimensions" ); Default4( 480.0, 270.0, 0.00208, 0.0037 ); >;
	float4 AtmosUnk1D0 < Attribute( "AtmosUnk1D0" ); Default4( 0,0,0,0 ); >;
	float AtmosUnk1E0 < Attribute( "AtmosUnk1E0" ); Default1( 0 ); >;
	float AtmosSunIntensity < Attribute( "AtmosSunIntensity" ); Default1( 0.05923 ); >;
	float AtmosUnk1E8 < Attribute( "AtmosUnk1E8" ); Default1( 0 ); >;
	float ExposureScale < Attribute( "ExposureScale" ); Default1( 0.65 ); >;
	float ExposureIllumRelative < Attribute( "ExposureIllumRelative" ); Default1( 1 ); >;

	float4 cb0_50 < Default4(0.92537f, 0.0f, 0.37906f, 0.37906f ); Attribute( "AtmosSunDirRight"); >;
	float4 cb0_51 < Default4(-0.22681f, 0.80123f, 0.5537f, 0.5537f ); Attribute( "AtmosSunDirUp"); >;
	float4 cb0_52 < Default4(-0.30372, -0.59835, 0.74144, 0.74144 ); Attribute( "AtmosSunDir"); >;

    float4 MainPs( PixelInput i ) : SV_Target0
    {
		float4 v1 = i.o1;
		float4 v2 = i.o2;
		float4 o0,r0,r1,r2,r3;
		
		float4 cb0[53] =
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
		float4(0.00, 0.00, 0.00, 0.00 ),                    
		float4(0.00, 0.00, 0.00, 0.00),                     
		float4(0.825, 0.825, 0.825, 0.825 ),                
		float4(0.8162, 0.8162, 0.8162, 0.8162  ),           
		float4(0.10, 0.10, 0.10, 0.10 ),                    
		float4(0.00, 0.00, 0.00, 0.00 ),                    
		float4(0.42879, 0.42879, 0.42879, 0.42879 ),        
		float4(0.00, 0.00, 0.00, 0.00  ),                   
		float4(0.00, 0.00, 0.00, 0.00   ),                  
		float4(0.4489, 0.71961, 0.52976, 0.00  ),           
		float4(0.00, 0.00, 0.00, 0.00 ),                    
		float4(0.00, 0.00, 0.00, 0.00 ),                    
		float4(480.00, 270.00, 0.00208, 0.0037 ),           
		float4(0.00, 0.00, 0.00, 0.00),                     
		float4(0.00, 0.00, 0.00, 0.00 ),                    
		float4(-9.83477E-07, 1.00583E-06, -2.23517E-08, 0.00),
		float4(0.00, 0.00, 0.00, 0.00),                      
		float4(-0.85, -0.85, -0.85, -0.85 ),                 
		float4(1.52893, 1.52893, 1.52893, 1.52893),          
		float4(0.20, 0.20, 0.20, 0.20 ),    
		float4(0.00, 0.00, 0.00, 0.00 ),       
		float4(0.00, 0.00, 0.00, 0.00 ),          
		float4(0.00, 0.00, 0.00, 0.00 ),             
		float4(0.00, 0.00, 0.00, 0.00 ),                
		float4(0.00, 0.00, 0.00, 0.00 ),                   
		float4(0.00, 0.00, 0.00, 0.00 ),                      
		float4(0.00, 0.00, 0.00, 0.00 ),                      
		float4(0.00, 0.00, 0.00, 0.00 ),                      
		float4(0.00, 0.00, 0.00, 0.00 ),                      
		float4(0.00, 0.00, 0.00, 0.00       ),                
		float4(0.76289, 0.00, -0.64653, -0.64653  ),         
		float4(-0.46525, 0.69437, -0.54899, -0.54899 ),       
		float4(0.44893, 0.71962, 0.52973, 0.52973),      
		};
		cb0[22] = float4(AtmosRotation.xxxx);
		cb0[23] = float4(AtmosIntensity.xxxx);
		cb0[24] = float4(AtmosUnk1BC.xxxx); // Global Channel 36 : 05E3F279 ? (dont think)
		cb0[26] = float4(AtmosTimeOfDay.xxxx);
		cb0[29] = AtmosSunDir;
		cb0[32] = AtmosRTDimensions;
		cb0[35] = AtmosUnk1D0;
		cb0[37] = float4(AtmosUnk1E0.xxxx);
		cb0[38] = float4(AtmosSunIntensity.xxxx);
		cb0[39] = float4(AtmosUnk1E8.xxxx);
		cb0[47] = GlobalChannel43;
		cb0[48] = GlobalChannel40;
		cb0[50] = cb0_50;
		cb0[51] = cb0_51;
		cb0[52] = cb0_52;
		
		r0.x = dot(v1.xyz, v1.xyz);
		r0.x = rsqrt(r0.x);
		r0.xyz = v1.xyz * r0.xxx;
		r0.w = abs(r0.x) + -abs(r0.y);
		r1.x = abs(r0.x) + abs(r0.y);
		r0.w = r0.w / r1.x;
		r1.x = cmp(9.99999975e-006 < r1.x);
		r0.w = r0.w * 0.125 + 0.125;
		r1.yz = cmp(r0.xy >= float2(0,0));
		r1.yz = r1.yz ? float2(1,1) : float2(-1,-1);
		r0.w = r0.w * r1.y + 0.25;
		r0.w = r0.w * r1.z;
		r0.w = frac(r0.w);
		r0.w = 1 + -r0.w;
		r0.w = r1.x ? r0.w : 0.5;
		r0.w = cb0[22].x + r0.w;
		r1.x = -r0.w;
		r1.z = cb0[26].x;
		r1.y = r0.z * -0.5 + 0.5;
		r2.xyzw = g_t1.Sample(s2_s, r1.xyz).xyzw;
		r1.xyzw = g_t0.Sample(s2_s, r1.xyz).xyzw;
		r2.xyzw = r2.xyzw + -r1.xyzw;
		r1.xyzw = cb0[47].xxxx * r2.xyzw + r1.xyzw;
		r1.xyzw = cb0[23].xxxx * r1.xyzw;
		r2.xy = float2(-0.5,-0.5) + v2.xy;
		r2.zw = cb0[32].zw * float2(0.5,0.5);
		r2.xy = r2.xy * cb0[32].zw + r2.zw;
		r0.w = g_t2.Sample(s1_s, r2.xy).x;
		r0.w = -1 + r0.w;
		r2.x = dot(r0.xyz, -cb0[29].xyz);
		r2.y = r2.x * -0.5 + 0.5;
		r2.x = -r2.x * 1.99899995 + cb0[37].x;
		r2.x = cb0[37].x * r2.x + 1;
		r2.x = log2(r2.x);
		r2.x = -1.5 * r2.x;
		r2.x = exp2(r2.x);
		r1.w = r2.x * r1.w;
		r1.w = cb0[38].x * r1.w;
		r1.w = min(512, r1.w);
		r2.x = log2(r2.y);
		r2.x = cb0[48].x * r2.x;
		r2.x = exp2(r2.x);
		r2.y = saturate(cb0[24].x * r2.x);
		r2.x = saturate(cb0[39].x * r2.x);
		r0.w = r2.y * r0.w + 1;
		r1.xyz = r1.xyz * r0.www;
		r0.w = dot(cb0[52].xyz, r0.xyz);
		r3.z = 1 + r0.w;
		r3.x = dot(cb0[50].xyz, r0.xyz);
		r3.y = dot(cb0[51].xyz, r0.xyz);
		r0.x = dot(r3.xyz, r3.xyz);
		r0.x = rsqrt(r0.x);
		r0.xy = r3.xy * r0.xx;
		r0.xy = r0.xy * float2(0.5,0.5) + float2(0.5,0.5);
		r0.x = g_t3.Sample(s1_s, r0.xy).x;
		r0.y = -1 + r0.x;
		o0.w = r1.w * r0.x;
		r0.x = r2.x * r0.y + 1;
		r0.y = cmp(0.000000 == cb0[38].x);
		r0.x = r0.y ? 1 : r0.x;
		r0.yzw = r1.xyz * r0.xxx;
		r1.xyz = -r1.xyz * r0.xxx + cb0[35].xyz;
		o0.xyz = cb0[35].www * r1.xyz + r0.yzw;

		return o0;
    }
}