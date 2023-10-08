using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AOETargeting : MonoBehaviour
{
    public Ray LastRay { get; private set; }
    public RaycastHit LastHit { get; private set; }

    private GameObject TargatePlacementInstance;

    public void Activate(GameObject targetPlacementPrefab)
    {
        this.enabled = true;

        if(TargatePlacementInstance)
        {
            Destroy(TargatePlacementInstance);
        }

        TargatePlacementInstance = Instantiate(targetPlacementPrefab);

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }
    public void Deactivate()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Destroy(TargatePlacementInstance);
        this.enabled = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        this.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        //get cursor screen position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        //trace ray to enviorment
        if (Physics.Raycast(ray, out hit))
        {
            Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red);
            LastRay = ray;
            LastHit = hit;

            TargatePlacementInstance.transform.position = hit.point + hit.normal*0.01f;
            TargatePlacementInstance.transform.forward = hit.normal;
        }
    }

    RaycastHit GetLastHit()
    {
        return LastHit;
    }
}
