using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace AptosUnityLogin.AuthController{

public class LoginUIController : MonoBehaviour
{
    private bool isLoggedIn;
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private Button CloseButton;
    [SerializeField] private Button LoginButton;

    public void Login()
    {

        isLoggedIn = true;
        loginPanel.SetActive(true);
        mainPanel.SetActive(false);
    }

    public void Logout()
    {
        isLoggedIn = false;
        loginPanel.SetActive(true);
        mainPanel.SetActive(false);
    }

    public bool IsLoggedIn()
    {
        return isLoggedIn;
    }


    void Start()
    {
        loginPanel.SetActive(false);
        mainPanel.SetActive(false);
        isLoggedIn = false;
        LoginButton.onClick.AddListener(Login);
        CloseButton.onClick.AddListener(() => {
            loginPanel.SetActive(false);
            mainPanel.SetActive(true);
        
        });
    }

}
}