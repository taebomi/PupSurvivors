using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PupSurvivors.Stage
{
    public class InteractableChecker : MonoBehaviour
    {
        public IInteractable ClosestInteractable { get; private set; }
    
        private LinkedList<IInteractable> _interactables;

        private CancellationTokenSource _destroyCts;

        private void Awake()
        {
            _destroyCts = new CancellationTokenSource();
            _interactables = new LinkedList<IInteractable>();
            FindClosestInteractable().Forget();
        }

        private void OnDestroy()
        {
            _destroyCts.CancelAndDispose();
        }

        private async UniTaskVoid FindClosestInteractable()
        {
            while (_destroyCts.IsCancellationRequested is false)
            {
                IInteractable closestInteractable = null;
                var closestDistSqr = float.MaxValue;
                foreach (var interactable in _interactables)
                {
                    if (!interactable.CanInteract)
                    {
                        continue;
                    }
                    var distSqr = (transform.position - interactable.transform.position).sqrMagnitude;
                    if (distSqr < closestDistSqr)
                    {
                        closestInteractable = interactable;
                        closestDistSqr = distSqr;
                    }
                }

                if (ClosestInteractable != closestInteractable)
                {
                    ClosestInteractable?.SetInteraction(false);
                    closestInteractable?.SetInteraction(true);
                    ClosestInteractable = closestInteractable;
                }

                await UniTask.Yield(_destroyCts.Token);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var interactable = other.GetComponent<IInteractable>();
            if (interactable == null)
            {
                Debug.LogWarning($"{other.transform.name} - {typeof(IInteractable)}이 존재하지 않음");
                return;
            }

            _interactables.AddLast(interactable);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var interactable = other.GetComponent<IInteractable>();
            _interactables.Remove(interactable);
        }
    }
}