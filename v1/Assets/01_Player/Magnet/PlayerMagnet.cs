using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerMagnet : MonoBehaviour
{
    [SerializeField] private CircleCollider2D circleCollider2D;

    private PlayerController _player;
    private Transform _thisTr;
    private CancellationTokenSource _disableCts;
    
    private void Awake()
    {
        _player = GetComponentInParent<PlayerController>();
        _thisTr = transform;
    }

    private void OnEnable()
    {
        _disableCts = new CancellationTokenSource();
    }

    private void OnDisable()
    {
        _disableCts.CancelAndDispose();
    }

    public void OnStatsUpdated(CharacterStats stats)
    {
        circleCollider2D.radius = stats.magnet * 0.01f;
    }
    
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (StageManager.Instance.ItemColliderDict.TryGetValue(col.GetInstanceID(), out var item))
        {
            if (item.CurrentState != ItemObjectBase.State.CanUse)
            {
                return;
            }
            
            Obtain(item).Forget();
        }

    }

    private async UniTaskVoid Obtain(ItemObjectBase item)
    {
        item.ChangeState(ItemObjectBase.State.Drawing);

        var itemTr = item.transform;
        var itemMainSrTr = item.MainSr.transform;

        await Bounce();
        await Draw();
        
        item.Use(_player);

        async UniTask Bounce()
        {
            const float bounceDistance = 1.5f;
            const float bounceDuration = 0.5f;
            
            
            var firstPos = itemTr.position;
            var destPos = (firstPos - _thisTr.position).normalized * bounceDistance;

            var timer = 0f;
            while (timer < bounceDuration)
            {
                var easingValue = Easing.OutQuad(timer, bounceDuration);
                itemTr.position = firstPos + destPos * easingValue;
                itemMainSrTr.localPosition = new Vector3(0, easingValue * 0.5f);
                timer += Time.deltaTime;
                await UniTask.Yield(_disableCts.Token);
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

                itemTr.position = MoveTowards(itemTr.position, transform.position, drawingSpeed * Time.deltaTime);
                itemMainSrTr.localPosition =
                    new Vector3(0f, Mathf.MoveTowards(itemMainSrTr.localPosition.y, 0f, Time.deltaTime));

                timer += Time.deltaTime;
                await UniTask.Yield(_disableCts.Token);
            }
            
            
            Vector3 MoveTowards(Vector3 current, Vector3 target, float maxDistanceDelta)
            {
                var num1 = target.x - current.x;
                var num2 = target.y - current.y;
                var num3 = target.z - current.z;
                sqrDist = (float)(num1 * (double)num1 + num2 * (double)num2 + num3 * (double)num3);
                if (sqrDist == 0.0 || maxDistanceDelta >= 0.0 &&
                    sqrDist <= maxDistanceDelta * (double)maxDistanceDelta)
                    return target;
                var num4 = (float)Math.Sqrt(sqrDist);
                return new Vector3(current.x + num1 / num4 * maxDistanceDelta,
                    current.y + num2 / num4 * maxDistanceDelta, current.z + num3 / num4 * maxDistanceDelta);
            }
        }
    }
}
