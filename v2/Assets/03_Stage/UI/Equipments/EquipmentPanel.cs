using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PupSurvivors.Equipment;
using UnityEngine;
using UnityEngine.UI;

namespace PupSurvivors.Stage.UI
{
    public class EquipmentPanel : Button
    {
        [SerializeField] private EquipmentIcon[] weaponIcons, accessoryIcons;

        private PlayerEquipment _playerEquipment;

        public void Initialize(PlayerEquipment playerEquipment)
        {
            _playerEquipment = playerEquipment;

            SetMaxEquipmentNum();

            OnWeaponChanged(playerEquipment.EquippedWeaponDict.Values);
            OnAccessoryChanged(playerEquipment.EquippedAccessoryDict.Values);

            playerEquipment.EquippedWeaponChangedEvent.AddListener(OnWeaponChanged);
            playerEquipment.EquippedAccessoryChangedEvent.AddListener(OnAccessoryChanged);
            return;

            void SetMaxEquipmentNum()
            {
                for (var i = 0; i < playerEquipment.CurMaxWeaponNum; i++)
                {
                    weaponIcons[i].gameObject.SetActive(true);
                }
                for (var i = playerEquipment.CurMaxWeaponNum; i < weaponIcons.Length; i++)
                {
                    weaponIcons[i].gameObject.SetActive(false);
                    
                }
                
                for (var i = 0; i < playerEquipment.CurMaxAccessoryNum; i++)
                {
                    accessoryIcons[i].gameObject.SetActive(true);
                }
                for (var i = playerEquipment.CurMaxAccessoryNum; i < accessoryIcons.Length; i++)
                {
                    accessoryIcons[i].gameObject.SetActive(false);
                    
                }
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_playerEquipment)
            {
                _playerEquipment.EquippedWeaponChangedEvent.RemoveListener(OnWeaponChanged);
                _playerEquipment.EquippedAccessoryChangedEvent.RemoveListener(OnAccessoryChanged);
            }
        }

        private void OnWeaponChanged(IEnumerable<WeaponBase> weapons)
        {
            var idx = 0;
            foreach (var weapon in weapons)
            {
                weaponIcons[idx].Set(weapon);
                idx++;
            }

            for (var i = idx; i < weaponIcons.Length; i++)
            {
                weaponIcons[i].Clear();
            }
        }

        private void OnAccessoryChanged(IEnumerable<AccessoryBase> accessories)
        {
            var idx = 0;
            foreach (var accessory in accessories)
            {
                accessoryIcons[idx].Set(accessory);
                idx++;
            }

            for (var i = idx; i < accessoryIcons.Length; i++)
            {
                accessoryIcons[i].Clear();
            }
        }
    }
}