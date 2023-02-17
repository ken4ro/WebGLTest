using UnityEngine;


public class GlassMaskPanel : MonoBehaviour
{
    /// <summary>
    /// マスク処理用パネル
    /// </summary>
    [SerializeField] GameObject glassMaskPanel = null;

    /// <summary>
    /// 表示を有効にする
    /// </summary>
    public void Enable()
    {
        glassMaskPanel.SetActive(true);
    }

    /// <summary>
    /// 表示を無効にする
    /// </summary>
    public void Disable()
    {
        glassMaskPanel.SetActive(false);
    }
}
