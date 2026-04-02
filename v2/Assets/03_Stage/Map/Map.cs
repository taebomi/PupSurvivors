using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace PupSurvivors.Stage
{
    public class Map : MonoBehaviour
    {
        private MapManager _mapManager;

        [field: SerializeField] public bool Indoor { get; private set; }

        private Tilemap[] _tilemaps;
        private CancellationTokenSource _fadingCts;

        public const int Length = 96;
        private const float FadeDuration = 0.25f;
        private const float FadeSpeed = 1 / FadeDuration;

        private void Awake()
        {
            _mapManager = GetComponentInParent<MapManager>();
            _tilemaps = GetComponentsInChildren<Tilemap>(true);


            // SetActive(false);
        }

        private void OnDisable()
        {
            _fadingCts?.Cancel();
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            _mapManager.ActivateMap(this);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            _mapManager.DeactivateMap(this);
        }


        public void SetActive(bool value)
        {
            foreach (Transform childTr in transform)
            {
                childTr.gameObject.SetActive(true);
            }
        }

        public async UniTaskVoid Enter()
        {
            SetActive(true);
            if (_fadingCts != null)
            {
                _fadingCts.Cancel();
                _fadingCts.Dispose();
            }

            _fadingCts = new CancellationTokenSource();

            var currentColor = new Color(1f, 1f, 1f, 0f);
            var timer = FadeDuration;
            while (timer > 0f)
            {
                currentColor.a += FadeSpeed * Time.deltaTime;
                foreach (var tilemap in _tilemaps)
                {
                    tilemap.color = currentColor;
                }

                timer -= Time.deltaTime;
                await UniTask.Yield(_fadingCts.Token);
            }

            foreach (var tilemap in _tilemaps)
            {
                tilemap.color = Color.white;
            }
        }

        public async UniTaskVoid Exit()
        {
            if (_fadingCts != null)
            {
                _fadingCts.Cancel();
                _fadingCts.Dispose();
            }
            _fadingCts = new CancellationTokenSource();

            var currentColor = new Color(1f, 1f, 1f, 1f);
            var timer = FadeDuration;
            while (timer > 0)
            {
                currentColor.a -= FadeSpeed * Time.deltaTime;
                foreach (var tilemap in _tilemaps)
                {
                    tilemap.color = currentColor;
                }

                timer -= Time.deltaTime;
                await UniTask.Yield(_fadingCts.Token);
            }

            foreach (var tilemap in _tilemaps)
            {
                tilemap.color = new Color(1f, 1f, 1f, 0f);
            }

            SetActive(false);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, new Vector3(Length, Length));
        }
    }
}