using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

public class VisibilityObject : MonoBehaviour
{
    private VisibilityChecker _checker;

    private void Awake()
    {
        _checker = GetComponentInParent<VisibilityChecker>();
        _checker.AddVisibilityObject();
    }

    private void OnBecameVisible()
    {
        _checker.UpdateVisibility(true);
    }

    private void OnBecameInvisible()
    {
        _checker.UpdateVisibility(false);
    }
}
