using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

public partial class StageManager
{
    public Transform ObjectPoolContainer { get; private set; }
    public Transform PlayerPoolContainer { get; private set; }
    
    private void InitializePool()
    {
        ObjectPoolContainer = new GameObject("Object Pool").transform;
        ObjectPoolContainer.SetParent(transform);
        PlayerPoolContainer = new GameObject("Player").transform;
        PlayerPoolContainer.SetParent(ObjectPoolContainer);
        
        CreateEmotionBubblePool();
    }

    #region 이모티콘풍선

    [FormerlySerializedAs("emotionBubblePrefab")] [SerializeField] private EmoticonBubble emoticonBubblePrefab;
    public IObjectPool<EmoticonBubble> EmotionBubblePool { get; private set; }

    private void CreateEmotionBubblePool()
    {
        EmotionBubblePool = new ObjectPool<EmoticonBubble>(CreateEmotionBubble, OnGetEmotionBubble,
            OnReleaseEmotionBubble, OnDestroyEmotionBubble, false, 4);
    }

    private EmoticonBubble CreateEmotionBubble()
    {
        var emotionBubble = Instantiate(emoticonBubblePrefab, ObjectPoolContainer);
        emotionBubble.SetManagedPool(EmotionBubblePool);
        return emotionBubble;
    }

    private static void OnGetEmotionBubble(EmoticonBubble emoticonBubble)
    {
        emoticonBubble.gameObject.SetActive(true);
    }

    private static void OnReleaseEmotionBubble(EmoticonBubble emoticonBubble)
    {
        emoticonBubble.gameObject.SetActive(false);
    }

    private static void OnDestroyEmotionBubble(EmoticonBubble emoticonBubble)
    {
        Destroy(emoticonBubble.gameObject);
    }

    #endregion
}