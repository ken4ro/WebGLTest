using System;

using UnityEngine;


public partial class LocalAIService
{
    [Serializable]
    public class LocalAISettings : ISerializationCallbackReceiver
    {
        [NonSerialized]
        public string ScenarioPath;
        [NonSerialized]
        public string ScenarioId;

        [SerializeField]
        private string scenario_path;
        [SerializeField]
        private string scenario_id;

        public void OnBeforeSerialize()
        {
            scenario_path = ScenarioPath;
            //api_key = ApiKey;
            //bot_id = BotId;
            scenario_id = ScenarioId;
        }

        public void OnAfterDeserialize()
        {
            ScenarioPath = scenario_path;
            //ApiKey = api_key;
            //BotId = bot_id;
            ScenarioId = scenario_id;
        }
    }


    [Serializable]
    public class GetUserIdRequest : ISerializationCallbackReceiver
    {
        [NonSerialized]
        public string BotId;

        [SerializeField]
        private string botId;

        public void OnBeforeSerialize()
        {
            botId = BotId;
        }

        public void OnAfterDeserialize()
        {
            BotId = botId;
        }
    }

    [Serializable]
    public class GetUserIdResponse : ISerializationCallbackReceiver
    {
        [NonSerialized]
        public string AppUserId;

        [SerializeField]
        private string appUserId;

        public void OnBeforeSerialize()
        {
            appUserId = AppUserId;
        }

        public void OnAfterDeserialize()
        {
            AppUserId = appUserId;
        }
    }

    [Serializable]
    public class DialogueRequest : ISerializationCallbackReceiver
    {
        [NonSerialized]
        public string AppUserId;
        [NonSerialized]
        public string BotId;
        [NonSerialized]
        public string VoiceText;
        [NonSerialized]
        public bool InitTalkingFlag;
        [NonSerialized]
        public string InitTopicId;

        [SerializeField]
        private string appUserId;
        [SerializeField]
        private string botId;
        [SerializeField]
        private string voiceText;
        [SerializeField]
        private bool initTalkingFlag;
        [SerializeField]
        private string initTopicId;

        public void OnBeforeSerialize()
        {
            appUserId = AppUserId;
            botId = BotId;
            voiceText = VoiceText;
            initTalkingFlag = InitTalkingFlag;
            initTopicId = InitTopicId;
        }

        public void OnAfterDeserialize()
        {
            AppUserId = appUserId;
            BotId = botId;
            VoiceText = voiceText;
            InitTalkingFlag = initTalkingFlag;
            InitTopicId = initTopicId;
        }
    }

    [Serializable]
    public class DialogueResponse : ISerializationCallbackReceiver
    {
        [NonSerialized]
        public DialogueResponseSystemText SystemText;

        [SerializeField]
        private DialogueResponseSystemText systemText;

        public void OnBeforeSerialize()
        {
            systemText = SystemText;
        }

        public void OnAfterDeserialize()
        {
            SystemText = systemText;
        }
    }

    [Serializable]
    public class DialogueResponseSystemText : ISerializationCallbackReceiver
    {
        [NonSerialized]
        public string Expression;
        [NonSerialized]
        public string Utterance;

        [SerializeField]
        private string expression;
        [SerializeField]
        private string utterance;

        public void OnBeforeSerialize()
        {
            expression = Expression;
            utterance = Utterance;
        }

        public void OnAfterDeserialize()
        {
            Expression = expression;
            Utterance = utterance;
        }
    }
}
