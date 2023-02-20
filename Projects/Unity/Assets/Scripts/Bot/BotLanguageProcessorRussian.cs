using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static BotManager;

public class BotLanguageProcessorRussian : ABotLanguageProcessor
{
    public override string NoMatchText { get; set; } = "Я не очень хорошо понял. Пожалуйста, говорите снова.";

    public override string GetText(BotResponse response) => response.Text.Ru;

    public override string GetVoice(BotResponse response) => response.Voice.Ru;

    protected override string GetSelectText(BotResponseSelect select) => select.Ru;
}
