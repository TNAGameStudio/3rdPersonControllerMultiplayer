using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : NetworkBehaviour
{
    // Start is called before the first frame update
    public Slider HealthSlider;
    public Gradient HealthBarGradient;
    public GameObject AttachedObject;

    public override void OnNetworkSpawn()
    {
        if(IsOwner)
        {
            //dont display for player
            gameObject.SetActive(false);
        }

        //if the entity that owns this is the player character, dont show
        Health HealthScript = GetComponentInParent<Health>();
        HealthScript.OnHealthChange += HealthChanged;
    }

    public override void OnNetworkDespawn()
    {
        Health HealthScript = GetComponentInParent<Health>();
        HealthScript.OnHealthChange -= HealthChanged;
    }

    private void HealthChanged(int MaxHealth, int CurrentHealth)
    {
        SetMaxHealth(MaxHealth);
        SetHealth(CurrentHealth);
    }

    void SetMaxHealth(int maxHealth)
    {
        HealthSlider.maxValue = maxHealth;
    }

    void SetHealth(int Health)
    {
        HealthSlider.value = Health;
    }

}
