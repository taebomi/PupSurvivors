using System;
using AYellowpaper.SerializedCollections;
using PupSurvivors.Enemy;

namespace PupSurvivors.Equipment
{
    [Serializable]
    public class WeaponStats
    {
        public float defaultDamage; // 기본 공격력
        public const float RandomMinDamage = 0.85f;
        public const float RandomMaxDamage = 1.15f;

        public float cooldown; // 쿨타임 
        public float duration; // 지속시간
        public float interval; // 공격 간 딜레이

        public int amount; // 개수
        public float knockbackPower; // 넉백량
        public float size; // 크기
        public float speed; // 투사체 속도
        public int pierce; // 관통 가능 횟수

        public SerializedDictionary<FloatOption, float> floatOptionDict;

        public void CalculateStats(WeaponStats ori, CharacterStats add)
        {
            defaultDamage = ori.defaultDamage * add.damageMultiplier;
            cooldown = ori.cooldown * add.weaponCooldown;
            duration = ori.duration * add.duration;
            interval = ori.interval;
            amount = ori.amount; // todo 스텟 추가
            knockbackPower = ori.knockbackPower; // todo 스텟 추가
            size = ori.size * add.area;
            speed = ori.speed * add.projectileSpeed;
            pierce = ori.pierce; // todo 스텟 추가
            floatOptionDict = ori.floatOptionDict;
        }

        public enum FloatOption
        {
            MaxRange,
            DefaultRange = 100,
            AddRange = 110,
            
            AttackDuration = 1000,
            AttackDelay = 1100,
            
        }
    }
}