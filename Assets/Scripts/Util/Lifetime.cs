using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lifetime : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] public float lifetime = 3.0f;

    void Start()
    {
        Destroy(gameObject, lifetime);  
    }
}
