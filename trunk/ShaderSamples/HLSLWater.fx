//////////////////////////////////////////////////////////////////////////////////////////////////
// File: HLSLWater.fx
// Author: Chris Smith
// Date Created: 6/18/06
// Description: Renders water using a normal map refraction technique
// Disclaimer: Use this however you want, but I am not responsible for anything
//////////////////////////////////////////////////////////////////////////////////////////////////
sampler mysamp : register( R0 );
float4x4 WorldViewProj; //World * View * Projection
float4x4 TexTransform;  //Texture transformation matrix
float    TexScroll;     //Texure offset (used to scroll the texture across the plane)

texture ProjTex;
texture Water1Tex;
texture Water2Tex;

sampler ProjTexSampler = sampler_state
{
    Texture = <ProjTex>;
    MinFilter = LINEAR;  
    MagFilter = LINEAR;
    MipFilter = LINEAR;
    AddressU  = Clamp;
    AddressV  = Clamp;
};

sampler Water1TexSampler = sampler_state
{
    Texture = <Water1Tex>;
    MinFilter = LINEAR;  
    MagFilter = LINEAR;
    MipFilter = LINEAR;
};

sampler Water2TexSampler = sampler_state
{
    Texture = <Water2Tex>;
    MinFilter = LINEAR;  
    MagFilter = LINEAR;
    MipFilter = LINEAR;
};

////////////////////////////////////////////////////////////
// Structures
////////////////////////////////////////////////////////////
struct A2V{
    float4 Position : POSITION;
    float2 TexCoord0 : TEXCOORD0;
};

struct V2P{
    float4 Position  : POSITION;
    float2 TexCoord0 : TEXCOORD0;
    float2 TexCoord1 : TEXCOORD1;
    float4 TexCoord2 : TEXCOORD2;
};

////////////////////////////////////////////////////////////
// Vertex Shaders
////////////////////////////////////////////////////////////
void VS(in A2V IN, out V2P OUT)
{
    //Copy normal map texture coordinates through
    OUT.TexCoord0 = IN.TexCoord0 + float2(TexScroll, 0.0f);
    OUT.TexCoord1 = IN.TexCoord0 + float2(0.0f, TexScroll);
    
    //Projected texture coordinates
    OUT.TexCoord2 = mul(IN.Position, TexTransform);
    blah = 1;
    
    //Transform model-space vertex position to screen space:
    OUT.Position = mul(IN.Position, WorldViewProj);
    V2P ourstruct;
}

////////////////////////////////////////////////////////////
// Pixel Shader
////////////////////////////////////////////////////////////
float4 PS(in V2P IN) : COLOR 
{
    //Uncompress the normal maps
    float3 Water1 = 2.0f * tex2D(Water1TexSampler, IN.TexCoord0).rgb - 1.0f;
    float3 Water2 = 2.0f * tex2D(Water2TexSampler, IN.TexCoord1).rgb - 1.0f;
    
    float3 FinalWater = normalize(Water1 + Water2) * 0.5f;
    if( FinalWater > 1 )
    {
		float x = 0;
	}
    
    //Find the color of the object
    float4 Color = tex2Dproj(ProjTexSampler, IN.TexCoord2 + float4(FinalWater, 0.0f));
    
    //Give the textue a slightly blue tint
    Color.xy -= 0.1f;

    //Return the final color
    return Color;
}

////////////////////////////////////////////////////////////
// Techniques
////////////////////////////////////////////////////////////
technique Water
{
    pass P0
    {
        VertexShader = compile vs_1_1 VS();
        PixelShader = compile ps_2_0 PS();
    }
}
