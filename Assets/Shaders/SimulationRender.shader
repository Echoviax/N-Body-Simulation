Shader "Custom/SimulationRender"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,0,1) // Yellow
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct Particle
            {
                float3 position;
                float3 velocity;
                float mass;
            };

            StructuredBuffer<Particle> _ParticleBuffer;
            float4 _Color;

            struct appdata 
            {
                uint vertexID : SV_VertexID;
            };

            struct v2f 
            {
                float4 vertex : SV_POSITION; // Position on screen
            };

            v2f vert (appdata v)
            {
                v2f o;
                
                float3 pos = _ParticleBuffer[v.vertexID].position;
                o.vertex = UnityObjectToClipPos(pos);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
}
