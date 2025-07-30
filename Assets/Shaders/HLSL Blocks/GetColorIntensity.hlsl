void GetColorIntensity_float(float3 Color, out float Intensity)
{
    float r, g, b;
    r = Color.r * 0.222f;
    g = Color.g * 0.707f;
    b = Color.b * 0.071f;

    Intensity = r + g + b;
}
