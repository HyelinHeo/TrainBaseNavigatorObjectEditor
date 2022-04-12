using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using MPXObject;
using MPXFile;
using MPXRemote.Message;

namespace MpxUnityObject
{ 
    public class MUOExporter : Singleton<MUOExporter>
    {
        public string error;

        public void Save(MPXUnityObject go, string filePath)
        {
            MpxUnityObjectFile file = new MpxUnityObjectFile(go);

            if (MPXFileManager.Save(file, filePath, ref error))
            {
                Debug.LogFormat("save ok: {0}", filePath);
            }
            else
            {
                Debug.LogErrorFormat("save error: {0}", filePath);
            }
        }
    }
}
