using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
public class MaterialHolder : MonoBehaviour
{
    public static MaterialHolder instance;
    public bool isLoadSuccess = false;
    private string BundleURL = "https://drive.google.com/uc?export=download&id=1uFXT_5WrCl-Uxi_sxojNhG5zHyCogOHt";
    public string AssetName;
    public int version = 0;
    void Awake()
    {
        instance = this;
        #if UNITY_ANDROID
        BundleURL = "https://drive.google.com/uc?export=download&id=1LEvZc712O1mi5tO_g8aakkzvZJjHVp0Q";
        #endif
        #if UNITY_STANDALONE_WIN
        BundleURL = "https://drive.google.com/uc?export=download&id=1uFXT_5WrCl-Uxi_sxojNhG5zHyCogOHt";
        #endif
        StartCoroutine(DownloadMaterials(AssetName, BundleURL, version));
    }
    public Dictionary<string, Material> Materials = new Dictionary<string, Material>();
    public Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>();
    public List<string> Names = new List<string>();
    public int MaterialCount;
    public bool LoadFromAssetBundle(AssetBundle assetBundle){
        if(assetBundle != null){
            // Load all textures
            Sprites.Clear();
            Names.Clear();
            Texture2D[] texture = assetBundle.LoadAllAssets<Texture2D>();
            foreach(Texture2D t in texture){
                Sprites[t.name] = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f));
                Materials[t.name] = new Material(Shader.Find("Standard"));
                Materials[t.name].mainTexture = t;
                if(t.name=="TakoDachi"){
                    Materials[t.name].DisableKeyword("_ALPHATEST_ON");
                    Materials[t.name].DisableKeyword("_ALPHABLEND_ON");
                    Materials[t.name].EnableKeyword("_ALPHAPREMULTIPLY_ON");
                }
                else{
                    Materials[t.name].EnableKeyword("_ALPHATEST_ON");
                    Materials[t.name].DisableKeyword("_ALPHABLEND_ON");
                    Materials[t.name].DisableKeyword("_ALPHAPREMULTIPLY_ON");
                }
                Names.Add(t.name);
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
        isLoadSuccess = (LoadFromAssetBundle(assetBundle));
        yield return null;
#else
        // Wait for the Caching system to be ready
        while (!Caching.ready)
            yield return null;

        // Start the download
        using(UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(url)){
            yield return www.SendWebRequest();
            if (www.error != null)
                throw new System.Exception("WWW download:" + www.error);
            isLoadSuccess = LoadFromAssetBundle(DownloadHandlerAssetBundle.GetContent(www));

        } // memory is freed from the web stream (www.Dispose() gets called implicitly)
#endif
    }
    // public bool getPrefab(int index, out GameObject obj){
    //     if(index < 0 || index >= Names.Count){
    //         obj = null;
    //         return false;
    //     }
    //     obj = Prefabs[Names[index]];
    //     return true;
    // }
    public bool getMaterial(int index, out Material obj){
        if(index < 0 || index >= Names.Count){
            obj = null;
            return false;
        }
        obj = Materials[Names[index]];
        return true;
    }
    public bool getSprite(int index, out Sprite obj){
        if(index < 0 || index >= Names.Count){
            obj = null;
            return false;
        }
        obj = Sprites[Names[index]];
        return true;
    }
}
