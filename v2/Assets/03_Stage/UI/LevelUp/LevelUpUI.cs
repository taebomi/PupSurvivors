using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using PupSurvivors.Equipment;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace PupSurvivors.Stage.UI
{
    public class LevelUpUI : MonoBehaviour
    {
        [SerializeField] private IntEventChannelSO levelUpEventSO;
        [SerializeField] private StageStateChannelSO stageStateChannel;

        [SerializeField] private Canvas canvas;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RawImage backgroundImage;
        [SerializeField] private RectTransform titleRT;


        [SerializeField] private LevelUpRewardPanel[] rewardPanelArr;
        [SerializeField] private RefreshButton refreshButton;

        private Dictionary<PlayerEquipment, int> _playerNotEquippedSelectedCountDict;
        private const int MaxNotEquippedSelectedCount = 5;
        private EquipmentData[] _rewardArr;

        private int _rewardCount;

        private int _curActiveRewardPanelCount;

        private int _levelUpCount;

        private CancellationTokenSource _destroyCts;

        public void OnOpenFinished()
        {
            _curActiveRewardPanelCount++;
        }

        public void OnCloseFinished()
        {
            _curActiveRewardPanelCount--;
        }


        private void Awake()
        {
            foreach (var rewardPanel in rewardPanelArr)
            {
                rewardPanel.gameObject.SetActive(false);
            }

            canvas.enabled = false;

            _curActiveRewardPanelCount = 0;
            _levelUpCount = 0;

            _rewardArr = new EquipmentData[3];
            _playerNotEquippedSelectedCountDict = new Dictionary<PlayerEquipment, int>(4);
            _destroyCts = new CancellationTokenSource();

            stageStateChannel.OnStageStateChanged += OnStageStateChanged;
            levelUpEventSO.OnEventRaised += OnLevelUpEventRaised;
        }

        private void OnDestroy()
        {
            _destroyCts.CancelAndDispose();
            stageStateChannel.OnStageStateChanged -= OnStageStateChanged;
            levelUpEventSO.OnEventRaised -= OnLevelUpEventRaised;
        }

        private void OnStageStateChanged(StageState state)
        {
            switch (state)
            {
                case StageState.None:
                    break;
                case StageState.Init:
                    foreach (var player in StageManager.Instance.CurPlayers)
                    {
                        _playerNotEquippedSelectedCountDict.Add(player.Equipment, 0);
                    }

                    break;
                case StageState.Ready:
                    break;
                case StageState.Start:
                    break;
                case StageState.GameOver:
                    break;
                case StageState.GameFinished:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private bool CanRewardable()
        {
            return StageManager.Instance.CurPlayers.Any(player => player.Equipment.IsAvailable);
        }

        private void OnLevelUpEventRaised(int _)
        {
            _levelUpCount++;
            if (_levelUpCount != 1)
            {
                return;
            }

            if (!CanRewardable())
            {
                return;
            }

            ShowUI().Forget();
        }

        private async UniTaskVoid ShowUI()
        {
            Time.timeScale = 0f;
            canvas.enabled = true;
            PopTitle(true).Forget();
            await Fade(true);

            while (_levelUpCount != 0)
            {
                // 플레이어 마다 보상
                foreach (var player in StageManager.Instance.CurPlayers)
                {
                    if (!player.Equipment.IsAvailable)
                    {
                        continue;
                    }

                    var playerEquipment = player.Equipment;

                    var refreshCount = 1;
                    UpdateRefreshButton();
                    refreshButton.onClick.AddListener(delegate { OnRefreshButtonClicked().Forget(); });

                    RefreshRewards(playerEquipment);
                    OpenRewardPanels().Forget();

                    await UniTask.WaitUntil(IsRewardPanelsOpened,
                        cancellationToken: _destroyCts.Token);
                    SetInteractable(true);


                    var onClickCts = new CancellationTokenSource();
                    var cts = CancellationTokenSource.CreateLinkedTokenSource(onClickCts.Token, _destroyCts.Token);
                    var selectedIdx = await UniTask.WhenAny(Enumerable.Select(rewardPanelArr, Selector));

                    SetInteractable(false);
                    refreshButton.onClick.RemoveAllListeners();

                    playerEquipment.LevelUpReward(_rewardArr[selectedIdx].equipmentName);

                    // 패널 모두 사라질 때까지 대기
                    CloseRewards(selectedIdx).Forget();
                    await UniTask.WaitUntil(IsRewardPanelsClosed, cancellationToken: _destroyCts.Token);

                    continue;

                    UniTask Selector(LevelUpRewardPanel rewardPanel) => rewardPanel.OnClickAsync(cts.Token);


                    async UniTask OnRefreshButtonClicked()
                    {
                        SetInteractable(false);
                        CloseRewards().Forget();
                        await UniTask.WaitUntil(IsRewardPanelsClosed, cancellationToken: _destroyCts.Token);

                        RefreshRewards(playerEquipment);
                        UpdateRefreshButton();

                        OpenRewardPanels().Forget();
                        await UniTask.WaitUntil(IsRewardPanelsOpened, cancellationToken: _destroyCts.Token);
                        SetInteractable(true);
                    }

                    void UpdateRefreshButton()
                    {
                        refreshCount = (int)(refreshCount * 2.5f);
                        refreshButton.SetPrice(refreshCount * 10);
                    }
                }

                _levelUpCount--;
            }

            // 페이드 및 UI 제거

            PopTitle(false).Forget();
            await Fade(false);
            canvas.enabled = false;
            Time.timeScale = 1f;

            return;


            bool IsRewardPanelsOpened() => _curActiveRewardPanelCount == _rewardCount;
            bool IsRewardPanelsClosed() => _curActiveRewardPanelCount == 0;

            async UniTaskVoid PopTitle(bool isForward)
            {
                const float xSize = 370f;
                const float ySize = 70f;

                var timer = 0f;
                const float duration = 0.5f;
                while (timer < duration)
                {
                    var value = isForward ? timer : duration - timer;
                    titleRT.sizeDelta = new Vector2(xSize * Easing.OutBack(value, duration), ySize);

                    timer += Time.unscaledDeltaTime;
                    await UniTask.Yield(_destroyCts.Token);
                }

                titleRT.sizeDelta = isForward ? new Vector2(xSize, ySize) : new Vector2(0f, ySize);
            }

            async UniTask OpenRewardPanels()
            {
                for (var i = 0; i < _rewardCount; i++)
                {
                    rewardPanelArr[i].Open();
                    const double delay = 0.1;
                    await UniTask.Delay(TimeSpan.FromSeconds(delay), ignoreTimeScale: true,
                        cancellationToken: _destroyCts.Token);
                }
            }


            async UniTask CloseRewards(int idx = -1)
            {
                for (var i = 0; i < _rewardCount; i++)
                {
                    rewardPanelArr[i].Close(idx == i);
                    const double delay = 0.1;
                    await UniTask.Delay(TimeSpan.FromSeconds(delay), ignoreTimeScale: true,
                        cancellationToken: _destroyCts.Token);
                }
            }

            void RefreshRewards(PlayerEquipment playerEquipment)
            {
                _rewardCount = GetRewardArray(playerEquipment);
                for (var i = 0; i < _rewardCount; i++)
                {
                    var reward = _rewardArr[i];
                    // 나중에 리워드가 장비 외에 다른 것도 생기면 rewardable 인터페이스 만들어서 추가
                    if (playerEquipment.IsEquipped(reward.equipmentName, out var equipment))
                    {
                        rewardPanelArr[i].Set(equipment);
                    }
                    else
                    {
                        rewardPanelArr[i].Set(reward);
                    }
                }

                switch (_rewardCount)
                {
                    case 1:
                        rewardPanelArr[0].SetPosition(new Vector2(0f, 0f));
                        break;
                    case 2:
                        rewardPanelArr[0].SetPosition(new Vector2(-280f, 0f));
                        rewardPanelArr[1].SetPosition(new Vector2(280f, 0f));
                        break;
                    case 3:
                        rewardPanelArr[0].SetPosition(new Vector2(-410f, 0f));
                        rewardPanelArr[1].SetPosition(new Vector2(0f, 0f));
                        rewardPanelArr[2].SetPosition(new Vector2(410f, 0f));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private async UniTask Fade(bool value)
        {
            if (value)
            {
                canvas.enabled = true;
            }

            var timer = 0f;
            const float duration = 0.25f;
            const float speed = 1f / duration;
            while (timer < duration)
            {
                canvasGroup.alpha = value ? timer * speed : 1f - timer * speed;

                timer += Time.unscaledDeltaTime;
                await UniTask.Yield(_destroyCts.Token);
            }

            if (value)
            {
                canvasGroup.alpha = 1f;
            }
            else
            {
                canvasGroup.alpha = 0f;
                canvas.enabled = false;
            }
        }

        private void SetInteractable(bool value)
        {
            refreshButton.interactable = value;
            foreach (var rewardPanel in rewardPanelArr)
            {
                rewardPanel.interactable = value;
            }
        }

        private int GetRewardArray(PlayerEquipment playerEquipment)
        {
            var playerLuck = playerEquipment.PlayerLuck;

            var craftableCount = GetCraftable();
            if (craftableCount != 0)
            {
                _playerNotEquippedSelectedCountDict[playerEquipment] = 0;
            }

            if (craftableCount == _rewardArr.Length)
            {
                return craftableCount;
            }

            var addableCount = GetAddable();

            return craftableCount + addableCount;


            int GetCraftable()
            {
                var craftableTable = new PlayerEquipment.EquipmentTable(playerEquipment.CraftableTable);
                var count = Mathf.Clamp(craftableTable.DataSet.Count, 0, _rewardArr.Length);
                for (var i = 0; i < count; i++)
                {
                    var randomData = craftableTable.PickRandomItem(playerLuck);
                    _rewardArr[i] = randomData;
                    craftableTable.Remove(randomData);
                }

                return count;
            }

            int GetAddable()
            {
                var equippedSet =
                    new HashSet<EquipmentData>(
                        playerEquipment.EquippedDict.Values.Select(equipment => equipment.EquipmentData));

                var addableTable = playerEquipment.AddableTable;
                var equippedAddableTable = new PlayerEquipment.EquipmentTable(addableTable.DataSet);
                equippedAddableTable.DataSet.IntersectWith(equippedSet);
                equippedAddableTable.TotalWeight =
                    equippedAddableTable.DataSet.Sum(equipmentData => equipmentData.weight);
                var unequippedAddableTable = new PlayerEquipment.EquipmentTable(addableTable.DataSet);
                unequippedAddableTable.DataSet.ExceptWith(equippedSet);
                unequippedAddableTable.TotalWeight = addableTable.TotalWeight - equippedAddableTable.TotalWeight;

                var count = Mathf.Clamp(addableTable.DataSet.Count, 0, _rewardArr.Length - craftableCount);
                for (var idx = craftableCount; idx < craftableCount + count; idx++)
                {
                    const float equippedSelectProbability = 0.2f;
                    if (equippedAddableTable.DataSet.Count > 0 &&
                        (_playerNotEquippedSelectedCountDict[playerEquipment] > MaxNotEquippedSelectedCount ||
                         Random.value < equippedSelectProbability))
                    {
                        _playerNotEquippedSelectedCountDict[playerEquipment] = 0;
                        var randomData = equippedAddableTable.PickRandomItem(playerLuck);
                        if (randomData)
                        {
                            _rewardArr[idx] = randomData;
                            equippedAddableTable.Remove(randomData);
                        }
                    }
                    else
                    {
                        _playerNotEquippedSelectedCountDict[playerEquipment]++;
                        var randomData = unequippedAddableTable.PickRandomItem(playerLuck);
                        if (randomData)
                        {
                            _rewardArr[idx] = randomData;
                            unequippedAddableTable.Remove(randomData);
                        }
                    }
                }

                return count;
            }
        }
    }
}