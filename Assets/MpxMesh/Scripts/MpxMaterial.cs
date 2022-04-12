using MPXObject;
using System;
using UnityEngine;
using Object = MPXObject.Object;

namespace MpxUnityObject
{
    [Serializable]
    public class MpxMaterial : Object
    {
        public string ShaderName;
        public MPXObject.Color MainColor;
        public MPXObject.Color SpecColor;
        public MPXObject.Color EmiColor;
        public float Glossiness;
        public MpxImage MainTexture;

        public MpxMaterial() { }

        public MpxMaterial(Material mat)
        {
            Name = mat.name;
            if (mat.shader != null)
            {
                ShaderName = mat.shader.name;

                MainColor = ToMpxColor(mat.color);
                SpecColor = ToMpxColor(mat.GetColor("_SpecColor"));
                EmiColor = ToMpxColor(mat.GetColor("_EmissionColor"));
                Glossiness = mat.GetFloat("_Glossiness");
                if (mat.mainTexture != null)
                {
                    Texture2D texture = (Texture2D)mat.mainTexture;
                    MainTexture = new MpxImage(MpxImage.Format.JPG, texture.EncodeToJPG());
                }
            }
        }

        public Material ToMaterial()
        {
            if (!string.IsNullOrEmpty(ShaderName))
            {
                Material mat = new Material(Shader.Find(ShaderName));
                mat.color = ToColor(MainColor);
                mat.SetColor("_SpecColor", ToColor(SpecColor));
                mat.SetColor("_EmissionColor", ToColor(EmiColor));
                mat.SetFloat("_Glossiness", Glossiness);

                if (MainTexture != null)
                {
                    Texture2D texture = new Texture2D(128, 128);
                    texture.LoadImage(MainTexture.Bytes);
                    texture.hideFlags = HideFlags.HideAndDontSave;
                    texture.filterMode = FilterMode.Point;
                    texture.Apply();

                    mat.mainTexture = texture;
                }
                return mat;

            }
            return null;
        }


        public static UnityEngine.Color ToColor(MPXObject.Color color)
        {
            float r = (float)color.Red / MPXObject.Color.MAX_VALUE;
            float g = (float)color.Green / MPXObject.Color.MAX_VALUE;
            float b = (float)color.Blue / MPXObject.Color.MAX_VALUE;
            float a = (float)color.Alpha / MPXObject.Color.MAX_VALUE;
            return new UnityEngine.Color(r, g, b, a);
        }

        public static MPXObject.Color ToMpxColor(UnityEngine.Color color)
        {
            return new MPXObject.Color((int)(color.r * MPXObject.Color.MAX_VALUE), (int)(color.g * MPXObject.Color.MAX_VALUE), (int)(color.b * MPXObject.Color.MAX_VALUE), (int)(color.a * MPXObject.Color.MAX_VALUE));
        }
    }
}