using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "PupSurvivors/Event Channel/Bool", fileName = "BoolEventChannelSO", order = 0)]
public class BoolEventChannelSO : ScriptableObject
{
    public UnityAction<bool> OnEventRaised = default;

    public void RaiseEvent(bool value)
    {
        OnEventRaised?.Invoke(value);
    }
}
