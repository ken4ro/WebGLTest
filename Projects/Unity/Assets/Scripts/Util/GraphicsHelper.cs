using System.IO;
using UnityEngine;
using Cysharp.Threading.Tasks;
using static SignageSettings;

public static class GraphicsHelper
{
    /// <summary>
    /// 画像取得
    /// </summary>
    /// <param name="type"></param>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static async UniTask<Texture2D> LoadImage(ImageAccessTypes type, string filePath)
    {
        Texture2D texture2D = null;
        if (string.IsNullOrEmpty(filePath)) return texture2D;

        //texture2D = AssetBundleManager.Instance.LoadTexture2DFromResourcePack(Path.GetFileNameWithoutExtension(filePath));
        switch (type)
        {
            // アセット
            case ImageAccessTypes.UnityAsset:
                texture2D = AssetBundleManager.Instance.LoadTexture2DFromResourcePack(Path.GetFileNameWithoutExtension(filePath));
                if (texture2D == null)
                    Debug.LogError("SetImage error: Load texture from resource pack failed.");
                break;
            // ローカル
            case ImageAccessTypes.Local:
                texture2D = TextureHelper.CreateTexture2DFromFile(filePath);
                if (texture2D == null)
                    Debug.LogError("SetImage error: Load texture from localfile failed.");
                break;
            // Web
            case ImageAccessTypes.Web:
                texture2D = await TextureHelper.CreateTexture2DFromWeb(filePath);
                if (texture2D == null)
                    Debug.LogError("SetImage error: Load texture from webfile failed.");
                break;
        }
        if (texture2D == null)
        {
            Debug.LogError("SetImage error: Load texture from resource pack failed.");
        }

        return texture2D;
    }

}
