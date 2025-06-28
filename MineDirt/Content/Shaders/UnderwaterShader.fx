// This sampler will hold the texture of our rendered scene (the RenderTarget)
sampler2D ScreenTexture : register(s0);

// These are your parameters passed from C#
bool IsUnderwater;
float Time;

// The Pixel Shader's input must match the Vertex Shader's output
float4 PS_Main(float4 pos : SV_POSITION, float2 tex : TEXCOORD0) : SV_TARGET
{
    // Get the original color of the pixel from the scene we rendered.
    float4 originalColor = tex2D(ScreenTexture, tex);

    if (!IsUnderwater)
    {
        return originalColor;
    }

    // --- IF WE ARE UNDERWATER, APPLY EFFECTS ---
    float xOffset = sin(tex.y * 20.0 + Time * 1.0) * 0.0005;
    float yOffset = cos(tex.x * 20.0 + Time * 1.5) * 0.0005;
    float2 distortedUV = tex + float2(xOffset, yOffset);
    float4 distortedColor = tex2D(ScreenTexture, distortedUV);

    float3 underwaterColor = float3(0.1, 0.3, 0.7);
    float fogAmount = tex.y * 0.6;
    float3 finalColor = lerp(distortedColor.rgb, underwaterColor, fogAmount);

    return float4(finalColor, originalColor.a);
}

// The technique that MonoGame will use.
technique PostProcess
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_Main();
    }
}