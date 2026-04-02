using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

public class RefreshButton : Button
{
    [SerializeField] private TMP_Text text;

    public void SetPriceText(int price)
    {
        text.text = $" <sprite name=\"Dice\"> {price.ToString(),4}<sprite name=\"Nyan\">";
    }

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        base.DoStateTransition(state, instant);
        text.rectTransform.offsetMax = state switch
        {
            SelectionState.Normal => new Vector2(0f, 0f),
            SelectionState.Highlighted => new Vector2(0f, 0f),
            SelectionState.Pressed => new Vector2(0f, -25f),
            SelectionState.Selected => new Vector2(0f, 0f),
            SelectionState.Disabled => new Vector2(0f, 0f),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
        };
    }
    
}