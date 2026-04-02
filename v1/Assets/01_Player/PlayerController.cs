using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using PupSurvivors.Enemy;
using PupSurvivors.Equipment;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public partial class PlayerController : Singleton<PlayerController>, IMovable
{
    [SerializeField] private Animator ani;
    [field: SerializeField] public SpriteRenderer Sr { get; private set; }
    [field: SerializeField] public Rigidbody2D Rb { get; private set; }

    [field: SerializeField] public PlayerHealthSystem HealthSystem { get; private set; }
    [field: SerializeField] public InteractableChecker InteractableChecker { get; private set; }
    [field: SerializeField] public PlayerFollower Follower { get; private set; }
    [field: SerializeField] public PlayerMask Mask { get; private set; }
    [field: SerializeField] public PlayerEquipment Equipment { get; private set; }
    [field: SerializeField] public PlayerStats Stats { get; private set; }

    public CharacterData CharacterData { get; private set; }

    #region Movement

    public float CurrentSpeed { get; set; }
    public Vector2 velocity;

    // 초당 이동속도 변화량
    public float accelerationSpeed;
    public float deaccelerationSpeed;

    #endregion


    public bool IsRight { get; private set; }

    private CancellationTokenSource _disableCts;

    protected override void AwakeAfter()
    {
        InitializeSkill();
        StageManager.Instance.AddInitQueue(InitializeCharacter());
        InitializeStateMachine();
        BlinkEyes().Forget();
    }

    public async UniTask InitializeCharacter()
    {
        var currentCharacterName = GameManager.Instance.PlayData.selectedCharacterName;
        CharacterData = await Addressables.LoadAssetAsync<CharacterData>($"CharacterData/{currentCharacterName}");
        InitializeStats(CharacterData);
    }

    public void OnStageStateChanged(StageManager.StageState state)
    {
        switch (state)
        {
            case StageManager.StageState.Init:
                break;
            case StageManager.StageState.Start:
                OnStageStarted();
                break;
            case StageManager.StageState.GameOver:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    private void OnStageStarted()
    {
        Equipment.AddEquipment(CharacterData.defaultEquipmentName);
        InitializeInput();
    }

    private void OnEnable()
    {
        _disableCts = new CancellationTokenSource();
    }

    private void OnDisable()
    {
        _disableCts.CancelAndDispose();
    }

    private void OnDestroy()
    {
        if (CharacterData != null)
        {
            Addressables.Release(CharacterData);
        }
    }

    private void Update()
    {
        UpdateDashBuffer();
        UpdateStateMachine();
    }

    // 이동, 데쉬, 죽음 상태
    private void FixedUpdate()
    {
        FixedUpdateStateMachine();
    }


    public void SetMovementAnimation(bool isRight, bool isMoving)
    {
        IsRight = isRight;
        ani.SetBool(TaeBoMiCache.IsRight, isRight);
        ani.SetBool(TaeBoMiCache.IsMoving, isMoving);
    }

    public void SetFaceAnimation(TaeBoMiCache.FaceStateType faceStateType)
    {
        ani.SetInteger(TaeBoMiCache.FaceState, (int)faceStateType);
    }

    private async UniTaskVoid BlinkEyes()
    {
        // todo - 나중에 죽으면 취소되게 수정하기
        while (true)
        {
            ani.SetTrigger(TaeBoMiCache.EyeBlinkTrigger);
            await UniTask.Delay(Random.Range(1500, 2500));
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Enemy"))
        {
            return;
        }

        var power = collision.gameObject.GetComponent<EnemyBase>().Stats.power;
        HealthSystem.Damage(power);
    }
}