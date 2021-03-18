using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class AssetBundleTools : MonoBehaviour
{
    [MenuItem("Tools/AssetBundle/ChangeDllToBytes")]
    static void ChangeDllToBytesAndSetName()
    {
        AssetDatabase.Refresh();
        string src = string.Format("{0}{1}", Application.streamingAssetsPath, "/HotFix_Project.dll");
        string des = string.Format("{0}{1}", Application.dataPath, "/HotFix/hotfix_project.bytes");
        FileInfo fileSrc = new FileInfo(src);
        FileInfo fileDes = new FileInfo(des);
        if (File.Exists(des))
        {
            fileDes.Delete();
        }
        if (fileSrc.Exists)
        {
            fileSrc.CopyTo(des);
        }
        AssetDatabase.Refresh();
        string importerPath = "Assets" + des.Substring(Application.dataPath.Length);  //获取以Assets开始的路径
        AssetImporter assetImporter = AssetImporter.GetAtPath(importerPath);
        assetImporter.assetBundleName = "hotfix_project.bundle";
    }

    [MenuItem("Tools/AssetBundle/BuildAll")]
    static void BuildAll()
    {
        BuildPipeline.BuildAssetBundles(Application.streamingAssetsPath, BuildAssetBundleOptions.None, BuildTarget.StandaloneOSX);
        AssetDatabase.Refresh();
    }
}
