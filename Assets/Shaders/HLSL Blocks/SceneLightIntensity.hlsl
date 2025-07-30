void GetGreyscaleLightIntensity_float(float3 Color, out float Intensity)
{
    float r, g, b;
    r = Color.r * 0.222f;
    g = Color.g * 0.707f;
    b = Color.b * 0.071f;

    Intensity = r + g + b;
}

float LightIntensity(float3 lightColor, float3 lightDirection, float3 worldPosition)
{
    float dotp = dot(lightDirection, worldPosition);
    float lightIntensity = 0;
    GetGreyscaleLightIntensity_float(lightColor, lightIntensity);
    return saturate(dotp * lightIntensity);
}

void TotalLightIntensity_float(float3 AmbientColor, float3 WorldPosition, out float Intensity)
{
    #ifdef SHADERGRAPH_PREVIEW
    Intensity = 1;	
    #else
    Intensity = 0;
    GetGreyscaleLightIntensity_float(AmbientColor, Intensity);
    Light mainLight = GetMainLight();
    Intensity += LightIntensity(mainLight.color, mainLight.direction, WorldPosition);

    int pixelLightCount = GetAdditionalLightsCount();
    for (int i = 0; i < pixelLightCount; ++i)
    {
        Light light = GetAdditionalLight(i, WorldPosition);
        Intensity += LightIntensity(light.color, light.direction, WorldPosition);
    }

    Intensity = saturate(Intensity);
    #endif
}
