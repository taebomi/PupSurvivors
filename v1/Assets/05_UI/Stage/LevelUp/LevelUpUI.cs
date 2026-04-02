using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Coffee.UIExtensions;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using PupSurvivors.Equipment;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace PupSurvivors.Stage.UI.LevelUp
{
    public class LevelUpUI : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private CanvasGroup canvasGroup;

        [SerializeField] private RectTransform titleRectTransform;

        [SerializeField] private RewardPanel[] rewardPanels;
        [SerializeField] private ParticleSystem effectParticleSystem;

        [SerializeField] private RefreshButton refreshButton;

        private List<EquipmentData> _equipmentDataList;

        private EquipmentTextData _equipmentTextData;

        private int _selectableRewardNum;
        private int _refreshCount;

        private CancellationTokenSource _disableCts;

        private void Awake()
        {
            _equipmentDataList = new List<EquipmentData>(3);
            _disableCts = new CancellationTokenSource();
            StageManager.Instance.AddInitQueue(LoadEquipmentTexts());
        }

        private void OnDisable()
        {
            _disableCts.CancelAndDispose();
        }

        private void SetInteractable(bool value)
        {
            refreshButton.interactable = value;
            foreach (var rewardPanel in rewardPanels)
            {
                rewardPanel.interactable = value;
            }
        }

        private async UniTask LoadEquipmentTexts()
        {
            var text = await Addressables.LoadAssetAsync<TextAsset>(
                $"Equipment_{GameManager.Instance.PlayData.language}");
            _equipmentTextData = JsonConvert.DeserializeObject<EquipmentTextData>(text.text);
        }

        public async UniTask ShowUI(PlayerController player)
        {
            canvas.enabled = true;

            _refreshCount = 0;

            effectParticleSystem.Play();
            // todo 가격 조건 체크 추가하기
            refreshButton.onClick.AddListener(delegate { RefreshRewardList(player).Forget(); });

            // 투명 -> 불투명
            Fade(true).Forget();
            TitlePop(true).Forget();
            RefreshRewardList(player).Forget();

            // await UniTask.WaitUntil(() => RewardPanel.CurrentActiveNum == rewardPanels.Length,
            //     cancellationToken: _disableCts.Token);

            // # 버튼 누를 때까지 대기
            var onClickCts = new CancellationTokenSource();
            var cts = CancellationTokenSource.CreateLinkedTokenSource(_disableCts.Token, onClickCts.Token);

            UniTask Selector(RewardPanel upgradeContainer) => upgradeContainer.Button.OnClickAsync(cts.Token);

            var selectedIdx = await UniTask.WhenAny(Enumerable.Select(rewardPanels, Selector));
            cts.CancelAndDispose();
            onClickCts.Dispose();

            if (_equipmentDataList.Count != 0)
            {
                player.Equipment.AddEquipment(_equipmentDataList[selectedIdx].equipmentName);
            }
            else
            {
                
            }

            refreshButton.onClick.RemoveAllListeners();
            TitlePop(false).Forget();
            effectParticleSystem.Stop(true);


            SetInteractable(false);
            for (var i = 0; i < rewardPanels.Length; i++)
            {
                rewardPanels[i].Close(selectedIdx == i);
            }

            await UniTask.WaitUntil(() => RewardPanel.CurrentActiveNum == 0,
                cancellationToken: _disableCts.Token); // 팝업 사라질때까지 대기
            await Fade(false);

            canvas.enabled = false;
        }

        private async UniTask TitlePop(bool isForward)
        {
            var timer = 0f;
            const float duration = 0.5f;
            while (timer < duration)
            {
                var value = isForward ? timer : duration - timer;
                titleRectTransform.sizeDelta = new Vector2(360f * Easing.OutBack(value, duration), 70f);

                timer += Time.unscaledDeltaTime;
                await UniTask.Yield(_disableCts.Token);
            }

            titleRectTransform.sizeDelta = isForward ? new Vector2(360f, 70f) : new Vector2(0f, 70f);
        }

        private async UniTask Fade(bool isForward)
        {
            var timer = 0f;
            const float duration = 0.25f;
            const float multiplier = 1f / 0.5f;
            while (timer < duration)
            {
                if (isForward)
                {
                    canvasGroup.alpha = timer * multiplier;
                }
                else
                {
                    canvasGroup.alpha = 1 - timer * multiplier;
                }

                timer += Time.unscaledDeltaTime;
                await UniTask.Yield(_disableCts.Token);
            }

            canvasGroup.alpha = isForward ? 1f : 0f;
        }

        private async UniTask RefreshRewardList(PlayerController player)
        {
            SetInteractable(false);

            _refreshCount++;
            refreshButton.SetPriceText(_refreshCount * 10);

            // # 보이는 게 있으면 닫기 애니메이션, 완료까지 대기
            for (var i = 0; i < RewardPanel.CurrentActiveNum; i++)
            {
                rewardPanels[i].Close(false);
                await UniTask.Delay(100, true, cancellationToken: _disableCts.Token);
            }

            await UniTask.WaitUntil(IsRewardPanelsClosed, cancellationToken: _disableCts.Token);


            // # 장비 목록 갱신
            player.Equipment.GetLevelUpEquipmentDataList(_equipmentDataList);
            _selectableRewardNum = _equipmentDataList.Count;

            for (var i = 0; i < _selectableRewardNum; i++)
            {
                var equipmentName = _equipmentDataList[i].equipmentName;
                var textData = _equipmentTextData[equipmentName];

                if (player.Equipment.EquippedDict.TryGetValue(equipmentName,
                        out var equipmentBase))
                {
                    rewardPanels[i].Set(_equipmentDataList[i],
                        equipmentBase.GetEquipmentData().GetMaxLevel() - 1 == equipmentBase.CurrentLevel
                            ? -1
                            : equipmentBase.CurrentLevel,
                        textData.name,
                        textData.description[equipmentBase.CurrentLevel]);
                }
                else
                {
                    const int currentLevel = 0;
                    rewardPanels[i].Set(_equipmentDataList[i], currentLevel,
                        textData.name, textData.description[currentLevel]);
                }
            }

            // todo datalist가 없을 경우에는 체력 회복, 마나 회복, 골드 획득 등으로 대체


            switch (_selectableRewardNum)
            {
                case 1:
                    rewardPanels[0].SetPosition(new Vector2(0f, 0f));
                    break;
                case 2:
                    rewardPanels[0].SetPosition(new Vector2(-280f, 0f));
                    rewardPanels[1].SetPosition(new Vector2(280f, 0f));
                    break;
                case 3:
                    rewardPanels[0].SetPosition(new Vector2(-410f, 0f));
                    rewardPanels[1].SetPosition(new Vector2(0f, 0f));
                    rewardPanels[2].SetPosition(new Vector2(410f, 0f));
                    break;
            }


            // # 보상 펼치기 애니메이션
            for (var i = 0; i < _selectableRewardNum; i++)
            {
                rewardPanels[i].Open();
                await UniTask.Delay(100, true, cancellationToken: _disableCts.Token);
            }

            await UniTask.WaitUntil(IsRewardPanelsOpened, cancellationToken: _disableCts.Token);

            SetInteractable(true);
            refreshButton.Select();
        }

        private bool IsRewardPanelsClosed()
        {
            return RewardPanel.CurrentActiveNum == 0;
        }

        private bool IsRewardPanelsOpened()
        {
            return RewardPanel.CurrentActiveNum == _selectableRewardNum;
        }
    }
}