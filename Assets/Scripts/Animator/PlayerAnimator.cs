using Unity.Netcode;
using UnityEngine;

public class PlayerAnimator : NetworkBehaviour
{
    [SerializeField] private PlayerController m_player;
    private Animator m_animator;

    void Awake()
    {
        m_animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!IsOwner)
            return;
    }

}
