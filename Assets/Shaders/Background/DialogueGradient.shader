Shader "UI/Dialogue Gradient"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _BottomThreshold ("Bottom Threshold", Range(0, 1)) = 0
        _FadeHeight ("Fade Height", Range(0.01, 1)) = 1
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        Cull Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            sampler2D _MainTex;
            float _BottomThreshold;
            float _FadeHeight;
            float4 _ClipRect;

            struct Varyings
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 canvasPosition : TEXCOORD1;
                half alpha : TEXCOORD2;
            };

            Varyings vert(appdata_full input)
            {
                Varyings output;
                output.position = UnityObjectToClipPos(input.vertex);
                output.canvasPosition = input.vertex.xy;
                output.uv = input.texcoord.xy;
                output.alpha = input.color.a;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float fadeStart = min(_BottomThreshold, _FadeHeight - 0.0001);
                half fade = 1.0h - smoothstep(fadeStart, _FadeHeight, input.uv.y);
                half alpha = fade * input.alpha * tex2D(_MainTex, input.uv).a;
                #ifdef UNITY_UI_CLIP_RECT
                    alpha *= UnityGet2DClipping(input.canvasPosition, _ClipRect);
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                    clip(alpha - 0.001h);
                #endif
                return half4(0, 0, 0, alpha);
            }
            ENDCG
        }
    }
}
