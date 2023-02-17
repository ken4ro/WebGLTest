using UnityEngine;
using UnityEngine.UI;

using TMPro;


public class SpeakingMessage : MonoBehaviour
{
    /// <summary>
    /// 発話中メッセージ
    /// </summary>
    [SerializeField] GameObject speakingMessage = null;

    private Image _speakingMessageImage = null;
    private TextMeshProUGUI _speakingMessageTextPro = null;

    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize()
    {
        _speakingMessageImage = speakingMessage.GetComponent<Image>();
        _speakingMessageTextPro = speakingMessage.GetComponentInChildren<TextMeshProUGUI>();
    }

    /// <summary>
    /// 表示を有効にする
    /// </summary>
    public void Enable() => speakingMessage.SetActive(true);

    /// <summary>
    /// 表示を無効にする
    /// </summary>
    public void Disable() => speakingMessage.SetActive(false);

    /// <summary>
    /// 発話中メッセージのテキストを取得
    /// </summary>
    /// <returns></returns>
    public string GetSpeakingText() => _speakingMessageTextPro.text;

    /// <summary>
    /// 発話中メッセージのテキストをセット
    /// </summary>
    /// <param name="text"></param>
    public void SetSpeakingText(string text) => _speakingMessageTextPro.text = text;
}
