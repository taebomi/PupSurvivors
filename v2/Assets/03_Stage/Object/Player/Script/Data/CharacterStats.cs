using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CharacterStats
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

    public CharacterStats()
    {
    hp = 0f;
    hpRecovery = 0f;
    sp = 0;
    skillCooldown = 0f;
    skillHaste = 0f;
    movementSpeed = 0f;
    damageMultiplier = 0f;
    criticalRate = 0f;
    criticalMultiplier = 0f;
    area = 0f;
    projectileSpeed = 0f;
    duration = 0f;
    weaponCooldown = 0f;
    weaponHaste = 0f;
    magnet = 0f;
    luck = 0;
    expMultiplier = 0f;
    goldMultiplier = 0f;
    }

    public CharacterStats(CharacterStats ori)
    {
        hp = ori.hp;
        hpRecovery = ori.hpRecovery;
        sp = ori.sp;
        skillCooldown = ori.skillCooldown;
        skillHaste = ori.skillHaste;
        movementSpeed = ori.movementSpeed;
        damageMultiplier = ori.damageMultiplier;
        criticalRate = ori.criticalRate;
        criticalMultiplier = ori.criticalMultiplier;
        area = ori.area;
        projectileSpeed = ori.projectileSpeed;
        duration = ori.duration;
        weaponCooldown = ori.weaponCooldown;
        weaponHaste = ori.weaponHaste;
        magnet = ori.magnet;
        luck = ori.luck;
        expMultiplier = ori.expMultiplier;
        goldMultiplier = ori.goldMultiplier;
    }

    public void Add(CharacterStats add)
    {
        hp += add.hp;
        hpRecovery += add.hpRecovery;

        sp += add.sp;
        skillHaste += add.skillHaste;

        movementSpeed += add.movementSpeed;

        damageMultiplier += add.damageMultiplier;
        criticalRate += add.criticalRate;
        criticalMultiplier += add.criticalMultiplier;

        area += add.area;
        projectileSpeed += add.projectileSpeed;
        duration += add.duration;
        weaponHaste += add.weaponHaste;
        magnet += add.magnet;

        luck += add.luck;
        expMultiplier += add.expMultiplier;
        goldMultiplier += add.goldMultiplier;
    }

    public void Calculate(CharacterStats add)
    {
        hp *= 1 + add.hp;
        hpRecovery *= 1 + add.hpRecovery;
        sp = Mathf.Clamp(sp + add.sp, 0, 5);
        skillHaste += add.skillHaste;
        movementSpeed += add.movementSpeed;
        damageMultiplier *= 1 + add.damageMultiplier;
        criticalRate += add.criticalRate;
        criticalMultiplier += add.criticalMultiplier;
        // todo 크리 확률/계수 -가 될 때 데미지 약해지도록 하기
        area *= 1 + add.area;
        projectileSpeed *= 1 + add.projectileSpeed;
        duration *= 1 + add.duration;
        weaponHaste += add.weaponHaste;
        magnet = magnet + add.magnet > 0.1f ? magnet + add.magnet : 0.1f;
        luck += add.luck;
        // todo 음수인 경우?
        expMultiplier *= 1 + add.expMultiplier;
        goldMultiplier *= 1 + add.goldMultiplier;
        
        
        if (skillHaste >= 0)
        {
            skillCooldown = 100 / (100 + skillHaste);
        }
        else
        {
            skillCooldown = 2 - 100 / (100 - skillHaste);
        }

        if (weaponHaste >= 0)
        {
            weaponCooldown = 100 / (100 + weaponHaste);
        }
        else
        {
            weaponCooldown = 2 - 100 / (100 - weaponHaste);
        }

    }
}
