using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cinemachine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using PupSurvivors.Stage.PathFinding;
using PupSurvivors.System;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace PupSurvivors.Stage
{
    public partial class CameraManager : Singleton<CameraManager>
    {
        [field: SerializeField] public CinemachineVirtualCamera MainVCam { get; private set; }
        [field: SerializeField] public Camera MainCam { get; private set; }
        [SerializeField] private CinemachineTargetGroup cinemachineTargetGroup;

        public const float Ratio = 16f / 9f;
        public const float HalfHeight = 9f;
        public const float HalfWidth = HalfHeight * Ratio;


        public UnityEvent<Vector2> posChanged;

        private CancellationTokenSource _destroyCts;

        protected override void Awake()
        {
            base.Awake();
            _destroyCts = new CancellationTokenSource();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _destroyCts.CancelAndDispose();
        }

        public void Initialize(IEnumerable<Player> players)
        {
            cinemachineTargetGroup.m_Targets = null;
            foreach (var player in players)
            {
                cinemachineTargetGroup.AddMember(player.transform, 1f, 0f);
            }
        }

        private void Start()
        {   
            Check().Forget();
        }

        private async UniTaskVoid Check()
        {
            while (true)
            {
                posChanged.Invoke(MainVCam.transform.position);

                await UniTask.Yield(_destroyCts.Token);
            }
        }
    }
}