using System;
using Cysharp.Threading.Tasks;
using PupSurvivors.Equipment;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace PupSurvivors.Stage.UI.LevelUp
{
    public class RewardPanel : Button
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Animator ani;
        [field: SerializeField] public Button Button { get; private set; }
        [SerializeField] private Image containerImage, iconContainerImage, iconImage, underlineImage;
        [SerializeField] private Image[] progressImages;
        [SerializeField] private TMP_Text nameText, descriptionText, levelText;

        [SerializeField] private SpriteContainer panelSprites;
        [SerializeField] private SpriteContainer iconContainerSprites, underlineSprites, progressSprites;


        [SerializeField] private AudioClip openCloseSe, selectSe;

        private int _currentPanelSpriteIdx;

        public static int CurrentActiveNum { get; private set; }


        /// <summary>
        /// 세팅 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="level">업글 시 만렙일 경우 -1 넣어주기</param>
        /// <param name="equipmentName"></param>
        /// <param name="description"></param>
        public void Set(EquipmentData data, int level, string equipmentName, string description)
        {
            iconImage.sprite = data.icon;
            nameText.text = equipmentName;
            descriptionText.text = description;

            var rarity = (int)data.rarity;
            _currentPanelSpriteIdx = rarity * 2;
            containerImage.sprite = panelSprites.data[_currentPanelSpriteIdx];
            iconContainerImage.sprite = iconContainerSprites.data[rarity];
            underlineImage.sprite = underlineSprites.data[rarity];

            var maxLevel = data.GetMaxLevel();
            switch (level)
            {
                case 0:
                    levelText.text = "NEW!";
                    levelText.color = new Color(0.9359344f, 1f, 0.240566f);
                    // 
                    if (data.rarity is not EquipmentRarity.Normal)
                    {
                        SetCraft(data.equipmentName);
                    }
                    else
                    {
                        SetProgress(data.rarity, level, maxLevel);
                    }

                    break;
                case -1:
                    levelText.text = "MAX";
                    levelText.color = new Color(0.7450981f, 0.1882353f, 0.2895021f);
                    SetProgress(data.rarity, maxLevel - 1, maxLevel);
                    break;
                default:
                    levelText.text = $"Lv. {level + 1}";
                    levelText.color = new Color(0.227451f, 0.1254902f, 0.06666667f);
                    SetProgress(data.rarity, level, maxLevel);
                    break;
            }
        }

        public void SetPosition(Vector2 pos)
        {
            rectTransform.anchoredPosition = pos;
        }

        private void SetCraft(string equipmentName)
        {
            var requiredDataList = StageManager.Instance.EquipmentDB.recipeDict[equipmentName]
                .requiredEquipmentDataList;

            var length = requiredDataList.Length * 2 - 1;
            var startIdx = 4 - requiredDataList.Length;

            for (var idx = 0; idx < startIdx; idx++)
            {
                progressImages[idx].enabled = false;
            }

            for (var idx = startIdx; idx < startIdx + length; idx++)
            {
                var progressImage = progressImages[idx];
                progressImage.enabled = true;
                var currentIdx = idx - startIdx;
                progressImage.sprite = currentIdx % 2 is 0
                    ? requiredDataList[(int)(currentIdx * 0.5f)].icon
                    : progressSprites.data[6];
            }

            for (var idx = startIdx + length; idx < progressImages.Length; idx++)
            {
                progressImages[idx].enabled = false;
            }
        }

        private void SetProgress(EquipmentRarity rarity, int currentLevel, int maxLevel)
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

            var startIdx = 3 - (int)(maxLevel * 0.5f);

            for (var idx = 0; idx < startIdx; idx++)
            {
                progressImages[idx].enabled = false;
            }

            for (var idx = startIdx; idx < startIdx + maxLevel; idx++)
            {
                var progressImage = progressImages[idx];
                progressImage.enabled = true;
                progressImage.sprite = currentLevel >= idx - startIdx ? onSprite : offSprite;
            }

            for (var idx = startIdx + maxLevel; idx < progressImages.Length; idx++)
            {
                progressImages[idx].enabled = false;
            }
        }

        public void Open()
        {
            gameObject.SetActive(true);
            SoundManager.Instance.PlaySoundEffect(openCloseSe).Forget();
        }
        public void Close(bool isSelected)
        {
            if (isSelected)
            {
                ani.SetTrigger(TaeBoMiCache.Burn);
                SoundManager.Instance.PlaySoundEffect(selectSe).Forget();
            }
            else
            {
                ani.SetTrigger(TaeBoMiCache.Close);
                SoundManager.Instance.PlaySoundEffect(openCloseSe).Forget();
            }
        }


        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            Select();
            Highlight(true);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            Highlight(false);
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

        private void Highlight(bool value)
        {
            ani.SetBool(TaeBoMiCache.IsSelected, value);
            containerImage.sprite = panelSprites.data[_currentPanelSpriteIdx + (value ? 1 : 0)];
        }

        public void AnimationEvent_OpenFinished()
        {
            CurrentActiveNum++;
        }

        public void AnimationEvent_CloseFinished()
        {
            CurrentActiveNum--;
            gameObject.SetActive(false);
        }
    }
}