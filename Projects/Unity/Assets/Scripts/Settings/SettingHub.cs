using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingHub : SingletonBase<SettingHub>
{
    public enum AccessTypes
    {
        Cache,
        Master
    }

    public enum SettingTypes
    {
        Application,
        Signage,
        Variable,
        System
    }

    #region base
    public class BaseNode<T>
    {
        public BaseNode(ref T org)
        {
            Origin = org;
            RefreshSetting();
        }
        public T Cache { get; set; }
        public T Origin { get; set; }
        public void RefreshSetting() => Cache = Origin;
    }
    #endregion

    #region system

    public class SystemNode
    {
        public SystemNode() => ResetSetting();
        public SystemSettings Cache { get; set; }
        public SystemSettings Master { get => SystemSettings.Instance; }

        /// <summary>
        /// リフレッシュ
        /// </summary>
        public void ResetSetting() => Cache = SystemSettings.Instance.Clone();

    }
    public SystemNode System = new SystemNode();

    /// <summary>
    /// システム変数上書き
    /// </summary>
    /// <param name="accessType"></param>
    /// <param name="tag"></param>
    /// <param name="value"></param>
    public void OverwriteSystem(AccessTypes accessType, SystemTags tag, string value)
    {
        var block = accessType == AccessTypes.Cache ? System.Cache : System.Master;
        switch (tag)
        {

            //旧バージョン依存でsignageであることに注意
            case SystemTags.CurrentLanguage:
                SignageSettings.CurrentLanguage.Value = (SignageSettings.Language)Enum.Parse(typeof(SignageSettings.Language), value,true);
                break;

            case SystemTags.PreSendPayload:
                block.PreSendPayload = value;
                break;

        }
    }

    #endregion

    #region signage
    /// <summary>
    /// サイネージセッティング
    /// </summary>
    public class SignageNode
    {
        public SignageNode() => ResetSetting();
        public SignageSettings.SignageSetting Cache { get; set; }
        public SignageSettings.SignageSetting Master { get => SignageSettings.Settings; set => SignageSettings.Settings = value; }

        public void ResetSetting() => Cache = SignageSettings.Settings.Clone();
    }
    public SignageNode Signage = new SignageNode();
    public enum SignageTags { BaseTextSpeed, InputLimitTime, RestartWaitTime, ReturnWaitTime, DelayTime }
    public enum SystemTags { CurrentLanguage, PreSendPayload }

    /// <summary>
    /// サイネージ設定上書き
    /// </summary>
    /// <param name="accessType"></param>
    /// <param name="tag"></param>
    /// <param name="value"></param>
    public void OverwriteSignage(AccessTypes accessType, SignageTags tag, string value)
    {
        var block = accessType == AccessTypes.Cache ? Signage.Cache : Signage.Master;
        switch (tag)
        {
            case SignageTags.BaseTextSpeed:
                block.BaseTextSpeed = int.Parse(value);
                break;

            case SignageTags.InputLimitTime:
                block.InputLimitTime = int.Parse(value);
                break;

            case SignageTags.RestartWaitTime:
                block.RestartWaitTime = int.Parse(value);
                break;

            case SignageTags.ReturnWaitTime:
                block.ReturnWaitTime = int.Parse(value);
                break;

            case SignageTags.DelayTime:
                block.DelayTime = int.Parse(value);
                break;
        }
    }

    #endregion

    #region variableReplace
    public class VariableReplaceNode
    {
        public VariableReplaceNode() => ResetSetting();
        public VariableReplaceManager Cache { get; set; }
        public VariableReplaceManager Master { get => VariableReplaceManager.Instance; }

        public void ResetSetting() => Cache = VariableReplaceManager.Instance.Clone();
    }
    public VariableReplaceNode Variable = new VariableReplaceNode();

    public void OverwriteVariable(AccessTypes accessType, string tag, string value)
    {
        var block = accessType == AccessTypes.Cache ? Variable.Cache : Variable.Master;
        block.SetValue(tag, value);
    }

    public void ClearVariable(AccessTypes accessType, string tag)
    {
        var block = accessType == AccessTypes.Cache ? Variable.Cache : Variable.Master;
        block.ClearValue(tag);

    }

    #endregion

    /// <summary>
    /// 初期化
    /// </summary>
    public void ResetSetting()
    {
        Signage.ResetSetting();
        Variable.ResetSetting();
        System.ResetSetting();
    }
    
}
