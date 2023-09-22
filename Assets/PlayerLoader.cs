using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerLoader : NetworkBehaviour
{
    [SerializeField] public string[] CharacterNames;
    [SerializeField] public GameObject[] CharacterPrefabs;

    [ServerRpc(RequireOwnership = false)] //server owns this object but client can request a spawn
    public void SpawnPlayerServerRpc(ulong clientId, string playerName)
    {
        int playerIndex = 0;
        for (playerIndex = 0; playerIndex < CharacterNames.Length; playerIndex++)
        {
            if (CharacterNames[playerIndex] == playerName)
            {
                break;
            }
        }

        GameObject characterPrefab = CharacterPrefabs[playerIndex];
        GameObject newPlayer = Instantiate(characterPrefab);

        var netObj = newPlayer.GetComponent<NetworkObject>();
        newPlayer.SetActive(true);
        netObj.SpawnAsPlayerObject(clientId, true);
    }
 
    public override void OnNetworkSpawn()
    {
        if(!IsClient)
        {
            //dont spawn if running pure server
            return;
        }

        Debug.Log("PlayerLoader Called");
        GameObject nameObject = GameObject.FindGameObjectWithTag("PlayerName");
        string playerName = nameObject.GetComponent<PlayerName>().CharacterName;

        //spawn the correct player for this client
        SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId, playerName);
    }
}
