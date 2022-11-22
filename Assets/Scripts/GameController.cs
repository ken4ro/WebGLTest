using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static GlobalState;

/// <summary>
// 主にゲーム全体のステート管理やプロセス遷移を担う
/// </summary>
public class GameController : SingletonMonoBehaviour<GameController>
{
    [SerializeField]
    public Camera MainCamera = null;

    /// <summary>
    /// メインスレッドコンテキスト
    /// </summary>
    public SynchronizationContext MainContext { get; private set; } = null;

    protected override async void Awake()
    {
        base.Awake();

        //// 他のゲームオブジェクトの初期化前に実行する必要があるので同期的に処理を行う(原則 await 禁止)

        if (await WebServerManager.Instance.HealthCheck())
        {
            // アセットバージョンアップ
            await VersionUpdateManager.Instance.AssetUpdateSync();
        }

        // メインスレッド同期用コンテキストを取得しておく
        MainContext = SynchronizationContext.Current;

        // 使用キャラクターセット
        var identifier = VersionUpdateManager.Instance.GetAvatarIdentifier();
        if (Enum.TryParse(identifier, true, out CharacterModel characterModel))
        {
            GlobalState.Instance.CurrentCharacterModel = characterModel;
        }
        else if (identifier == "una2d_webgl")
        {
            GlobalState.Instance.CurrentCharacterModel = CharacterModel.Una2D;
        }
        else
        {
            Debug.LogError($"Mismatch between identifier and character model: {identifier}");
        }

        // キャラクターオブジェクト作成
        AssetBundleManager.Instance.LoadAvatarAssetBundle();
        LoadCharacterObject();

        // キャラクター表示
        CharacterManager.Instance.Enable();
    }

    void OnApplicationQuit()
    {
        // 各ゲームオブジェクトが破棄される前に行なければならない後始末
        // 同期的に実行する(原則 await 禁止)
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void LoadCharacterObject()
    {
        var assetBundle = AssetBundleManager.Instance.AvatarAssetBundle;

        // キャラクターオブジェクト作成
        GameObject characterObject = assetBundle.LoadAsset<GameObject>("Una2D");
        if (characterObject != null)
        {
            Debug.Log($"LoadCharacterObject: LoadAsset completed.");
        }
        else
        {
            Debug.Log($"LoadCharacterObject: LoadAsset failed.");
        }
        //switch (GlobalState.Instance.CurrentCharacterModel)
        //{
        //    case CharacterModel.Una3D:
        //        characterObject = assetBundle.LoadAsset<GameObject>("Una");
        //        break;
        //    case CharacterModel.Una2D:
        //        characterObject = assetBundle.LoadAsset<GameObject>("Una2D");
        //        break;
        //    case CharacterModel.Una2D_Rugby:
        //        characterObject = assetBundle.LoadAsset<GameObject>("Una2D_Rugby");
        //        break;
        //}
        characterObject = Instantiate(characterObject);
        if (characterObject != null)
        {
            Debug.Log($"LoadCharacterObject: Instantiate completed.");
        }
        else
        {
            Debug.Log($"LoadCharacterObject: Instantiate failed.");
        }

        // Front Canvas より奥に描画するようにする
        var frontCanvasIndex = GameObject.Find("FrontCanvas").transform.GetSiblingIndex();
        characterObject.transform.SetSiblingIndex(frontCanvasIndex);
    }
}