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

    [MenuItem("Tools/List Shaders")]
    static void ListEditorShaders()
    {
    var shaders = new List<Shader>();
    var mats = Resources.FindObjectsOfTypeAll<Material>();
    foreach (var mat in mats)
    {
        if (!shaders.Contains(mat.shader) && mat.shader.name.IndexOf("Hidden/") != 0)
        {
            shaders.Add(mat.shader);
        }
    }
    foreach (var shader in shaders)
    {
        Debug.Log(shader.name);
    }
    }
}
