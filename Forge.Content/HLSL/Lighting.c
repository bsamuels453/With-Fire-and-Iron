float4x4 mtx_World;
float4x4 mtx_View;
float4x4 mtx_Projection;

float4 f4_AmbientColor;
float3 f3_EyePosition;



struct LightingAttribs{
	float Ka; //ambient reflective
	float Kd; //diffuse reflective
	float Ks; //specular reflective
	float A;  //shininess
};

float4 calcPhong(LightingAttribs attribs, float4 lColor, float3 nVec, float3 lVec, float3 vVec, float3 r){
    float4 Ia = attribs.Ka;
    float4 Id = attribs.Kd * saturate( dot(nVec,lVec) );
    float4 Is = attribs.Ks * pow( saturate(dot(r,vVec)), attribs.A );
 
    return Ia + (Id + Is) * lColor;
}

float4 localToWorld(float4 vec){
	return mul(vec, mtx_World);
}

float4 worldToView(float4 vec){
	return mul(vec, mtx_View);
}

float4 viewToProjection(float4 vec){
	return mul(vec, mtx_Projection);
}