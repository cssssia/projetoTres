using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HatchController : MonoBehaviour
{
    private Animator m_animator;

    private void Start()
    {
        m_animator = GetComponent<Animator>();
    }

    public Coroutine OpenHatch()
    {
        return StartCoroutine(IOpenHatch());
    }

    bool m_opened = false;
    IEnumerator IOpenHatch()
    {
        m_opened = false;
        m_animator.SetTrigger("Open");

        while (m_opened) yield return null;
    }
    public void OpenedHatchAnimEvent()
    {
        m_opened = true;
    }

    public Coroutine CloseHatch()
    {
        return StartCoroutine(ICloseHatch());
    }

    bool m_closed = false;
    IEnumerator ICloseHatch()
    {
        m_closed = false;
        m_animator.SetTrigger("Close");

        while (m_closed) yield return null;
    }

    public void ClosedHatchAnimEvent()
    {
        m_closed = true;
    }
}
