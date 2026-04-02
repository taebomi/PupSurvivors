using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    [SerializeField] private AudioSource seAudioSource;
    private HashSet<AudioClip> _playingSoundEffectStack;

    protected override void AwakeAfter()
    {
        _playingSoundEffectStack = new HashSet<AudioClip>();
    }

    public async UniTaskVoid PlaySoundEffect(AudioClip se)
    {
        if (_playingSoundEffectStack.Contains(se))
        {
            return;
        }

        _playingSoundEffectStack.Add(se);
        seAudioSource.PlayOneShot(se);
        await UniTask.Yield();
        _playingSoundEffectStack.Remove(se);

    }
}
