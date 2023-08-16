using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Unityカメラをキャプチャして送信する
/// カメラオブジェクトにアタッチ
/// </summary>
public class SendCameraTexture : MonoBehaviour
{
    private int _cameraTextureWidth = 0;
    private int _cameraTextureHeight = 0;

    private Texture2D _tmpTexture2D = null;
    private RenderTexture _tmpRenderTexture = null;
    private ConcurrentQueue<AsyncGPUReadbackRequest> _gpuRequest = new ConcurrentQueue<AsyncGPUReadbackRequest>();
    private ChunkSplitter _chunkSplitter = new ChunkSplitter();

    private bool isCaptureActive = false;

    // Start is called before the first frame update
    void Start()
    {
        _cameraTextureWidth = GlobalState.Instance.VideoResolution.Width;
        _cameraTextureHeight = GlobalState.Instance.VideoResolution.Height;

        _tmpTexture2D = new Texture2D(_cameraTextureWidth, _cameraTextureHeight, TextureFormat.RGBA32, false);
        _tmpRenderTexture = new RenderTexture(_tmpTexture2D.width, _tmpTexture2D.height, 24, RenderTextureFormat.ARGB32);

        if (!SystemInfo.supportsAsyncGPUReadback)
        {
            Debug.LogError("AsyncGPUReadback not supported");
        }
    }

    void OnDestroy()
    {
        Destroy(_tmpTexture2D);
        Destroy(_tmpRenderTexture);
    }

    void Update()
    {
        if (!ConnectionManager.Instance.IsAvailable) return;

        if (isCaptureActive)
        {
            // GPUリクエスト結果解析
            ParseGPURequest();
        }
    }

    // カメラコンポーネントがアタッチされていると呼ばれる
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination);

        if (!ConnectionManager.Instance.IsAvailable) return;

        // カメラテクスチャをGPUリクエスト
        _tmpRenderTexture.Release();
        Graphics.Blit(source, _tmpRenderTexture);
        _gpuRequest.Enqueue(AsyncGPUReadback.Request(_tmpRenderTexture));
    }

    public void EnableCapture() => isCaptureActive = true;

    public void DisableCapture() => isCaptureActive = false;

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
#if false
                    switch (GlobalState.Instance.ApplicationGlobalSettings.VideoCodec)
                    {
                        case ApplicationSettings.VideoCodecType.MJPEG:
                            // JPEGにエンコード
                            _tmpTexture2D.LoadRawTextureData(buffer);
                            _tmpTexture2D.Apply();
                            sendData = _tmpTexture2D.EncodeToJPG(25);
                            sendDataList = _chunkSplitter.Split(sendData);
                            //Debug.Log($"SendCameraTexture jpeg encoded size: {sendData.Length}");
                            // 送信
                            foreach (var sd in sendDataList)
                            {
                                ConnectionManager.Instance.Send(DataType.Capture, sd).Forget();
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
                            //Debug.Log($"SendCameraTexture webp encoded size: {sendData.Length}");
                            // 送信
                            foreach (var sd in sendDataList)
                            {
                                ConnectionManager.Instance.Send(DataType.Capture, sd).Forget();
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
                    //Debug.Log($"SendCameraTexture jpeg encoded size: {sendData.Length}");
                    // 送信
                    foreach (var sd in sendDataList)
                    {
                        _ = ConnectionManager.Instance.Send(DataType.Capture, sd);
                    }
#endif

                    _gpuRequest.TryDequeue(out req);
                    break;
                }
            }
            else
            {
                Debug.Log("TryPeek failed");
                break;
            }
        }
    }
}
