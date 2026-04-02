using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PupSurvivors.Stage
{
    public abstract class HealthSystemBase : MonoBehaviour
    {
        protected float MaxHp;

        private float _curHp;

        public float CurHp
        {
            get => _curHp;
            protected set => _curHp = Mathf.Clamp(value, 0f, MaxHp);
        }

        public bool IsLive => CurHp > 0f;


        public void SetMaxHp(float maxHp)
        {
            MaxHp = maxHp;
        }

        public void Damage(float damage)
        {
            _curHp -= damage;
        }
    }
}