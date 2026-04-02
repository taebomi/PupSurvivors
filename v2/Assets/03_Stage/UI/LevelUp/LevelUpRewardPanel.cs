using System;
using System.Runtime.CompilerServices;
using System.Threading;
using JetBrains.Annotations;
using PupSurvivors.Equipment;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

namespace PupSurvivors.Stage.UI
{
    public class LevelUpRewardPanel : Button
    {
        [SerializeField] private EquipmentDB equipmentDB;
        [SerializeField] private AudioEventChannelSO seEventChannelSO;

        [SerializeField] private RectTransform rt;
        [SerializeField] private Animator ani;

        [SerializeField] private Image panelImage, iconBoxImage, iconImage, underlineImage;
        [SerializeField] private Image[] progressImages;
        [SerializeField] private TMP_Text titleText, descriptionText, levelText;

        [SerializeField] private SpriteContainer panelSprites, iconBoxSprites, underlineSprites, progressSprites;

        [SerializeField] private AudioClip openCloseSE, burnSE;

        private EquipmentRarity _curRarity;

        private const int ProgressImageNum = 7;
        private const int ProgressImageCenterIdx = ProgressImageNum / 2;

        private CancellationTokenSource _destroyCts;

        [SerializeField] private UnityEvent onOpenFinished, onCloseFinished;


        /// <summary>
        /// 보유 중인 무기일 경우
        /// </summary>
        /// <param name="equipmentBase"></param>
        public void Set(EquipmentBase equipmentBase)
        {
            var equipmentData = equipmentBase.EquipmentData;
            var curLevel = equipmentBase.CurLevel;
            var maxLevel = equipmentData.GetMaxLevel();
            if (curLevel + 1 == maxLevel) // 다음이 만렙일 경우
            {
                levelText.text = "MAX";
                levelText.color = new Color(0.7450981f, 0.1882353f, 0.2895021f);
            }
            else
            {
                levelText.text = $"LV. {curLevel + 1}";
                levelText.color = new Color(0.227451f, 0.1254902f, 0.06666667f);
            }

            SetTitleAndDescription(equipmentData.equipmentName, curLevel + 1);
            SetIcon(equipmentData.icon);
            SetRarity(equipmentData.rarity);
            SetProgress(equipmentData.rarity, curLevel, maxLevel);
        }

        /// <summary>
        /// 첫 획득일 경우
        /// </summary>
        /// <param name="equipmentData"></param>
        public void Set(EquipmentData equipmentData)
        {
            var equipmentName = equipmentData.equipmentName;
            levelText.text = "NEW!";
            levelText.color = new Color(0.9359344f, 1f, 0.240566f);
            SetTitleAndDescription(equipmentName);
            SetIcon(equipmentData.icon);
            SetRarity(equipmentData.rarity);

            if (equipmentData.rarity is not EquipmentRarity.Normal) // 조합으로 상위 등급 첫 획득 시, 조합법 출력
            {
                SetCraft(equipmentName);
            }
            else // 노멀등급 첫 획득 시,
            {
                SetProgress(equipmentData.rarity, 0, equipmentData.GetMaxLevel());
            }
        }

        public void SetPosition(Vector2 pos)
        {
            rt.anchoredPosition = pos;
        }

        private void SetTitleAndDescription(string equipmentName, int level = 1)
        {
            const string tableName = "EquipmentText";
            var title = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, $"{equipmentName}_Title");
            var description =
                LocalizationSettings.StringDatabase.GetLocalizedString(tableName,
                    $"{equipmentName}_Description{level}");
            titleText.text = title;
            descriptionText.text = description;
        }

        private void SetIcon(Sprite sprite)
        {
            iconImage.sprite = sprite;
        }

        private void SetRarity(EquipmentRarity rarity)
        {
            _curRarity = rarity;
            switch (rarity)
            {
                case EquipmentRarity.Normal:
                    panelImage.sprite = panelSprites.data[0];
                    iconBoxImage.sprite = iconBoxSprites.data[0];
                    underlineImage.sprite = underlineSprites.data[0];
                    break;
                case EquipmentRarity.Rare:
                    panelImage.sprite = panelSprites.data[2];
                    iconBoxImage.sprite = iconBoxSprites.data[1];
                    underlineImage.sprite = underlineSprites.data[1];
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(rarity), rarity, null);
            }
        }

        private void SetProgress(EquipmentRarity rarity, int curLevel, int maxLevel)
        {
            Sprite onSprite, offSprite;
            switch (rarity)
            {
                case EquipmentRarity.Normal:
                    onSprite = progressSprites.data[0];
                    offSprite = progressSprites.data[1];
                    break;
                case EquipmentRarity.Rare:
                    onSprite = progressSprites.data[2];
                    offSprite = progressSprites.data[3];
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(rarity), rarity, null);
            }

            var startIdx = ProgressImageCenterIdx - maxLevel / 2;
            var endIdx = ProgressImageCenterIdx + maxLevel / 2;

            for (var idx = 0; idx < startIdx; idx++)
            {
                progressImages[idx].enabled = false;
            }

            for (var idx = startIdx; idx <= endIdx; idx++)
            {
                var progressImage = progressImages[idx];
                progressImage.enabled = true;
                progressImage.sprite = curLevel >= idx - startIdx ? onSprite : offSprite;
            }

            for (var idx = endIdx; idx < ProgressImageNum; idx++)
            {
                progressImages[idx].enabled = false;
            }
        }

        private void SetCraft(string equipmentName)
        {
            var requiredDataList = equipmentDB.recipeDict[equipmentName].ingredients;

            var startIdx = ProgressImageCenterIdx - requiredDataList.Length + 1;
            var endIdx = ProgressImageCenterIdx + requiredDataList.Length;

            for (var idx = 0; idx < startIdx; idx++)
            {
                progressImages[idx].enabled = false;
            }

            for (var idx = startIdx; idx <= endIdx; idx++)
            {
                var progressImage = progressImages[idx];
                progressImage.enabled = true;
                var curIdx = idx - startIdx;
                progressImage.sprite = curIdx % 2 is 0
                    ? requiredDataList[curIdx / 2].equipmentData.icon
                    : progressSprites.data[^1];
            }

            for (var idx = endIdx; idx < ProgressImageNum; idx++)
            {
                progressImages[idx].enabled = false;
            }
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            Select();
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            Highlight(true);
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            Highlight(false);
        }

        public void AniEvent_OpenFinished()
        {
            onOpenFinished.Invoke();
        }

        public void AniEvent_CloseFinished()
        {
            onCloseFinished.Invoke();
            gameObject.SetActive(false);
        }

        public void Open()
        {
            gameObject.SetActive(true);
            ani.SetTrigger(TaeBoMiCache.Open);
            seEventChannelSO.RaiseEvent(openCloseSE);
        }

        public void Close(bool willBurn = false)
        {
            if (willBurn)
            {
                ani.SetTrigger(TaeBoMiCache.Burn);
                seEventChannelSO.RaiseEvent(burnSE);
            }
            else
            {
                ani.SetTrigger(TaeBoMiCache.Close);
                seEventChannelSO.RaiseEvent(openCloseSE);
            }
        }

        private void Highlight(bool value)
        {
            ani.SetBool(TaeBoMiCache.IsSelected, value);
            var spriteIdx = _curRarity switch
            {
                EquipmentRarity.Normal => 0,
                EquipmentRarity.Rare => 2,
                _ => throw new ArgumentOutOfRangeException()
            };

            panelImage.sprite = panelSprites.data[spriteIdx + (value ? 1 : 0)];
        }
    }
}