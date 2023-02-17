using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static BotManager;

public abstract class ABotLanguageProcessor
{
    public abstract string NoMatchText { get; set; }

    //protected List<string> _selectTexts = new List<string>();
    protected List<BotResponseSelect> _selectObjects = new List<BotResponseSelect>();

    public abstract string GetText(BotResponse response);

    public abstract string GetVoice(BotResponse response);

    protected abstract string GetSelectText(BotResponseSelect select);

    /// <summary>
    /// 選択肢オブジェクト取得
    /// </summary>
    /// <param name="response">ResponseJson</param>
    /// <returns></returns>
    public List<BotResponseSelect> GetSelectObjects(BotResponse response)
    {
        try
        {
            _selectObjects.Clear();
            _selectObjects.AddRange(response.Selects);
        }
        catch
        {
            return null;
        }
        return _selectObjects;
    }

    
}
