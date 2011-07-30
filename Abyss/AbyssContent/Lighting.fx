// Effect applies normalmapped lighting to a 2D sprite.

//float3 LightDirection;
float2 LightPosition;
float4x4 Orthographic;
float4x4 View;
float3 LightColor = 500;
float3 AmbientColor = 0.1;

float2 shadow_vertex1;
float2 shadow_vertex2;
float2 distanceToShadowCaster;

static const float PI = 3.14159265f;

sampler TextureSampler : register(s0);

float cross2D(float2 p1, float2 p2) {
   return p1.x*p2.y - p1.y*p2.x;
}

bool inTriangle(float2 pos, float2 V1, float2 V2, float2 V3)
{
	float2 vect1;
	float2 vect2;
	float2 vect3;

	vect1.x = pos.x - V1.x;
	vect1.y = pos.y - V1.y;

	vect2.x = pos.x - V2.x;
	vect2.y = pos.y - V2.y;

	vect3.x = pos.x - V3.x;
	vect3.y = pos.y - V3.y;


	float Ver1 = cross2D(vect1, vect2);
	float Ver2 = cross2D(vect2, vect3);
	float Ver3 = cross2D(vect3, vect1);
	return (Ver1 >= 0 && Ver2 >= 0 && Ver3 >= 0) || (Ver1 < 0 && Ver2 < 0 && Ver3 < 0);
}

bool inTriangleSlow(float2 pos, float2 V1, float2 V2, float2 V3)
{
	float2 vec1 = V1 - pos;
	float2 vec2 = V2 - pos;
	float2 vec3 = V3 - pos;

	float sum = dot(vec1, vec2) + dot(vec2, vec3) + dot(vec3, vec1);
	if( sum < 2*PI + 0.01 && sum > 2*PI - 0.01 )
		return true;
	else return false;

}


float4 main(float4 color : COLOR0, float2 texCoord : TEXCOORD0, float2 screenPos : TEXCOORD1) : COLOR0
{
    // Look up the texture values.
    float4 tex = tex2D(TextureSampler, texCoord);
	screenPos = (screenPos + 1)/2;
	screenPos = mul(Orthographic, screenPos);
    float lightAmount = min( 1/ ( distance(LightPosition, screenPos)), 0.01  );
	float shadowModifier = 1;

	if(inTriangle(screenPos, shadow_vertex1, shadow_vertex2, LightPosition))
	{
		//float inShadow = max(distance(LightPosition, screenPos) - distanceToShadowCaster, 0);
		//if(inShadow)
			shadowModifier = 0;
	}
    
    color.rgb *= AmbientColor + lightAmount * LightColor;// * shadowModifier;
    
    return tex * color;
}

void SpriteVertexShader(	inout float4 color    : COLOR0,
                            inout float2 texCoord : TEXCOORD0,
                            inout float4 position : POSITION0,
							out float2 screenPos : TEXCOORD1)
    {
		screenPos = position.xy;
    }


technique Normalmap
{
    pass Pass1
    {
		VertexShader = compile vs_3_0 SpriteVertexShader(); 
        PixelShader = compile ps_3_0 main();
	}
}
