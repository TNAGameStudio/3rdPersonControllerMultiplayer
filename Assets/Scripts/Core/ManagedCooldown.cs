using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public struct Cooldown : INetworkSerializable, System.IEquatable<Cooldown> 
{
    public FixedString32Bytes name;
    public float cooldown;
    public float lastReset;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            var reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out name);
            reader.ReadValueSafe(out cooldown);
            reader.ReadValueSafe(out lastReset);
        }
        else
        {
            var writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(name);
            writer.WriteValueSafe(cooldown);
            writer.WriteValueSafe(lastReset);
        }
    }

    public bool Equals(Cooldown other)
    {
        return name == other.name;
    }
}

public class ManagedCooldown : NetworkBehaviour
{

    NetworkList<Cooldown> playerCooldowns;

    private void Awake()
    {
        playerCooldowns = new NetworkList<Cooldown>();
    }

    public void SetCooldown(FixedString32Bytes name, float cooldown)
    {
        Cooldown cool;
        cool.name = name;
        cool.cooldown = cooldown;
        cool.lastReset = -cooldown;

        if(IsServer)
        {
            playerCooldowns.Add(cool);
        }

    }

    public int GetCooldownId(string name)
    {
        int index = 0;
        foreach(Cooldown cool in playerCooldowns)
        {
            if(cool.name == name)
            {
                return index;
            }

            index++;
        }

        return -1;
    }


    public bool OnCooldown(int cooldownID)
    {
        if(cooldownID >= playerCooldowns.Count)
        {
            Debug.Log($"Invalid cooldown {cooldownID}");
            return false;
        }

        Cooldown cooldown = playerCooldowns[cooldownID];
        return (NetworkManager.Singleton.ServerTime.Time - cooldown.lastReset) < cooldown.cooldown;
    }

    public float GetRemainingCooldownTime(int cooldownID)
    {
        if (cooldownID >= playerCooldowns.Count)
        {
            Debug.Log($"Invalid cooldown {cooldownID}");
            return 0.0f;
        }


        Cooldown cooldown = playerCooldowns[cooldownID];
        float remainingTime = (float)(cooldown.cooldown - (NetworkManager.Singleton.ServerTime.Time - cooldown.lastReset));
        return remainingTime;
    }

    public bool ConsumeCooldown(int cooldownID)
    {
        if (!NetworkManager.Singleton.IsServer || OnCooldown(cooldownID))
        {
            return false;
        }

        Cooldown cool = playerCooldowns[cooldownID];
        cool.lastReset = (float)NetworkManager.Singleton.ServerTime.Time;
        playerCooldowns.Insert(cooldownID, cool);
        return true;
    }
}