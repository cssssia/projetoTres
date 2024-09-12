using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCardAnimation : MonoBehaviour
{
    [SerializeField] private CardBehavior cardBehavior;
    public bool reset;

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit);

            if(hit.transform != null)
            {
                CardTransform cardTr = new()
                {
                    Position = hit.transform.position,
                    Rotation = new Vector3(0, 0 ,0)
                };

                cardBehavior.AnimateToPlace(cardTr, CardAnimType.PLAY);
            }
        }

        if(reset)
        {
            cardBehavior.ResetTransform();
            reset = false;
        }
    }
}
