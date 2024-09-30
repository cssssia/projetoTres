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
        GameInput.Instance.OnInteractAction += GameInput_OnClickDownMouse;
        GameInput.Instance.OnStopInteractAction += GameInputOnClickUpMouse;
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
        if (Physics.Raycast(l_ray, out l_mousePosRaycastHit))
        {
            CheckHoverOnObject(l_mousePosRaycastHit.transform.gameObject);
        }

        m_handBehavior.UpdateMousePos(Input.mousePosition);
    }

    private void CheckHoverOnObject(GameObject p_gameObject)
    {
        //atualmente, só está checando cartas, mas aqui podemos chegar itens tambem

        bool l_find = m_handBehavior.CheckHoverObject(p_gameObject);
    }

    private void GameInput_OnClickDownMouse(object p_sender, System.EventArgs e)
    {
        Ray l_ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(l_ray, out l_mousePosRaycastHit))
        {
            if (l_mousePosRaycastHit.transform != null && l_mousePosRaycastHit.transform.gameObject != null)
                CheckClickOnObjects(l_mousePosRaycastHit.transform.gameObject);
            else CheckClickOnObjects(null);
        }
    }

    private void CheckClickOnObjects(GameObject p_gameObject)
    {
        //atualmente, só está checando cartas, mas aqui podemos chegar itens tambem

        bool l_find = m_handBehavior.CheckClickObject(p_gameObject);
    }

    private void GameInputOnClickUpMouse(object p_sender, System.EventArgs e)
    {
        m_handBehavior.CheckClickUp();
    }
}
