using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PupSurvivors.Stage
{
    public class HitEffect : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer targetSr;
    
        public async UniTaskVoid Play(CancellationToken token)
        {
            var timer = targetSr.color.r * 0.1f;
            while (timer < 0.1f)
            {
                var value = 1 - Easing.OutSine(timer, 0.1f);
                targetSr.color = new Color(value, value, value, 1f);
                timer += Time.deltaTime;
                await UniTask.Yield(token);
            }

            while (timer > 0f)
            {
                var value = 1 - Easing.OutSine(timer, 0.1f);
                targetSr.color = new Color(value, value, value, 1f);
                timer -= Time.deltaTime;
                await UniTask.Yield(token);
            }
        }
    }
}