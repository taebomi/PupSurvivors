using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace PupSurvivors.Equipment
{
    public class Shield : WeaponBase
    {
        public Stack<ShieldSummoned> SummonedStack { get; private set; }
        public ProjectilePool<ShieldProjectile> ProjectilePool { get; private set; }

        private int _summonedNum;

        protected override async UniTask InitializeAfter()
        {
            SummonedStack = new Stack<ShieldSummoned>(3);
            _summonedNum = 1;
            ProjectilePool = new ProjectilePool<ShieldProjectile>(this, InitProjectile);
            await ProjectilePool.Initialize();
            await SummonedWeaponBase<ShieldSummoned, Shield>.Initialize(this);
            SummonedStack.Push(SummonedWeaponBase<ShieldSummoned, Shield>.CreateInstance());
            AttackAction += Attack;
            CheckCooldown().Forget();
        }

        private static void InitProjectile(ShieldProjectile projectile)
        {
            projectile.Init(Player.transform);
        }

        private async UniTaskVoid CheckCooldown()
        {
            var timer = 0f;

            while (true)
            {
                if (timer > ModifiedWeaponStats.cooldown)
                {
                    timer = 0f;
                    Attack();
                }

                if (SummonedStack.Count == _summonedNum)
                {
                    timer += Time.deltaTime;
                }

                await UniTask.Yield(DestroyCts.Token);
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private void Attack()
        {
            if (!SummonedStack.TryPop(out var result))
            {
                result = SummonedWeaponBase<ShieldSummoned, Shield>.CreateInstance();
                _summonedNum++;
            }

            result.Attack();
        }


        protected override void OnDestroy()
        {
            base.OnDestroy();
            foreach (var shieldSummoned in SummonedStack)
            {
                if (shieldSummoned)
                {
                    Destroy(shieldSummoned.gameObject);
                }
            }

            ProjectilePool.Dispose();
            SummonedWeaponBase<ShieldSummoned, Shield>.Dispose();
        }
    }
}