using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
public partial class BotManager : SingletonBase<BotManager>
{
    public enum CommandTypes
    {
        move,
        clear,
        execute
    }

    /// <summary>
    /// ダイアログオプション
    /// </summary>
    [Serializable]
    public class BotResponseScript : ISerializationCallbackReceiver
    {
        [NonSerialized]
        public string Type;
        [NonSerialized]
        public string[] ParameterArray;
        [NonSerialized]
        public string Parameter;

        [SerializeField]
        private string type;
        [SerializeField]
        private string parameter;

        public void OnAfterDeserialize()
        {
            Type = type;
            Parameter = parameter;

            if (parameter == null)
                ParameterArray = null;
            else
                ParameterArray = parameter.Split(',');
        }

        public void OnBeforeSerialize()
        {
            type = Type.ToString();
            parameter = string.Join(",", ParameterArray);
        }
    }

}
