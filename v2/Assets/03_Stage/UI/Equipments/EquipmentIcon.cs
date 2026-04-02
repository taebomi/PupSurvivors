using System;
using System.Collections;
using System.Collections.Generic;
using PupSurvivors.Equipment;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PupSurvivors.Stage.UI
{
    public class EquipmentIcon : MonoBehaviour
    {
        // todo 마우스 올릴 시 정보 표시하기
        [SerializeField] private Image background, icon;
        [SerializeField] private TMP_Text level;
        [SerializeField] private SpriteContainer backgroundSprites;

        private void Awake() => Clear();
        public void Set(EquipmentBase equipment)
        {
            var data = equipment.EquipmentData;
            background.sprite = data.rarity switch
            {
                EquipmentRarity.Normal => backgroundSprites.data[1],
                EquipmentRarity.Rare => backgroundSprites.data[2],
                _ => throw new Exception("알 수 없는 Rarity")
            };

            icon.sprite = data.icon;
            level.text = equipment.CurLevel == data.GetMaxLevel() ? "MAX" : $"Lv. {equipment.CurLevel}";
            icon.enabled = level.enabled = true;
        }

        public void Clear()
        {
            background.sprite = backgroundSprites.data[0];
            icon.enabled = level.enabled = false;
        }
    }
}