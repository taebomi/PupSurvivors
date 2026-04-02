using System.Collections;
using System.Collections.Generic;
using PupSurvivors.Input;
using PupSurvivors.System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PupSurvivors.System.Debug
{
    public class DebugManager : UniqueSingleton<DebugManager>, PupSurvivorsInputAction.IDebugActions
    {
        [SerializeField] private Canvas debugUI;
        [field:SerializeField] public Transform MenuContainerTr { get; private set; }
        [field:SerializeField] public Transform SubMenuContainerTr { get; private set; }
    
        [SerializeField] private DebugMenu menuPrefab;
        [SerializeField] private Transform subMenuPrefab;
        [SerializeField] private DebugToggle togglePrefab;
    
        protected override void Initialize()
        {
            InputManager.Instance.InputAction.Debug.Enable();
            InputManager.Instance.InputAction.Debug.SetCallbacks(this);
        }

        public void OnToggleDebugMode(InputAction.CallbackContext context)
        {
            if (context.phase is InputActionPhase.Started)
            {
                debugUI.enabled = !debugUI.enabled;
            }
        }
        
        public DebugToggle CreateToggle(string subMenuName, string toggleName)
        {
            var subMenuTr = GetSubMenu(subMenuName);

            var toggle = Instantiate(togglePrefab, subMenuTr);
            toggle.Set(toggleName);
            return toggle;
        }

        private Transform GetSubMenu(string subMenuName)
        {
            var subMenuTr = SubMenuContainerTr.Find(subMenuName);
            if (subMenuTr == null)
            {
                subMenuTr = Instantiate(subMenuPrefab, SubMenuContainerTr);
                subMenuTr.name = subMenuName;
                subMenuTr.gameObject.SetActive(false);
                CreateMenu(subMenuName, subMenuTr);
            }

            return subMenuTr;
        }
        

        private void CreateMenu(string menuName, Component subMenuTr)
        {
            var menu = Instantiate(menuPrefab, MenuContainerTr);
            menu.Init(menuName, subMenuTr.gameObject);
        }

    }
}