using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PupSurvivors.Stage
{
    public partial class Player : MonoBehaviour
    {
        public int PlayerNumber { get; private set; }

        [field:SerializeField] public PlayerHealthSystem HealthSystem { get; private set; }
        [field: SerializeField] public PlayerEquipment Equipment { get; private set; }

        public EnemyFinder EnemyFinder { get; private set; }
        [field: SerializeField] public PlayerFollower Follower { get; private set; }
        [field: SerializeField] public PlayerFace Face { get; private set; }
        [field: SerializeField] public InteractableChecker InteractableChecker { get; private set; }


        [field: SerializeField] public Rigidbody2D Rb { get; private set; }
        [field: SerializeField] public Animator Animator { get; private set; }


        public Vector2 Direction { get; set; }
        public float CurSpeed { get; set; }
        private const float AccelerationDuration = 0.25f;
        public const float AccelerationMultiplier = 1 / AccelerationDuration;
        private const float DeaccelerationDuration = 0.1f;
        public const float DeaccelerationMultiplier = 1 / DeaccelerationDuration;

        private void Update()
        {
            UpdateInput();
            UpdateStateMachine();
        }


        private void FixedUpdate()
        {
            FixedUpdateStateMachine();
        }

        private void LateUpdate()
        {
            LateUpdateStateMachine();
        }

        private void OnCollisionStay2D(Collision2D other)
        {
            if (!other.gameObject.CompareTag("Enemy"))
            {
                return;
            }
            
            // todo 데미지 받아와서 체력 감소시키기
        }

        public async UniTask Initialize(int playerNumber, string characterName)
        {
            PlayerNumber = playerNumber;
            EnemyFinder = new EnemyFinder(this);
            InitializeStateMachine();
            await InitCharacter(characterName);
            InitializeStats();
            HealthSystem.Initialize(CurStats.hp);
            if (Equipment)
            {
                Equipment.Initialize();
            }

            InitializeInput();
            InitializeStateMachine();

            Face.StartLooking().Forget();
        }


        public void SetFaceAnimation(FaceType faceType)
        {
            Animator.SetInteger(TaeBoMiCache.FaceType, (int)faceType);
        }

        public void UpdateMoveAnimation()
        {
            switch (Direction.x)
            {
                case > 0:
                    Animator.SetBool(TaeBoMiCache.IsRight, true);
                    break;
                case < 0:
                    Animator.SetBool(TaeBoMiCache.IsRight, false);
                    break;
            }

            Animator.SetInteger(TaeBoMiCache.AniType, CurSpeed > 0f ? (int)AniType.Move : (int)AniType.Idle);
        }

    }
}