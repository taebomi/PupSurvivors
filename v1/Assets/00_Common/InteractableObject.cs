using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(EmoticonableObject), typeof(IInteractable))]
public class InteractableObject : MonoBehaviour
{
    private IInteractable _interactable;
    private EmoticonableObject _emoticonableObject;

    [SerializeField] private Collider2D interactableAreaCollider2D;

    private bool _isInteractable;

    [field: SerializeField] public UnityEvent PlayerInteractableEvent { get; private set; }
    [field: SerializeField] public UnityEvent PlayerDisinteractableEvent { get; private set; }

    private void Awake()
    {
        _interactable = GetComponent<IInteractable>();
        _emoticonableObject = GetComponent<EmoticonableObject>();
    }

    public void Interact()
    {
        if (_isInteractable)
        {
            ShowEmoticon(false);
            _interactable.Interact();
        }
    }

    public void ShowEmoticon(bool value)
    {
        if (value)
        {
            _emoticonableObject.CreateEmotionBubble();
        }
        else
        {
            _emoticonableObject.RemoveEmotionBubble();
        }
    }

    public void SetInteractable(bool value)
    {
        _isInteractable = value;
        interactableAreaCollider2D.enabled = value;
    }

    // 플레이어가 상호작용 영역으로 들어올 시
    private void OnTriggerEnter2D(Collider2D col)
    {
        PlayerInteractableEvent.Invoke();
        PlayerController.Instance.InteractableChecker.AddInteractableObject(this);
    }

    // 플레이어가 상호작용 영역 밖으로 벗어날 시
    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerDisinteractableEvent.Invoke();
        PlayerController.Instance.InteractableChecker.RemoveInteractableObject(this);
    }
}