using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Febucci.UI.Effects;
using PupSurvivors.Stage.Item;
using PupSurvivors.Stage.UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PupSurvivors.Stage
{
    public class MiniBossChest : MonoBehaviour
    {
        [SerializeField] private EmotionBubbleController emotionBubbleController;
        [SerializeField] private InteractionListener interactionListener;

        [SerializeField] private SpriteRenderer bodySr, shadowSr;

        [SerializeField] private SpriteContainer chestSprites;

        private int _nyan = 3000;
        private float _ratioExp = 3.66f;


        private CancellationTokenSource _destroyCts;

        public static int EpicCount;
        private const int MaxEpicCount = 5;

        public enum Grade
        {
            Common, // 75%
            Rare, // 20%
            Epic, // 5%
        }

        private Grade _grade;

        private static readonly int[] NumPerGrade = { 3, 7, 20 };

        // todo 미니맵에 표시
        // todo 상자 등급이 높으면 특수 효과를 보여주며 경험치, 냥 드롭하기
        // todo 사운드 추가하기

        private void Awake()
        {
            _destroyCts = new CancellationTokenSource();
            interactionListener.SetInteractable(true);
        }

        private void OnDestroy()
        {
            _destroyCts.CancelAndDispose();
        }

        public void OnSetInteraction(bool value)
        {
            if (value)
            {
                emotionBubbleController.SetEmotion(EmotionBubble.Emotion.Chest);
            }
            else
            {
                emotionBubbleController.RemoveEmotionBubble();
            }
        }

        public void Set(float expValue, int nyanValue)
        {
            // 등급 정해주기
            _grade = Random.value switch
            {
                < 0.75f => Grade.Common,
                < 0.95f => Grade.Rare,
                _ => Grade.Epic,
            };
            if (_grade is Grade.Epic || EpicCount > MaxEpicCount)
            {
                _grade = Grade.Epic;
                EpicCount = 0;
            }
            else
            {
                EpicCount++;
            }

            // 등급에 따라 세팅
            switch (_grade)
            {
                case Grade.Common:
                    bodySr.sprite = chestSprites.data[0];
                    _ratioExp = expValue;
                    _nyan = nyanValue;
                    break;
                case Grade.Rare:
                    bodySr.sprite = chestSprites.data[2];
                    _ratioExp = expValue * 2;
                    _nyan = nyanValue * 2;
                    break;
                case Grade.Epic:
                    bodySr.sprite = chestSprites.data[4];
                    _ratioExp = expValue * 4;
                    _nyan = nyanValue * 4;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Interact(Player _)
        {
            interactionListener.SetInteractable(false);
            Open().Forget();
        }

        private async UniTaskVoid Open()
        {
            bodySr.sprite = chestSprites.data[(int)_grade * 2 + 1];
            var trPos = transform.position;

            // 냥 생성
            var numOfNyan = NumPerGrade[(int)_grade];
            var randomNyanList = Nyan.GenerateRandomNyanList(_nyan, numOfNyan);
            foreach (var nyan in randomNyanList)
            {
                StageManager.Instance.CreateNyan(nyan, trPos, trPos + GetRandomPosInDonut(1f, 2f));

                const double interval = 0.1;
                await UniTask.Delay(TimeSpan.FromSeconds(interval), cancellationToken: _destroyCts.Token);
            }

            // 잠깐 대기
            const double delayForNyanToExp = 1;
            await UniTask.Delay(TimeSpan.FromSeconds(delayForNyanToExp), cancellationToken: _destroyCts.Token);

            // 경험치 보석 생성
            var gemNumArr = ExpGem.GetGemNumFromRatio(_ratioExp);
            for (var i = 0; i < gemNumArr.Count; i++)
            {
                var numOfGem = gemNumArr[i];
                var grade = (ExpGem.Grade)i;
                
                if (numOfGem == 0)
                {
                    continue;
                }
                
                for (var j = 0; j < numOfGem; j++)
                {
                    StageManager.Instance.CreateExpGem(grade, trPos, trPos + GetRandomPosInDonut(2.5f, 3.5f));

                    const double gemInterval = 0.05f;
                    await UniTask.Delay(TimeSpan.FromSeconds(gemInterval), cancellationToken: _destroyCts.Token);
                }

                const double gradeInterval = 0.5f;
                await UniTask.Delay(TimeSpan.FromSeconds(gradeInterval), cancellationToken: _destroyCts.Token);
            }
            
            await Fade();
            Destroy(gameObject);

            return;

            Vector3 GetRandomPosInDonut(float insideRadius, float outsideRadius)
            {
                var randomAngle = Random.Range(0f, 2f * Mathf.PI);
                var randomDist = Random.Range(insideRadius, outsideRadius);
                return new Vector3(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)) * randomDist;
            }

            async UniTask Fade()
            {
                var timer = 0f;
                const float duration = 0.5f;
                while (timer < duration)
                {
                    var color = new Color(1f, 1f, 1f, 1 - timer / duration);
                    bodySr.color = color;
                    shadowSr.color = color;
                    timer += Time.deltaTime;
                    await UniTask.Yield(_destroyCts.Token);
                }
            }
        }
    }
}