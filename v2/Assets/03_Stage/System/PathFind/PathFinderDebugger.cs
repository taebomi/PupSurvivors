using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using PupSurvivors.Stage.PathFinding;
using TMPro;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace PupSurvivors.Stage
{
    public class PathFinderDebugger : MonoBehaviour
    {
        [SerializeField] private Button groundBtn, airBtn;
        [SerializeField] private TMP_Dropdown playerDropdown;

        [SerializeField] private PathFinder pathfinder;
        [SerializeField] private Material spriteUnlitMat;

        private enum DebugMode
        {
            None = 0,
            Ground = 1 << 0,
            Air = 1 << 1,
        }

        [SerializeField] private DebugMode debugMode;

        private FlowField _curFlowField;
        private NativeArray<Cell> _curField;

        private Tilemap _tilemap;
        private TileBase _oriTile, _dirTile, _collTile, _noneTile;
        private CancellationTokenSource _debuggingCts;

        private bool _isInitialized;

        private void Awake()
        {
            _isInitialized = false;
        }

        private void OnDestroy()
        {
            if (_oriTile)
            {
                Addressables.Release(_oriTile);
            }

            if (_dirTile)
            {
                Addressables.Release(_dirTile);
            }

            if (_collTile)
            {
                Addressables.Release(_collTile);
            }

            if (_noneTile)
            {
                Addressables.Release(_noneTile);
            }

            _debuggingCts?.CancelAndDispose();
        }

        private async UniTask Initialize()
        {
            groundBtn.interactable = false;
            airBtn.interactable = false;

            playerDropdown.interactable = false;
            playerDropdown.options = new List<TMP_Dropdown.OptionData>();
            for (var idx = 0; idx < StageManager.Instance.CurPlayers.Length; idx++)
            {
                playerDropdown.options.Add(new TMP_Dropdown.OptionData($"Player{idx}"));
            }

            _tilemap = CreateTilemap();

            const string pathPrefix = "PathFinding/DebugTile_";
            _oriTile = await Addressables.LoadAssetAsync<TileBase>($"{pathPrefix}Ori");
            _dirTile = await Addressables.LoadAssetAsync<TileBase>($"{pathPrefix}Dir");
            _collTile = await Addressables.LoadAssetAsync<TileBase>($"{pathPrefix}Coll");
            _noneTile = await Addressables.LoadAssetAsync<TileBase>($"{pathPrefix}None");

            groundBtn.interactable = true;
            airBtn.interactable = true;
            playerDropdown.interactable = true;

            _isInitialized = true;
            return;

            Tilemap CreateTilemap()
            {
                var tilemap = new GameObject($"PathFinder",
                    typeof(Grid), typeof(Tilemap), typeof(TilemapRenderer)).GetComponent<Tilemap>();

                var grid = tilemap.GetComponent<Grid>();
                grid.cellSize = new Vector3(0.5f, 0.5f, 1f);

                var tilemapRenderer = tilemap.GetComponent<TilemapRenderer>();
                tilemapRenderer.sortingLayerName = "Debug";
                tilemapRenderer.material = spriteUnlitMat;
                _curFlowField = pathfinder.FlowFields[0];
                return tilemap;
            }
        }

        public void ChangePlayer(int playerIdx)
        {
            if (!_isInitialized)
            {
                Initialize().Forget();
            }

            _curFlowField = pathfinder.FlowFields[playerIdx];
            ChangeCurFlowField(debugMode);
        }

        public void OnGroundModeClicked() => OnModeClicked(DebugMode.Ground).Forget();

        public void OnAirModeToggled() => OnModeClicked(DebugMode.Air).Forget();

        private async UniTaskVoid OnModeClicked(DebugMode mode)
        {
            if (!_isInitialized)
            {
                await Initialize();
            }

            if (debugMode == DebugMode.None) // 꺼진 상태에서 켜는 경우
            {
                _tilemap.gameObject.SetActive(true);
                ChangeCurFlowField(mode);
                ShowDebugTiles().Forget();
                debugMode = mode;
            }
            else if (debugMode == mode) // 켜진 것 끄는 경우
            {
                _debuggingCts?.Cancel();
                _tilemap.gameObject.SetActive(false);
                debugMode = DebugMode.None;
            }
            else // 다른 것 켜진 상태에서 켜는 경우
            {
                ChangeCurFlowField(mode);
                debugMode = mode;
            }
        }

        private void ChangeCurFlowField(DebugMode onMode)
        {
            _curField = onMode switch
            {
                DebugMode.Ground => _curFlowField.GroundCells,
                DebugMode.Air => _curFlowField.AirCells,
                _ => throw new ArgumentOutOfRangeException(nameof(onMode), onMode, null)
            };
        }

        private async UniTaskVoid ShowDebugTiles()
        {
            _debuggingCts?.CancelAndDispose();
            _debuggingCts = new CancellationTokenSource();

            while (_debuggingCts.IsCancellationRequested is false)
            {
                _tilemap.ClearAllTiles();

                var centerPos = pathfinder.CenterWorldPoint;
                var oriPoint = new int2(centerPos.x - PathFinder.ColHalfSize, centerPos.y - PathFinder.RowHalfSize);

                const int rowStart = PathFinder.RowHalfSize - (int)CameraManager.HalfHeight * 2 - 1;
                const int rowEnd = PathFinder.RowHalfSize + (int)CameraManager.HalfHeight * 2 + 1;
                const int colStart = PathFinder.ColHalfSize - (int)CameraManager.HalfWidth * 2 - 1;
                const int colEnd = PathFinder.ColHalfSize + (int)CameraManager.HalfWidth * 2 + 1;

                for (var row = rowStart; row < rowEnd; row++)
                {
                    for (var col = colStart; col < colEnd; col++)
                    {
                        var curPoint = new Vector3Int(oriPoint.x + col, oriPoint.y + row);
                        var cell = _curField[Flatten(new int2(col, row))];
                        TileBase tile;
                        var matrix = Matrix4x4.identity;
                        switch (cell.Dist)
                        {
                            case > 0:
                                tile = _dirTile;
                                var quaternion = Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, cell.Dir));
                                matrix = Matrix4x4.TRS(Vector3.zero, quaternion, Vector3.one);
                                break;
                            case 0:
                                tile = _oriTile;
                                break;
                            case Cell.Collider:
                                tile = _collTile;
                                break;
                            case Cell.UnExplored:
                                tile = _noneTile;
                                break;
                            default:
                                tile = null;
                                break;
                        }

                        if (tile != null)
                        {
                            _tilemap.SetTile(curPoint, tile);
                            _tilemap.SetTransformMatrix(curPoint, matrix);
                        }
                    }
                }

                await UniTask.Yield(PlayerLoopTiming.LastTimeUpdate, _debuggingCts.Token);
            }
        }

        private static int Flatten(int2 point)
        {
            return point.x + point.y * PathFinder.ColSize;
        }
    }
}