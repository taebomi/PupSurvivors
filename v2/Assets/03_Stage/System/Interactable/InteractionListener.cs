using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace PupSurvivors.Stage
{
    // [RequireComponent(typeof(EmotionBubbleController))]
    public class InteractionListener : MonoBehaviour, IInteractable
    {
        public bool CanInteract { get; set; } 
        [field:SerializeField] public UnityEvent<Player> OnInteractRaised { get; private set; }
        [field:SerializeField] public UnityEvent<bool> OnSetInteractionRaised { get; private set; }

        public void Interact(Player subject)
        {
            OnInteractRaised.Invoke(subject);
        }

        public void SetInteraction(bool value)
        {
            OnSetInteractionRaised.Invoke(value);
        }

        public void SetInteractable(bool value)
        {
            CanInteract = value;
        }
    }
}