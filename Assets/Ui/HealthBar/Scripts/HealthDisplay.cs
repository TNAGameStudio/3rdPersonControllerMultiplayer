using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

/*
public class HealthDisplay : NetworkBehaviour
{
    [Header("References")]

    [SerializeField] private Health health;
    [SerializeField] private Image healthBarImage;

    public override void OnNetworkSpawn()
    {
        if(!IsClient)
        {
            return;
        }
        health.CurrentHealth.OnValueChanged += HandleHealthChanged;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsClient)
        {
            return;
        }
        health.CurrentHealth.OnValueChanged -= HandleHealthChanged;
        HandleHealthChanged(0, health.CurrentHealth.Value);
    }

    private void HandleHealthChanged(int Old, int New)
    {
        float healthPercent = New /(float)health.MaxHealth;
        healthBarImage.fillAmount = healthPercent;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
*/