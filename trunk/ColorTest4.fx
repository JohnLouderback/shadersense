//--------------------------------------------------------------------------------------
// This shader computes standard transform and lighting
//--------------------------------------------------------------------------------------
VS_OUTPUT RenderSceneVS( float4 vPos : POSITION, 
                         float3 vNormal : NORMAL,
                         float2 vTexCoord0 : TEXCOORD0,
                         uniform int nNumLights,
                         uniform bool bTexture,
                         uniform bool bAnimate )
{
    VS_OUTPUT Output;
    
    float3 vNormalWorldSpace;
  
    float4 vAnimatedPos = vPos;
    
    // Animation the vertex based on time and the vertex's object space position
    if( bAnimate )
		vAnimatedPos += float4(vNormal, 0) * (sin(g_fTime+5.5)+0.5)*5;
    
    // Transform the position from object space to homogeneous projection space
    Output.Position = mul(vAnimatedPos, g_mWorldViewProjection);