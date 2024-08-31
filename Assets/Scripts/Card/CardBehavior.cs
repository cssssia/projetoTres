using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class CardTransform
{
    public Vector3 Position;
    public Vector3 Rotation;
}


public class CardBehavior : MonoBehaviour
{
    [SerializeField] private float m_cardAnimTime;
    [SerializeField] private AnimationCurve m_cardAnimCurve;
    private Vector3 m_startPosition;
    private Vector3 m_startRotation;

    private void Start()
    {
        m_startPosition = transform.position;
        m_startRotation = transform.rotation.eulerAngles;
    }

    public void ResetTransform()
    {
        transform.position = m_startPosition;
        transform.rotation = Quaternion.Euler(m_startRotation); 
    }

    public void AnimateToPlace(CardTransform p_cardTransform)
    {
        StartCoroutine(IAnimateToPlace(p_cardTransform));
    }

    Vector3 l_tempPosition;
    Vector3 l_initialPosition;
    Vector3 l_tempRotation;
    Vector3 l_initialRotation;
    IEnumerator IAnimateToPlace(CardTransform p_cardTransform)
    {
        l_initialPosition = transform.position;
        l_initialRotation = transform.rotation.eulerAngles;

        for (float time = 0f; time < m_cardAnimTime; time += Time.deltaTime)
        {
            l_tempPosition = Vector3.Lerp(l_initialPosition, p_cardTransform.Position, m_cardAnimCurve.Evaluate(time / m_cardAnimTime));
            l_tempRotation = Vector3.Lerp(l_initialRotation, p_cardTransform.Rotation, m_cardAnimCurve.Evaluate(time / m_cardAnimTime));

            transform.position = l_tempPosition;
            transform.rotation = Quaternion.Euler(l_tempRotation);

            yield return null;

        }
    }
}
