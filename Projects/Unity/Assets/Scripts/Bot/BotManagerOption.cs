using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SignageSettings;

public partial class BotManager : SingletonBase<BotManager>
{

    /// <summary>
    /// ダイアログオプション
    /// </summary>
    [Serializable]
    public class BotResponseOption : ISerializationCallbackReceiver
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


    public enum OptionTypes
    {
        fullScreen
    }

    /// <summary>
    /// リクエスト結果からオプション一覧を取得
    /// </summary>
    /// <returns></returns>
    public List<BotResponseOption> GetOptions()
    {
        List<BotResponseOption> list;
        try { list = new List<BotResponseOption>(Response.Options); }
        catch { list = new List<BotResponseOption>(); };

        return list;
    }

    /// <summary>
    /// イメージアクセスタイプ取得
    /// </summary>
    /// <returns></returns>
    public ImageAccessTypes GetImageAccessType()
    {
        if (Response.Image.StartsWith("http"))
        {
            return ImageAccessTypes.Web;
        }

        try
        {
            var idx = Array.Find(Response.Options, p => p.Type == "imageAccessType");
            return idx == null ? SettingHub.Instance.Signage.Cache.ImageAccessType : (ImageAccessTypes)Enum.Parse(typeof(ImageAccessTypes), idx.ParameterArray[0]);
        }
        catch
        {
            return SettingHub.Instance.Signage.Cache.ImageAccessType;
        }
    }

    public static string GetSelectParameter(List<BotResponseOption> options, OptionTypes type)
    {
        var str = type.ToString();
        foreach(var opt in options)
        {
            if (opt.Type != str)
                continue;

            return opt.Parameter;
        }

        return null;
    }

}
