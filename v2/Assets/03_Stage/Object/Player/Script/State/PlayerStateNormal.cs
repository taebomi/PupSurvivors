using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PupSurvivors.Stage
{
    public class PlayerStateNormal : PlayerStateBase
    {
        public override PlayerState ThisEnum => PlayerState.Normal;

        private CancellationTokenSource _stateCts;

        public PlayerStateNormal(Player player) : base(player)
        {
            OnStateEnter = StateEnter;
            OnUpdate = Update;
            OnFixedUpdate = FixedUpdate;
            OnLateUpdate = LateUpdate;
            OnStateExit = StateExit;
            OnActButtonDown = ActButtonDowned;
        }

        private void StateEnter()
        {
            _stateCts = new CancellationTokenSource();
            Player.SetFaceAnimation(FaceType.Normal);
            BlinkEyes().Forget();
        }

        private void StateExit()
        {
            _stateCts.CancelAndDispose();
        }

        private void Update()
        {
            Player.Direction = Player.LastInputDir;
            CalculateAcceleration();
            Player.UpdateMoveAnimation();
        }

        private void FixedUpdate()
        {
            Player.Rb.linearVelocity = Player.LastInputDir * Player.CurSpeed;
        }

        private void LateUpdate()
        {
            var mainCam = CameraManager.Instance.MainCam;
            var firstPos = mainCam.WorldToViewportPoint(Player.transform.position);
            var changedPos = firstPos;
            if (changedPos.x < 0f) changedPos.x = 0f;
            if (changedPos.y < 0f) changedPos.y = 0f;
            if (changedPos.x > 1f) changedPos.x = 1f;
            if (changedPos.y > 1f) changedPos.y = 1f;
            if (firstPos != changedPos)
            {
                Player.transform.position = mainCam.ViewportToWorldPoint(changedPos);
            }
        }

        private void ActButtonDowned()
        {
            var closestInteractable = Player.InteractableChecker.ClosestInteractable;
             if (closestInteractable != null)
            {
                Player.ResetActButtonCounter();
                closestInteractable.Interact(Player);
                return;
            }
            
            // todo 스킬 사용 시 여기서 체크
        }

        private void CalculateAcceleration()
        {
            var curSpeed = Player.CurSpeed;
            var maxSpeed = Player.CurStats.movementSpeed;
            if (Player.CurInputDir != Vector2.zero) // 가속
            {
                curSpeed += maxSpeed * Player.AccelerationMultiplier * Time.deltaTime;
                if (curSpeed > maxSpeed)
                {
                    curSpeed = maxSpeed;
                }
            }
            else // 감속
            {
                curSpeed -= maxSpeed * Player.DeaccelerationMultiplier * Time.deltaTime;
                if (curSpeed < 0f)
                {
                    curSpeed = 0f;
                }
            }

            Player.CurSpeed = curSpeed;
        }

        private async UniTaskVoid BlinkEyes()
        {
            while (_stateCts.IsCancellationRequested is false)
            {
                await UniTask.Delay(Random.Range(1000, 5000), cancellationToken: _stateCts.Token);
                Player.Animator.SetInteger(TaeBoMiCache.FaceType, (int)FaceType.Close);
                await UniTask.Delay(Random.Range(750, 250), cancellationToken: _stateCts.Token);
                Player.Animator.SetInteger(TaeBoMiCache.FaceType, (int)FaceType.Normal);
            }
        }
    }
}