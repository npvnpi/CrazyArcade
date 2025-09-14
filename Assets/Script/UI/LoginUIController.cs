using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginUIController : MonoBehaviour
{
    public TMP_InputField userId;
    public TMP_InputField password;
    public Button loginButton;
    private NetworkManager networkManager;

    void Start()
    {
        GameObject networkGo = GameObject.Find("NetworkManager");
        networkManager =  networkGo.GetComponent<NetworkManager>();
        networkManager.Connect("127.0.0.1", 5000);
        // ��ư Ŭ�� ����
        loginButton.onClick.RemoveAllListeners();
        loginButton.onClick.AddListener(OnClickLogin);
    }

    private void OnClickLogin()
    {
        var id = userId.text.Trim();
        var pw = password.text; // ����� Trim �� �ϴ� �� ���� ����

        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw))
        {
            Debug.LogWarning("���̵�/��й�ȣ�� �Է�����!");
            return;
        }
        Debug.Log($"id:{id}, pw:{pw}");
    }

    void Update()
    {
        
    }
}
