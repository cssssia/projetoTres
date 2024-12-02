using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandPositioner : MonoBehaviour
{
    [Header("Host")]
    public Vector3 positionToHost;
    public Vector3 rotationToHost;
    [Header("Client")]
    public Vector3 positionToClient;
    public Vector3 rotationToClient;


    [NaughtyAttributes.Button]
    public void SetHost()
    {
        transform.position = positionToHost;
        transform.rotation = Quaternion.Euler(rotationToHost);
    }

    [NaughtyAttributes.Button]
    public void SetClient()
    {
        transform.position = positionToClient;
        transform.rotation = Quaternion.Euler(rotationToClient);
    }
}
