#ifndef COMMON_PIXEL_SHADING_H
#define COMMON_PIXEL_SHADING_H

#include "common/material.hlsl"
#include "common/GBuffer.hlsl"
#include "common/classes/Decals.hlsl"

float4 DoAtmospherics( float3 vPositionWs, float2 vPositionSs, float4 vColor, bool bAdditiveBlending = false )
{
    vPositionWs = vPositionWs.xyz;
    float3 vPositionToCameraWs = vPositionWs.xyz - g_vCameraPositionWs.xyz;

    if ( g_bFogEnabled )
	{
		if( bAdditiveBlending )
		{
			//
			// When blending additively, dest pixel contains the fog term.
			// Therefore, we want to scale alpha with fog amount in front of us,
			// rather than double-adding the fog
			//
			if ( g_bGradientFogEnabled )
				vColor.a *= 1.0 - CalculateGradientFog( vPositionWs, vPositionToCameraWs ).a;

			if ( g_bCubemapFogEnabled )
				vColor.a *= 1.0 - CalculateCubemapFog( vPositionWs, vPositionToCameraWs ).a;

			if ( g_bVolumetricFogEnabled )
				vColor.a *= CalculateVolumetricFog( vPositionWs.xyz, vPositionSs.xy ).a;
		}
        else
		{
			vColor.rgb = ApplyGradientFog( vColor.rgb, vPositionWs.xyz, vPositionToCameraWs.xyz );
			vColor.rgb = ApplyCubemapFog( vColor.rgb, vPositionWs.xyz, vPositionToCameraWs.xyz );
			vColor.rgb = ApplyVolumetricFog( vColor.rgb, vPositionWs.xyz, vPositionSs.xy );
		}
	}

    return vColor;
}

float4 DoPostProcessing( const Material material, float4 color )
{
    // Remove alpha if we are not transparent, might be shit but screenshots are being written
    // with alpha in some shaders
    #ifndef CUSTOM_MATERIAL_INPUTS
        #if ( !S_ALPHA_TEST && !S_TRANSLUCENT && !TRANSLUCENT )
        {
            color.a = 1.0f;
        }
        #endif
    #endif

    return color;
}

//-----------------------------------------------------------------------------
// 
// A simple shading model that uses the Valve lighting model.
// Just converts the parameters from Material into the internal format used
// by the Valve lighting model.
//
// Right now we're aiming for accuracy over flexibility, most people don't need
// to write their own shading models, in the future it'd be nice to be more
// explicit.
//
//-----------------------------------------------------------------------------
class TestShadingModel
{
    //
    // Converts our Material struct to a FinalCombinerInput_t
    // PS_InitFinalCombiner assumes that you want to control the "optional" parameters yourself.
    // These *need* to be set up by you if you want correct lighting.
    //
    // PS_FinalCombiner should be called at the end and works on the FinalCombinerInput_t data only. 
    // This does lighting, tonemapping, etc. in a standardized way.
    //
    static CombinerInput MaterialToCombinerInput( Material m )
    {
        CombinerInput o = PS_InitFinalCombiner();
      
        o.vPositionWithOffsetWs = m.WorldPositionWithOffset;
        o.vPositionWs = m.WorldPosition;
        o.vPositionSs = m.ScreenPosition;

        o.vNormalWs = m.Normal;
        o.vGeometricNormalWs = m.GeometricNormal;
        o.vNormalTs = NormalWorldToTangent( m.Normal, m.GeometricNormal, m.WorldTangentU, m.WorldTangentV );

        o.vRoughness = m.Roughness.xx;
        o.vEmissive = m.Emission;
        o.flAmbientOcclusion = m.AmbientOcclusion;
        o.vTransmissiveMask = m.Transmission;
        
        // Sane default
        // o.flSSSCurvature = length( fwidth( o.vNormalWs.xyz ) ) / length( fwidth( o.vPositionWithOffsetWs.xyz ) );

        return o;    
    }

    static float4 Shade( Material m )
    {
        // !!!
        // It's very odd we're not just using PS_FinalCombiner( FinalCombinerInput_t finalCombinerInput ) ?!
        // !!!

        // Want it right before the lighting
        Decals::Apply( m.WorldPosition, m.ScreenPosition.xy, m );

        LightingTerms_t lightingTerms = InitLightingTerms();
        CombinerInput combinerInput = MaterialToCombinerInput( m );

        // Hmm is this the right place for it
        combinerInput = CalculateDiffuseAndSpecularFromAlbedoAndMetalness( combinerInput, m.Albedo.rgb, m.Metalness );

        // Roughness adjusted to fix geometric specular aliasing
        combinerInput.vRoughness.xy = AdjustRoughnessByGeometricNormal( combinerInput.vRoughness.xy, combinerInput.vGeometricNormalWs.xyz );
        ComputeDirectLighting( lightingTerms, combinerInput );

        CalculateIndirectLighting( lightingTerms, combinerInput );

        float3 vDiffuseAO = CalculateDiffuseAmbientOcclusion( combinerInput, lightingTerms );
		lightingTerms.vIndirectDiffuse.rgb *= vDiffuseAO.rgb;
		lightingTerms.vDiffuse.rgb *= lerp( float3( 1.0, 1.0, 1.0 ), vDiffuseAO.rgb, combinerInput.flAmbientOcclusionDirectDiffuse );
        
		float3 vSpecularAO = CalculateSpecularAmbientOcclusion( combinerInput, lightingTerms );
        lightingTerms.vIndirectSpecular.rgb *= vSpecularAO.rgb;
		lightingTerms.vSpecular.rgb *= lerp( float3( 1.0, 1.0, 1.0 ), vSpecularAO.rgb, combinerInput.flAmbientOcclusionDirectSpecular );

        float3 vDiffuse = ( ( lightingTerms.vDiffuse.rgb + lightingTerms.vIndirectDiffuse.rgb ) * combinerInput.vDiffuseColor.rgb ) + combinerInput.vEmissive.rgb;
        float3 vSpecular = lightingTerms.vSpecular.rgb + lightingTerms.vIndirectSpecular.rgb;

        float4 color = float4( vDiffuse + vSpecular, combinerInput.flOpacity );

        if( DepthNormals::WantsDepthNormals() )
            return DepthNormals::Output( m.Normal, m.Roughness );

        if( ToolsVis::WantsToolsVis() )
            return DoToolsVis( color, m, lightingTerms );

        // Composite atmospherics after lighting
        color = DoAtmospherics( m.WorldPosition, m.ScreenPosition.xy, color );
        
        return color;
    }

#ifdef COMMON_PS_INPUT_DEFINED
    //  We still have this crap because old hammer maps use lightmapping, this fetches lightmap UV as well
    //  But I don't want to bloat material API with yet another thing
    //  Lightmapped UV feels silly to me with our GI goals but we don't have a replacement now
    static CombinerInput MaterialToCombinerInput( PixelInput i, Material m )
    {
        CombinerInput o = PS_InitFinalCombiner();

        o = PS_CommonProcessing( i );
        
        // this should not be here
        #if ( S_ALPHA_TEST )
        {
            o.flOpacity = m.Opacity * o.flOpacity;

            // Shaded Prepass should always be conservative
            if ( DepthNormals::WantsDepthNormals() )
                clip(o.flOpacity - .999);

            // Clip first to try to kill the wave if we're in an area of all zero
            clip( o.flOpacity - .001 );

            o.flOpacity = AdjustOpacityForAlphaToCoverage( o.flOpacity, g_flAlphaTestReference, g_flAntiAliasedEdgeStrength, i.vTextureCoords.xy );
            clip( o.flOpacity - 0.001 );
        }
        #elif ( S_TRANSLUCENT )
        {
            o.flOpacity *= m.Opacity * g_flOpacityScale;
        }
        #else
            o.flOpacity *= m.Opacity;
        #endif

        o = CalculateDiffuseAndSpecularFromAlbedoAndMetalness( o, m.Albedo.rgb, m.Metalness );

        o.vNormalWs = m.Normal;
        o.vNormalTs = NormalWorldToTangent( m.Normal, i.vNormalWs, i.vTangentUWs, i.vTangentVWs );
        o.vRoughness = m.Roughness.xx;
        o.vEmissive = m.Emission;
        o.flAmbientOcclusion = m.AmbientOcclusion;
        o.vTransmissiveMask = m.Transmission;

        o.vGeometricNormalWs.xyz = i.vNormalWs;
        o.vRoughness.xy = AdjustRoughnessByGeometricNormal( o.vRoughness.xy, o.vGeometricNormalWs.xyz );

        return o;
    }

    static float4 Shade( PixelInput i, Material m )
    {
        m.WorldPosition.xyz = i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz;
        m.WorldPositionWithOffset = i.vPositionWithOffsetWs;
        m.ScreenPosition = i.vPositionSs;
        m.GeometricNormal = i.vNormalWs;

        // Want it right before the lighting
        Decals::Apply( m.WorldPosition, m.ScreenPosition.xy, m );

        LightingTerms_t lightingTerms = InitLightingTerms();
        CombinerInput combinerInput = MaterialToCombinerInput( i, m );

        // Hmm is this the right place for it
        combinerInput = CalculateDiffuseAndSpecularFromAlbedoAndMetalness( combinerInput, m.Albedo.rgb, m.Metalness );

        ComputeDirectLighting( lightingTerms, combinerInput );
        CalculateIndirectLighting( lightingTerms, combinerInput );

        float3 vDiffuseAO = CalculateDiffuseAmbientOcclusion( combinerInput, lightingTerms );
		lightingTerms.vIndirectDiffuse.rgb *= vDiffuseAO.rgb;
		lightingTerms.vDiffuse.rgb *= lerp( float3( 1.0, 1.0, 1.0 ), vDiffuseAO.rgb, combinerInput.flAmbientOcclusionDirectDiffuse );
        
		float3 vSpecularAO = CalculateSpecularAmbientOcclusion( combinerInput, lightingTerms );
        lightingTerms.vIndirectSpecular.rgb *= vSpecularAO.rgb;
		lightingTerms.vSpecular.rgb *= lerp( float3( 1.0, 1.0, 1.0 ), vSpecularAO.rgb, combinerInput.flAmbientOcclusionDirectSpecular );

        float3 diffuse = ( ( lightingTerms.vDiffuse.rgb + lightingTerms.vIndirectDiffuse.rgb ) * combinerInput.vDiffuseColor.rgb ) + combinerInput.vEmissive.rgb;
        float3 specular = lightingTerms.vSpecular.rgb + lightingTerms.vIndirectSpecular.rgb;

        float4 color = float4( diffuse + specular, combinerInput.flOpacity );

        if( DepthNormals::WantsDepthNormals() )
            return DepthNormals::Output( m.Normal, m.Roughness );

        if (ToolsVis::WantsToolsVis())
            return DoToolsVis( color, m, lightingTerms );

        color = DoAtmospherics(m.WorldPosition, m.ScreenPosition.xy, color );

        return color;
    }
#endif

    /// <summary>
    /// Tools visualization for the standard shading model.
    /// </summary>
    static float4 DoToolsVis(inout float4 color, Material m, LightingTerms_t lightingTerms)
    {
        ToolsVis toolVis = ToolsVis::Init(color, lightingTerms.vDiffuse.rgb, lightingTerms.vSpecular.rgb, lightingTerms.vIndirectDiffuse.rgb, lightingTerms.vIndirectSpecular.rgb, lightingTerms.vTransmissive.rgb );

        toolVis.HandleFlatOverlayColor(m.Albedo, color);
        toolVis.HandleFullbright(color, m.Albedo, m.WorldPosition, m.Normal);
        toolVis.HandleDiffuseLighting(color);
        toolVis.HandleSpecularLighting(color);
        toolVis.HandleTransmissiveLighting(color);
        toolVis.HandleLightingComplexity(color, (uint2)m.ScreenPosition.xy, m.WorldPosition, m.Normal);
        toolVis.HandleAlbedo(color, m.Albedo);
        toolVis.HandleReflectivity(color, m.Albedo);
        toolVis.HandleRoughness(color, float2(m.Roughness, m.Roughness));
        toolVis.HandleDiffuseAmbientOcclusion(color, min( m.AmbientOcclusion, min( lightingTerms.flBakedAmbientOcclusion, lightingTerms.flDynamicAmbientOcclusion ) ) );
        toolVis.HandleSpecularAmbientOcclusion(color,min( m.AmbientOcclusion, min( lightingTerms.flBakedAmbientOcclusion, lightingTerms.flDynamicAmbientOcclusion ) ) );
        toolVis.HandleShaderIDColor(color);
        toolVis.HandleCubemapReflections(color, m.WorldPosition, m.Normal, (uint2)m.ScreenPosition.xy);
        toolVis.HandleNormalTs(color, m.TangentNormal);
        toolVis.HandleNormalWs(color, m.Normal);
        toolVis.HandleTangentUWs(color, m.WorldTangentU);
        toolVis.HandleTangentVWs(color, m.WorldTangentV);
        toolVis.HandleGeometricNormalWs(color, m.GeometricNormal);
        toolVis.HandleBentNormalWs(color, float3(0,0,0));
        toolVis.HandleBentGeometricNormalWs(color, float3(0,0,0));
        toolVis.HandleGeometricRoughness(color, m.GeometricNormal);
        toolVis.HandleCurvature(color, 0);
        toolVis.HandleTiledRenderingColors(color, m.Albedo, m.ScreenPosition.xy);

        // What the fuck
#ifndef CUSTOM_MATERIAL_INPUTS
        Texture2D representativeTexture = g_tColor;
        toolVis.ShowUVs(color, m.Albedo, representativeTexture, m.TextureCoords);
        toolVis.ShowMipUtilization(color, m.Albedo, representativeTexture, m.TextureCoords);
#endif
        // Disable bloom
        color.rgb = saturate(color.rgb);

        return color;
    }
};

#endif // COMMON_PIXEL_SHADING_H