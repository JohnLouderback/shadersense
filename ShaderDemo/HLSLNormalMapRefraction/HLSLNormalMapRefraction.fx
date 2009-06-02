//////////////////////////////////////////////////////////////////////////////////////////////////
// File: HLSLNormalMapRefraction.fx
// Author: Chris Smith
// Date Created: 3/22/06
// Description: Renders a scene with a normal map refracted projected texture
// Disclaimer: Use this however you want, but I am not responsible for anything
//////////////////////////////////////////////////////////////////////////////////////////////////

float4x4 WorldViewProj; //World * View * Projection
float4x4 TexTransform;  //Texture transformation matrix

texture ProjTex;
texture NormalTex;

sampler ProjTexSampler = sampler_state
{
    Texture = <ProjTex>;
    MinFilter = LINEAR;  
    MagFilter = LINEAR;
    MipFilter = LINEAR;
};

sampler NormalTexSampler = sampler_state
{
    Texture = <NormalTex>;
    MinFilter = LINEAR;  
    MagFilter = LINEAR;
    MipFilter = LINEAR;
};

////////////////////////////////////////////////////////////
// Structures
////////////////////////////////////////////////////////////
struct MainA2V{
    float4 Position : POSITION;
    float2 TexCoord0 : TEXCOORD0;
};

struct MainV2P{
    float4 Position  : POSITION;
    float2 TexCoord0 : TEXCOORD0;
    float4 TexCoord1 : TEXCOORD1;
};

////////////////////////////////////////////////////////////
// Vertex Shaders
////////////////////////////////////////////////////////////
void MainVS(in MainA2V IN, out MainV2P OUT)
{
    //Copy normal map texture coordinates through
    OUT.TexCoord0 = IN.TexCoord0;
    
    //Projected texture coordinates
    OUT.TexCoord1 = mul(IN.Position, TexTransform);
    
    //Transform model-space vertex position to screen space:
    OUT.Position = mul(IN.Position, WorldViewProj);
}

////////////////////////////////////////////////////////////
// Pixel Shader
////////////////////////////////////////////////////////////
float4 MainPS(in MainV2P IN) : COLOR 
{
    //Uncompress the normal map
    float3 Normal = 2.0f * tex2D(NormalTexSampler, IN.TexCoord0).rgb - 1.0f;
    
    //Find the color of the object
    float4 Color = tex2Dproj(ProjTexSampler, IN.TexCoord1 + float4(Normal, 0.0f));
    
    //Return the final color
    return Color;
}

////////////////////////////////////////////////////////////
// Techniques
////////////////////////////////////////////////////////////
technique NormalMapRefraction
{
    pass P0
    {
        CullMode = NONE;
        VertexShader = compile vs_1_1 MainVS();
        PixelShader = compile ps_2_0 MainPS();
    }
}
