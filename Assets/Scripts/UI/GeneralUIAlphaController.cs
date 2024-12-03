using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralUIAlphaController : MonoBehaviour
{
    void Start()
    {
        GetComponent<CanvasGroup>().alpha = 1f;      
    }

}
