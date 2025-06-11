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

// Pixel shader
float4 PS_Main(VertexOutput input) : COLOR
{
    // Sample the texture
    float4 color = tex2D(TextureSampler, input.TexCoord);

    // Apply the light value to the color
    color.rgb *= input.Light; // Modulate RGB channels by the light value

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