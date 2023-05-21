using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using System.Text;
using UnityEngine.UI;
public class MaterialHolder : MonoBehaviour
{
    public static MaterialHolder instance;
    public string BundleURL;
    public string AssetName;
    public int version;
    void Awake()
    {
        instance = this;
        StartCoroutine(DownloadMaterials(AssetName, BundleURL, version));
    }
    public Dictionary<string, GameObject> Prefabs = new Dictionary<string, GameObject>();
    public Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();
    public List<string> Names = new List<string>();
    public int MaterialCount;
    public bool LoadFromAssetBundle(AssetBundle assetBundle){
        if(assetBundle != null){
            // Load all prefabs
            Prefabs.Clear();
            GameObject[] prefab = assetBundle.LoadAllAssets<GameObject>();
            foreach(GameObject p in prefab){
                Prefabs.Add(p.name, p);
                Debug.Log("Load prefab " + p.name + " successfully");
                Names.Add(p.name);
            }
            // Load all textures
            Sprites.Clear();
            Texture2D[] texture = assetBundle.LoadAllAssets<Texture2D>();
            foreach(Texture2D t in texture){
                Sprites[t.name] = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f));
                Debug.Log("Load texture " + t.name + " successfully");
            }
            return true;
        }
        return false;
    }
    public IEnumerator DownloadMaterials(string asset, string url, int version){
#if UNITY_EDITOR
        string path = Application.dataPath + "/../AssetBundles/materials";
        AssetBundle assetBundle = AssetBundle.LoadFromFile(path);
        if(LoadFromAssetBundle(assetBundle)) Debug.Log("Load asset bundle" + asset + "successfully");
        else Debug.Log("Load asset bundle" + asset + "failed");
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
            LoadFromAssetBundle(DownloadHandlerAssetBundle.GetContent(www));

        } // memory is freed from the web stream (www.Dispose() gets called implicitly)
#endif
    }
    public GameObject getPrefab(int index){
        return Prefabs[Names[index]];
    }
    public Sprite getSprite(int index){
        return Sprites[Names[index]];
    }
}
