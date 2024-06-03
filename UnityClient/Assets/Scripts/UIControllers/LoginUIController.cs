using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginUIController : MonoBehaviour
{
    private bool isLoggedIn;
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private Button LoginButton ;

    public void Login()
    {
        if(isLoggedIn)
            return;
        
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
    }

    void Update()
    {
        
    }
}
