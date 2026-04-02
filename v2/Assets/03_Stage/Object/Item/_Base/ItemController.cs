using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace PupSurvivors.Stage.Item
{
    /// <summary>
    /// 아이템 위치 설정 / 특정 위치로 드롭 / 위아래로 흔들리는 효과 조작
    /// </summary>
    public abstract class ItemController : MonoBehaviour, IItem
    {
        [SerializeField] protected ItemColliderDictSO itemColliderDictSO;
        [SerializeField] protected AudioEventChannelSO seEventChannelSO;

        [SerializeField] protected SortingGroup bodySortingGroup;
        [SerializeField] protected Transform bodyTr;
        [SerializeField] protected Rigidbody2D rb;
        [SerializeField] protected CircleCollider2D circleColl;

        [SerializeField] protected AudioClip drawSe, useSe;

        public bool IsVisible { get; private set; }

        protected bool CanFlow;
        private float _bodyYPos, _flowTimer;

        private CancellationTokenSource _destroyCts;

        public abstract void Set(Vector3 spawnPos);
        public abstract UniTaskVoid Set(Vector3 spawnPos, Vector3 destPos);
        public abstract void Apply(Player target);


        protected virtual void Awake()
        {
            _destroyCts = new CancellationTokenSource();
            itemColliderDictSO.CollDict.Add(circleColl, this);

            rb.simulated = false;
            _flowTimer = Random.Range(0f, Mathf.PI * 2f);
            _bodyYPos = bodyTr.localPosition.y;
        }

        private void OnDestroy()
        {
            _destroyCts.CancelAndDispose();
            itemColliderDictSO.CollDict.Remove(circleColl);
        }

        public virtual void OnVisibleChanged(bool isVisible)
        {
            IsVisible = isVisible;
            if (isVisible && CanFlow)
            {
                Flow().Forget();
            }
        }

        protected async UniTask MoveTo(Vector3 firstPos, Vector3 destPos)
        {
            var dist = (firstPos - destPos).magnitude;
            const float speed = 3f;
            var movingDuration = dist / speed;
            var timer = 0f;
            while (timer < movingDuration)
            {
                var t = timer / movingDuration;
                transform.position = Vector3.Slerp(firstPos, destPos, t);
                const float jumpYPos = 2f;
                var curBodyYPos = _bodyYPos + jumpYPos * Mathf.Sin(Mathf.PI * t);
                bodyTr.localPosition = new Vector3(0f, curBodyYPos);
                timer += Time.deltaTime;
                await UniTask.Yield(_destroyCts.Token);
            }
        }

        protected async UniTaskVoid Flow()
        {
            while (IsVisible && CanFlow)
            {
                const float halfFlowingHeight = 0.125f;
                var curHeight = _bodyYPos + halfFlowingHeight * Mathf.Sin(_flowTimer);
                bodyTr.localPosition = new Vector3(0f, curHeight);
                _flowTimer += Time.deltaTime;
                await UniTask.Yield(_destroyCts.Token);
            }
        }

        public async UniTaskVoid Obtain(Player target)
        {
            CanFlow = false;
            rb.simulated = false;
            bodySortingGroup.sortingOrder = 1;

            seEventChannelSO.RaiseEvent(drawSe);

            var tr = transform;
            var targetTr = target.transform;

            await Bounce();
            await Draw();
            Apply(target);

            return;


            async UniTask Bounce()
            {
                const float bounceDist = 1.5f;
                const float bounceDuration = 0.5f;

                var firstPos = tr.position;
                var destPos = (firstPos - targetTr.position).normalized * bounceDist;
                var bodyYPos = bodyTr.localPosition.y;

                var timer = 0f;
                while (timer < bounceDuration)
                {
                    var easingValue = Easing.OutQuad(timer, bounceDuration);
                    tr.position = firstPos + destPos * easingValue;
                    bodyTr.localPosition = new Vector3(0f, bodyYPos + easingValue * 0.5f);
                    timer += Time.deltaTime;
                    await UniTask.Yield(_destroyCts.Token);
                }
            }

            async UniTask Draw()
            {
                var timer = 0f;
                var sqrDist = float.MaxValue;

                while (sqrDist > 0.1f)
                {
                    const float speedIncreasingInSineDuration = 0.5f;
                    const float defaultChasingSpeed = 12f;
                    const float increasingChasingSpeed = defaultChasingSpeed * 1 / speedIncreasingInSineDuration;

                    // 일정 시간 inSine 형태로 속도 증가, 이후로 계속 linear하게 증가함
                    float drawingSpeed;
                    if (timer < speedIncreasingInSineDuration)
                    {
                        drawingSpeed = Easing.InSine(timer, speedIncreasingInSineDuration) * defaultChasingSpeed;
                    }
                    else
                    {
                        drawingSpeed = timer * increasingChasingSpeed;
                    }

                    tr.position = MoveTowards(tr.position, targetTr.position, drawingSpeed * Time.deltaTime);
                    bodyTr.localPosition =
                        new Vector3(0f, Mathf.MoveTowards(bodyTr.localPosition.y, 0f, Time.deltaTime));

                    timer += Time.deltaTime;
                    await UniTask.Yield(_destroyCts.Token);
                }

                return;

                Vector3 MoveTowards(Vector3 current, Vector3 dest, float maxDistanceDelta)
                {
                    var num1 = dest.x - current.x;
                    var num2 = dest.y - current.y;
                    var num3 = dest.z - current.z;
                    sqrDist = (float)(num1 * (double)num1 + num2 * (double)num2 + num3 * (double)num3);
                    if (sqrDist == 0.0 || maxDistanceDelta >= 0.0 &&
                        sqrDist <= maxDistanceDelta * (double)maxDistanceDelta)
                        return dest;
                    var num4 = (float)Math.Sqrt(sqrDist);
                    return new Vector3(current.x + num1 / num4 * maxDistanceDelta,
                        current.y + num2 / num4 * maxDistanceDelta, current.z + num3 / num4 * maxDistanceDelta);
                }
            }
        }
    }
}