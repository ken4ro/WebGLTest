using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using static BotManager;
using Cysharp.Threading.Tasks;

public interface IBotService
{
    UniTask Initialize();

    UniTask<BotRequestResult> Reset();

    UniTask<BotRequestResult> Request(bool isInit, string inputText);
}
