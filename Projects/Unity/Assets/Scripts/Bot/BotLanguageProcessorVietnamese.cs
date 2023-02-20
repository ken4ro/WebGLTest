using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static BotManager;

public class BotLanguageProcessorVietnamese : ABotLanguageProcessor
{
    public override string NoMatchText { get; set; } = "Tôi không hiểu. Xin hãy nói lại.";

    public override string GetText(BotResponse response) => response.Text.Vi;

    public override string GetVoice(BotResponse response) => response.Voice.Vi;

    protected override string GetSelectText(BotResponseSelect select) => select.Vi;
}
