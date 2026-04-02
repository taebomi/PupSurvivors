using System;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public record CharacterStats()
{
    public float hp;
    public float hpRecovery;

    public int sp;
    public float skillCooldown;
    public float skillHaste;

    public float movementSpeed;

    public float damageMultiplier;
    public float criticalRate;
    public float criticalMultiplier;

    public float area;
    public float projectileSpeed;
    public float duration;
    public float weaponCooldown;
    public float weaponHaste;
    public float magnet;

    public int luck;
    public float expMultiplier;
    public float goldMultiplier;

    public void Reset()
    {
    hp = 0f;
    hpRecovery = 0f;

    sp = 0;
    skillCooldown = 0f;
    skillHaste = 0f;
    movementSpeed = 0f;
    damageMultiplier =0f;
    criticalRate = 0f;
    criticalMultiplier = 0f;
    area = 0f;
    projectileSpeed = 0f;
    duration = 0f;
    weaponCooldown =0f;
    weaponHaste = 0f;
    magnet = 0f;
    luck = 0;
    expMultiplier = 0f;
    goldMultiplier = 0f;
    }

    public void AddStats(CharacterStats stats)
    {
        hp += stats.hp;
        hpRecovery += stats.hpRecovery;

        sp += stats.sp;
        skillHaste += stats.skillHaste;

        movementSpeed += stats.movementSpeed;

        damageMultiplier += stats.damageMultiplier;
        criticalRate += stats.criticalRate;
        criticalMultiplier += stats.criticalMultiplier;

        area += stats.area;
        projectileSpeed += stats.projectileSpeed;
        duration += stats.duration;
        weaponHaste += stats.weaponHaste;
        magnet += stats.magnet;

        luck += stats.luck;
        expMultiplier += stats.expMultiplier;
        goldMultiplier += stats.goldMultiplier;
    }

    public CharacterStats Calculate(CharacterStats add)
    {
        var newStats = new CharacterStats
        {
            hp = hp * (1 + add.hp),
            hpRecovery = hpRecovery * (1 + add.hpRecovery),
            sp = Mathf.Clamp(sp + add.sp, 0, 5),
            skillHaste = skillHaste + add.skillHaste,
            movementSpeed = movementSpeed + add.movementSpeed,
            damageMultiplier = damageMultiplier * (1 + add.damageMultiplier),
            criticalRate = criticalRate + add.criticalRate,
            criticalMultiplier = criticalMultiplier + add.criticalMultiplier,
            // todo 크리 확률/계수 -가 될 때 데미지 약해지도록 하기
            area = area * (1 + add.area),
            projectileSpeed = projectileSpeed * (1 + add.projectileSpeed),
            duration = duration * (1 + add.duration),
            weaponHaste = weaponHaste + add.weaponHaste,
            magnet = magnet + add.magnet > 0.1f ? magnet + add.magnet : 0.1f,
            luck = luck + add.luck,
            // todo 음수인 경우?
            expMultiplier = expMultiplier * (1 + add.expMultiplier),
            goldMultiplier = goldMultiplier * (1 + add.goldMultiplier)
        };


        if (newStats.skillHaste >= 0)
        {
            newStats.skillCooldown = 100 / (100 + newStats.skillHaste);
        }
        else
        {
            newStats.skillCooldown = 2 - 100 / (100 - newStats.skillHaste);
        }

        if (newStats.weaponHaste >= 0)
        {
            newStats.weaponCooldown = 100 / (100 + newStats.weaponHaste);
        }
        else
        {
            newStats.weaponCooldown = 2 - 100 / (100 - newStats.weaponHaste);
        }

        return newStats;
    }
}


[CreateAssetMenu(menuName = "TaeBoMi/CharacterData", fileName = "CharacterData")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    // todo : 캐릭터 여러 종이면 여기서 애니메이터 오버라이드 추가
    
    [SerializeField] private CharacterStats stats;
    
    public string defaultEquipmentName;
    
    public CharacterStats GetStats() => stats with { };
}