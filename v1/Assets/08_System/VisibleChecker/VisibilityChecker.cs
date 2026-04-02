using UnityEngine;
using UnityEngine.Events;

public class VisibilityChecker : MonoBehaviour
{
    [field: SerializeField] public UnityEvent VisibleEvent { get; private set; }
    [field: SerializeField] public UnityEvent InvisibleEvent { get; private set; }

    private int _visibleCount;
    private int _visibilityObjectNum;

    private void Awake()
    {
        _visibleCount = 0;
        _visibilityObjectNum = 0;
    }

    public void AddVisibilityObject()
    {
        _visibilityObjectNum++;
    }

    public void UpdateVisibility(bool value)
    {
        if (value)
        {
            _visibleCount++;
        }
        else
        {
            _visibleCount--;
        }

        _visibleCount = Mathf.Clamp(_visibleCount, 0, _visibilityObjectNum + 1);

        if (_visibleCount == _visibilityObjectNum)
        {
            VisibleEvent.Invoke();
        }
        else if (_visibleCount == 0)
        {
            InvisibleEvent.Invoke();
        }
    }
}