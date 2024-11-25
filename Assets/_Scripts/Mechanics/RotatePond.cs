using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class RotatePond : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 5;
    // Start is called before the first frame update
    void Start()
    {
        if (rotationSpeed <= 0)
            rotationSpeed = 5;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}
