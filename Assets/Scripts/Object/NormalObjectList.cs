using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalObjectList : MonoBehaviour
{
    public MPXLibraryNomalObject[] Objects;

    private void Awake()
    {
        CreateObject.Inst.SetObjectList(Objects);
    }
}
