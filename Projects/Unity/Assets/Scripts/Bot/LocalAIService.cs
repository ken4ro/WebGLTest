using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UniRx;
using static BotManager;
using Cysharp.Threading.Tasks;

/* dynamic 問題のためいったん無効
public partial class LocalAIService : IBotService
{
    /// <summary>
    /// 全角→半角変換テーブル
    /// Repl側でなぜか全角に変換されて返してくることがある
    /// 全角未対応のフォント対策
    /// </summary>
    public static readonly Dictionary<char, char> CharacterByteTable = new Dictionary<char, char>() {
        {'１','1'},{'２','2'},{'３','3'},{'４','4'},{'５','5'},
        {'６','6'},{'７','7'},{'８','8'},{'９','9'},{'０','0'},
        {'Ａ','A'},{'Ｂ','B'},{'Ｃ','C'},{'Ｄ','D'},{'Ｅ','E'},
        {'Ｆ','F'},{'Ｇ','G'},{'Ｈ','H'},{'Ｉ','I'},{'Ｊ','J'},
        {'Ｋ','K'},{'Ｌ','L'},{'Ｍ','M'},{'Ｎ','N'},{'Ｏ','O'},
        {'Ｐ','P'},{'Ｑ','Q'},{'Ｒ','R'},{'Ｓ','S'},{'Ｔ','T'},
        {'Ｕ','U'},{'Ｖ','V'},{'Ｗ','W'},{'Ｘ','X'},{'Ｙ','Y'},
        {'Ｚ','Z'},
        {'ａ','a'},{'ｂ','b'},{'ｃ','c'},{'ｄ','d'},{'ｅ','e'},
        {'ｆ','f'},{'ｇ','g'},{'ｈ','h'},{'ｉ','i'},{'ｊ','j'},
        {'ｋ','k'},{'ｌ','l'},{'ｍ','m'},{'ｎ','n'},{'ｏ','o'},
        {'ｐ','p'},{'ｑ','q'},{'ｒ','r'},{'ｓ','s'},{'ｔ','t'},
        {'ｕ','u'},{'ｖ','v'},{'ｗ','w'},{'ｘ','x'},{'ｙ','y'},
        {'ｚ','z'},
        {'　',' '},
    };

    /// <summary>
    /// APIキー
    /// プロジェクト毎に固有の値が設定される
    /// デフォルトは「穴吹2019」プロジェクト
    /// </summary>
    public string ApiKey { get; set; } = "CPeHyw2gVc6oVu2oP3UTeoJNASKYZJNVzbSGQb4J";

    /// <summary>
    /// ボットID
    /// BOT毎に固有の値が設定される
    /// デフォルトは「マンション管理人」ボット
    /// </summary>
    public string BotId { get; set; } = "b4s3kldj559y0ag";

    /// <summary>
    /// シナリオID
    /// シナリオ毎に固有の値が設定される
    /// デフォルトは「v3」シナリオ
    /// </summary>
    public string ScenarioId { get; set; } = "s4sm2pztvgl80oc";

    public string ScenarioPath { get; set; } = "";

    // Relp-AI ユーザーID
    private static string _replAIUserId = "";

    // シナリオデータ
    private dynamic _scenarioData = null;

    private string _focusNodeId = "";

    private List<string> _interruptLinks = new List<string>();

    /// <summary>
    /// 初期化
    /// </summary>
    public void Initialize()
    {
        LoadSettingsSync();
        _interruptLinks = GetInterruptLink();
    }

    public async UniTask<BotRequestResult> Reset()
    {
        var response = await GetUserIdAsync();

        _replAIUserId = response.result;

        return response;
    }

    public async UniTask<BotRequestResult> Request(bool isInit, string inputText)
    {
        var response = await Dialogue(_replAIUserId, isInit, inputText);

        return response;
    }

    // ユーザーID取得(非同期)
    private async UniTask<BotRequestResult> GetUserIdAsync() => GetUserIdSync();

    // ユーザーID取得
    private BotRequestResult GetUserIdSync()
    {

        var ret = new BotRequestResult();
        var guid = System.Guid.NewGuid();
        ret.result = guid.ToString();
        return ret;

    }


    /// <summary>
    /// 対話
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="isInit">初期化フラグ</param>
    /// <param name="inputText">ユーザ発話</param>
    /// <returns></returns>
    private async UniTask<BotRequestResult> Dialogue(string userId, bool isInit, string inputText)
    {
        if (userId != _replAIUserId || isInit)
            RestartScenario();


        var ret = new BotRequestResult();

        //次のnodeを検索する
        var nowNode = GetNodeData(_focusNodeId);
        switch (nowNode.type)
        {

            case "InflowSystemNode":
                {
                    List<string> links = GetNodeLink(nowNode.id);
                    if(links.Count <= 0)
                        ret.Status = HttpStatusCode.NotFound;
                    else
                    {
                        var rnd = new System.Random();
                        var json = GetNodeData( links[rnd.Next(links.Count)]);

                        _focusNodeId = json.id;

                        var base64EncodedBytes = System.Convert.FromBase64String(json.response);
                        ret.result = Encoding.UTF8.GetString(base64EncodedBytes);
                        ret.Status = HttpStatusCode.OK;
                    }
                }
                break;


            case "SystemNode":
                {
                    //発話ノードID取得
                    var id = SpeechToNextNode(inputText);

                    var links = GetNodeLink(id);
                    if (links.Count <= 0)
                        ret.Status = HttpStatusCode.NotFound;

                    else
                    {
                        var rnd = new System.Random();
                        var json = GetNodeData(links[rnd.Next(links.Count)]);

                        _focusNodeId = json.id;

                        var base64EncodedBytes = System.Convert.FromBase64String(json.response);
                        ret.result = Encoding.UTF8.GetString(base64EncodedBytes);
                        ret.Status = HttpStatusCode.OK;
                    }
                }
                break;

            case "UserNode":
                ret.Status = HttpStatusCode.BadGateway;
                break;

        }

        return ret;

    }


    /// <summary>
    /// 発話内容を内包するnodeIDを取得
    /// </summary>
    /// <param name="speech"></param>
    /// <returns></returns>
    private string SpeechToNextNode(string speech)
    {
        //関連するリンク一覧を作成
        var links = GetNodeLink(_focusNodeId);
        links.AddRange(_interruptLinks);

        var candidateId = "";

        foreach (var id in links)
        {
            var node = GetNodeData(id);
            if (node.type != "UserNode")
                continue;

            foreach (var utterance in node.utterances)
            {
                //アスタリスクがある場合は最後に拾う
                if (utterance == "*")
                    candidateId = node.id;

                if (speech.IndexOf(utterance) >= 0)
                    return node.id;
            }

        }

        return candidateId;
    }

    // 設定ファイル読み込み(同期)
    private void LoadSettingsSync()
    {
        var settingFileAsset = AssetBundleManager.Instance.LoadTextAssetFromResourcePack("LocalAISettings");
        if (settingFileAsset == null)
        {
            Debug.LogError("Repl setting file load error.");
            return;
        }
        var json = settingFileAsset.text.Trim(new char[] { '\uFEFF' });
        var settingObj = JsonUtility.FromJson<LocalAISettings>(json);
        ScenarioId = settingObj.ScenarioId;
        ScenarioPath = settingObj.ScenarioPath;

        LoadScenario();
    }


    /// <summary>
    /// シナリオデータ読み込み
    /// </summary>
    private void LoadScenario()
    {
        _scenarioData = "";

        switch (SettingHub.Instance.Signage.Cache.ChatbotService)
        {
            case SignageSettings.ChatbotServices.CAIAsset:
                LoadAssetScenario();
                break;

            case SignageSettings.ChatbotServices.CAIFile:
                LoadFileScenario();
                break;

            case SignageSettings.ChatbotServices.CAIWeb:
                LoadWebScenarioAsync();
                break;
        }
    }

    private async void LoadWebScenarioAsync()
    {
        string[] arr = ScenarioPath.Split(',');
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(arr[0], true);
        if (arr.Length >= 3)
        {
            //ベーシック認証
            var auth = "Basic " + System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(arr[1] + ":" + arr[2]));
            request.SetRequestHeader("AUTHORIZATION", auth);
        }

        await request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            Debug.Log(request.error);
            return;
        }

        if (request.isHttpError || request.isNetworkError)
        {
            Debug.Log(request.error);
            return;
        }


        _scenarioData = request.downloadHandler.text;
        return;
    }

    private void LoadAssetScenario()
    {
        var settingFileAsset = AssetBundleManager.Instance.LoadTextAssetFromResourcePack(ScenarioPath);
        if (settingFileAsset == null)
        {
            Debug.LogError("scenario data load error.");
            return;
        }
        _scenarioData = DynamicJson.Parse(settingFileAsset.text.Trim(new char[] { '\uFEFF' }));
        RestartScenario();
    }

    private void LoadFileScenario()
    {

        try
        {
            StreamReader sr = new StreamReader(ScenarioPath);
            string str = sr.ReadToEnd();
            sr.Close();

            _scenarioData = DynamicJson.Parse(str);
            RestartScenario();
        }
        catch {

            Debug.LogError("scenario data load error.");
            return;
        }


    }


    /// <summary>
    /// シナリオリスタート
    /// </summary>
    private void RestartScenario()
    {
        try
        {
            foreach (var node in _scenarioData.nodes)
            {
                if (node.type != "InflowSystemNode")
                    continue;
                _focusNodeId = node.id;
                return;
            }
        }
        catch { }

    }

    /// <summary>
    /// ノードデータ取得
    /// </summary>
    /// <returns></returns>
    private dynamic GetNodeData(string id)
    {
        try
        {
            foreach (var node in _scenarioData.nodes)
            {
                if (node.id != id)
                    continue;
           
                return node;

            }
        }
        catch { }
        return null;

    }


    /// <summary>
    /// 指定IDの起点とするリンク先一覧を取得
    /// </summary>
    /// <param name="id">起点nodeID</param>
    /// <returns></returns>
    private List<string> GetNodeLink(string id)
    {
        List<string> links = new List<string>();

        try
        {
            foreach (var link in _scenarioData.links)
            {
                if (link.from == id)
                    links.Add(link.to);

            }
        }
        catch { }



        return links;
    }

    /// <summary>
    /// 割込ノードに関連するリンク先一覧を取得
    /// </summary>
    /// <returns></returns>
    private List<string> GetInterruptLink()
    {
        List<string> links = new List<string>();

        //割込ノードを取得
        foreach (var node in _scenarioData.nodes)
        {
            if (node.type == "InflowInterruptNode")
            {
                List<string> interrupts = GetNodeLink(node.id);
                if(interrupts.Count>0)
                    links.AddRange(interrupts);
            }
                
        }

        return links;
    }

}
*/

public partial class LocalAIService : IBotService
{
    /// <summary>
    /// 全角→半角変換テーブル
    /// Repl側でなぜか全角に変換されて返してくることがある
    /// 全角未対応のフォント対策
    /// </summary>
    public static readonly Dictionary<char, char> CharacterByteTable = new Dictionary<char, char>() {
        {'１','1'},{'２','2'},{'３','3'},{'４','4'},{'５','5'},
        {'６','6'},{'７','7'},{'８','8'},{'９','9'},{'０','0'},
        {'Ａ','A'},{'Ｂ','B'},{'Ｃ','C'},{'Ｄ','D'},{'Ｅ','E'},
        {'Ｆ','F'},{'Ｇ','G'},{'Ｈ','H'},{'Ｉ','I'},{'Ｊ','J'},
        {'Ｋ','K'},{'Ｌ','L'},{'Ｍ','M'},{'Ｎ','N'},{'Ｏ','O'},
        {'Ｐ','P'},{'Ｑ','Q'},{'Ｒ','R'},{'Ｓ','S'},{'Ｔ','T'},
        {'Ｕ','U'},{'Ｖ','V'},{'Ｗ','W'},{'Ｘ','X'},{'Ｙ','Y'},
        {'Ｚ','Z'},
        {'ａ','a'},{'ｂ','b'},{'ｃ','c'},{'ｄ','d'},{'ｅ','e'},
        {'ｆ','f'},{'ｇ','g'},{'ｈ','h'},{'ｉ','i'},{'ｊ','j'},
        {'ｋ','k'},{'ｌ','l'},{'ｍ','m'},{'ｎ','n'},{'ｏ','o'},
        {'ｐ','p'},{'ｑ','q'},{'ｒ','r'},{'ｓ','s'},{'ｔ','t'},
        {'ｕ','u'},{'ｖ','v'},{'ｗ','w'},{'ｘ','x'},{'ｙ','y'},
        {'ｚ','z'},
        {'　',' '},
    };

    /// <summary>
    /// APIキー
    /// プロジェクト毎に固有の値が設定される
    /// デフォルトは「穴吹2019」プロジェクト
    /// </summary>
    public string ApiKey { get; set; } = "CPeHyw2gVc6oVu2oP3UTeoJNASKYZJNVzbSGQb4J";

    /// <summary>
    /// ボットID
    /// BOT毎に固有の値が設定される
    /// デフォルトは「マンション管理人」ボット
    /// </summary>
    public string BotId { get; set; } = "b4s3kldj559y0ag";

    /// <summary>
    /// シナリオID
    /// シナリオ毎に固有の値が設定される
    /// デフォルトは「v3」シナリオ
    /// </summary>
    public string ScenarioId { get; set; } = "s4sm2pztvgl80oc";

    public string ScenarioPath { get; set; } = "";

    // Relp-AI ユーザーID
    private static string _replAIUserId = "";

    // シナリオデータ
    private dynamic _scenarioData = null;

    private string _focusNodeId = "";

    private List<string> _interruptLinks = new List<string>();

    /// <summary>
    /// 初期化
    /// </summary>
    public UniTask Initialize()
    {
        return UniTask.CompletedTask;
    }

    public async UniTask<BotRequestResult> Reset()
    {
        return null;
    }

    public async UniTask<BotRequestResult> Request(bool isInit, string inputText)
    {
        return null;
    }
}