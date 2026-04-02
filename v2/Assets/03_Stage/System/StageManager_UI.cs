using System.Collections;
using System.Collections.Generic;
using PupSurvivors.Pool;
using UnityEngine;

namespace PupSurvivors.Stage
{
    public partial class StageManager
    {
        private Transform _worldUIContainer;

        private void InitializeUI()
        {
            _worldUIContainer = new GameObject("World UI").transform;
            InitializeFloatingDamage();
            InitializeEmotionBubble();
        }


        #region EmotionBubble

        public ObjectPool<EmotionBubble> EmotionBubblePool { get; private set; }
        [SerializeField] private EmotionBubble emotionBubblePrefab;
        private Transform _emotionBubbleContainer;

        private void InitializeEmotionBubble()
        {
            _emotionBubbleContainer = new GameObject("Emotion Bubble").transform;
            _emotionBubbleContainer.SetParent(_worldUIContainer);

            EmotionBubblePool = new ObjectPool<EmotionBubble>(() =>
                {
                    var emotionBubble = Instantiate(emotionBubblePrefab, _emotionBubbleContainer);
                    emotionBubble.Initialize(EmotionBubblePool);
                    return emotionBubble;
                }, 
                5,
                emotionBubble => emotionBubble.gameObject.SetActive(true),
                emotionBubble => emotionBubble.gameObject.SetActive(false));
        }

        #endregion

        #region Floating Damage

        private LimitedObjectPool<FloatingDamage> _floatingDamagePool;

        [SerializeField] private FloatingDamage floatingDamagePrefab;
        private Transform _floatingDamageContainer;

        private void InitializeFloatingDamage()
        {
            _floatingDamageContainer = new GameObject("Floating Damage").transform;
            _floatingDamageContainer.SetParent(_worldUIContainer);
            _floatingDamagePool = new LimitedObjectPool<FloatingDamage>(
                () =>
                {
                    var floatingDamage = Instantiate(floatingDamagePrefab, _floatingDamageContainer);
                    floatingDamage.Initialize(_floatingDamagePool);
                    return floatingDamage;
                }, 75);
        }

        public void ShowFloatingDamage(int damage, bool isCritical, Vector3 pos)
        {
            var floatingDamage = _floatingDamagePool.Get();
            floatingDamage.Set(damage, isCritical, pos);
        }

        #endregion
    }
}