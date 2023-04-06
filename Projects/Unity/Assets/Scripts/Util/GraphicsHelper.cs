using System.IO;
using UnityEngine;
using Cysharp.Threading.Tasks;

public static class GraphicsHelper
{
    /// <summary>
    /// 画像取得
    /// </summary>
    /// <param name="type"></param>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static Texture2D LoadImage(string filePath)
    {
        Texture2D texture2D = null;
        if (string.IsNullOrEmpty(filePath)) return texture2D;

        texture2D = AssetBundleManager.Instance.LoadTexture2DFromResourcePack(Path.GetFileNameWithoutExtension(filePath));
        if (texture2D == null)
        {
            Debug.LogError("SetImage error: Load texture from resource pack failed.");
        }

        return texture2D;
    }

}
