using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMask : MonoBehaviour
{
    [SerializeField] private SpriteMask mask;
    
    public void ActivateMask(bool value)
    {
        mask.enabled = value;
    }
}
