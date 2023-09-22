using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Health : NetworkBehaviour
{
    //I want to consider making this a structure, the issue is if synchronization becomes lest efficent
    public NetworkVariable<int> MaxHealth = new NetworkVariable<int>();
    public NetworkVariable<int> CurrentHealth = new NetworkVariable<int>();


    private bool isDead;

    public Action<Health> OnDie;
    public Action<int, int> OnHealthChange;

    // Start is called before the first frame update
    void Start()
    {
     
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnNetworkSpawn()
    {
        MaxHealth.OnValueChanged += MaxHealthChanged;
        CurrentHealth.OnValueChanged += CurrentHealthChanged;

        if (!IsServer)
        { return; }

        //only server can set values
        CurrentHealth.Value = MaxHealth.Value;
    }

    public override void OnNetworkDespawn()
    {
        MaxHealth.OnValueChanged -= MaxHealthChanged;
        CurrentHealth.OnValueChanged -= CurrentHealthChanged;
    }

    //broadcast to listeners, ui elements, hud, animation whatever.
    private void MaxHealthChanged(int Previous, int Current)
    {
        OnHealthChange?.Invoke(Current, MaxHealth.Value);
    }

    private void CurrentHealthChanged(int Previous, int Current)
    {
        OnHealthChange?.Invoke(MaxHealth.Value, Current);
    }

    public void TakeDamage(int damageValue)
    {
        ModifyHealth(-damageValue);
    }

    public void RestoreHealth(int healValue)
    {
        ModifyHealth(healValue);
    }

    private void ModifyHealth(int value)
    {
        if(!IsServer || isDead)
        {
            return;
        }

        int newHealth = CurrentHealth.Value + value;
       
        CurrentHealth.Value = Mathf.Clamp(newHealth, 0, MaxHealth.Value);

        if(CurrentHealth.Value == 0)
        {
            OnDie?.Invoke(this);
            isDead = true;
        }
    }
}
