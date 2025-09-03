MODES
{
	Default();
}

CS
{
	#define cmp -
	#include "system.fxc"

	Texture2D g_t0 < Attribute( "LUT2D_Processed" ); SrgbRead(true); >;
	RWTexture3D<float4> OutputTexture < Attribute( "LUT3D" ); >;

	[numthreads( 8, 8, 1 )]
	void MainCs( uint3 vThreadID : SV_DispatchThreadID )
	{
		float4 cb0[27] = 
		{
			float4(0.00, 0.00, 0.00, 0.00),
			float4(0.00, 0.00, 0.00, 0.00),
			float4(0.00, 0.00, 0.00, 0.00),
			float4(0.00, 0.00, 0.00, 0.00),
			float4(0.00, 0.00, 0.00, 0.00),
			float4(0.00, 0.00, 0.00, 0.00),
			float4(0.00, 0.00, 0.00, 0.00),
			float4(32.00, 1024.00, 0.00, 0.00),
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
			float4(12.50, 0.00, 0.00, 0.00),
		};
		
		// Needs manual fix for instruction:
		// unknown dcl_: dcl_thread_group 8, 8, 1
		float4 r0,r1,r2,r3;
		uint4 bitmask, uiDest;
		float4 fDest;

		// Needs manual fix for instruction:
		// unknown dcl_: dcl_uav_typed_texture3d (float,float,float,float) u0
		r0.xy = (uint2)vThreadID.xz;
		r0.x = r0.y * cb0[7].x + r0.x;
		r0.x = (uint)r0.x;
		r0.y = vThreadID.y;
		r0.zw = float2(0,0);
		r0.xyz = g_t0.Load(r0.xyz).xyz;
		r1.x = dot(float3(0.970843613,0.0269652624,0.00210750522), r0.xyz);
		r1.y = dot(float3(0.0110121472,0.986875057,0.00214175484), r0.xyz);
		r1.z = dot(float3(0.0109303994,0.0269407779,0.962179005), r0.xyz);
		r0.xyz = float3(3.20000005,3.20000005,3.20000005) * r0.xyz;
		r2.xyz = r1.xyz * float3(-9665475,-9665475,-9665475) + float3(554921.688,554921.688,554921.688);
		r2.xyz = r2.xyz * r1.xyz;
		r3.xyz = r1.xyz * float3(-800575.313,-800575.313,-800575.313) + float3(-1792565.63,-1792565.63,-1792565.63);
		r3.xyz = r1.xyz * r3.xyz + float3(51205.2852,51205.2852,51205.2852);
		r2.xyz = r2.xyz / r3.xyz;
		r2.xyz = float3(1.07499135,1.07499135,1.07499135) * r2.xyz;
		r0.w = max(cb0[26].x, 0);
		r2.xyz = min(r0.www, r2.xyz);
		r3.xyz = float3(6.94926977,6.94926977,6.94926977) * r1.xyz;
		r1.xyz = cmp(r1.xyz < float3(0.0813999996,0.0813999996,0.0813999996));
		r3.xyz = log2(r3.xyz);
		r3.xyz = float3(2.79999995,2.79999995,2.79999995) * r3.xyz;
		r3.xyz = exp2(r3.xyz);
		r1.xyz = r1.xyz ? r3.xyz : r2.xyz;
		r2.x = dot(float3(1.60475004,-0.531080008,-0.0736699998), r1.xyz);
		r2.y = dot(float3(-0.102080002,1.10812998,-0.00604999997), r1.xyz);
		r2.z = dot(float3(-0.00326999999,-0.0727600008,1.07602), r1.xyz);
		r1.xyz = max(float3(0,0,0), r2.xyz);
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
		r0.x = cmp(0 != cb0[25].x);
		r0.xyz = r0.xxx ? r1.xyz : r2.xyz;
		r0.w = 0;
		// No code for instruction (needs manual fix):
		// store_uav_typed u0.xyzw, vThreadID.xyzz, r0.xyzw
	
		OutputTexture[vThreadID.xyz] = r0;
	}	
}