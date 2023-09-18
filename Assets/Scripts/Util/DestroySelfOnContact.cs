using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroySelfOnContact : MonoBehaviour
{
    private void OnTriggerEnter(Collider collision)
    {
        GameObject.Destroy(gameObject); 
    }
}
