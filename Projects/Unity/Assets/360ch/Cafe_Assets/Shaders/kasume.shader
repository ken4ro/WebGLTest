// Shader created with Shader Forge v1.38 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:0,bdst:1,dpts:2,wrdp:True,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:False,qofs:0,qpre:2,rntp:3,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:9361,x:33620,y:32737,varname:node_9361,prsc:2|normal-7095-RGB,emission-2052-OUT,clip-851-A,olwid-8203-OUT,olcol-3682-OUT;n:type:ShaderForge.SFN_LightAttenuation,id:8068,x:32299,y:33042,varname:node_8068,prsc:2;n:type:ShaderForge.SFN_LightVector,id:6869,x:30964,y:32965,varname:node_6869,prsc:2;n:type:ShaderForge.SFN_NormalVector,id:9684,x:31002,y:33126,prsc:2,pt:True;n:type:ShaderForge.SFN_HalfVector,id:9471,x:31026,y:33271,varname:node_9471,prsc:2;n:type:ShaderForge.SFN_Dot,id:7782,x:31319,y:33010,cmnt:Lambert,varname:node_7782,prsc:2,dt:1|A-6869-OUT,B-9684-OUT;n:type:ShaderForge.SFN_Dot,id:3269,x:31319,y:33184,cmnt:Blinn-Phong,varname:node_3269,prsc:2,dt:1|A-9684-OUT,B-9471-OUT;n:type:ShaderForge.SFN_Multiply,id:2746,x:32329,y:32882,cmnt:Specular Contribution,varname:node_2746,prsc:2|A-7209-OUT,B-5750-OUT,C-4865-RGB;n:type:ShaderForge.SFN_Tex2d,id:851,x:31155,y:31494,ptovrint:False,ptlb:1st_col0,ptin:_1st_col0,varname:node_851,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:66321cc856b03e245ac41ed8a53e0ecc,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Color,id:5927,x:31829,y:31478,ptovrint:False,ptlb:world_col,ptin:_world_col,varname:node_5927,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Exp,id:1700,x:31962,y:33293,varname:node_1700,prsc:2,et:1|IN-9978-OUT;n:type:ShaderForge.SFN_Slider,id:5328,x:31273,y:33427,ptovrint:False,ptlb:Gloss,ptin:_Gloss,varname:node_5328,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.3172894,max:1;n:type:ShaderForge.SFN_Power,id:5267,x:31691,y:33013,varname:node_5267,prsc:2|VAL-3269-OUT,EXP-1700-OUT;n:type:ShaderForge.SFN_Add,id:2159,x:32520,y:32710,cmnt:Combine,varname:node_2159,prsc:2|A-7028-OUT,B-2746-OUT;n:type:ShaderForge.SFN_ConstantLerp,id:9978,x:31722,y:33359,varname:node_9978,prsc:2,a:1,b:11|IN-5328-OUT;n:type:ShaderForge.SFN_Color,id:4865,x:32064,y:33062,ptovrint:False,ptlb:Spec Color,ptin:_SpecColor,varname:node_4865,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.3235294,c2:0.3235294,c3:0.3235294,c4:1;n:type:ShaderForge.SFN_Multiply,id:544,x:32140,y:32136,cmnt:Diffuse Color,varname:node_544,prsc:2|A-7915-OUT,B-5927-RGB;n:type:ShaderForge.SFN_Posterize,id:7209,x:31975,y:32726,varname:node_7209,prsc:2|IN-7782-OUT,STPS-3213-OUT;n:type:ShaderForge.SFN_Vector1,id:3213,x:31714,y:32922,varname:node_3213,prsc:2,v1:2;n:type:ShaderForge.SFN_Posterize,id:5750,x:31934,y:32941,varname:node_5750,prsc:2|IN-5267-OUT,STPS-3213-OUT;n:type:ShaderForge.SFN_Lerp,id:7028,x:32275,y:32519,varname:node_7028,prsc:2|A-8205-OUT,B-544-OUT,T-7209-OUT;n:type:ShaderForge.SFN_Tex2d,id:547,x:31192,y:32275,ptovrint:False,ptlb:2nd_col0,ptin:_2nd_col0,varname:node_547,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,tex:b66bceaf0cc0ace4e9bdc92f14bba709,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:8205,x:32032,y:32454,varname:node_8205,prsc:2|A-708-OUT,B-5927-RGB;n:type:ShaderForge.SFN_Multiply,id:9698,x:32809,y:33326,varname:node_9698,prsc:2|A-2159-OUT,B-7275-OUT;n:type:ShaderForge.SFN_Color,id:1126,x:32202,y:33348,ptovrint:False,ptlb:shadow_col,ptin:_shadow_col,varname:node_1126,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Lerp,id:4436,x:32885,y:32883,varname:node_4436,prsc:2|A-9698-OUT,B-2159-OUT,T-3962-OUT;n:type:ShaderForge.SFN_Slider,id:3242,x:32003,y:33525,ptovrint:False,ptlb:line_size,ptin:_line_size,varname:node_3242,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.02610557,max:0.1;n:type:ShaderForge.SFN_Multiply,id:8203,x:32443,y:33433,varname:node_8203,prsc:2|A-3242-OUT,B-780-R;n:type:ShaderForge.SFN_Tex2d,id:780,x:31912,y:33630,ptovrint:False,ptlb:line_size_tex,ptin:_line_size_tex,varname:node_780,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:3682,x:32724,y:33541,varname:node_3682,prsc:2|A-7915-OUT,B-9745-RGB;n:type:ShaderForge.SFN_Color,id:9745,x:32211,y:33691,ptovrint:False,ptlb:outline_col,ptin:_outline_col,varname:node_9745,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:0.5,c2:0.5,c3:0.5,c4:1;n:type:ShaderForge.SFN_Lerp,id:368,x:31391,y:31743,varname:node_368,prsc:2|A-6021-RGB,B-851-RGB,T-4048-OUT;n:type:ShaderForge.SFN_Slider,id:4048,x:30775,y:31843,ptovrint:False,ptlb:tex_change01,ptin:_tex_change01,varname:node_4048,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_Tex2d,id:6021,x:31155,y:31662,ptovrint:False,ptlb:1st_col1,ptin:_1st_col1,varname:node_6021,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:9534,x:31155,y:31831,ptovrint:False,ptlb:1st_col2,ptin:_1st_col2,varname:node_9534,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:7672,x:31155,y:32003,ptovrint:False,ptlb:1st_col3,ptin:_1st_col3,varname:node_7672,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Lerp,id:4462,x:31455,y:31903,varname:node_4462,prsc:2|A-9534-RGB,B-368-OUT,T-5837-OUT;n:type:ShaderForge.SFN_Lerp,id:7915,x:31573,y:32054,varname:node_7915,prsc:2|A-7672-RGB,B-4462-OUT,T-9386-OUT;n:type:ShaderForge.SFN_Slider,id:5837,x:30775,y:31982,ptovrint:False,ptlb:tex_change02,ptin:_tex_change02,varname:node_5837,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_Slider,id:9386,x:30773,y:32137,ptovrint:False,ptlb:tex_change03,ptin:_tex_change03,varname:node_9386,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1,max:1;n:type:ShaderForge.SFN_Tex2d,id:6901,x:31192,y:32439,ptovrint:False,ptlb:2nd_col1,ptin:_2nd_col1,varname:node_6901,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:9502,x:31192,y:32607,ptovrint:False,ptlb:2nd_col2,ptin:_2nd_col2,varname:node_9502,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:2426,x:31192,y:32777,ptovrint:False,ptlb:2nd_col3,ptin:_2nd_col3,varname:node_2426,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Lerp,id:4730,x:31552,y:32416,varname:node_4730,prsc:2|A-6901-RGB,B-547-RGB,T-4048-OUT;n:type:ShaderForge.SFN_Lerp,id:3895,x:31552,y:32540,varname:node_3895,prsc:2|A-9502-RGB,B-4730-OUT,T-5837-OUT;n:type:ShaderForge.SFN_Lerp,id:708,x:31552,y:32664,varname:node_708,prsc:2|A-2426-RGB,B-3895-OUT,T-9386-OUT;n:type:ShaderForge.SFN_Fresnel,id:8357,x:32837,y:31927,varname:node_8357,prsc:2|EXP-6727-OUT;n:type:ShaderForge.SFN_Slider,id:6727,x:32362,y:31872,ptovrint:False,ptlb:rim_size,ptin:_rim_size,varname:node_6727,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:1.122702,max:10;n:type:ShaderForge.SFN_Posterize,id:5861,x:32767,y:32164,varname:node_5861,prsc:2|IN-8357-OUT,STPS-6166-OUT;n:type:ShaderForge.SFN_Add,id:8050,x:33064,y:32578,varname:node_8050,prsc:2|A-1963-OUT,B-4436-OUT;n:type:ShaderForge.SFN_Slider,id:5658,x:32633,y:32637,ptovrint:False,ptlb:rim_pow,ptin:_rim_pow,varname:node_5658,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:0,cur:0.7777778,max:1;n:type:ShaderForge.SFN_Lerp,id:2052,x:33260,y:32721,varname:node_2052,prsc:2|A-8050-OUT,B-4436-OUT,T-5658-OUT;n:type:ShaderForge.SFN_Slider,id:6166,x:32290,y:32093,ptovrint:False,ptlb:rim_posterize,ptin:_rim_posterize,varname:node_6166,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,min:2,cur:2,max:100;n:type:ShaderForge.SFN_Tex2d,id:7095,x:33230,y:32149,ptovrint:False,ptlb:nrm,ptin:_nrm,varname:node_7095,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:3,isnm:True;n:type:ShaderForge.SFN_Multiply,id:1963,x:33007,y:32352,varname:node_1963,prsc:2|A-5861-OUT,B-6662-RGB;n:type:ShaderForge.SFN_Tex2d,id:6662,x:32618,y:32348,ptovrint:False,ptlb:rimtex,ptin:_rimtex,varname:node_6662,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:897,x:32374,y:33160,ptovrint:False,ptlb:shadow_Msk,ptin:_shadow_Msk,varname:node_897,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Multiply,id:3962,x:32549,y:32964,varname:node_3962,prsc:2|A-8068-OUT,B-8068-OUT;n:type:ShaderForge.SFN_Add,id:7275,x:32592,y:33255,varname:node_7275,prsc:2|A-897-RGB,B-1126-RGB;proporder:5927-851-6021-9534-7672-547-6901-9502-2426-4048-5837-9386-5328-4865-1126-3242-780-9745-6727-5658-6166-7095-6662-897;pass:END;sub:END;*/

Shader "Shader Forge/kasume" {
    Properties {
        _world_col ("world_col", Color) = (0.5,0.5,0.5,1)
        _1st_col0 ("1st_col0", 2D) = "white" {}
        _1st_col1 ("1st_col1", 2D) = "white" {}
        _1st_col2 ("1st_col2", 2D) = "white" {}
        _1st_col3 ("1st_col3", 2D) = "white" {}
        _2nd_col0 ("2nd_col0", 2D) = "white" {}
        _2nd_col1 ("2nd_col1", 2D) = "white" {}
        _2nd_col2 ("2nd_col2", 2D) = "white" {}
        _2nd_col3 ("2nd_col3", 2D) = "white" {}
        _tex_change01 ("tex_change01", Range(0, 1)) = 1
        _tex_change02 ("tex_change02", Range(0, 1)) = 1
        _tex_change03 ("tex_change03", Range(0, 1)) = 1
        _Gloss ("Gloss", Range(0, 1)) = 0.3172894
        _SpecColor ("Spec Color", Color) = (0.3235294,0.3235294,0.3235294,1)
        _shadow_col ("shadow_col", Color) = (0.5,0.5,0.5,1)
        _line_size ("line_size", Range(0, 0.1)) = 0.02610557
        _line_size_tex ("line_size_tex", 2D) = "white" {}
        _outline_col ("outline_col", Color) = (0.5,0.5,0.5,1)
        _rim_size ("rim_size", Range(0, 10)) = 1.122702
        _rim_pow ("rim_pow", Range(0, 1)) = 0.7777778
        _rim_posterize ("rim_posterize", Range(2, 100)) = 2
        _nrm ("nrm", 2D) = "bump" {}
        _rimtex ("rimtex", 2D) = "white" {}
        _shadow_Msk ("shadow_Msk", 2D) = "white" {}
        [HideInInspector]_Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    }
    SubShader {
        Tags {
            "Queue"="AlphaTest"
            "RenderType"="TransparentCutout"
        }
        Pass {
            Name "Outline"
            Tags {
            }
            Cull Front
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal d3d11_9x 
            #pragma target 3.0
            uniform sampler2D _1st_col0; uniform float4 _1st_col0_ST;
            uniform float _line_size;
            uniform sampler2D _line_size_tex; uniform float4 _line_size_tex_ST;
            uniform float4 _outline_col;
            uniform float _tex_change01;
            uniform sampler2D _1st_col1; uniform float4 _1st_col1_ST;
            uniform sampler2D _1st_col2; uniform float4 _1st_col2_ST;
            uniform sampler2D _1st_col3; uniform float4 _1st_col3_ST;
            uniform float _tex_change02;
            uniform float _tex_change03;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                UNITY_FOG_COORDS(1)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                float4 _line_size_tex_var = tex2Dlod(_line_size_tex,float4(TRANSFORM_TEX(o.uv0, _line_size_tex),0.0,0));
                o.pos = UnityObjectToClipPos( float4(v.vertex.xyz + v.normal*(_line_size*_line_size_tex_var.r),1) );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                float4 _1st_col0_var = tex2D(_1st_col0,TRANSFORM_TEX(i.uv0, _1st_col0));
                clip(_1st_col0_var.a - 0.5);
                float4 _1st_col3_var = tex2D(_1st_col3,TRANSFORM_TEX(i.uv0, _1st_col3));
                float4 _1st_col2_var = tex2D(_1st_col2,TRANSFORM_TEX(i.uv0, _1st_col2));
                float4 _1st_col1_var = tex2D(_1st_col1,TRANSFORM_TEX(i.uv0, _1st_col1));
                float3 node_7915 = lerp(_1st_col3_var.rgb,lerp(_1st_col2_var.rgb,lerp(_1st_col1_var.rgb,_1st_col0_var.rgb,_tex_change01),_tex_change02),_tex_change03);
                return fixed4((node_7915*_outline_col.rgb),0);
            }
            ENDCG
        }
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal d3d11_9x 
            #pragma target 3.0
            uniform sampler2D _1st_col0; uniform float4 _1st_col0_ST;
            uniform float4 _world_col;
            uniform float _Gloss;
            uniform sampler2D _2nd_col0; uniform float4 _2nd_col0_ST;
            uniform float4 _shadow_col;
            uniform float _tex_change01;
            uniform sampler2D _1st_col1; uniform float4 _1st_col1_ST;
            uniform sampler2D _1st_col2; uniform float4 _1st_col2_ST;
            uniform sampler2D _1st_col3; uniform float4 _1st_col3_ST;
            uniform float _tex_change02;
            uniform float _tex_change03;
            uniform sampler2D _2nd_col1; uniform float4 _2nd_col1_ST;
            uniform sampler2D _2nd_col2; uniform float4 _2nd_col2_ST;
            uniform sampler2D _2nd_col3; uniform float4 _2nd_col3_ST;
            uniform float _rim_size;
            uniform float _rim_pow;
            uniform float _rim_posterize;
            uniform sampler2D _nrm; uniform float4 _nrm_ST;
            uniform sampler2D _rimtex; uniform float4 _rimtex_ST;
            uniform sampler2D _shadow_Msk; uniform float4 _shadow_Msk_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 bitangentDir : TEXCOORD4;
                LIGHTING_COORDS(5,6)
                UNITY_FOG_COORDS(7)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 _nrm_var = UnpackNormal(tex2D(_nrm,TRANSFORM_TEX(i.uv0, _nrm)));
                float3 normalLocal = _nrm_var.rgb;
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float4 _1st_col0_var = tex2D(_1st_col0,TRANSFORM_TEX(i.uv0, _1st_col0));
                clip(_1st_col0_var.a - 0.5);
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
////// Emissive:
                float4 _rimtex_var = tex2D(_rimtex,TRANSFORM_TEX(i.uv0, _rimtex));
                float4 _2nd_col3_var = tex2D(_2nd_col3,TRANSFORM_TEX(i.uv0, _2nd_col3));
                float4 _2nd_col2_var = tex2D(_2nd_col2,TRANSFORM_TEX(i.uv0, _2nd_col2));
                float4 _2nd_col1_var = tex2D(_2nd_col1,TRANSFORM_TEX(i.uv0, _2nd_col1));
                float4 _2nd_col0_var = tex2D(_2nd_col0,TRANSFORM_TEX(i.uv0, _2nd_col0));
                float4 _1st_col3_var = tex2D(_1st_col3,TRANSFORM_TEX(i.uv0, _1st_col3));
                float4 _1st_col2_var = tex2D(_1st_col2,TRANSFORM_TEX(i.uv0, _1st_col2));
                float4 _1st_col1_var = tex2D(_1st_col1,TRANSFORM_TEX(i.uv0, _1st_col1));
                float3 node_7915 = lerp(_1st_col3_var.rgb,lerp(_1st_col2_var.rgb,lerp(_1st_col1_var.rgb,_1st_col0_var.rgb,_tex_change01),_tex_change02),_tex_change03);
                float node_3213 = 2.0;
                float node_7209 = floor(max(0,dot(lightDirection,normalDirection)) * node_3213) / (node_3213 - 1);
                float3 node_2159 = (lerp((lerp(_2nd_col3_var.rgb,lerp(_2nd_col2_var.rgb,lerp(_2nd_col1_var.rgb,_2nd_col0_var.rgb,_tex_change01),_tex_change02),_tex_change03)*_world_col.rgb),(node_7915*_world_col.rgb),node_7209)+(node_7209*floor(pow(max(0,dot(normalDirection,halfDirection)),exp2(lerp(1,11,_Gloss))) * node_3213) / (node_3213 - 1)*_SpecColor.rgb)); // Combine
                float4 _shadow_Msk_var = tex2D(_shadow_Msk,TRANSFORM_TEX(i.uv0, _shadow_Msk));
                float3 node_4436 = lerp((node_2159*(_shadow_Msk_var.rgb+_shadow_col.rgb)),node_2159,(attenuation*attenuation));
                float3 emissive = lerp(((floor(pow(1.0-max(0,dot(normalDirection, viewDirection)),_rim_size) * _rim_posterize) / (_rim_posterize - 1)*_rimtex_var.rgb)+node_4436),node_4436,_rim_pow);
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "FORWARD_DELTA"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One
            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDADD
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal d3d11_9x 
            #pragma target 3.0
            uniform sampler2D _1st_col0; uniform float4 _1st_col0_ST;
            uniform float4 _world_col;
            uniform float _Gloss;
            uniform sampler2D _2nd_col0; uniform float4 _2nd_col0_ST;
            uniform float4 _shadow_col;
            uniform float _tex_change01;
            uniform sampler2D _1st_col1; uniform float4 _1st_col1_ST;
            uniform sampler2D _1st_col2; uniform float4 _1st_col2_ST;
            uniform sampler2D _1st_col3; uniform float4 _1st_col3_ST;
            uniform float _tex_change02;
            uniform float _tex_change03;
            uniform sampler2D _2nd_col1; uniform float4 _2nd_col1_ST;
            uniform sampler2D _2nd_col2; uniform float4 _2nd_col2_ST;
            uniform sampler2D _2nd_col3; uniform float4 _2nd_col3_ST;
            uniform float _rim_size;
            uniform float _rim_pow;
            uniform float _rim_posterize;
            uniform sampler2D _nrm; uniform float4 _nrm_ST;
            uniform sampler2D _rimtex; uniform float4 _rimtex_ST;
            uniform sampler2D _shadow_Msk; uniform float4 _shadow_Msk_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float3 normalDir : TEXCOORD2;
                float3 tangentDir : TEXCOORD3;
                float3 bitangentDir : TEXCOORD4;
                LIGHTING_COORDS(5,6)
                UNITY_FOG_COORDS(7)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.tangentDir = normalize( mul( unity_ObjectToWorld, float4( v.tangent.xyz, 0.0 ) ).xyz );
                o.bitangentDir = normalize(cross(o.normalDir, o.tangentDir) * v.tangent.w);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos( v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
                float3x3 tangentTransform = float3x3( i.tangentDir, i.bitangentDir, i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 _nrm_var = UnpackNormal(tex2D(_nrm,TRANSFORM_TEX(i.uv0, _nrm)));
                float3 normalLocal = _nrm_var.rgb;
                float3 normalDirection = normalize(mul( normalLocal, tangentTransform )); // Perturbed normals
                float4 _1st_col0_var = tex2D(_1st_col0,TRANSFORM_TEX(i.uv0, _1st_col0));
                clip(_1st_col0_var.a - 0.5);
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 finalColor = 0;
                fixed4 finalRGBA = fixed4(finalColor * 1,0);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
        Pass {
            Name "ShadowCaster"
            Tags {
                "LightMode"="ShadowCaster"
            }
            Offset 1, 1
            Cull Back
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_SHADOWCASTER
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_fog
            #pragma only_renderers d3d9 d3d11 glcore gles gles3 metal d3d11_9x 
            #pragma target 3.0
            uniform sampler2D _1st_col0; uniform float4 _1st_col0_ST;
            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                V2F_SHADOW_CASTER;
                float2 uv0 : TEXCOORD1;
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos( v.vertex );
                TRANSFER_SHADOW_CASTER(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                float4 _1st_col0_var = tex2D(_1st_col0,TRANSFORM_TEX(i.uv0, _1st_col0));
                clip(_1st_col0_var.a - 0.5);
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
