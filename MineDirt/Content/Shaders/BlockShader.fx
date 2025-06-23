// Transformation matrices
float4x4 WorldViewProjection;
texture2D TextureAtlas : register(t0);
sampler2D TextureSampler = sampler_state
{
    texture = <TextureAtlas>;
};

// Vertex input structure
struct VertexInput
{
    float3 Position : POSITION0; // Position (byte)
    float2 TexCoord : TEXCOORD0; // UV (byte)
    float Light : COLOR0; // Light value
};

// Vertex output structure
struct VertexOutput
{
    float4 Position : POSITION; // Transformed position
    float2 TexCoord : TEXCOORD0; // Passed-through texture coordinates
    float Light : COLOR0; // Light value
};

// Vertex shader
VertexOutput VS_Main(VertexInput input)
{
    VertexOutput output;
    
    // Transform and decompress position
    output.Position = mul(float4(input.Position, 1.0f), WorldViewProjection);

    // Decompress UVs
    output.TexCoord = input.TexCoord;
    output.Light = input.Light;
    
    return output;
}

// Pixel shader (only this function changed)
float4 PS_Main(VertexOutput IN) : COLOR0
{
    // Sample base color and apply vertex lighting
    float4 color = tex2D(TextureSampler, IN.TexCoord);
    color.rgb *= IN.Light;

    // --- Compute UV relative to the sprite in the atlas ---
    // If your atlas is laid out in a uniform grid of N×M tiles,
    // set TileCount = float2(N, M):
    static const float2 TileCount = float2(16, 16);
    float2 localUV = frac(IN.TexCoord * TileCount);

    // --- Centered vignette in that local space ---
    float2 centered = localUV - 0.5;
    float  dist = length(centered);
    // darken smoothly from radius=0.4→0.5
    float  vig = IN.Light - smoothstep(0.01, 2.5, dist);
    color.rgb *= vig;

    return color;
}

// Techniques
technique BasicTechnique
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 PS_Main();
    }
}