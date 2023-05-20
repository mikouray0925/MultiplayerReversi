using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
public class MaterialHolder : MonoBehaviour
{
    MaterialHolder instance;
    public string BundleURL;
    public string AssetName;
    public int version;

    void Awake()
    {
        instance = this;
        StartCoroutine(DownloadMaterials(AssetName, BundleURL, version));
    }
    public Dictionary<string, Material> materials = new Dictionary<string, Material>();

    public bool LoadMaterialsFromAssetBundle(AssetBundle assetBundle){
        if(assetBundle != null){
            Material[] mats = assetBundle.LoadAllAssets<Material>();
            foreach(Material mat in mats){
                materials.Add(mat.name, mat);
            }
            assetBundle.Unload(true);
            foreach(KeyValuePair<string, Material> mat in materials){
                Debug.Log(mat.Key);
            }
            return true;
        }
        return false;
    }
    public IEnumerator DownloadMaterials(string asset, string url, int version){
#if UNITY_EDITOR
        string path = Application.dataPath + "/../AssetBundles/";
        AssetBundle assetBundle = AssetBundle.LoadFromFile(path + "AssetBundles");
        AssetBundleManifest manifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        string[] dependencies = manifest.GetAllDependencies("AssetBundles"); //Pass the name of the bundle you want the dependencies for.
        foreach(string dependency in dependencies){
            AssetBundle bundle = AssetBundle.LoadFromFile(path + dependency);
            LoadMaterialsFromAssetBundle(bundle);
        }
        yield return null;
#else
        // Wait for the Caching system to be ready
        while (!Caching.ready)
            yield return null;

        // Start the download
        using(UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(url, (uint)version, 0)){
            yield return www;
            if (www.error != null)
                throw new System.Exception("WWW download:" + www.error);
            LoadMaterialsFromAssetBundle(DownloadHandlerAssetBundle.GetContent(www));

        } // memory is freed from the web stream (www.Dispose() gets called implicitly)
#endif
    }
}
