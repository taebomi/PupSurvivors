using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PupSurvivors.Stage.UI
{
    public class RefreshButton : Button
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private float pressedYPos;

        public void SetPrice(int price)
        {
            text.text = $"<sprite name=\"Refresh\"> {price.ToString(),4}<sprite name=\"Nyan\">";
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);
            text.rectTransform.offsetMax = state switch
            {
                SelectionState.Normal => new Vector2(0f, 0f),
                SelectionState.Highlighted => new Vector2(0f, 0f),
                SelectionState.Pressed => new Vector2(0f, pressedYPos),
                SelectionState.Selected => new Vector2(0f, 0f),
                SelectionState.Disabled => new Vector2(0f, 0f),
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
        }
    }
}