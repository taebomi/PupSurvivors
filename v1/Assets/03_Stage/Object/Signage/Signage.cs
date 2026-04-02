using PupSurvivors.Stage;
using UnityEngine;

public class Signage : MonoBehaviour, IInteractable
{
    [SerializeField] private InteractableObject interactableObject;

    [TextArea]
    public string message;
    
    private void Awake()
    {
        interactableObject.SetInteractable(true);
    }

    public void Interact()
    {
        StageUIManager.Instance.ShowMessageBox(message);
        
    }
}
