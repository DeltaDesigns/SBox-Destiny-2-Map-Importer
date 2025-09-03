MODES
{
    Forward();
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
    float4 o0 : TEXCOORD0;
	float4 o1 : TEXCOORD1;
	
	#if ( PROGRAM == VFX_PROGRAM_VS )
		float4 vPositionPs		: SV_Position;
	#endif

	#if ( ( PROGRAM == VFX_PROGRAM_PS ) )
		float4 vPositionSs		: SV_Position;
	#endif
};

VS
{
	#define cmp -
    
    PixelInput MainVs( VertexInput i )
    {
        PixelInput o;
		float4 r0,r1;

		o.vPositionPs = float4( i.vPositionOs.xy, 0.0f, 1.0f );
		
		float3 vCameraPos = g_vCameraPositionWs/39.37;
		float4 cb12[10] = {
			g_matWorldToProjection,
			float4(cross(g_vCameraUpDirWs, -g_vCameraDirWs),1),
			float4(g_vCameraUpDirWs,1),
			float4(-g_vCameraDirWs,1),
			float4(vCameraPos,1),
			float4(g_vViewportSize, g_vInvViewportSize),
			float4(0,1,0,0)
		};
		
		r0.x = (uint)i.v0.x;
		r0.xyz = r0.xxx * float3(0.25,0.5,0.5) + float3(0.125,0.25,0.25);
		r0.xyz = frac(r0.xyz);
		r0.xyz = cmp(r0.xyz >= float3(0.5,0.5,0.5));
		r1.yzw = r0.xyz ? float3(1,1,-1) : float3(-1,-1,1);
		r1.x = r0.x ? 2 : 0;
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
	#define CUSTOM_TEXTURE_FILTERING
    #define cmp -
	#include "postprocess/common.hlsl" 
	#include "common/classes/_classes.hlsl"
	
    RenderState( DepthWriteEnable, false );
    RenderState( DepthEnable, false );
	
	SamplerState s1_s < Filter(MIN_MAG_LINEAR_MIP_POINT); AddressU(CLAMP); AddressV(CLAMP); AddressW(CLAMP); ComparisonFunc(NEVER); MaxAniso(1); >;
	Texture2D g_t0 < Attribute("LUT2D"); SrgbRead(true); >;

	float CurrentTime < Attribute( "CurrentTime" ); Default1( 0.0 ); >;
	float ExposureScale < Attribute( "ExposureScale" ); Default1( 0.65 ); >;
	float ExposureIllumRelative < Attribute( "ExposureIllumRelative" ); Default1( 1 ); >;

    float4 MainPs( PixelInput i ) : SV_Target0
    {
		float2 v0 = i.vPositionSs.xy;
		
		// is actually 241 count but is all 0 anyways
		float4 cb7[1] =
		{
			float4(0,0,0,0),
		};
		
		float4 cb0[24] =
		{
			float4(0.00, 0.00, 0.00, 0.00),
			float4(0.00, 0.00, 0.00, 0.00),
			float4(0.00, 0.00, 0.00, 0.00),
			float4(0.00, 0.00, 0.00, 0.00),
			float4(0.00, 0.00, 0.00, 0.00),
			float4(0.00, 0.00, 0.00, 0.00),
			float4(0.00, 0.00, 0.00, 0.00),
			float4(32.00, 1024.00, 0.00, 0.00),
			float4(0.03125, -5.00, 14.00, 2.50),
			float4(0.00, 0.00, 0.00, 0.00),
			float4(0.00, 0.00, 0.00, 0.00),
			float4(0.00, 0.00, 0.00, 0.00),
			float4(0.00, 0.00, 0.00, 0.00),
			float4(0.00, 0.00, 0.00, 0.00),
			float4(0.00, 0.00, 0.00, 0.00),
			float4(1.00, 1.00, 1.00, 1.00),
			float4(0.00, 0.00, 0.00, 0.00),
			float4(0.00, 0.00, 0.00, 0.00),
			float4(0.00, 0.00, 0.00, 0.00),
			float4(0.00, 0.00, 0.00, 0.00),
			float4(0.40, 0.40, 0.40, 0.40),
			float4(-1.00, -1.00, -1.00, -1.00),
			float4(-1.00, -1.00, -1.00, -1.00),
			float4(0.50, 0.50, 0.50, 0.50),
		};
		
		// Frame
		float4 cb13[8] =
		{ 
			float4(CurrentTime, CurrentTime, 0.05, 0.016),
			float4(ExposureScale, ExposureIllumRelative*16, ExposureScale, ExposureIllumRelative),
			float4((CurrentTime + 33.75) * 1.258699, (CurrentTime + 60.0) * 0.9583125, (CurrentTime + 60.0) * 8.789123, (CurrentTime + 33.75) * 2.311535),
			float4(0.5,0.5,0,0),
			float4(1,1,0,1),
			float4(0,0,512,0),
			float4(0,1,sin(CurrentTime * 6.0) * 0.5 + 0.5,0),
			float4(0,0.5,180,0),
		};
		
		const float4 icb[] = { { 0, -0.083333, 0.041667, 0},
								  { 0.041667, 0.013889, 0.097222, 0},
								  { 0.138889, 0.097222, 0.166667, 0},
								  { 0.305556, 0.166667, 0.472222, 0},
								  { 0.491667, 0.458333, 0.527778, 0},
								  { 0.616667, 0.513889, 0.708333, 0},
								  { 0.791667, 0.708333, 0.875000, 0},
								  { 0.908333, 0.875000, 0.944444, 0} };
								  
		float4 o0,r0,r1,r2,r3,r4,r5,r6,r7,r8,r9,r10,r11,r12,r13;
		uint4 bitmask, uiDest;
		float4 fDest;

		r0.xy = float2(-0.5,-0.5) + v0.xy;
		r0.z = 1 / cb0[7].x;
		r0.w = r0.x * r0.z;
		r0.w = frac(r0.w);
		r1.x = cb0[7].x + -1;
		r1.y = cb0[7].x / r1.x;
		r1.x = 1 / r1.x;
		r0.x = r0.x * r0.z + -r0.w;
		r2.xyz = r1.yxx * r0.wyx;
		r0.xyz = float3(1,1,1) + -r2.xyz;
		r0.w = 1 / cb0[8].w;
		r0.xyz = log2(r0.xyz);
		r0.xyz = r0.www * r0.xyz;
		r0.xyz = exp2(r0.xyz);
		r0.xyz = float3(1,1,1) + -r0.xyz;
		r0.xyz = r0.xyz * cb0[8].zzz + cb0[8].yyy;
		r0.xyz = exp2(r0.xyz);
		r0.xyz = -cb0[8].xxx + r0.xyz;
		r0.xyz = min(float3(512,512,512), r0.xyz);
		r1.x = dot(float3(0.613132417,0.339538008,0.0474166945), r0.xyz);
		r1.y = dot(float3(0.0701243803,0.916393995,0.0134515241), r0.xyz);
		r1.z = dot(float3(0.0205876566,0.109574571,0.869785428), r0.xyz);
		r0.xyz = float3(3.20000005,3.20000005,3.20000005) * r1.xyz;
		r2.x = dot(float3(0.970843613,0.0269652624,0.00210750522), r0.xyz);
		r2.y = dot(float3(0.0110121472,0.986875057,0.00214175484), r0.xyz);
		r2.z = dot(float3(0.0109303994,0.0269407779,0.962179005), r0.xyz);
		r0.xyz = float3(0.0245785993,0.0245785993,0.0245785993) + r2.xyz;
		r0.xyz = r2.xyz * r0.xyz + float3(-9.05370034e-005,-9.05370034e-005,-9.05370034e-005);
		r3.xyz = r2.xyz * float3(0.983729005,0.983729005,0.983729005) + float3(0.432951003,0.432951003,0.432951003);
		r2.xyz = r2.xyz * r3.xyz + float3(0.238080993,0.238080993,0.238080993);
		r0.xyz = r0.xyz / r2.xyz;
		r2.x = saturate(dot(float3(1.60475004,-0.531080008,-0.0736699998), r0.xyz));
		r2.y = saturate(dot(float3(-0.102080002,1.10812998,-0.00604999997), r0.xyz));
		r2.z = saturate(dot(float3(-0.00326999999,-0.0727600008,1.07602), r0.xyz));
		r2.w = 1;
		r0.x = saturate(dot(r2.xyzw, cb0[10].xyzw));
		r0.y = saturate(dot(r2.xyzw, cb0[11].xyzw));
		r0.z = saturate(dot(r2.xyzw, cb0[12].xyzw));
		r3.xy = float2(0.0302734375,31) * r2.xz;
		r0.w = floor(r3.y);
		r1.w = r0.w * 0.03125 + 0.03125;
		r1.w = min(0.96875, r1.w);
		r0.w = r0.w * 0.03125 + r3.x;
		r4.y = 0.00048828125 + r0.w;
		r4.z = r2.y * 0.96875 + 0.015625;
		r0.w = r2.x * 0.0302734375 + r1.w;
		r4.x = 0.00048828125 + r0.w;
		r0.w = frac(r3.y);
		r3.xyz = g_t0.Sample(s1_s, r4.yz).xyz;
		r4.xyz = g_t0.Sample(s1_s, r4.xz).xyz;
		r4.xyz = r4.xyz + -r3.xyz;
		r3.xyz = r0.www * r4.xyz + r3.xyz;
		r0.xyz = saturate(cb0[15].xxx * r3.xyz + r0.xyz);
		r3.x = dot(float3(0.643038273,0.311186761,0.0457754582), r0.xyz);
		r3.y = dot(float3(0.0592686906,0.931436479,0.00929491594), r0.xyz);
		r3.z = dot(float3(0.00596190104,0.0639290139,0.930118382), r0.xyz);
		r0.xyz = r3.xyz * float3(-0.432950944,-0.432950944,-0.432950944) + float3(0.0245785806,0.0245785806,0.0245785806);
		r4.xyz = r3.xyz * r3.xyz;
		r5.xyz = float3(0.938068271,0.938068271,0.938068271) * r3.xyz;
		r4.xyz = r4.xyz * float3(-0.749383032,-0.749383032,-0.749383032) + r5.xyz;
		r4.xyz = float3(0.000966254738,0.000966254738,0.000966254738) + r4.xyz;
		r4.xyz = sqrt(r4.xyz);
		r3.xyz = r3.xyz * float3(0.983729005,0.983729005,0.983729005) + float3(-1,-1,-1);
		r3.xyz = r3.xyz + r3.xyz;
		r0.xyz = -r4.xyz + r0.xyz;
		r0.xyz = r0.xyz / r3.xyz;
		r3.x = dot(float3(1.03037536,-0.0280939545,-0.00219434849), r0.xyz);
		r3.y = dot(float3(-0.0114728473,1.0136739,-0.0022312447), r0.xyz);
		r3.z = dot(float3(-0.0113838771,-0.0280634761,1.03939509), r0.xyz);
		r0.xyz = float3(0.3125,0.3125,0.3125) * r3.xyz;
		r0.w = cmp(0 != cb0[19].x);
		r3.xy = max(r1.xy, r1.yz);
		r3.xy = max(r3.xy, r1.zx);
		r1.w = max(r2.x, r2.y);
		r1.w = max(r1.w, r2.z);
		r1.w = max(9.99999975e-006, r1.w);
		r1.w = r3.x / r1.w;
		r1.w = max(1, r1.w);
		r2.x = -cb0[21].x + r3.x;
		r2.y = cb0[22].x + -cb0[21].x;
		r2.x = saturate(r2.x / r2.y);
		r2.x = 1 + -r2.x;
		r2.x = log2(r2.x);
		r2.x = cb0[23].x * r2.x;
		r2.x = exp2(r2.x);
		r2.x = 1 + -r2.x;
		r2.x = cb0[20].x * r2.x;
		r2.yzw = r0.xyz * r1.www + -r0.xyz;
		r2.xyz = r2.xxx * r2.yzw + r0.xyz;
		r0.xyz = r0.www ? r2.xyz : r0.xyz;
		r0.w = min(r1.x, r1.y);
		r0.w = min(r0.w, r1.z);
		r0.w = r3.x + -r0.w;
		r1.w = dot(r1.xyz, float3(0.300000012,0.589999974,0.109999999));
		r2.xyz = r1.xyz + -r1.www;
		r3.xzw = r1.xyz + -r1.yzx;
		r2.w = max(abs(r3.z), abs(r3.w));
		r2.w = max(abs(r3.x), r2.w);
		r2.w = cmp(r2.w < 0.0329999998);
		r3.x = cmp(r1.y < r1.z);
		r4.xy = r1.zy;
		r4.zw = float2(-1,0.666666687);
		r5.xy = r4.yx;
		r5.zw = float2(0,-0.333333343);
		r4.xyzw = r3.xxxx ? r4.xyzw : r5.xyzw;
		r3.x = cmp(r1.x < r4.x);
		r5.xyz = r4.xyw;
		r5.w = r1.x;
		r4.xyw = r5.wyx;
		r4.xyzw = r3.xxxx ? r5.xyzw : r4.xyzw;
		r3.x = min(r4.w, r4.y);
		r3.x = r4.x + -r3.x;
		r3.z = r4.w + -r4.y;
		r3.w = r3.x * 6 + 1.00000001e-007;
		r3.z = r3.z / r3.w;
		r3.z = r4.z + r3.z;
		r5.x = abs(r3.z);
		r3.z = 1.00000001e-007 + r4.x;
		r3.x = r3.x / r3.z;
		r6.z = 0;
		r5.y = 0;
		r4.yzw = float3(0,0,0);
		r3.z = 0;
		while (true) {
		r3.w = (int)r3.z;
		r5.z = cmp(r3.w >= cb7[0].x);
		if (r5.z != 0) break;
		r3.w = r3.w * 16 + 1;
		r3.w = (int)r3.w;
		r5.z = (int)cb7[0].x;
		bitmask.w = ((~(-1 << 28)) << 4) & 0xffffffff;  r5.w = (((uint)r3.z << 4) & bitmask.w) | ((uint)1 & ~bitmask.w);
		switch (r5.z) {
		  case 1 :      r8.xyz = (int3)r5.www + int3(1,3,5);
		  bitmask.x = ((~(-1 << 28)) << 4) & 0xffffffff;  r9.x = (((uint)r3.z << 4) & bitmask.x) | ((uint)3 & ~bitmask.x);
		  bitmask.y = ((~(-1 << 28)) << 4) & 0xffffffff;  r9.y = (((uint)r3.z << 4) & bitmask.y) | ((uint)5 & ~bitmask.y);
		  r5.z = max(cb7[0].z, 0);
		  r5.z = min(2, r5.z);
		  r5.z = -1 + r5.z;
		  r6.w = cmp(0 < r5.z);
		  r7.w = cmp(r5.z < 0);
		  r6.w = (int)-r6.w + (int)r7.w;
		  r6.w = (int)r6.w;
		  r6.w = -r0.w * r6.w + 1;
		  r5.z = r5.z * r6.w + 1;
		  r5.z = max(0, r5.z);
		  r10.xyz = r5.zzz * r2.xyz + r1.www;
		  r10.xyz = max(float3(0,0,0), r10.xyz);
		  r9.xzw = cb7[0].xyz * r10.yyy;
		  r9.xzw = r10.xxx * cb7[0].xyz + r9.xzw;
		  r8.xyw = r10.zzz * cb7[0].xyz + r9.xzw;
		  r8.xyw = cb7[0].xyz + r8.xyw;
		  r9.xyz = float3(4.76000023,4.76000023,4.76000023) + -r8.xyw;
		  r8.xyz = cb7[0].xxx * r9.xyz + r8.xyw;
		  r8.xyz = max(float3(0,0,0), r8.xyz);
		  r7.xyz = min(cb13[5].zzz, r8.xyz);
		  break;
		  case 2 :      r8.xy = (int2)r5.ww + int2(1,3);
		  r9.xyzw = max(cb7[0].yyww, float4(0,0,0,0));
		  r8.yzw = max(cb7[0].xyz, float3(0,0,0));
		  r8.yzw = min(float3(2,2,2), r8.yzw);
		  r9.xyzw = min(float4(0.959999979,0.959999979,0.959999979,0.959999979), r9.xyzw);
		  bitmask.z = ((~(-1 << 28)) << 4) & 0xffffffff;  r5.z = (((uint)r3.z << 4) & bitmask.z) | ((uint)3 & ~bitmask.z);
		  r6.w = max(cb7[0].y, 0);
		  r6.w = min(0.959999979, r6.w);
		  r8.yzw = float3(-1,-1,-1) + r8.yzw;
		  r10.xy = cb7[0].zw + -cb7[0].zw;
		  r10.zw = cb7[0].zw + cb7[0].zw;
		  r10.zw = r10.zw + -r10.xy;
		  r10.xy = -r10.xy + r3.yy;
		  r10.zw = float2(1,1) / r10.zw;
		  r10.xy = saturate(r10.xy * r10.zw);
		  r10.zw = r10.xy * float2(-2,-2) + float2(3,3);
		  r10.xy = r10.xy * r10.xy;
		  r11.xy = r10.zw * r10.xy;
		  r10.xy = -r10.zw * r10.xy + float2(1,1);
		  r7.w = r11.x * r10.y;
		  r10.yzw = cmp(float3(0,0,0) < r8.yzw);
		  r11.xzw = cmp(r8.yzw < float3(0,0,0));
		  r10.yzw = (int3)-r10.yzw + (int3)r11.xzw;
		  r10.yzw = (int3)r10.yzw;
		  r10.yzw = -r0.www * r10.yzw + float3(1,1,1);
		  r8.yzw = r8.yzw * r10.yzw + float3(1,1,1);
		  r8.yzw = max(float3(0,0,0), r8.yzw);
		  r10.yzw = r8.yyy * r2.xyz + r1.www;
		  r10.yzw = max(float3(0,0,0), r10.yzw);
		  r11.xzw = r8.zzz * r2.xyz + r1.www;
		  r11.xzw = max(float3(0,0,0), r11.xzw);
		  r8.yzw = r8.www * r2.xyz + r1.www;
		  r8.yzw = max(float3(0,0,0), r8.yzw);
		  r12.x = max(r10.z, r10.w);
		  r12.x = max(r12.x, r10.y);
		  r12.yzw = cb7[0].xxx + float3(1,0.666666687,0.333333343);
		  r12.yzw = frac(r12.yzw);
		  r12.yzw = r12.yzw * float3(6,6,6) + float3(-3,-3,-3);
		  r12.yzw = saturate(float3(-1,-1,-1) + abs(r12.yzw));
		  r12.yzw = float3(-1,-1,-1) + r12.yzw;
		  r12.yzw = r9.yyy * r12.yzw + float3(1,1,1);
		  r12.xyz = r12.xxx * r12.yzw;
		  r13.xyzw = float4(0.25,0.75,0.25,0.75) * r9.xyzw;
		  r9.xyz = -r13.xxx * r10.yzw + r10.yzw;
		  r9.xyz = r13.yyy * r12.xyz + r9.xyz;
		  r10.y = max(r11.z, r11.w);
		  r10.y = max(r11.x, r10.y);
		  r12.xyz = cb7[0].zzz + float3(1,0.666666687,0.333333343);
		  r12.xyz = frac(r12.xyz);
		  r12.xyz = r12.xyz * float3(6,6,6) + float3(-3,-3,-3);
		  r12.xyz = saturate(float3(-1,-1,-1) + abs(r12.xyz));
		  r12.xyz = float3(-1,-1,-1) + r12.xyz;
		  r12.xyz = r9.www * r12.xyz + float3(1,1,1);
		  r10.yzw = r12.xyz * r10.yyy;
		  r11.xzw = -r13.zzz * r11.xzw + r11.xzw;
		  r10.yzw = r13.www * r10.yzw + r11.xzw;
		  r8.x = max(r8.z, r8.w);
		  r8.x = max(r8.y, r8.x);
		  r11.xzw = cb7[0].xxx + float3(1,0.666666687,0.333333343);
		  r11.xzw = frac(r11.xzw);
		  r11.xzw = r11.xzw * float3(6,6,6) + float3(-3,-3,-3);
		  r11.xzw = saturate(float3(-1,-1,-1) + abs(r11.xzw));
		  r11.xzw = float3(-1,-1,-1) + r11.xzw;
		  r11.xzw = r6.www * r11.xzw + float3(1,1,1);
		  r11.xzw = r11.xzw * r8.xxx;
		  r12.xy = float2(0.25,0.75) * r6.ww;
		  r8.xyz = -r12.xxx * r8.yzw + r8.yzw;
		  r8.xyz = r12.yyy * r11.xzw + r8.xyz;
		  r10.yzw = r10.yzw * r7.www;
		  r9.xyz = r9.xyz * r10.xxx + r10.yzw;
		  r7.xyz = r8.xyz * r11.yyy + r9.xyz;
		  break;
		  case 3 :      r5.z = (int)r5.w + 1;
		  r8.xyzw = max(cb7[0].yyww, float4(0,0,0,0));
		  bitmask.w = ((~(-1 << 28)) << 4) & 0xffffffff;  r6.w = (((uint)r3.z << 4) & bitmask.w) | ((uint)3 & ~bitmask.w);
		  r9.xy = max(cb7[0].xy, float2(0,0));
		  r9.xy = min(float2(2,2), r9.xy);
		  r8.xyzw = min(float4(0.959999979,0.959999979,0.959999979,0.959999979), r8.xyzw);
		  r9.xy = float2(-1,-1) + r9.xy;
		  r6.w = cb7[0].z + -cb7[0].w;
		  r7.w = cb7[0].z + cb7[0].w;
		  r7.w = r7.w + -r6.w;
		  r6.w = -r6.w + r3.y;
		  r7.w = 1 / r7.w;
		  r6.w = saturate(r7.w * r6.w);
		  r7.w = r6.w * -2 + 3;
		  r6.w = r6.w * r6.w;
		  r9.z = r7.w * r6.w;
		  r6.w = -r7.w * r6.w + 1;
		  r10.xy = cmp(float2(0,0) < r9.xy);
		  r10.zw = cmp(r9.xy < float2(0,0));
		  r10.xy = (int2)-r10.xy + (int2)r10.zw;
		  r10.xy = (int2)r10.xy;
		  r10.xy = -r0.ww * r10.xy + float2(1,1);
		  r9.xy = r9.xy * r10.xy + float2(1,1);
		  r9.xy = max(float2(0,0), r9.xy);
		  r10.xyz = r9.xxx * r2.xyz + r1.www;
		  r10.xyz = max(float3(0,0,0), r10.xyz);
		  r9.xyw = r9.yyy * r2.xyz + r1.www;
		  r9.xyw = max(float3(0,0,0), r9.xyw);
		  r7.w = max(r10.y, r10.z);
		  r7.w = max(r10.x, r7.w);
		  r11.xyz = cb7[0].xxx + float3(1,0.666666687,0.333333343);
		  r11.xyz = frac(r11.xyz);
		  r11.xyz = r11.xyz * float3(6,6,6) + float3(-3,-3,-3);
		  r11.xyz = saturate(float3(-1,-1,-1) + abs(r11.xyz));
		  r11.xyz = float3(-1,-1,-1) + r11.xyz;
		  r11.xyz = r8.yyy * r11.xyz + float3(1,1,1);
		  r11.xyz = r11.xyz * r7.www;
		  r12.xyzw = float4(0.25,0.75,0.25,0.75) * r8.xyzw;
		  r8.xyz = -r12.xxx * r10.xyz + r10.xyz;
		  r8.xyz = r12.yyy * r11.xyz + r8.xyz;
		  r7.w = max(r9.y, r9.w);
		  r7.w = max(r9.x, r7.w);
		  r10.xyz = cb7[0].zzz + float3(1,0.666666687,0.333333343);
		  r10.xyz = frac(r10.xyz);
		  r10.xyz = r10.xyz * float3(6,6,6) + float3(-3,-3,-3);
		  r10.xyz = saturate(float3(-1,-1,-1) + abs(r10.xyz));
		  r10.xyz = float3(-1,-1,-1) + r10.xyz;
		  r10.xyz = r8.www * r10.xyz + float3(1,1,1);
		  r10.xyz = r10.xyz * r7.www;
		  r9.xyw = -r12.zzz * r9.xyw + r9.xyw;
		  r9.xyw = r12.www * r10.xyz + r9.xyw;
		  r9.xyz = r9.xyw * r9.zzz;
		  r7.xyz = r8.xyz * r6.www + r9.xyz;
		  break;
		  case 4 :      if (r2.w == 0) {
			r8.x = r5.x;
			r8.y = r3.x;
			r8.z = r4.x;
			r5.z = 0;
			while (true) {
			  r6.w = cmp((int)r5.z >= 8);
			  if (r6.w != 0) break;
			  r6.w = (int)r5.z + (int)r5.w;
			  r6.w = (int)r6.w + 1;
			  r7.w = cmp(r5.x >= icb[r5.z+0].x);
			  r8.w = cmp(r5.x < icb[r5.z+0].z);
			  r7.w = r7.w ? r8.w : 0;
			  if (r7.w != 0) {
				r7.w = icb[r5.z+0].z + -icb[r5.z+0].x;
				r8.w = -icb[r5.z+0].x + r5.x;
				r7.w = r8.w / r7.w;
				r8.w = cb7[0].x * 0.00277777785 + icb[r5.z+0].x;
				r9.x = icb[r5.z+0].z + -r8.w;
				r6.x = r7.w * r9.x + r8.w;
				r6.y = cb7[0].y * 0.0125000002;
			  } else {
				r7.w = icb[r5.z+0].x + -icb[r5.z+0].y;
				r8.w = cmp(r5.x < icb[r5.z+0].x);
				r9.x = cmp(icb[r5.z+0].y < r5.x);
				r8.w = r8.w ? r9.x : 0;
				r9.x = icb[r5.z+0].x + -r7.w;
				r9.y = -r9.x + r5.x;
				r7.w = r9.y / r7.w;
				r9.y = cb7[0].x * 0.00277777785 + icb[r5.z+0].x;
				r9.y = r9.y + -r9.x;
				r9.x = r7.w * r9.y + r9.x;
				r9.y = cb7[0].y * 0.0125000002;
				r6.xy = r8.ww ? r9.xy : r5.xy;
			  }
			  r6.x = r6.x + -r5.x;
			  r6.xyw = r8.xyz + r6.xyz;
			  r8.xyz = max(float3(0,0,0), r6.xyw);
			  r5.z = (int)r5.z + 1;
			}
			r6.xyw = float3(1,0.666666687,0.333333343) + r8.xxx;
			r6.xyw = frac(r6.xyw);
			r6.xyw = r6.xyw * float3(6,6,6) + float3(-3,-3,-3);
			r6.xyw = saturate(float3(-1,-1,-1) + abs(r6.xyw));
			r6.xyw = float3(-1,-1,-1) + r6.xyw;
			r6.xyw = r8.yyy * r6.xyw + float3(1,1,1);
			r7.xyz = r8.zzz * r6.xyw;
		  } else {
			r7.xyz = r1.xyz;
		  }
		  break;
		  default :
		  r7.xyz = r1.xyz;
		  break;
		}
		r4.yzw = r7.xyz * cb7[0].yyy + r4.yzw;
		r3.z = (int)r3.z + 1;
		}
		r0.xyz = r4.yzw + r0.xyz;
		r0.xyz = max(float3(0,0,0), r0.xyz);
		o0.xyz = min(cb13[5].zzz, r0.xyz);
		o0.w = 0;
		
		return float4(o0.xyz, o0.w);
    }
}