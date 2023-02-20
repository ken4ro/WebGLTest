using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class DialogflowService
{
    [Serializable]
    public class DialogflowSettings : ISerializationCallbackReceiver
    {
        [NonSerialized]
        public string ProjectId;

        [SerializeField]
        private string project_id;

        public void OnBeforeSerialize()
        {
            project_id = ProjectId;
        }

        public void OnAfterDeserialize()
        {
            ProjectId = project_id;
        }
    }
}
