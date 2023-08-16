using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

/// <summary>
/// Webカメラをキャプチャして送信する
/// </summary>
public class WebCameraPanel : MonoBehaviour
{
    private int _webCameraTextureWidth = 0;
    private int _webCameraTextureHeight = 0;

    private WebCamTexture _webCameraTexture = null;
    private RawImage _displayImage = null;

    private Texture2D _tmpTexture2D = null;
    private RenderTexture _tmpRenderTexture = null;
    private Rect _tmpRect = Rect.zero;

    private ConcurrentQueue<AsyncGPUReadbackRequest> _gpuRequest = new ConcurrentQueue<AsyncGPUReadbackRequest>();
    private ChunkSplitter _chunkSplitter = new ChunkSplitter();

    void Update()
    {
        if (!ConnectionManager.Instance.IsAvailable) return;

        if (_gpuRequest.Count == 0)
        {
            // GPUリクエスト
            GPURequest();
        }
        else
        {
            // GPUリクエスト結果解析
            ParseGPURequest();
        }
    }

    public void Initialize()
    {
        _webCameraTextureWidth = GlobalState.Instance.VideoResolution.Width;
        _webCameraTextureHeight = GlobalState.Instance.VideoResolution.Height;

        _webCameraTexture = new WebCamTexture(_webCameraTextureWidth, _webCameraTextureHeight);
        _tmpTexture2D = new Texture2D(_webCameraTextureWidth, _webCameraTextureHeight, TextureFormat.RGBA32, false);
        _tmpRenderTexture = new RenderTexture(_webCameraTextureWidth, _webCameraTextureHeight, 24, RenderTextureFormat.ARGB32);
        _tmpRect = new Rect(0, 0, _tmpRenderTexture.width, _tmpRenderTexture.height);
        _displayImage = GetComponent<RawImage>();
        _displayImage.texture = _webCameraTexture;
    }

    public void Enable() => gameObject.SetActive(true);

    public void Disable() => gameObject.SetActive(false);

    public void Play()
    {
        _webCameraTexture.Play();
    }

    public void Stop()
    {
        _webCameraTexture.Stop();
    }

    private void GPURequest()
    {
        // WebカメラテクスチャをGPUリクエスト
        _tmpRenderTexture.Release();
        Graphics.Blit(_webCameraTexture, _tmpRenderTexture);
        _gpuRequest.Enqueue(AsyncGPUReadback.Request(_tmpRenderTexture));
    }

    private void ParseGPURequest()
    {
        while (_gpuRequest.Count > 0)
        {
            if (_gpuRequest.TryPeek(out AsyncGPUReadbackRequest req))
            {
                if (!req.done)
                {
                    break;
                }
                else if (req.hasError)
                {
                    _gpuRequest.TryDequeue(out req);
                    continue;
                }
                else
                {
                    // テクスチャデータをセット
                    var buffer = req.GetData<Color32>();
                    byte[] sendData = null;
                    List<byte[]> sendDataList = null;
#if false // WebPアセット及びH264アセットはWebGL未対応
                    switch (GlobalState.Instance.ApplicationGlobalSettings.VideoCodec)
                    {
                        case ApplicationSettings.VideoCodecType.MJPEG:
                            // JPEGにエンコード
                            _tmpTexture2D.LoadRawTextureData(buffer);
                            _tmpTexture2D.Apply();
                            sendData = _tmpTexture2D.EncodeToJPG(25);
                            sendDataList = _chunkSplitter.Split(sendData);
                            //Debug.Log($"WebCameraTexture jpeg encoded size: {sendData.Length}");
                            // 送信
                            foreach (var sd in sendDataList)
                            {
                                ConnectionManager.Instance.Send(DataType.Camera, sd).Forget();
                            }
                            break;
                        case ApplicationSettings.VideoCodecType.MWEBP:
                            // WebPにエンコード
                            _tmpTexture2D.LoadRawTextureData(buffer);
                            _tmpTexture2D.Apply();
                            sendData = _tmpTexture2D.EncodeToWebP(25, out var encodeError);
                            if (encodeError != Error.Success)
                            {
                                Debug.LogError($"WebP encode error: {encodeError}");
                                return;
                            }
                            sendDataList = _chunkSplitter.Split(sendData);
                            //Debug.Log($"WebCameraTexture webp encoded size: {sendData.Length}");
                            // 送信
                            foreach (var sd in sendDataList)
                            {
                                ConnectionManager.Instance.Send(DataType.Camera, sd).Forget();
                            }
                            break;
                        case ApplicationSettings.VideoCodecType.H264:
                            // テクスチャセット
                            // H.264エンコードは非同期で行われる
                            nvPipeEncodeManager.SetTextureColor(buffer);
                            break;
                        default:
                            Debug.LogError("Video codec is invalid.");
                            break;
                    }
#else
                    // JPEGにエンコード
                    _tmpTexture2D.LoadRawTextureData(buffer);
                    _tmpTexture2D.Apply();
                    sendData = _tmpTexture2D.EncodeToJPG(25);
                    sendDataList = _chunkSplitter.Split(sendData);
                    //Debug.Log($"WebCameraTexture jpeg encoded size: {sendData.Length}");
#endif
                    // 送信
                    foreach (var sd in sendDataList)
                    {
                        _ = ConnectionManager.Instance.Send(DataType.Camera, sd);
                    }

                    _gpuRequest.TryDequeue(out req);
                    break;
                }
            }
            else
            {
                Debug.LogError("TryPeek failed");
                break;
            }
        }
    }
}
