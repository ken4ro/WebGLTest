using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using static SettingHub;

public partial class BotManager : SingletonBase<BotManager>
{
    public enum CommandTypes
    {
        move,
        clear,
        execute
    }

    /// <summary>
    /// ダイアログオプション
    /// </summary>
    [Serializable]
    public class BotResponseScript : ISerializationCallbackReceiver
    {
        [NonSerialized]
        public string Type;
        [NonSerialized]
        public string[] ParameterArray;
        [NonSerialized]
        public string Parameter;

        [SerializeField]
        private string type;
        [SerializeField]
        private string parameter;

        public void OnAfterDeserialize()
        {
            Type = type;
            Parameter = parameter;

            if (parameter == null)
                ParameterArray = null;
            else
                ParameterArray = parameter.Split(',');
        }

        public void OnBeforeSerialize()
        {
            type = Type.ToString();
            parameter = string.Join(",", ParameterArray);
        }
    }

    /// <summary>
    /// 外部プロセス標準出力時コールバック
    /// </summary>
    public Action<string> OnExternalProcessStandardOutput { get; set; } = null;

    /// <summary>
    /// 外部プロセス終了時コールバック
    /// </summary>
    public Action OnExternalProcessExited { get; set; } = null;

    private Process _externalProcess = null;

    /// <summary>
    /// リクエスト結果からオプション一覧を取得
    /// </summary>
    /// <returns></returns>
    public List<BotResponseScript> GetScripts()
    {
        List<BotResponseScript> list;
        try { list = new List<BotResponseScript>(Response.Scripts); }
        catch { list = new List<BotResponseScript>(); };

        return list;
    }

    /// <summary>
    /// スクリプト実行
    /// </summary>
    /// <param name="scripts"></param>
    public void ExecutionScript(List<BotResponseScript> scripts)
    {
        foreach (var script in scripts)
        {
            switch (script.Type)
            {
                case "move":
                    FixVariableMove(script);
                    break;

                case "clear":
                    FixVariableClear(script);
                    break;

                case "execute":
                    ExecuteExternalProcess(script);
                    break;
            }
        }
    }

    private void FixVariableMove(BotResponseScript script)
    {
        try
        {
            var path = script.ParameterArray[0].Split('.');
            var targetSetting = (SettingTypes)Enum.Parse(typeof(SettingTypes), path[0]);
            switch (targetSetting)
            {

                //サイネージ設定
                case SettingTypes.Signage:
                    var targetAccess = (AccessTypes)Enum.Parse(typeof(AccessTypes), path[1]);
                    var targetSignageTag = (SignageTags)Enum.Parse(typeof(SignageTags), path[2]);
                    SettingHub.Instance.OverwriteSignage(targetAccess, targetSignageTag, script.ParameterArray[1]);
                    break;

                //置換変数
                case SettingTypes.Variable:
                    targetAccess = (AccessTypes)Enum.Parse(typeof(AccessTypes), path[1]);
                    SettingHub.Instance.OverwriteVariable(targetAccess, "$" + path[2] + "$", script.ParameterArray[1]);
                    break;

                //システム変数
                case SettingTypes.System:
                    targetAccess = (AccessTypes)Enum.Parse(typeof(AccessTypes), path[1]);
                    var targetSystemTag = (SystemTags)Enum.Parse(typeof(SystemTags), path[2]);
                    SettingHub.Instance.OverwriteSystem(targetAccess, targetSystemTag, script.ParameterArray[1]);
                    break;

            }
        }
        catch { }
    }

    private void FixVariableClear(BotResponseScript script)
    {
        try
        {
            var path = script.ParameterArray[0].Split('.');
            if (SettingTypes.Variable != (SettingTypes)Enum.Parse(typeof(SettingTypes), path[0]))
                return;

            //置換変数
            var targetAccess = (AccessTypes)Enum.Parse(typeof(AccessTypes), path[1]);
            SettingHub.Instance.ClearVariable(targetAccess, "$"+path[2]+"$");

        }
        catch { }
    }

    private void ExecuteExternalProcess(BotResponseScript script)
    {
        try
        {
            // 起動するプロセス名とコマンドライン引数を取得
            var processPathLength = script.ParameterArray[0].IndexOf(' ');
            var processPath = processPathLength == -1 ? script.ParameterArray[0] : script.ParameterArray[0].Substring(0, processPathLength);
            processPath = FileHelper.GetCurrentDirectory() + processPath;
            UnityEngine.Debug.Log($"processPath: {processPath}");
            var processOptions = processPathLength == -1 ? "" : script.ParameterArray[0].Substring(processPathLength + 1);
            UnityEngine.Debug.Log($"processOptions: {processOptions}");
            // 起動オプション
            var processStartInfo = new ProcessStartInfo(processPath, processOptions)
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            _externalProcess = new Process
            {
                EnableRaisingEvents = true,
                StartInfo = processStartInfo
            };
            _externalProcess.OutputDataReceived += (sender, ev) =>
            {
                // 標準出力を検出
                UnityEngine.Debug.Log($"stdout={ev.Data}");
                if (!string.IsNullOrEmpty(ev.Data))
                {
                    OnExternalProcessStandardOutput?.Invoke(ev.Data);
                }
            };
            _externalProcess.Exited += (sender, ev) =>
            {
                // プロセス終了
                UnityEngine.Debug.Log("process end.");
                OnExternalProcessExited?.Invoke();
                _externalProcess.Dispose();
                _externalProcess = null;
            };
            // 起動
            _externalProcess.Start();
            _externalProcess.BeginOutputReadLine(); // 非同期で標準出力を検出する
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"BootExternalProcess exception: {e.Message}");
        }
    }
}
