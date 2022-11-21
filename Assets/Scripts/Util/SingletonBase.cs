// ジェネリックなシングルトン基底クラス
public class SingletonBase<T> where T : class, new()
{
    /// <summary>
    /// インスタンス
    /// </summary>
    public static T Instance { get; } = new T();

    protected SingletonBase() {}
}
