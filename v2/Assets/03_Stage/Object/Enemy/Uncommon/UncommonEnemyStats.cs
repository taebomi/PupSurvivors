using System;
using System.Collections;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace PupSurvivors.Enemy
{
    [Serializable]
    public class UncommonEnemyStats
    {
        public int hp; // 체력
        public int power; // 닿았을 때 입을 데미지
        public float speed; // 기본 이동 속도
        public float acceleration; // 기본 이동 속도 복구량 ( 넉백 )
        public float mass; // 무게
        // 엘리트면 이대로 랜덤 배율로 드롭
        // 미니보스면 처치 소요 시간에 따라 비율로 드롭
        public int nyan;
        public int addExp;
        public float ratioExp;
        
        public SerializedDictionary<FloatOption, float> floatOptionDict;
        public SerializedDictionary<IntOption, float> intOptionDict;

        public enum FloatOption
        {
            Attack = 00000,
            AttackDuration =00001,
            AttackDuration1,
            AttackDuration2,
            AttackDurationChange = 00010,
            AttackMaxDuration = 90,
            AttackCooldown = 01000,
            
            
            
            Charging = 03000,
            ChargingDuration = 03001,
            
            ProjectileDuration = 05000,
            
            Movement = 10000,
            MaxSpeed = 10001,
            
            Misc = 90000,
            GroggyDuration = 90001,
            Option1 = 91000,
            Option2,
            Option3,
        }

        public enum IntOption
        {
        
        }
    }
}