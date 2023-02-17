using UnityEngine;


public class LoadingIcon : MonoBehaviour
{
    /// <summary>
    /// キャラクター処理待ち中アイコン表示用ゲームオブジェクト
    /// </summary>
    [SerializeField] GameObject loadingIcon = null;

    /// <summary>
    /// 表示を有効にする
    /// </summary>
    public void Enable() => loadingIcon.SetActive(true);

    /// <summary>
    /// 表示を無効にする
    /// </summary>
    public void Disable() => loadingIcon.SetActive(false);
}
