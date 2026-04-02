using PupSurvivors.Stage;
using UnityEngine;

public interface IInteractable
{
    public Transform transform { get; }
    public bool CanInteract { get; }
    
    void Interact(Player subject);
    void SetInteraction(bool value);
}