using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UniRx;
using Cysharp.Threading.Tasks;

public class TextureHelper
{
    // キャッシュ済みのスプライトマップ<ファイルパス、スプライトオブジェクト>
    private static Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
    private static Rect _spriteRect = new Rect();

    /// <summary>
    /// スプライトを取得
    /// </summary>
    /// <param name="relativePath">exeからの相対パス</param>
    /// <returns></returns>
    public static Sprite GetSprite(string relativePath)
    {
        if (!File.Exists(relativePath))
        {
            Debug.Log($"TextureManager sprite file not found: {relativePath}");
            return null;
        }

        Sprite sprite = null;

        if (spriteCache.ContainsKey(relativePath))
        {
            // キャッシュから読み込む
            sprite = spriteCache[relativePath];
        }
        else
        {
            // スプライトを新規作成
            sprite = CreateSpriteFromFile(relativePath);
            spriteCache.Add(relativePath, sprite);
        }

        return sprite;
    }

    public static Sprite CreateSpriteFromFile(string relativePath)
    {
        if (!File.Exists(relativePath))
        {
            Debug.Log($"SpriteManager.CreateSpriteFromFile file not found: {relativePath}");
            return null;
        }
        var texture = CreateTexture2DFromFile(relativePath);
        _spriteRect.x = 0;
        _spriteRect.y = 0;
        _spriteRect.width = texture.width;
        _spriteRect.height = texture.height;
        return Sprite.Create(texture, _spriteRect, Vector2.zero, 1.0f);
    }

    
    /// <summary>
    /// ローカルデータからテクスチャ作成(PNGのみ)
    /// </summary>
    /// <param name="relativePath">パス</param>
    /// <returns></returns>
    public static Texture2D CreateTexture2DFromFile(string relativePath)
    {
        Texture2D texture = null;
        using (var file = new FileStream(relativePath, FileMode.Open, FileAccess.Read))
        {
            using (var reader = new BinaryReader(file))
            {
                var data = reader.ReadBytes((int)reader.BaseStream.Length);
                int pos = 16;
                // width
                int width = 0;
                for (var i = 0; i < 4; i++)
                {
                    width = width * 256 + data[pos++];
                }
                // height
                int height = 0;
                for (var i = 0; i < 4; i++)
                {
                    height = height * 256 + data[pos++];
                }
                // Texture2D作成
                // TODO: ボトルネックになる様なら native texture を使って書き換える
                texture = new Texture2D(width, height);
                texture.LoadImage(data);
            }
        }
        return texture;
    }


    /// <summary>
    /// オンライン画像からテクスチャ作成
    /// </summary>
    /// <param name="url">url+ベーシック認証情報</param>
    /// <returns></returns>
    public static async UniTask<Texture2D> CreateTexture2DFromWeb(string url)
    {

        if (String.IsNullOrEmpty(url))
            return null;


        string[] arr = url.Split(',');
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(arr[0], true);
        if (arr.Length >= 3)
        {
            //ベーシック認証
            var auth = "Basic " + System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(arr[1] + ":" + arr[2]));
            request.SetRequestHeader("AUTHORIZATION", auth);
        }

        try
        {
            await request.SendWebRequest();
        }
        catch (Exception e)
        {
            Debug.LogError($"CreateTexture2DFromWeb Exception: {e.Message}");
            return null;
        }

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
            return null;
        }

        return DownloadHandlerTexture.GetContent(request);
    }

}
