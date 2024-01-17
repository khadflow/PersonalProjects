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
            float offset = -0.55f;
            //Debug.Log(_degrees);
            if (_degrees == 90.0f || _degrees == 270.0f) { // left side of the screen

                clk_collider.center = new Vector3(clk_collider.center.x, clk_collider.center.y, clk_collider.center.z + offset); // = clk_collider.center;// += 0.1f;// = new Vector3(clk_collider.transform.position.x + 0.1f, clk_collider.transform.position.y, clk_collider.transform.position.z);

            }
        }

        public void setDegrees(float degrees)
        {
            _degrees = degrees;
        }
    }
}
