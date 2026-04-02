using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class InteractableChecker : MonoBehaviour
{
    public LinkedList<InteractableObject> InteractableObjects { get; private set; } = new();
    public InteractableObject CurrentClosestInteractable { get; private set; }

    private CancellationTokenSource _checkingClosestCts;

    public void AddInteractableObject(InteractableObject interactableObject)
    {
        InteractableObjects.AddLast(interactableObject);
        switch (InteractableObjects.Count)
        {
            case 1:
                CurrentClosestInteractable = InteractableObjects.First.Value;
                CurrentClosestInteractable.ShowEmoticon(true);
                break;
            case 2:
                _checkingClosestCts = new CancellationTokenSource();
                CheckClosestInteractableObject().Forget();
                break;
        }
    }

    public void RemoveInteractableObject(InteractableObject interactableObject)
    {
        InteractableObjects.Remove(interactableObject);
        switch (InteractableObjects.Count)
        {
            case 0:
                CurrentClosestInteractable.ShowEmoticon(false);
                CurrentClosestInteractable = null;
                break;
            case 1:
                _checkingClosestCts.CancelAndDispose();
                _checkingClosestCts = null;
                break;
        }
    }

    private async UniTaskVoid CheckClosestInteractableObject()
    {
        while (true)
        {
            InteractableObject closestObject = null;
            var closestDistSqr = float.MaxValue;
            foreach (var interactableObject in InteractableObjects)
            {
                var distSqr = (transform.position - interactableObject.transform.position).sqrMagnitude;
                if (distSqr < closestDistSqr)
                {
                    closestObject = interactableObject;
                    closestDistSqr = distSqr;
                }
            }

            if (closestObject != CurrentClosestInteractable)
            {
                closestObject.ShowEmoticon(true);
                CurrentClosestInteractable.ShowEmoticon(false);
                CurrentClosestInteractable = closestObject;
            }

            await UniTask.Yield(_checkingClosestCts.Token);
        }
    }

    private void OnDisable()
    {
        _checkingClosestCts?.CancelAndDispose();
    }
}
