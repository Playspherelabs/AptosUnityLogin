using UnityEngine;
using UnityEngine.SceneManagement;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        CheckAuthStatus();
    }

    private void CheckAuthStatus()
    {
        string idToken = PlayerPrefs.GetString("id_token");
        string mnemonicsKey = PlayerPrefs.GetString("mnemonicsKey");
        string accountAddress = PlayerPrefs.GetString("keylessAccountAddress");
        string petraAddress = PlayerPrefs.GetString("petraAddress");

        if (!string.IsNullOrEmpty(idToken))
        {
            // Google Auth detected
            LoadMainScene();
        }
        else if (!string.IsNullOrEmpty(mnemonicsKey))
        {
            // Local Wallet Auth detected
            LoadMainScene();
        }
        else if (!string.IsNullOrEmpty(accountAddress))
        {
            // Petra Wallet Auth detected
            LoadMainScene();
        }
        else
        {
            // No valid authentication found, reload to Login Scene.
            SceneManager.LoadScene("LoginScene");
        }
    }

    public void HandleSuccessfulLogin(string method)
    {
        CleanUpOtherAuthMethods(method);
        PlayerPrefs.Save();
        LoadMainScene();
    }

    private void CleanUpOtherAuthMethods(string method)
    {
        if (method != "google")
        {
            PlayerPrefs.DeleteKey("id_token");
            PlayerPrefs.DeleteKey("keylessAccountAddress");
        };
        if (method != "localwallet") PlayerPrefs.DeleteKey("mnemonicsKey");
        if (method != "petra") PlayerPrefs.DeleteKey("petraAddress");
    }

    private void LoadMainScene()
    {
        SceneManager.LoadScene("MainScene");
    }
}
