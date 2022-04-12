using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MPXUnityObjectChild : MonoBehaviour
{
    public MeshRenderer Ren;
    public Material[] Materials;
    public Color[] DefaultColors;

    private void Start()
    {
   
    }

    public void SetRenderer(Material material)
    {
        Ren = gameObject.AddComponent<MeshRenderer>();
        Materials = new Material[1];
        Materials[0] = material;

        Ren.sharedMaterials = Materials;

        SetDefaultColors();
    }

    public void SetRenderer(Material[] materials)
    {
        Ren = gameObject.AddComponent<MeshRenderer>();
        Materials = materials;

        Ren.sharedMaterials = materials;
        SetDefaultColors();
    }

    void SetDefaultColors()
    {
        if (Materials != null)
        {
            DefaultColors = new Color[Materials.Length];
            for (int i = 0; i < Materials.Length; i++)
            {
                DefaultColors[i] = Materials[i].color;
            }
        }
    }

    public void SetRenderer(MeshRenderer ren)
    {
        if (ren != null)
        {
            Ren = ren;
            Materials = ren.sharedMaterials;
            SetDefaultColors();
        }
    }

}
