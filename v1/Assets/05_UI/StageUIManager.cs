using System.Collections;
using System.Collections.Generic;
using PupSurvivors.ObjectPool;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace PupSurvivors.Stage
{
    public class StageUIManager : Singleton<StageUIManager>
    {
        [SerializeField] private MessageBox messagebox;
        
        
        protected override void AwakeAfter()
        {
            _floatingDamagePool = new LimitedObjectPool<FloatingDamage>(CreateFloatingDamage, 75);
        }

        public void ShowMessageBox(string message)
        {
            Time.timeScale = 0f;
            messagebox.gameObject.SetActive(true);
            messagebox.ShowMessage(message);
        }

        public void RemoveMessageBox()
        {
            messagebox.gameObject.SetActive(false);
            Time.timeScale = 1f;
        }
        


        #region Floating Damage

        public void CreateFloatingDamage(int damage, bool isCritical, Vector3 pos)
        {
            var floatingDamage = _floatingDamagePool.Get();
            floatingDamage.Set(damage, isCritical, pos);
        }

        #region Floating Damage Pool

        [SerializeField] private FloatingDamage floatingDamagePrefab;
        [SerializeField] private Transform floatingDamageContainer;
        private LimitedObjectPool<FloatingDamage> _floatingDamagePool;

        private FloatingDamage CreateFloatingDamage()
        {
            var floatingDamage = Instantiate(floatingDamagePrefab, floatingDamageContainer);
            floatingDamage.SetManagedPool(_floatingDamagePool);
            return floatingDamage;
        }

        #endregion

        #endregion
    }
}