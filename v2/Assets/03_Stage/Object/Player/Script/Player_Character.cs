using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace PupSurvivors.Stage
{
    public partial class Player
    {
        [field:SerializeField] public CharacterData CurCharacterData { get; private set; }

        public async UniTask InitCharacter(string characterName)
        {
            var handle = Addressables.LoadAssetAsync<CharacterData>($"CharacterData/{characterName}");
            CurCharacterData = await handle;
            // todo 캐릭터 여러 종류 되면 애니메이터 오버라이드 컨트롤러 사용해서 외형 바꿔주기
            Addressables.Release(handle);
        }
    }
}