using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarterAssets
{
    public class ClkCollider : MonoBehaviour
    {

        private BoxCollider clk_collider;
        private float _degrees = -1.0f;

        // Start is called before the first frame update
        void Start()
        {
            clk_collider = GetComponent<BoxCollider>();
        }

        // Update is called once per frame
        void Update()
        {
 
        }

        private void LateUpdate()
        {
            float offset = -1.5f;

            if (_degrees == 90.0f || _degrees == 270.0f)
            {

                clk_collider.center = new Vector3(clk_collider.center.x, clk_collider.center.y, clk_collider.center.z + offset);

            }
        }

        public void setDegrees(float degrees)
        {
            _degrees = degrees;
        }
    }
}
