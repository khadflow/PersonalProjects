using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarterAssets
{
    public class WeaponDamageProjectiles : MonoBehaviour
    {

        public GameObject bullet;
        private GameObject bull;
        private float shootForce = 5.0f;
        private bool Attacking = false;
        private string Tag;
        public bool switchCondition = true;

        private float NextShootTime;
        private float ShootCooldown = 2.0f;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (NextShootTime < Time.time && Attacking)
            {
                NextShootTime = Time.time + ShootCooldown;
                bull = Instantiate(bullet, new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z), Quaternion.identity);
                bull.GetComponent<WeaponDamageHandler>().Attack();
                bull.transform.tag = Tag;
                bull.GetComponent<WeaponDamageHandler>().tag = Tag;
                float direction = 1.0f;
                if (!switchCondition && Tag == "Player")
                {
                    direction = -1.0f;
                } else if (switchCondition && Tag == "Player2")
                {
                    direction = -1.0f;
                }
                bull.GetComponent<Rigidbody>().AddForce(new Vector3(direction, 0.0f, 0.0f) * shootForce, ForceMode.Impulse);
                Attacking = false;
            }
            else if (NextShootTime < Time.time + 1.0f)
            {
                Destroy(bull);
            }
        }

        public void Attack()
        {
            Attacking = true;
        }

        public void SetTag(string tag)
        {
            Tag = tag;
        }
    }
}