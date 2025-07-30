Shader "Custom/EdgeDetectionShader"
{
    Properties
    {
        _EdgeColor ("Edge Color", Color) = (1, 0, 0, 1)
        _EdgeWidth ("Edge Width", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }

        Pass
        {
            Name "EdgePass"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 4.5
            #pragma multi_compile_fog
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // Properties
            float _EdgeWidth;
            float4 _EdgeColor;

            // Vertex Shader
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : POSITION;
                float3 normal : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex); // Standard vertex transformation to clip space
                o.normal = v.normal;
                return o;
            }

            // Geometry Shader
            [maxvertexcount(2)]
            void geom(triangle v2f input[3], inout TriangleStream<v2f> triStream)
            {
                // Loop through each edge of the triangle (i.e., edges 0-1, 1-2, 2-0)
                for (int i = 0; i < 3; i++)
                {
                    int j = (i + 1) % 3;

                    // Create an edge from vertices input[i] to input[j]
                    v2f edge[2];
                    edge[0] = input[i];
                    edge[1] = input[j];

                    // Output this edge as a line
                    triStream.Append(edge[0]);
                    triStream.Append(edge[1]);
                }

                triStream.RestartStrip();
            }

            // Fragment Shader
            half4 frag(v2f i) : SV_Target
            {
                return _EdgeColor; // Set edge color
            }
            ENDHLSL
        }
    }

    Fallback "Diffuse"
}
