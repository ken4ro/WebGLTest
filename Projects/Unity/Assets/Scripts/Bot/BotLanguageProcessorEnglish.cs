using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static BotManager;

public class BotLanguageProcessorEnglish : ABotLanguageProcessor
{
    public override string NoMatchText { get; set; } = "I did not understand well. Please talk again.";

    public override string GetText(BotResponse response) => response.Text.En;

    public override string GetVoice(BotResponse response) => response.Voice.En;

    protected override string GetSelectText(BotResponseSelect select) => select.En;
}
