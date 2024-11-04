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
        GameInput.Instance.OnStopInteractAction += GameInput_OnClickUpMouse;
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
        //atualmente, s� est� checando cartas, mas aqui podemos chegar itens tambem

        bool l_find = m_handBehavior.CheckHoverObject(p_gameObject);
    }

    Ray l_rayClickDown;
    private void GameInput_OnClickDownMouse(object p_sender, System.EventArgs e)
    {
        l_rayClickDown = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(l_rayClickDown, out l_mousePosRaycastHit))
        {
            if (l_mousePosRaycastHit.transform != null)
                CheckClickOnObjects(l_mousePosRaycastHit.transform.gameObject);
            else CheckClickOnObjects(null);
        }
    }

    private void CheckClickOnObjects(GameObject p_gameObject)
    {
        //atualmente, s� est� checando cartas, mas aqui podemos chegar itens tambem

        bool l_find = m_handBehavior.CheckClickObject(p_gameObject);
    }

    private void GameInput_OnClickUpMouse(object p_sender, System.EventArgs e)
    {
        m_handBehavior.CheckClickUp(true, null, null);
    }
}
