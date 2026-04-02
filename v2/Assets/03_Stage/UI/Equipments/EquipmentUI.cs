using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PupSurvivors.Stage.UI
{
    public class EquipmentUI : MonoBehaviour
    {
        [SerializeField] private StageStateChannelSO stageStateChannel;

        [SerializeField] private EquipmentPanel[] equipmentPanels;

        private CancellationTokenSource _destroyCts;

        private void Awake()
        {
            _destroyCts = new CancellationTokenSource();
            stageStateChannel.OnStageStateChanged += OnStageStateChanged;
        }

        private void OnDestroy()
        {
            stageStateChannel.OnStageStateChanged -= OnStageStateChanged;
            _destroyCts.CancelAndDispose();
        }

        private void OnStageStateChanged(StageState state)
        {
            switch (state)
            {
                case StageState.None:
                    break;
                case StageState.Init:
                    Initialize(StageManager.Instance.CurPlayers.Select(player => player.Equipment));
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

        private void Initialize(IEnumerable<PlayerEquipment> playerEquipments)
        {
            var idx = 0;
            foreach (var playerEquipment in playerEquipments)
            {
                equipmentPanels[idx].Initialize(playerEquipment);
                idx++;
            }

            for (var i = idx; i < equipmentPanels.Length; i++)
            {
                equipmentPanels[i].gameObject.SetActive(false);
            }
        }
    }
}