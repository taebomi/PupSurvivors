using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace PupSurvivors.Stage
{
    public class CentralEmiyaField : StageManager
    {
        [SerializeField] private EnemySpawner enemySpawner;

        // 맵 관련
        [Header("Map"), SerializeField] private Transform centerMapTr;
        [SerializeField] private Transform[] horMapTr, verMapTr;
        [SerializeField] private Transform[] diagMapTr;

        private Vector2Int _lastCoordinate = new(int.MinValue, int.MinValue);

        // ReSharper disable once Unity.IncorrectMethodSignature
        protected override async UniTask Start()
        {
            await base.Start();
            enemySpawner.Initialize(MovementTracker);
            enemySpawner.StartWave().Forget();
        }
        
        
        
        
        
        
        #region 맵 컨트롤

        public void OnCamPosChanged(Vector2 pos)
        {
            var curCoordinate = Vector2Int.FloorToInt(pos / 32);
            if (curCoordinate == _lastCoordinate)
            {
                return;
            }

            var tileCoordinate = Vector2Int.RoundToInt(pos / 64);
            if (curCoordinate.x % 2 is 0)
            {
                if (curCoordinate.y % 2 is 0) // 1사분면
                {
                    for (var x = 0; x <= 1; x++)
                    {
                        for (var y = 0; y <= 1; y++)
                        {
                            SetPosition(tileCoordinate.x + x, tileCoordinate.y + y);
                        }
                    }
                }
                else // 4사분면
                {
                    for (var x = 0; x <= 1; x++)
                    {
                        for (var y = -1; y <= 0; y++)
                        {
                            SetPosition(tileCoordinate.x + x, tileCoordinate.y + y);
                        }
                    }
                }
            }
            else
            {
                if (curCoordinate.y % 2 is 0) // 2사분면
                {
                    for (var x = -1; x <= 0; x++)
                    {
                        for (var y = 0; y <= 1; y++)
                        {
                            SetPosition(tileCoordinate.x + x, tileCoordinate.y + y);
                        }
                    }
                }
                else // 3사분면
                {
                    for (var x = -1; x <= 0; x++)
                    {
                        for (var y = -1; y <= 0; y++)
                        {
                            SetPosition(tileCoordinate.x + x, tileCoordinate.y + y);
                        }
                    }
                }
            }

            _lastCoordinate = curCoordinate;

        }

        private void SetPosition(int x, int y)
        {
            Transform targetTr;
            var xIdx = x switch
            {
                > 0 => (x - 1) % 3,
                0 => -1,
                < 0 => (x % 3 + 3) % 3
            };

            var yIdx = y switch
            {
                > 0 => (y - 1) % 3,
                0 => -1,
                < 0 => (y % 3 + 3) % 3
            };

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (xIdx is -1 && yIdx is -1)
            {
                targetTr = centerMapTr;
            }
            else if (xIdx is -1)
            {
                targetTr = verMapTr[yIdx];
            }
            else if (yIdx is -1)
            {
                targetTr = horMapTr[xIdx];
            }
            else
            {
                targetTr = diagMapTr[xIdx + yIdx * 3];
            }

            targetTr.position = new Vector3(x * 64, y * 64);
        }
        #endregion
    }
}