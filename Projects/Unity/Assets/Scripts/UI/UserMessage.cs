using UnityEngine;

using TMPro;


public class UserMessage : MonoBehaviour
{
    /// <summary>
    /// ユーザーメッセージ表示用ゲームオブジェクト
    /// </summary>
    [SerializeField] GameObject userMessage = null;

    // ユーザーメッセージに属するコンポーネント
    private TextMeshProUGUI _userMessageTextPro = null;

    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize()
    {
        _userMessageTextPro = userMessage.GetComponentInChildren<TextMeshProUGUI>();
    }

    /// <summary>
    /// 有効にする
    /// </summary>
    public void Enable() => userMessage.SetActive(true);

    /// <summary>
    /// 無効にする
    /// </summary>
    public void Disable() => userMessage.SetActive(false);

    /// <summary>
    /// ユーザーメッセージのテキストをセット
    /// </summary>
    /// <param name="text"></param>
    /// <param name="isAnim"></param>
    /// <returns></returns>
    public void SetUserText(string text) => _userMessageTextPro.text = text;

    /// <summary>
    /// ユーザーメッセージのテキストを取得
    /// </summary>
    /// <returns></returns>
    public string GetUserText() => _userMessageTextPro.text;
}
