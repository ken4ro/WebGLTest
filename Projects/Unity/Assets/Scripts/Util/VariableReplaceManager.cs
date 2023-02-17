using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VariableReplaceManager : SingletonBase<VariableReplaceManager>
{
    private Dictionary<string, string> _table = new Dictionary<string, string>();

    /// <summary>
    /// 管理追加
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void SetValue(string key, string value) => _table[key] = value;

    /// <summary>
    /// 管理削除
    /// </summary>
    /// <param name="key"></param>
    public void ClearValue(string key) => _table.Remove(key);

    /// <summary>
    /// 管理値取得
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string GetValue(string key)
    {
        try
        {
            return _table[key];
        }
        catch
        {
            return null;
        }
    }


    public Dictionary<string, string> GetTable() => new Dictionary<string, string>(_table);

    /// <summary>
    /// テーブル初期化
    /// </summary>
    public void ClearTable() => _table.Clear();

    public VariableReplaceManager Clone()
    {

        var obj = (VariableReplaceManager)MemberwiseClone();
        obj._table = new Dictionary<string, string>(_table);
        return obj;
    }


}
