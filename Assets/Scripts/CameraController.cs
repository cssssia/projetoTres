using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CameraController : Singleton<CameraController>
{
    [SerializeField]private Camera m_camera;

    [Header("Host Camera")]
    [SerializeField] private Vector3 m_hostCameraPosition;
    [SerializeField] private Quaternion m_hostCameraRotation;

    [Header("Client Camera")]
    [SerializeField] private Vector3 m_clientCameraPosition;
    [SerializeField] private Quaternion m_clientCameraRotation;

    public void SetHostCamera()
    {
        m_camera.transform.SetPositionAndRotation(m_hostCameraPosition, m_hostCameraRotation);
    }

    public void SetClientCamera()
    {
        m_camera.transform.SetPositionAndRotation(m_clientCameraPosition, m_clientCameraRotation);
    }
}
