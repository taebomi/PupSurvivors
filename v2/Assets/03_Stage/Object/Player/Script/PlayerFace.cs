using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Xml.Schema;
using Cysharp.Threading.Tasks;
using PupSurvivors.Stage;
using UnityEngine;

public class PlayerFace : MonoBehaviour
{
    [SerializeField] private Player player;

    [SerializeField] private Transform faceTr;
    private Transform _target;

    public const float FaceDist = 0.1f;
    public const float LookDist = 3f;

    private CancellationTokenSource _lookingCts;

    private void OnDestroy()
    {
        _lookingCts?.CancelAndDispose();
    }

    public async UniTaskVoid StartLooking()
    {
        if (_lookingCts != null)
        {
            _lookingCts.Cancel();
            _lookingCts.Dispose();
        }

        _lookingCts = new CancellationTokenSource();


        var interactableChecker = player.InteractableChecker;
        var enemyFinder = player.EnemyFinder;
        while (_lookingCts.IsCancellationRequested is false)
        {
            var closestInteractable = interactableChecker.ClosestInteractable;
            if (closestInteractable != null &&
                (closestInteractable.transform.position - transform.position).sqrMagnitude < LookDist * LookDist)
            {
                _target = closestInteractable.transform;
            }
            else
            {
                var nearestDamagable = enemyFinder.NearestDamagable;
                if (nearestDamagable && (nearestDamagable.transform.position - transform.position).sqrMagnitude < LookDist * LookDist)
                {
                    _target = nearestDamagable.transform;
                }
                else
                {
                    _target = null;
                }
            }



            Vector3 destPos;
            if (_target)
            {
                var dir = _target.position - transform.position;
                destPos = dir.normalized * FaceDist;
            }
            else
            {
                destPos = Vector3.zero;
            }

            faceTr.localPosition = Vector3.Lerp(faceTr.localPosition, destPos, Time.deltaTime * 5f);

            await UniTask.Yield(_lookingCts.Token);
        }
    }

    public void StopLooking()
    {
        _lookingCts?.Cancel();
    }
}