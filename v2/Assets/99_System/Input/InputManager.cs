using System.Collections;
using System.Collections.Generic;
using PupSurvivors.Input;
using PupSurvivors.System;
using UnityEngine;

public class InputManager : UniqueSingleton<InputManager>
{
    public PupSurvivorsInputAction InputAction { get; private set; }
    protected override void Initialize()
    {
        InputAction = new PupSurvivorsInputAction();
        InputAction.Enable();
    }
}
