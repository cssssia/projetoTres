using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCardAnimation : MonoBehaviour
{
    [SerializeField] private CardBehavior cardBehavior;
    [SerializeField] private CardsOnHandBehavior m_handBehavior;
    public bool reset;

    private void Start()
    {
        GameInput.Instance.OnMoveMouse += GameInput_OnMoveMouse;

    }

    private void Update()
    {
        //if(Input.GetMouseButtonDown(0))
        //{
        //    RaycastHit hit;
        //    Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit);

        //    if(hit.transform != null)
        //    {
        //        CardTransform cardTr = new()
        //        {
        //            Position = hit.transform.position,
        //            Rotation = new Vector3(0, 0 ,0)
        //        };

        //        cardBehavior.AnimateToPlace(cardTr, CardAnimType.PLAY);
        //    }
        //}

        //if(reset)
        //{
        //    cardBehavior.ResetTransform();
        //    reset = false;
        //}
    }

    RaycastHit l_mousePosRaycastHit;
    private void GameInput_OnMoveMouse(object p_sender, System.EventArgs e)
    {
        Ray l_ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(l_ray, out l_mousePosRaycastHit, 10f))
        {
            if (l_mousePosRaycastHit.transform != null)
            {
                //Our custom method.
                Debug.Log("finde transform" + l_mousePosRaycastHit.transform.gameObject);
                CheckHoverOnObject(l_mousePosRaycastHit.transform.gameObject);
            }
        }
    }

    private void CheckHoverOnObject(GameObject p_gameObject)
    {
        bool l_find = m_handBehavior.CheckObject(p_gameObject);
        if (l_find) Debug.Log("hover");
    }

}
