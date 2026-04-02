using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace PupSurvivors.Stage.Map
{
    public class Map : MonoBehaviour
    {
        public bool isIndoor;
        private Tilemap[] _tilemaps;

        private const float FadeDuration = 0.25f;
        private const float FadeSpeed = 1 / FadeDuration;

        private CancellationTokenSource _disableCts, _fadingCts;

        private void Awake()
        {
            _tilemaps = GetComponentsInChildren<Tilemap>(true);
            Activate(false);
        }

        private void OnEnable()
        {
            _disableCts = new CancellationTokenSource();
        }

        private void OnDisable()
        {
            _disableCts.CancelAndDispose();
        }

        public void Activate(bool value)
        {
            transform.GetChild(0).gameObject.SetActive(value);
        }

        public async UniTaskVoid Enter()
        {
            Activate(true);
            _fadingCts?.CancelAndDispose();
            _fadingCts = new CancellationTokenSource();
            var cts = CancellationTokenSource.CreateLinkedTokenSource(_fadingCts.Token, _disableCts.Token);
            var currentColor = new Color(1f, 1f, 1f,0f);
            var timer = FadeDuration;
            
            while (timer > 0)
            {
                currentColor.a += FadeSpeed * Time.deltaTime;
                foreach (var tilemap in _tilemaps)
                {
                    tilemap.color = currentColor;
                }

                timer -= Time.deltaTime;
                await UniTask.Yield(cts.Token);
            }

            foreach (var tilemap in _tilemaps)
            {
                tilemap.color = Color.white;
            }
        }

        public async UniTaskVoid Exit()
        {
            _fadingCts?.CancelAndDispose();
            _fadingCts = new CancellationTokenSource();
            var cts = CancellationTokenSource.CreateLinkedTokenSource(_fadingCts.Token, _disableCts.Token);
            var currentColor = new Color(1f, 1f, 1f,1f);
            var timer = FadeDuration;
            while (timer > 0)
            {
                currentColor.a -= FadeSpeed * Time.deltaTime;
                foreach (var tilemap in _tilemaps)
                {
                    tilemap.color = currentColor;
                }

                timer -= Time.deltaTime;
                await UniTask.Yield(cts.Token);
            }

            foreach (var tilemap in _tilemaps)
            {
                tilemap.color = new Color(1f, 1f, 1f, 0f);
            }
            Activate(false);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            MapManager.Instance.ActivateMap(this);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            MapManager.Instance.DeactiveMap(this);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + new Vector3(48f, -48f), new Vector3(96f, 96f));
        }
    }
}