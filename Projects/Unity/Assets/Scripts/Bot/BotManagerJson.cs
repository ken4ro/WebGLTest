using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using static GlobalState;
using static SignageSettings;

public partial class BotManager
{
    [Serializable]
    public class BotResponse : ISerializationCallbackReceiver
    {
        [NonSerialized]
        public string Scene;
        [NonSerialized]
        public BotResponseVoice Voice;
        [NonSerialized]
        public BotResponseText Text;
        [NonSerialized]
        public string Image;
        [NonSerialized]
        public BotResponseBrowser Browser;
        [NonSerialized]
        public BotResponseSelect[] Selects;
        [NonSerialized]
        public BotResponseOption[] Options;
        [NonSerialized]
        public BotResponseScript[] Scripts;
        [NonSerialized]
        public string[] Send;
        [NonSerialized]
        public string Motion;
        [NonSerialized]
        public string Action;
        [NonSerialized]
        public string Movie;

        [SerializeField]
        private string scene;
        [SerializeField]
        private BotResponseVoice voice;
        [SerializeField]
        private BotResponseText text;
        [SerializeField]
        private string image;
        [SerializeField]
        private BotResponseBrowser browser;
        [SerializeField]
        private BotResponseSelect[] select;
        [SerializeField]
        private BotResponseOption[] option;
        [SerializeField]
        private BotResponseScript[] script;
        [SerializeField]
        private string[] send;
        [SerializeField]
        private string motion;
        [SerializeField]
        private string action;
        [SerializeField]
        private string movie;

        public void OnBeforeSerialize()
        {
            scene = Scene;
            voice = Voice;
            text = Text;
            image = Image;
            browser = Browser;
            select = Selects;
            option = Options;
            script = Scripts;
            send = Send;
            motion = Motion;
            action = Action;
            movie = Movie;
        }

        public void OnAfterDeserialize()
        {
            Scene = scene;
            Voice = voice;
            Text = text;
            Image = image;
            Browser = browser;
            Selects = select;
            Options = option;
            Scripts = script;
            Send = send;
            Motion = motion;
            Action = action;
            Movie = movie;
        }
    }

    [Serializable]
    public class BotResponseVoice : ISerializationCallbackReceiver
    {
        [NonSerialized]
        public string Jp;
        [NonSerialized]
        public string En;
        [NonSerialized]
        public string Ch;
        [NonSerialized]
        public string Ru;
        [NonSerialized]
        public string Ar;
        [NonSerialized]
        public string Vi;

        [SerializeField]
        private string jp;
        [SerializeField]
        private string en;
        [SerializeField]
        private string ch;
        [SerializeField]
        private string ru;
        [SerializeField]
        private string ar;
        [SerializeField]
        private string vi;

        public void OnBeforeSerialize()
        {
            jp = Jp;
            en = En;
            ch = Ch;
            ru = Ru;
            ar = Ar;
            vi = Vi;
        }

        public void OnAfterDeserialize()
        {
            Jp = jp;
            En = en;
            Ch = ch;
            Ru = ru;
            Ar = ar;
            Vi = vi;
        }
    }

    [Serializable]
    public class BotResponseText : ISerializationCallbackReceiver
    {
        [NonSerialized]
        public string Jp;
        [NonSerialized]
        public string En;
        [NonSerialized]
        public string Ch;
        [NonSerialized]
        public string Ru;
        [NonSerialized]
        public string Ar;
        [NonSerialized]
        public string Vi;

        [SerializeField]
        private string jp;
        [SerializeField]
        private string en;
        [SerializeField]
        private string ch;
        [SerializeField]
        private string ru;
        [SerializeField]
        private string ar;
        [SerializeField]
        private string vi;

        public void OnBeforeSerialize()
        {
            jp = Jp;
            en = En;
            ch = Ch;
            ru = Ru;
            ar = Ar;
            vi = Vi;
        }

        public void OnAfterDeserialize()
        {
            Jp = jp;
            En = en;
            Ch = ch;
            Ru = ru;
            Ar = ar;
            Vi = vi;
        }
    }

    /// <summary>
    /// ダイアログ選択肢
    /// </summary>
    [Serializable]
    public class BotResponseSelect : ISerializationCallbackReceiver
    {
        [NonSerialized]
        public Color Color;
        [NonSerialized]
        public string Image;
        [NonSerialized]
        public string Jp;
        [NonSerialized]
        public string En;
        [NonSerialized]
        public string Ch;
        [NonSerialized]
        public string Ru;
        [NonSerialized]
        public string Ar;
        [NonSerialized]
        public string Vi;

        [SerializeField]
        private string color;
        [SerializeField]
        private string image;
        [SerializeField]
        private string jp;
        [SerializeField]
        private string en;
        [SerializeField]
        private string ch;
        [SerializeField]
        private string ru;
        [SerializeField]
        private string ar;
        [SerializeField]
        private string vi;

        public void OnBeforeSerialize()
        {
            //color = Color;
            image = Image;
            jp = Jp;
            en = En;
            ch = Ch;
            ru = Ru;
            ar = Ar;
            vi = Vi;
        }

        public void OnAfterDeserialize()
        {
            Image = image;
            Jp = jp;
            En = en;
            Ch = ch;
            Ru = ru;
            Ar = ar;
            Vi = vi;

            if (!string.IsNullOrEmpty(color))
            {
                var col = color.Split(',');
                Color = new Color(float.Parse(col[0]) / 255, float.Parse(col[1]) / 255, float.Parse(col[2]) / 255, float.Parse(col[3]) / 255);
            }
            else
            {
                Color = new Color(43 / 255, 142f / 255, 54f / 255, 172f / 255);
            }
        }

        public static string GetTargetText(BotResponseSelect res, Language lang)
        {
            try
            {
                switch (lang)
                {
                    case Language.Arabic:
                        return res.Ar;
                    case Language.Chinese:
                        return res.Ch;
                    case Language.English:
                        return res.En;
                    case Language.Russian:
                        return res.Ru;
                    case Language.Vietnamese:
                        return res.Vi;
                    default:
                        return res.Jp;

                }
            }
            catch
            {
                return res.Jp;
            }

        }
    }

    [Serializable]
    public class BotResponseBrowser : ISerializationCallbackReceiver
    {
        [NonSerialized]
        public string Url;
        [NonSerialized]
        public int Scroll;
        [NonSerialized]
        public bool Operation;
        [NonSerialized]
        public BotResponseBrowserSize Size;

        [SerializeField]
        public string url;
        [SerializeField]
        public int scroll;
        [SerializeField]
        public bool operation;
        [SerializeField]
        public BotResponseBrowserSize size;

        public void OnBeforeSerialize()
        {
            url = Url;
            scroll = Scroll;
            operation = Operation;
            size = Size;
        }

        public void OnAfterDeserialize()
        {
            Url = url;
            Scroll = scroll;
            Operation = operation;
            Size = size;
        }

        [Serializable]
        public class BotResponseBrowserSize : ISerializationCallbackReceiver
        {
            [NonSerialized]
            public int Width;
            [NonSerialized]
            public int Height;

            [SerializeField]
            private int width;
            [SerializeField]
            private int height;

            public void OnBeforeSerialize()
            {
                width = Width;
                height = Height;
            }

            public void OnAfterDeserialize()
            {
                Width = width;
                Height = height;
            }
        }
    }

    static public string ReplaceVariable(string origin)
    {
        //変換テーブル作成
        var table = SettingHub.Instance.Variable.Cache.GetTable();
        table.Add("$language$", SignageSettings.CurrentLanguage.Value.ToString());
        table.Add("$talk$", StreamingSpeechToText.Instance.RecognitionCompleteText);
        return table.Aggregate(origin, (result, s) => result.Replace(s.Key, s.Value));
    }
}
