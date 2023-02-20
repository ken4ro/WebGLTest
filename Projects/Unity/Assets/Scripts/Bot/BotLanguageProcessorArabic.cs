using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using static BotManager;

public class BotLanguageProcessorArabic : ABotLanguageProcessor
{
    public override string NoMatchText { get; set; } = "لم افهم جيدا يرجى التحدث مرة أخرى.";

    public override string GetText(BotResponse response) => response.Text.Ar;

    public override string GetVoice(BotResponse response) => response.Voice.Ar;

    protected override string GetSelectText(BotResponseSelect select) => select.Ar;
}
