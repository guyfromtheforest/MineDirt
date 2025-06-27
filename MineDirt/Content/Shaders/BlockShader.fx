// Transformation matrices
float4x4 WorldViewProjection;
texture2D TextureAtlas : register(t0);
float3 ChunkWorldPosition; // The chunk's origin in world space
sampler2D TextureSampler = sampler_state
{
    texture = <TextureAtlas>;
};

// Vertex input structure
struct VertexInput
{
    float2 TexCoord : TEXCOORD0; // UV (byte)
    float PackedPosition : TEXCOORD1; // Our packed position arrives here
};

// Vertex output structure
struct VertexOutput
{
    float4 Position : POSITION; // Transformed position
    float2 TexCoord : TEXCOORD0; // Passed-through texture coordinates
    float Light : COLOR0; // Light value
};

float3 UnpackAndOffset(float packedData)
{
    // 1) Recover exact integer
    float p = floor(packedData + 0.5);

    // 2) Peel off X (4 bits)
    float p0_16 = floor(p / 16.0);      // p >> 4
    float blockX = p - p0_16 * 16.0;     // p % 16

    // 3) Peel off Z (next 4 bits)
    float p1_16 = floor(p0_16 / 16.0);  // p >> 8
    float blockZ = p0_16 - p1_16 * 16.0; // (p>>4) % 16

    // 4) Peel off cornerID (next 3 bits)
    float p2_8 = floor(p1_16 / 8.0);   // p >> 11
    float cornerF = p1_16 - p2_8 * 8.0;   // (p>>8) % 8

    float blockY = p2_8 % 256.0; // This is (p >> 11) % 256

    // 6) Decode cornerF’s three bits into offsets 0 or 1
    float ox = cornerF - floor(cornerF / 2.0) * 2.0;        // bit0 = cornerF % 2
    float oy = floor(cornerF / 2.0) - floor(cornerF / 4.0) * 2.0; // bit1 = (cornerF>>1)%2
    float oz = floor(cornerF / 4.0);                       // bit2 = cornerF >> 2

    // 7) Reconstruct and return
    return float3(blockX, blockY, blockZ) + float3(ox, oy, oz);
}

float UnpackLight(float packedData)
{
    // 1) Recover exact integer
    float p = floor(packedData + 0.5);

    // 2) Peel off X (4 bits)
    float p0_16 = floor(p / 16.0);

    // 3) Peel off Z (next 4 bits)
    float p1_16 = floor(p0_16 / 16.0);

    // 4) Peel off cornerID (next 3 bits)
    float p2_8 = floor(p1_16 / 8.0);

    // 5) p2_8 now contains (light << 8 | Y). Separate them.
    float lightVal_intermediate = floor(p2_8 / 256.0); // This is the raw light level (0-15)

    return lerp(0.3, 1.0, lightVal_intermediate / 15.0);
}

// Vertex shader
VertexOutput VS_Main(VertexInput input)
{
    VertexOutput output;

    float3 coords = UnpackAndOffset(input.PackedPosition);

    // --- 2. CALCULATE FINAL POSITION ---
    float3 worldPos = coords + ChunkWorldPosition;
    output.Position = mul(float4(worldPos, 1.0f), WorldViewProjection);

    // The other data is passed through directly.
    output.TexCoord = input.TexCoord;
    output.Light = UnpackLight(input.PackedPosition);
    
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