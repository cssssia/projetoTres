using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerUI : MonoBehaviour
{
    [SerializeField] private Button m_hostButton;
    [SerializeField] private Button m_clientButton;

    void Awake()
    {

        m_hostButton.onClick.AddListener(() => {
            Debug.Log("Host Started");
            CameraController.Instance.SetHostCamera();
            NetworkManager.Singleton.StartHost();
            Hide();
        });

        m_clientButton.onClick.AddListener(() => {
            Debug.Log("Client Started");
            CameraController.Instance.SetClientCamera();
            NetworkManager.Singleton.StartClient();
            Hide();
        });

    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

}
