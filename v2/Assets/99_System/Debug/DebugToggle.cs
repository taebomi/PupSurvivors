using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PupSurvivors.System.Debug
{
    public class DebugToggle : Toggle
    {
        protected override void Awake()
        {
            base.Awake();
            onValueChanged.AddListener(OnValueChanged);
        }

        public void Set(string toggleName)
        {
            transform.name = toggleName;
            GetComponentInChildren<TMP_Text>().text = toggleName;
        }

        private void OnValueChanged(bool value)
        {
            var rectTr = GetComponentInChildren<TMP_Text>().GetComponent<RectTransform>();

            if (value)
            {
                rectTr.offsetMin = new Vector2(0, -15);
                rectTr.offsetMax = new Vector2(0, -15);
            }
            else
            {
                rectTr.offsetMin = new Vector2(0, 0);
                rectTr.offsetMax = new Vector2(0, 0);
            }
        }
    }
}