using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PupSurvivors.Stage
{
    public class VisibilityChecker : MonoBehaviour
    {
        [field: SerializeField] public UnityEvent<bool> VisibleChangedEvent { get; private set; }

        public bool IsVisible { get; private set; }


        private void OnBecameVisible()
        {
            if (IsVisible)
            {
                return;
            }

            IsVisible = true;
            VisibleChangedEvent.Invoke(true);
        }

        private void OnBecameInvisible()
        {
            if (!IsVisible)
            {
                return;
            }

            IsVisible = false;
            VisibleChangedEvent.Invoke(false);
        }
    }
}