using System;
using System.Collections.Concurrent;
using System.Text;

using UnityEngine;


public class FaceInfoManager : SingletonMonoBehaviour<FaceInfoManager>
{
    private static readonly float ReceiveTimeoutSec = 5.0f;
    private float _lastReceivedTime = 0.0f;

    // 顔認識関連
    private ConcurrentQueue<FaceInfo> _faceInfoQueue = new ConcurrentQueue<FaceInfo>();

    private bool _isEnable = false;

    void LateUpdate()
    {
        if (!_isEnable) return;

        if ((Time.time - _lastReceivedTime) > ReceiveTimeoutSec)
        {
            // 顔情報が一定時間送られていない場合はアイドルアニメーション
            //CharacterManager.Instance.EnableAnimation();
        }
        else
        {
            // フェイストラッキング中は固定アニメーションを無効にする
            //CharacterManager.Instance.DisableAnimation();
        }

        if (_faceInfoQueue.Count > 0)
        {
            if (_faceInfoQueue.TryDequeue(out FaceInfo faceInfo))
            {
                // 顔座標更新
                CharacterManager.Instance.FaceUpdate(faceInfo);
            }
            else
            {
                Debug.Log("FaceInfoManager TryDequeue failed");
            }
        }
        else
        {
            //Debug.Log("FaceInfo queue is empty.");
        }
    }

    public void Enable() => _isEnable = true;

    public void Disable() => _isEnable = false;

    public void FaceInfoReceived(byte[] data)
    {
        if (!_isEnable) return;

        _lastReceivedTime = Time.time;
        var faceInfoJson = Encoding.UTF8.GetString(data);
        var faceInfoObj = JsonUtility.FromJson<FaceInfo>(faceInfoJson);
        _faceInfoQueue.Enqueue(faceInfoObj);
    }

    public void FaceInfoReceived(string faceInfoJson)
    {
        if (!_isEnable) return;

        _lastReceivedTime = Time.time;
        var faceInfoObj = JsonUtility.FromJson<FaceInfo>(faceInfoJson);
        _faceInfoQueue.Enqueue(faceInfoObj);
        Debug.Log($"FaceInfoReceived: yaw = {faceInfoObj.Yaw}, pitch = {faceInfoObj.Pitch}, roll = {faceInfoObj.Roll}");
    }

    // For Serialize/Deserialize

    [Serializable]
    public class FaceInfo : ISerializationCallbackReceiver
    {
        [NonSerialized]
        public double Pitch;
        [NonSerialized]
        public double Yaw;
        [NonSerialized]
        public double Roll;
        [NonSerialized]
        public double BodyPitch;
        [NonSerialized]
        public double BodyYaw;
        [NonSerialized]
        public double BodyRoll;

        [SerializeField]
        private double pitch;
        [SerializeField]
        private double yaw;
        [SerializeField]
        private double roll;
        [SerializeField]
        private double bodyPitch;
        [SerializeField]
        private double bodyYaw;
        [SerializeField]
        private double bodyRoll;

        public void OnBeforeSerialize()
        {
            pitch = Pitch;
            yaw = Yaw;
            roll = Roll;
            bodyPitch = BodyPitch;
            bodyYaw = BodyYaw;
            bodyRoll = BodyRoll;
        }

        public void OnAfterDeserialize()
        {
            Pitch = pitch;
            Yaw = yaw;
            Roll = roll;
            BodyPitch = bodyPitch;
            BodyYaw = bodyYaw;
            BodyRoll = bodyRoll;
        }
    }

}
