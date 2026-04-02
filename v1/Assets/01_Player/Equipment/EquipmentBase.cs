using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PupSurvivors.Equipment
{
    public abstract class EquipmentBase : MonoBehaviour
    {
        protected static PlayerController Player;
        public int CurrentLevel { get; protected set; }
    
        protected CancellationTokenSource DestroyCts;
    
        protected void Awake()
        {
            if (!Player)
            {
                Player = PlayerController.Instance;
            }
            DestroyCts = new CancellationTokenSource();
            CurrentLevel = 1;
            Initialize().Forget();
        }

        protected virtual void OnDestroy()
        {
            DestroyCts.CancelAndDispose();
        }

        protected abstract UniTaskVoid Initialize();



        public abstract EquipmentData GetEquipmentData();

        public abstract void LevelUp();
    }
}