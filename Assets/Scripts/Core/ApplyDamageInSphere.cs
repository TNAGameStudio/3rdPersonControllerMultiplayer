using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ApplyDamageInSphere : MonoBehaviour
{
    public enum ApplicationTime
    {
        OnStart = 0,
        OnDestruction
    }

    [SerializeField] public float Radius;
    [SerializeField] public int DamageTaken;
    [SerializeField] public ApplicationTime WhenToApply = ApplicationTime.OnStart;

    // Start is called before the first frame update

    private void Start()
    {
        if (WhenToApply == ApplicationTime.OnStart)
        {
            Apply();
        }
    }
    private void OnDestroy()
    {
        if(WhenToApply == ApplicationTime.OnDestruction)
        {
            Apply();
        }
    }

    private void Apply()
    {
        //I should probably use layer masks eventually here for speed
        Collider[] ObjectsInSphere = Physics.OverlapSphere(transform.position, Radius);

        foreach (Collider col in ObjectsInSphere)
        {
            Health healthScript = col.gameObject.GetComponent<Health>();
            if (healthScript)
            {
                Debug.Log("Health Script Found on " + col.gameObject.name);
                healthScript.TakeDamage(DamageTaken);
            }
        }
    }
}
