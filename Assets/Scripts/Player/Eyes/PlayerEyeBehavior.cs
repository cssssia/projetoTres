using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEyeBehavior : MonoBehaviour
{
    public MeshRenderer mesh;

    public void SetCover(bool p_cover)
    {
        mesh.enabled = !p_cover;
    }
}
