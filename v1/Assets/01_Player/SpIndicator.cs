using UnityEngine;

public class SpIndicator : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;

    private float _firstValue;
    private bool _isFirstUpdate;

    private const float IndicatorLength = 1.5f;
    private const float IndicatorHeight = 0.125f;
    
    private void Awake()
    {
        sr.enabled = false;
    }

    public void OnSpUsed()
    {
        _isFirstUpdate = true;
    }

    // 데쉬 게이지가 1 이하일 경우 채워지기 까지 얼마나 남았는지 보여줌.
    // 첫 데쉬 사용 후 남아있는 게이지 량에 비례한 속도로 보여줌.
    public void OnSpUpdated(float value)
    {
        if (value >= 1f) // 게이지가 1미만일 경우에만 보여줌
        {
            sr.enabled = false;
            return;
        }

        if (_isFirstUpdate) // 1 미만일 때 첫 번째 값 체크용
        {
            _isFirstUpdate = false;
            _firstValue = 1f - value;
            sr.enabled = true;
        }

        // 남은 시간에 비례하여 줄어들음.
        sr.size = new Vector2((1 - value) / _firstValue * IndicatorLength, IndicatorHeight);
    }
}