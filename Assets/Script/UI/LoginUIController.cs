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
        // 버튼 클릭 연결
        loginButton.onClick.RemoveAllListeners();
        loginButton.onClick.AddListener(OnClickLogin);
    }

    private void OnClickLogin()
    {
        var id = userId.text.Trim();
        var pw = password.text; // 비번은 Trim 안 하는 게 보통 좋아

        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(pw))
        {
            Debug.LogWarning("아이디/비밀번호를 입력해줘!");
            return;
        }
        Debug.Log($"id:{id}, pw:{pw}");
    }

    void Update()
    {
        
    }
}
