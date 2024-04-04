using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarterAssets
{
    public class WeaponDamageHandler : MonoBehaviour
    {
        private bool TakeDamage;
        private bool Attacking = false;
        private float NextAttackTime;
        private float CoolDownTime = 0.7f;

        void OnTriggerEnter(Collider opponent)
        {
            string tag = transform.tag;
            TakeDamage = false;
            if (tag != opponent.gameObject.tag)
            {
                TakeDamage = true;
            }
            C_Controller opp = opponent.GetComponent<C_Controller>();
            if (Attacking && TakeDamage && NextAttackTime < Time.time && opp != null)
            {
                opp.TakeDamage(1);
                TakeDamage = false;
                NextAttackTime = Time.time + CoolDownTime;
                Attacking = false;
            }
        }

        public float GetCoolDownTime()
        {
            return CoolDownTime;
        }

        void OnTriggerStay(Collider opponent)
        {
            
        }

        void OnTriggerExit(Collider opponent)
        {
           
        }

        public void Attack()
        {
            Attacking = true;
        }

        public void StopAttack()
        {
            Attacking = false;
        }
    }

}