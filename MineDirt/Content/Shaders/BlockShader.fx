float4x4 WorldViewProjection;
texture2D TextureAtlas : register(t0);
float3 ChunkWorldPosition;
sampler2D TextureSampler = sampler_state
{
    texture = <TextureAtlas>;
};

struct VertexInput
{
    float2 TexCoord : TEXCOORD0;
    float PackedPosition : TEXCOORD1;
};

struct VertexOutput
{
    float4 Position : POSITION;
    float2 TexCoord : TEXCOORD0;
    float Light : COLOR0;
};

float3 UnpackAndOffset(float packedData)
{
    float p = floor(packedData + 0.5);

    float p0_16 = floor(p / 16.0);
    float blockX = p - p0_16 * 16.0;

    float p1_16 = floor(p0_16 / 16.0);
    float blockZ = p0_16 - p1_16 * 16.0;

    float p2_8 = floor(p1_16 / 8.0);
    float cornerF = p1_16 - p2_8 * 8.0;

    float blockY = p2_8 % 256.0;

    float ox = cornerF - floor(cornerF / 2.0) * 2.0;
    float oy = floor(cornerF / 2.0) - floor(cornerF / 4.0) * 2.0;
    float oz = floor(cornerF / 4.0);

    return float3(blockX, blockY, blockZ) + float3(ox, oy, oz);
}

float UnpackLight(float packedData)
{
    float p = floor(packedData + 0.5);
    float p0_16 = floor(p / 16.0);
    float p1_16 = floor(p0_16 / 16.0);
    float p2_8 = floor(p1_16 / 8.0);
    float lightVal_intermediate = floor(p2_8 / 256.0);

    return lerp(0.3, 1.0, lightVal_intermediate / 15.0);
}

VertexOutput VS_Main(VertexInput input)
{
    VertexOutput output;

    float3 coords = UnpackAndOffset(input.PackedPosition);

    float3 worldPos = coords + ChunkWorldPosition;
    output.Position = mul(float4(worldPos, 1.0f), WorldViewProjection);

    output.TexCoord = input.TexCoord;
    output.Light = UnpackLight(input.PackedPosition);
    
    return output;
}

float4 PS_Main(VertexOutput IN) : COLOR0
{
    float4 color = tex2D(TextureSampler, IN.TexCoord);
    color.rgb *= IN.Light;

    static const float2 TileCount = float2(16, 16);
    float2 localUV = frac(IN.TexCoord * TileCount);

    float2 centered = localUV - 0.5;
    float  dist = length(centered);
    float  vig = IN.Light - smoothstep(0.01, 2.5, dist);
    color.rgb *= vig;

    return color;
}

technique BasicTechnique
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 PS_Main();
    }
}