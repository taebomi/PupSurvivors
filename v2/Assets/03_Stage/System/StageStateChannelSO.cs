using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PupSurvivors.Stage
{
    [CreateAssetMenu(menuName = "PupSurvivors/StageStateChannel", fileName = "StageStateChannelSO", order =9999)]
    public class StageStateChannelSO : ScriptableObject
    {
        public UnityAction<StageState> OnStageStateChanged;

        public void RaiseEvent(StageState state)
        {
            OnStageStateChanged?.Invoke(state);
        }
    }
    
    
    public enum StageState
    {
        None,
        Init,
        Ready,
        Start,
        GameOver,
        GameFinished,
    }

}