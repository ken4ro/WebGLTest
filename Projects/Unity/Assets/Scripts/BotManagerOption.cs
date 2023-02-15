using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BotManager : SingletonBase<BotManager>
{

    /// <summary>
    /// ダイアログオプション
    /// </summary>
    [Serializable]
    public class BotResponseOption : ISerializationCallbackReceiver
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


    public enum OptionTypes
    {
        fullScreen
    }

}
