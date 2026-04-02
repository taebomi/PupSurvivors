using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PupSurvivors.System.Debug
{
    public class DebugMenu : Toggle
    {
        private static DebugMenu _curDebugMenu;
        [SerializeField] private GameObject subMenuContainerTr;

        protected override void Awake()
        {
            base.Awake();
            onValueChanged.AddListener(OnValueChanged);
        }

        public void Init(string menuName, GameObject subMenuContainer)
        {
            transform.name = menuName;
            GetComponentInChildren<TMP_Text>().text = menuName;
            subMenuContainerTr = subMenuContainer;
        }

        private void OnValueChanged(bool value)
        {
            if (_curDebugMenu != null && _curDebugMenu != this)
            {
                _curDebugMenu.isOn = false;
            }

            var rectTr = GetComponentInChildren<TMP_Text>().GetComponent<RectTransform>();

            if (value)
            {
                subMenuContainerTr.gameObject.SetActive(true);
                rectTr.offsetMin = new Vector2(0, -15);
                rectTr.offsetMax = new Vector2(0, -15);
            }
            else
            {
                subMenuContainerTr.gameObject.SetActive(false);
                rectTr.offsetMin = new Vector2(0, 0);
                rectTr.offsetMax = new Vector2(0, 0);
            }

            _curDebugMenu = this;
        }
    }
}