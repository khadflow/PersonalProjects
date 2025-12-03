using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StarterAssets
{
    public class PlayerHealth : MonoBehaviour
    {
        private Slider slider;

        // Start is called before the first frame update
        void Start()
        {
            slider = GetComponent<Slider>();
            slider.maxValue = C_Controller.GetMaxHealth();
            slider.value = slider.maxValue;
        }

        public void SetHealth(float health)
        {
            slider = GetComponent<Slider>();
            slider.value = health; 
        }
    }
}