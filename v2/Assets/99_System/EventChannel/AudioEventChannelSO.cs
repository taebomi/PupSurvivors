using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "PupSurvivors/Event Channel/AudioClip", fileName = "AudioEventChannelSO", order = 1000)]
public class AudioEventChannelSO : ScriptableObject
{
    public UnityAction<AudioClip> OnAudioRequested;

    public void RaiseEvent(AudioClip audioClip)
    {
        OnAudioRequested?.Invoke(audioClip);
    }
}
