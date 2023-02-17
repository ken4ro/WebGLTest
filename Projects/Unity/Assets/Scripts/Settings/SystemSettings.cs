using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SystemSettings : SingletonBase<SystemSettings>
{

    public string PreSendPayload;       //通話前転送文字列


    public SystemSettings Clone() => (SystemSettings)MemberwiseClone();
}
