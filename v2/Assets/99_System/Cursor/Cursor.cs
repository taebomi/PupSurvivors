using PupSurvivors.Pool;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PupSurvivors.System
{
    public class Cursor : Singleton<Cursor>
    {
        [SerializeField] private ClickEffect pressEffectPrefab;
        [SerializeField] private Texture2D releaseTexture, pressedTexture; // 커서 텍스쳐

        private LimitedObjectPool<ClickEffect> _pressEffectPool;

        private const CursorMode CursorMode = UnityEngine.CursorMode.Auto; // Auto는 OS에서 설정하는 듯
        private readonly Vector2 _hotspot = Vector2.zero; // 커서 텍스쳐 포인터 눌리는 지점 위치, 좌상단이 0,0

        protected override void Awake()
        {
            base.Awake();
            UnityEngine.Cursor.SetCursor(releaseTexture, _hotspot, CursorMode);
            _pressEffectPool = new LimitedObjectPool<ClickEffect>(() =>
            {
                var effect = Instantiate(pressEffectPrefab, transform);
                effect.Initialize(_pressEffectPool);
                return effect;
            }, 3);
        }

        private void Update()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                UnityEngine.Cursor.SetCursor(pressedTexture, _hotspot, CursorMode);
                // 이펙트 생성
                var effect = _pressEffectPool.Get();
                effect.transform.position = Mouse.current.position.value;
                effect.gameObject.SetActive(true);
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                UnityEngine.Cursor.SetCursor(releaseTexture, _hotspot, CursorMode);
            }
        }

        public void ChangeCursorLockMode(CursorLockMode lockMode)
        {
            UnityEngine.Cursor.lockState = lockMode;
        }
    }
}