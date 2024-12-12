using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BetHandAnimator : MonoBehaviour
{
    public Animator handAnimator;
    public HatchController hatchController;

    bool l_waitingAnim;

    public Action<bool> OnArrivedPlayer;
    public Action OnDeliveredButton;
    public Action OnEndedAnim;

    private void Start()
    {
        hatchController = FindObjectOfType<HatchController>();
    }

    public IEnumerator GetEyebutton(Action p_action)
    {
        handAnimator.SetTrigger("Bet");

        yield return hatchController.OpenHatch();

        l_waitingAnim = true;
        
        while(l_waitingAnim) yield return null; // espera pra entregar o botão
        Debug.Log("entrega o botão");
        OnDeliveredButton.Invoke();

        l_waitingAnim = true;
        while(l_waitingAnim) yield return null; // espera pra acabar a animação
        p_action?.Invoke();

        Debug.Log("fecha a ecotilha");
        yield return hatchController.CloseHatch();
    }

    public void ArrivedPlayer() => OnArrivedPlayer?.Invoke(true);
    public void EndArrivedPlayer() => OnArrivedPlayer?.Invoke(false);

    public void DeliverButton()
    {
        l_waitingAnim = false;
    }

    public void EndAnim() => l_waitingAnim = false;
}
