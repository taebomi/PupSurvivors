using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using PupSurvivors.Stage;
using UnityEngine;

namespace PupSurvivors.Equipment
{
    public abstract class EquipmentBase : MonoBehaviour
    {
        public Player Player { get; private set; }
        public int CurLevel { get; protected set; } = 1;
        public abstract EquipmentData EquipmentData { get; }

        protected CancellationTokenSource DestroyCts;

        public bool IsMaxLevel() => CurLevel == EquipmentData.GetMaxLevel();

        private void Awake()
        {
            DestroyCts = new CancellationTokenSource();
        }

        protected virtual void OnDestroy()
        {
            DestroyCts.Cancel();
            DestroyCts.Dispose();
        }

        public void Initialize(Player player)
        {
            Player = player;
            Initialize();
        }

        protected abstract UniTaskVoid Initialize();
        public abstract void LevelUp();
    }
}