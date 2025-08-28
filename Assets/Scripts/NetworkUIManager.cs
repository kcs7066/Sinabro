using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUIManager : MonoBehaviour
{
    public Button hostButton;
    public Button clientButton;

    void Start()
    {
        // Host 버튼을 누르면 StartHost 함수가 실행되도록 연결
        hostButton.onClick.AddListener(() => {
            NetworkManager.Singleton.StartHost();
            HideButtons();
        });

        // Client 버튼을 누르면 StartClient 함수가 실행되도록 연결
        clientButton.onClick.AddListener(() => {
            NetworkManager.Singleton.StartClient();
            HideButtons();
        });
    }

    // 버튼을 누른 후에는 숨깁니다.
    void HideButtons()
    {
        hostButton.gameObject.SetActive(false);
        clientButton.gameObject.SetActive(false);
    }
}