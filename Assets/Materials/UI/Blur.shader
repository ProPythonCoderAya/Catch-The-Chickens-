Shader "UI/DepthBlur"
{
    Properties
    {
        _Blur ("Blur", Float) = 10.0
        _FarPlane ("Far Plane", Float) = 10.0
        _NearPlane ("Near Plane", Float) = 1.0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        GrabPass
        {
            "_BackgroundTex"
        }

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD1;
                fixed4 color : COLOR;
            };

            sampler2D _BackgroundTex;
            sampler2D _CameraDepthTexture;

            float _Blur;
            float _FarPlane;
            float _NearPlane;

            v2f vert(appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.screenPos = ComputeGrabScreenPos(o.vertex);
                o.color = v.color;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // --------------------------------------------------------
                // Screen UV
                // --------------------------------------------------------

                float2 uv = i.screenPos.xy / i.screenPos.w;

                // --------------------------------------------------------
                // Read scene depth
                // --------------------------------------------------------

                float rawDepth = SAMPLE_DEPTH_TEXTURE(
                    _CameraDepthTexture,
                    float2(uv.x, 1.0f-uv.y)
                );

                // Convert the sampled depth to normalized eye-space distance.
                // This remains correct regardless of the platform's depth-buffer direction.
                float distance = (LinearEyeDepth(rawDepth) - _NearPlane) / (_FarPlane - _NearPlane);

                // Increase blur progressively with distance from the camera.
                float blur = saturate(distance) * _Blur;
                
                // Convert pixels into UV coordinates.
                float2 texel = blur / _ScreenParams.xy;

                // --------------------------------------------------------
                // 9-sample blur
                // --------------------------------------------------------

                fixed4 col = 0;

                // Center
                col += tex2D(
                    _BackgroundTex,
                    uv
                );

                // Horizontal
                col += tex2D(
                    _BackgroundTex,
                    uv + float2(texel.x, 0)
                );

                col += tex2D(
                    _BackgroundTex,
                    uv + float2(-texel.x, 0)
                );

                // Vertical
                col += tex2D(
                    _BackgroundTex,
                    uv + float2(0, texel.y)
                );

                col += tex2D(
                    _BackgroundTex,
                    uv + float2(0, -texel.y)
                );

                // Diagonal
                col += tex2D(
                    _BackgroundTex,
                    uv + float2(texel.x, texel.y)
                );

                col += tex2D(
                    _BackgroundTex,
                    uv + float2(-texel.x, texel.y)
                );

                col += tex2D(
                    _BackgroundTex,
                    uv + float2(texel.x, -texel.y)
                );

                col += tex2D(
                    _BackgroundTex,
                    uv + float2(-texel.x, -texel.y)
                );

                // Average all nine samples.
                col /= 9.0;

                // Preserve UI alpha.
                col.a = i.color.a;

                return col;
            }

            ENDCG
        }
    }
}