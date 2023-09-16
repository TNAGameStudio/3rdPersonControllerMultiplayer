using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    private Rigidbody bulletRigidbody;

    void Awake() {
        bulletRigidbody = GetComponent<Rigidbody>();
    }

    void Start() {

        Debug.Log("ELDJKFLDJFLD:FJ:DLF");
        float speed = 10f;
        bulletRigidbody.velocity = transform.forward * speed;
    Debug.Log("Vector:" + transform.forward +  " and speed " + speed);
    }

    private void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);
    }

}
