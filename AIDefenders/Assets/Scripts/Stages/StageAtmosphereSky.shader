Shader "AIDefenders/AtmosphereSky"
{
    Properties { _Top("Zenith",Color)=(.03,.06,.16,1) _Horizon("Horizon",Color)=(.3,.4,.5,1) _Stars("Stars",Float)=0 _Moon("Moon",Float)=0 }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            float4 _Top,_Horizon; float _Stars,_Moon;
            struct v2f { float4 pos:SV_POSITION; float3 dir:TEXCOORD0; };
            v2f vert(float4 v:POSITION){v2f o;o.pos=UnityObjectToClipPos(v);o.dir=v.xyz;return o;}
            half4 frag(v2f i):SV_Target
            {
                float3 d=normalize(i.dir);float h=saturate(d.y);
                float3 col=lerp(_Horizon.rgb,_Top.rgb,pow(h,.48));
                float hash=frac(sin(dot(floor(d*900),float3(12.9898,78.233,45.164)))*43758.5453);
                col+=_Stars*step(.9988,hash)*smoothstep(.12,.55,h)*.6;
                float moon=smoothstep(.9992,.9997,dot(d,normalize(float3(-.45,.55,.7))));
                col+=_Moon*moon*float3(.72,.81,1);
                return half4(col,1);
            }
            ENDHLSL
        }
    }
}
