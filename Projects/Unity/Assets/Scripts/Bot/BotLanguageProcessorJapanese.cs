using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static BotManager;

public class BotLanguageProcessorJapanese : ABotLanguageProcessor
{
    public override string NoMatchText { get; set; } = "良く分かりませんでした。もう一度お話下さい。";

    public override string GetText(BotResponse response) => response.Text.Jp;

    public override string GetVoice(BotResponse response) => response.Voice.Jp;

    protected override string GetSelectText(BotResponseSelect select) => select.Jp;
}
