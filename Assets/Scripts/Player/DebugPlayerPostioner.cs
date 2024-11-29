using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugPlayerPostioner : MonoBehaviour
{
 public PlayerController playerController;

#if UNITY_EDITOR

    [NaughtyAttributes. Button]
    public void SetAsHost()
    {
     playerController.SetAsHost();
    }

    [NaughtyAttributes.Button]
    
    public void SetAsClient()
    {
     playerController.SetAsClient();
    }
#endif

}
