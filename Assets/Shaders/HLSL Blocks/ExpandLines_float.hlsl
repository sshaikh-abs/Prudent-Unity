void ExpandLine_float(float3 PositionOS, float3 PositionWS, float4 PositionCS, float3 CameraPosWS, float Thickness, out float3 OffsetPositionOS)
{
    // Get view direction
    float3 viewDir = normalize(PositionWS - CameraPosWS);

    // Find perpendicular direction in screen space
    float3 rightVector = normalize(cross(viewDir, float3(0, 1, 0))) * Thickness;

    // Offset position to simulate line thickness
    OffsetPositionOS = PositionOS + rightVector;
}
