using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class LP_HealthBar : MonoBehaviour
{
    private Slider slider;
    public static LP_HealthBar instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        slider = GetComponent<Slider>();
    }

    public void UpdateHealthValue(float Health)
    {
        slider.value = Health;
    }
}
