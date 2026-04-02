using UnityEngine;

public class EmoticonableObject : MonoBehaviour
{
    [SerializeField] private Transform emotionBubblePointTr;
    [SerializeField] private EmoticonBubble.EmoticonType currentEmoticonType;

    private EmoticonBubble _currentEmoticonBubble;

    // 감정표현 변경 필요시 설정
    public void SetEmotionType(EmoticonBubble.EmoticonType emoticonType)
    {
        currentEmoticonType = emoticonType;
    }

    // 현재 감정표현 생성
    public void CreateEmotionBubble()
    {
        if (_currentEmoticonBubble)
        {
            return;
        }

        _currentEmoticonBubble = StageManager.Instance.EmotionBubblePool.Get();
        _currentEmoticonBubble.Set(currentEmoticonType, emotionBubblePointTr);
    }

    // 감정표현 제거
    public void RemoveEmotionBubble()
    {
        if (!_currentEmoticonBubble)
        {
            return;
        }
        
        _currentEmoticonBubble.Remove();
        _currentEmoticonBubble = null;
    }
}
