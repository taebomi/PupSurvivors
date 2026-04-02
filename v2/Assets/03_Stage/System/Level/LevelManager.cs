using System.Collections;
using System.Collections.Generic;
using PupSurvivors.System;
using UnityEngine;

namespace PupSurvivors.Stage
{
    public class LevelManager : Singleton<LevelManager>
    {
        [Header("Level"), SerializeField] private ExpTable expTable;

        [Header("Level - Invoker"), SerializeField]
        private FloatEventChannelSO expRatioChangedEventSO;

        [SerializeField] private IntEventChannelSO levelUpEventSO;
        [SerializeField] private VoidEventChannelSO maxLevelEventSO;

        [Header("Level - Listener"), SerializeField]
        private FloatEventChannelSO expAddEventSO;

        [SerializeField] private FloatEventChannelSO expRatioAddEventSO;

        public int CurLevel { get; private set; }
        public int MaxLevel { get; private set; }
        private float _curExp, _requiredExp, _accumulatedExp;

        protected override void Awake()
        {
            base.Awake();
            CurLevel = 1;
            MaxLevel = expTable.requireExpArr.Length + 1;
            _curExp = 0f;
            _accumulatedExp = 0f;
            expAddEventSO.OnEventRaised += AddExp;
            expRatioAddEventSO.OnEventRaised += AddExpRatio;
            UpdateRequiredExp();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            expAddEventSO.OnEventRaised -= AddExp;
            expRatioAddEventSO.OnEventRaised -= AddExpRatio;
        }
        
        private void AddExp(float exp)
        {
            _accumulatedExp += exp;
            _curExp += exp;

            while (_curExp >= _requiredExp && CurLevel < MaxLevel)
            {
                _curExp -= _requiredExp;
                CurLevel++;
                levelUpEventSO.RaiseEvent(CurLevel);
                UpdateRequiredExp();
                if (CurLevel == MaxLevel)
                {
                    maxLevelEventSO.RaiseEvent();
                }
            }

            if (CurLevel < MaxLevel)
            {
                expRatioChangedEventSO.RaiseEvent(_curExp / _requiredExp);
            }
        }

        private void AddExpRatio(float ratio)
        {
            var value = _requiredExp * ratio;
            _curExp += value;
            while (_curExp >= _requiredExp && CurLevel < MaxLevel)
            {
                _curExp -= _requiredExp;
                var remainedRatio = _curExp / _requiredExp;
                CurLevel++;
                levelUpEventSO.RaiseEvent(CurLevel);
                UpdateRequiredExp();
                _curExp = _requiredExp * remainedRatio;
                if (CurLevel == MaxLevel)
                {
                    maxLevelEventSO.RaiseEvent();
                }
            }

            if (CurLevel < MaxLevel)
            {
                expRatioChangedEventSO.RaiseEvent(_curExp / _requiredExp);
            }
        }


        private void UpdateRequiredExp()
        {
            if (CurLevel < MaxLevel)
            {
                _requiredExp = expTable.requireExpArr[CurLevel - 1];
            }
            else
            {
                _requiredExp = float.PositiveInfinity;
            }
        }
    }
}