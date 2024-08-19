using System.Collections.Generic;
using UnityEngine;

public class CameraController : Singleton<CameraController>
{
    [SerializeField]private Camera m_camera;
    [SerializeField] private List<Vector3> m_cameraSpawnPositionList;
    [SerializeField] private List<Quaternion> m_cameraSpawnRotationList;

    public void SetCamera(int p_index)
    {
        m_camera.transform.SetPositionAndRotation(m_cameraSpawnPositionList[p_index], m_cameraSpawnRotationList[p_index]);
    }

}
