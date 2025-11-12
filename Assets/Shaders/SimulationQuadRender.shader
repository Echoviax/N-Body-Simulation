Shader "Custom/SimulationQuadRender"
{
    Properties
    {
        _Size ("Size", Float) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geo
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct Particle
            {
                float3 position;
                float3 velocity;
                float mass;
                float4 color;
            };

            StructuredBuffer<Particle> _ParticleBuffer;
            float _Size;

            struct appdata 
            {
                uint vertexID : SV_VertexID;
            };

            struct v2g
            {
                float3 pos: TEXCOORD0;
                float4 color : COLOR0;
            };

            v2g vert (appdata v)
            {
                v2g o;
                Particle p = _ParticleBuffer[v.vertexID];
                o.pos = p.position;
                o.color = p.color;
                return o;
            }

            struct g2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR0;
            };

            [maxvertexcount(4)]
            void geo(point v2g p[1], inout TriangleStream<g2f> triStream)
            {
                float3 pos = p[0].pos; 
                float4 color = p[0].color;
                
                float3 camRight = UNITY_MATRIX_V[0].xyz * _Size;
                float3 camUp = UNITY_MATRIX_V[1].xyz * _Size;

                g2f o;

                o.color = color;

                o.vertex = UnityObjectToClipPos(pos - camRight - camUp);
                triStream.Append(o);

                o.vertex = UnityObjectToClipPos(pos - camRight + camUp);
                triStream.Append(o);

                o.vertex = UnityObjectToClipPos(pos + camRight - camUp);
                triStream.Append(o);

                o.vertex = UnityObjectToClipPos(pos + camRight + camUp);
                triStream.Append(o);
            }

            fixed4 frag (g2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}
