using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarterAssets
{
    public class WeaponDamageHandler : MonoBehaviour
    {
        private bool TakeDamage;
        public bool Attacking = false;
        private float NextAttackTime;
        private float CoolDownTime = 1.0f;

        void OnTriggerEnter(Collider opponent)
        {
            Debug.Log("Enter");
            string tag = transform.tag;
            TakeDamage = false;
            if (tag != opponent.gameObject.tag)
            {
                TakeDamage = true;
            }
            C_Controller opp = opponent.GetComponent<C_Controller>();
            if (Attacking && TakeDamage && NextAttackTime < Time.time)
            {
                opp.TakeDamage(1);
                TakeDamage = false;
                NextAttackTime = Time.time + CoolDownTime;
                Attacking = false;
            } else
            {
                Attacking = false;
                TakeDamage= false;
            }
            Debug.Log("Exit");
        }

        void OnTriggerStay(Collider opponent)
        {
            
        }

        void OnTriggerExit(Collider opponent)
        {
           
        }
    }

}