// Effect uses a scrolling overlay texture to make different parts of
// an image fade in or out at different speeds.


sampler TextureSampler : register(s0);
sampler OverlaySampler : register(s1);


float4 main(float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
    // Look up the texture color.
    float4 tex = tex2D(TextureSampler, texCoord);
    
    // Look up the fade speed from the scrolling overlay texture.
    float masktex = tex2D(OverlaySampler, texCoord).a;
    
	//tex.a = masktex;
    // Apply a combination of the input color alpha and the fade speed.
    return tex;
}


technique Desaturate
{
	
    pass Pass1
    {
		//AlphaTestEnable = true;
        PixelShader = compile ps_2_0 main();
    }
}
