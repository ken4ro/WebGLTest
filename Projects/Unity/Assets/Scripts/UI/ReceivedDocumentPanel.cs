using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Coffee.UIExtensions;

public class ReceivedDocumentPanel : MonoBehaviour
{
    private static readonly int ReceiveTextureWidth = 320;
    private static readonly int ReceiveTextureHeight = 180;

    private RawImage _displayImage = null;
    private Texture2D _receiveTexture = null;
    private UIShiny _uiShiny = null;
    private List<byte> _receivedData = new List<byte>();

    public void Initialize()
    {
        _displayImage = GetComponentInChildren<RawImage>();
        _receiveTexture = new Texture2D(ReceiveTextureWidth, ReceiveTextureHeight);
        _uiShiny = GetComponent<UIShiny>();
    }

    public void Enable()
    {
        gameObject.SetActive(true);
        StartCoroutine("Shine");
    }

    public void Disable()
    {
        gameObject.SetActive(false);
        StopCoroutine("Shine");
    }

    public void ReceiveTextureData(byte[] data)
    {
        // テクスチャデータは分割送信される
        // TODO: コマンド化
        try
        {
            var receivedCommand = System.Text.Encoding.UTF8.GetString(data);
            if (receivedCommand == "split end")
            {
                // データ受信完了
                if (!gameObject.activeSelf) Enable();
                _receiveTexture.LoadImage(_receivedData.ToArray());
                _receiveTexture.Apply();
                _displayImage.texture = _receiveTexture;
                _receivedData.Clear();
            }
            else
            {
                _receivedData.AddRange(data);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    IEnumerator Shine()
    {
        var animationTime = 0.0f;
        while (true)
        {
            if (_uiShiny.effectFactor >= 1.0f)
            {
                animationTime = 0.0f;
                _uiShiny.effectFactor = 0.0f;
            }
            else
            {
                animationTime += Time.deltaTime;
                _uiShiny.effectFactor = animationTime * 0.5f;
            }
            yield return null;
        }
    }
}
