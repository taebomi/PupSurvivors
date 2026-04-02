using System;

namespace PupSurvivors.Enemy
{
    [Serializable]
    public class EnemyStats
    {
        public int hp;
        public int power;
        public float speed;
        public float acceleration;
        public int exp;


        public EnemyStats(EnemyStats copy)
        {
            hp = copy.hp;
            power = copy.power;
            speed = copy.speed;
            acceleration = copy.acceleration;
            exp = copy.exp;
        }
    

    }
}