using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GlobalState;
using static SignageSettings;

public class SpeakingComplete : IState
{
    private static readonly Dictionary<Language, string[]> LanguagePhraseMap = new Dictionary<Language, string[]>()
    {
        { Language.Japanese, new string[] { "日本語", "japanese", "ジャパニーズ"} },
        { Language.English, new string[] { "英語", "english", "イングリッシュ" } },
        { Language.Chinese, new string[] { "中国語", "chinese", "チャイニーズ" } },
        { Language.Russian, new string[] { "ロシア語", "russian", "ロシアン" } },
        { Language.Arabic, new string[] { "アラビア語", "arabic", "アラビック" } },
        { Language.Vietnamese, new string[] { "ベトナム語", "vietnamese", "ベトナミーズ" } }
    };

    public async void OnEnter()
    {
        // 選択肢をリセット
        UIManager.Instance.ResetSelectMessage();

        var text = StreamingSpeechToText.Instance.RecognitionCompleteText;
        if (!string.IsNullOrEmpty(text))
        {
            // 受付可能アイコンフェードアウト
            UIManager.Instance.FadeOutAcceptableIcon();
            // 発話完了用の効果音再生
            var audioTask = AudioManager.Instance.PlaySE(AudioManager.SEType.VoiceOut);
            // 発話したテキストを表示
            UIManager.Instance.EnableUserMessage();
            UIManager.Instance.SetUserText(text);
            // 言語切り替え
            // v3では言語切替は音声入力では行わない
            //ChangeLanguage(text);
            // 翻訳モードかつ日本語で無いなら翻訳する
            if (GlobalState.Instance.CurrentBotRequestMode.Value == BotRequestMode.Translation && CurrentLanguage.Value != Language.Japanese)
            {
                text = await GoogleService.Translation(text, (GoogleService.Language)CurrentLanguage.Value, GoogleService.Language.Japanese);
                Debug.Log($"Translated text: {text}");
            }
            // 発話したテキストをBotに投げる
            await BotManager.Instance.Request(false, text);
        }
    }

    public void OnUpdate()
    {

    }

    public void OnExit()
    {

    }

    private void ChangeLanguage(string text)
    {
        foreach (var lang in Settings.LanguageVoiceMap.Keys)
        {
            if (!LanguagePhraseMap.ContainsKey(lang)) continue;

            foreach (var phrase in LanguagePhraseMap[lang])
            {
                if (phrase.Contains(text.ToLower()))
                {
                    CurrentLanguage.Value = lang;
                    return;
                }
            }
        }
    }
}
