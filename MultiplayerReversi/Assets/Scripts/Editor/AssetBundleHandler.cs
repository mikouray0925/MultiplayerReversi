using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

public class AssetBundleHandler : MonoBehaviour
{
    
    [MenuItem("Assets/Build Asset Bundles")]
    public static void BuildAllAssetBundles()
    {
        string path = Application.dataPath + "/../AssetBundles";
        try{
            BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
        }
        catch(Exception e){
            Debug.Log(e.Message);
        }
    }

    
}
