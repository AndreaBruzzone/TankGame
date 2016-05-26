float4x4 World;
float4x4 View;
float4x4 Projection;
texture terrainTexture1;
texture terrainTexture2;
texture terrainTexture3;

float3 lightDirection;
float4 lightColor;
float lightBrightness;

float4 ambientLightColor;
float ambientLightLevel;

float maxElevation;
float trans1 = 0.50;
float trans2 = 0.75;

sampler2D textureSampler = sampler_state {
	Texture = (terrainTexture1);
	AddressU = Wrap;
	AddressV = Wrap;
};
sampler2D textureSamplerMid = sampler_state {
	Texture = (terrainTexture2);
	AddressU = Wrap;
	AddressV = Wrap;
};
sampler2D textureSamplerHigh = sampler_state {
	Texture = (terrainTexture3);
	AddressU = Wrap;
	AddressV = Wrap;
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float2 TextureCoordinate : TEXCOORD0;
	float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
	float4 Position : POSITION0;
	float2 TextureCoordinate : TEXCOORD0;
	float4 LightingColor : COLOR0;
	float Elevation : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.TextureCoordinate = input.TextureCoordinate;
	float4 normal = normalize(mul(input.Normal, World));
	float lightLevel = dot(normal, lightDirection);
	output.LightingColor = saturate(lightColor * lightBrightness * lightLevel);
	output.Elevation = input.Position.y;
	return output;
}
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float elevation = input.Elevation / maxElevation;
float4 lowColor = tex2D(
	textureSampler, input.TextureCoordinate);
float4 midColor = tex2D(
	textureSamplerMid, input.TextureCoordinate);
float4 highColor = tex2D(
	textureSamplerHigh, input.TextureCoordinate);
float4 pixelColor = lowColor;
if ((elevation >= trans1 - 0.05) && (elevation <= trans1 +
	0.05))
{
	float transWeight = ((trans1 + 0.05) - elevation) / 0.10;
	pixelColor = lowColor * transWeight;
	pixelColor += midColor * (1 - transWeight);
}
if ((elevation > trans1 + 0.05) && (elevation <= trans2 -
	0.05))
{
	pixelColor = midColor;
}
if ((elevation > trans2 - 0.05) && (elevation <= trans2 +
	0.05))
{
	float transWeight = ((trans2 + 0.05) - elevation) / 0.10;
	pixelColor = midColor * transWeight;
	pixelColor += highColor * (1 - transWeight);
}
if (elevation > trans2 + 0.05)
pixelColor = highColor;
pixelColor *= input.LightingColor;
pixelColor += (ambientLightColor * ambientLightLevel);
pixelColor.a = 1.0;
return pixelColor;
}
technique Technique1
{
	pass Pass1
	{
		// TODO: set renderstates here.
		VertexShader = compile vs_4_0 VertexShaderFunction();
		PixelShader = compile ps_4_0 PixelShaderFunction();
	}
}
