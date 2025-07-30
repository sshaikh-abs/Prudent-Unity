void MainLight_float(float3 WorldPos, out half3 Direction, out half ShadowAtten)
{
	#ifdef SHADERGRAPH_PREVIEW
		Direction = half3(0.5, 0.5, 0);
		ShadowAtten = 1;

	#else
		#if SHADOWS_SCREEN
			half4 clipPos = TransformWorldToClip(WorldPos);
			half4 shadowCoord = ComputeScreenPos(clipPos);
		#else
			half4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
		#endif

		Light mainLight = GetMainLight(shadowCoord);
		Direction = mainLight.direction;

		#if !defined(_MAIN_LIGHT_SHADOWS) || defined(_RECEIVE_SHADOWS_OFF)
			ShadowAtten = 1.0h;
		#endif

		#if SHADOWS_SCREEN
			ShadowAtten = SampleScreenSpaceShadowmap(shadowCoord);
		#else
			ShadowSamplingData shadowSamplingData = GetMainLightShadowSamplingData();
			half4 shadowParams = GetMainLightShadowParams();
			ShadowAtten = SampleShadowmap(TEXTURE2D_ARGS(_MainLightShadowmapTexture, sampler_MainLightShadowmapTexture),TransformWorldToShadowCoord(WorldPos), shadowSamplingData, shadowParams, false);
		#endif
	#endif
}