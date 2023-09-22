using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] public string CameraTag = "MainCamera";
    private Transform CameraTransform;

    private void Start()
    {
        GameObject camera = GameObject.FindGameObjectWithTag(CameraTag);
        CameraTransform = camera.transform;

    }
    private void LateUpdate()
    {
        transform.LookAt(transform.position + CameraTransform.forward);
    }
}
