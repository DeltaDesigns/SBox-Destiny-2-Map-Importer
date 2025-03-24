MODES
{
	VrForward();
}

FEATURES
{
	#include "common/features.hlsl"
}

COMMON
{
	#include "common/shared.hlsl"
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
		float4 vPositionPs        : SV_Position;
	#endif

	#if ( ( PROGRAM == VFX_PROGRAM_PS ) )
		float4 vPositionSs        : SV_Position;
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
	#include "common/pixel.hlsl"

	RenderState( DepthWriteEnable, false );
	RenderState( DepthEnable, false );

	Texture2D g_tColorBuffer < Attribute( "ColorBuffer" ); SrgbRead( false ); >;
	float fThreshold < Attribute("threshold"); Default(0.5); Range(0.0, 4.0); >;
	float fSmoothness < Attribute("smoothness"); Default(0.3); Range(0.0, 4.0); >;
	float2 vLightPosition < Attribute("lightposition"); >;
	float fLightIntensity < Attribute("lightintensity"); Default(1.0); Range(0.0, 5.0); >;
	float fDecay < Attribute("decay"); Default(0.96); Range(0.8, 1.0); >;
	float fExposure < Attribute("exposure"); Default(0.2); Range(0.0, 1.0); >;
	float fDensity < Attribute("density"); Default(0.5); Range(0.0, 2.0); >;
	float fWeight < Attribute("weight"); Default(0.3); Range(0.0, 1.0); >;
	int iSampleCount < Attribute("samplecount"); >;
	SamplerState s < Filter( Bilinear ); >;

	float smootherStep(float edge0, float edge1, float x)
	{
		float t = saturate((x - edge0) / (edge1 - edge0));
		return t * t * t * (t * (t * 6.0 - 15.0) + 10.0);
	}

	float luminance(float3 color)
	{
		return dot(color, float3(0.299, 0.587, 0.114));
	}

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		float3 vOriginalColor = g_tColorBuffer.Sample( s, i.vTexCoord ).rgb;
		float3 vHdrColor = pow(vOriginalColor, 4);
		float fLuminance = luminance(vHdrColor);
		
		float fMask = smoothstep(fThreshold - fSmoothness, fThreshold + fSmoothness, fLuminance);
		
		float3 vBrightColor = vOriginalColor * (fMask + 0.05 * smoothstep(0, 0.1, fMask));
		
		float2 vDeltaTexCoord = (i.vTexCoord - vLightPosition);
		vDeltaTexCoord *= fDensity * 0.01;
		
		float3 vColor = vBrightColor;
		
		// Perform radial blur by stepping back towards the light source
		float fIllumination = 1.0;
		float2 vCurrentTexCoord = i.vTexCoord;
		float fTotalWeight = 1.0;
		for (int nSample = 0; nSample < iSampleCount; nSample++)
		{
			vCurrentTexCoord -= vDeltaTexCoord;
			
			float3 vSample = g_tColorBuffer.Sample( s, vCurrentTexCoord ).rgb;
			
			float fSampleLum = luminance(pow(vSample, 4.0));
			float fSampleMask = smootherStep(fThreshold - fSmoothness, fThreshold + fSmoothness, fSampleLum);
			vSample *= (fSampleMask + 0.05 * smoothstep(0, 0.1, fSampleMask));

			fIllumination *= fDecay;
			float fSampleWeight = fIllumination * fWeight;
			vColor += vSample * fSampleWeight;
			fTotalWeight += fSampleWeight;
		}

		float2 vDitherCoord = mad( i.vPositionSs.xy, ( 1.0f / 256.0f ), g_vRandomFloats.xy );
		float fNoise = g_tBlueNoise.Sample( s, vDitherCoord ).r;

		float fDitherIntensity = 0.6 + 0.5 * fNoise;
		vColor *= fDitherIntensity;
		
		vColor *= fExposure;
		vColor += vBrightColor;

		float fOriginalLuminance = luminance(vOriginalColor);
		float fSkyMask = 1.0 - fOriginalLuminance;
		float3 vGodRays = vColor * fLightIntensity * fSkyMask;
		vColor = vOriginalColor + vGodRays * (1.0 - fOriginalLuminance);
		
		// Ensure the result is never darker than the original
		vColor = max(vOriginalColor, vColor);

		vColor = SrgbGammaToLinear(vColor);
		return float4(vColor, 1.0);
	}
}