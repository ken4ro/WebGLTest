using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static BotManager;

public class BotLanguageProcessorChinese : ABotLanguageProcessor
{
    public override string NoMatchText { get; set; } = "我不太懂。请再说一遍。";

    public override string GetText(BotResponse response) => response.Text.Ch;

    public override string GetVoice(BotResponse response) => response.Voice.Ch;

    protected override string GetSelectText(BotResponseSelect select) => select.Ch;
}
