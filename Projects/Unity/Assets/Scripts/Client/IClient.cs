/// <summary>
/// クライアントインターフェース
/// </summary>
public interface IClient
{
    void Initialize();

    void Dispose();

    string GetSignalingLoginKey();

    string GetSignalingLoginValue();

    string GetSignalingLogoutKey();

    string GetSignalingLogoutValue();

    string GetSignalingCancelKey();

    void AddHandler();

    void RemoveHandler();

    void ReceivedSystemCall(byte[] bytes);

    void ReceivedSignalingMessage(string msg);
}
