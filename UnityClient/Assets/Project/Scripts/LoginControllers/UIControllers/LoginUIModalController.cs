using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AptosUnityLogin.AuthController{

public class LoginUIModalController : MonoBehaviour
{
    [SerializeField] private Button GoogleAuthButton;
    [SerializeField] private Button PetraAuthButton;
    [SerializeField] private Button ImportAuthButton;
    [SerializeField] private Button CreateAuthButton;
    [SerializeField] private GameObject LoginModal;
    [SerializeField] private GameObject ImportModal;
    [SerializeField] private Button ImportModalClose;
    [SerializeField] private Button CreateModalClose;
    [SerializeField] private Button ImportMnemonicInputButton;
    [SerializeField] private TMP_InputField MnemonicInput;
    [SerializeField] private TMP_InputField MnemonicOutput;
    
    [SerializeField] private LocalWalletController localWalletController;

    [SerializeField] private GameObject CreateModal;


    public GoogleAuthController googleAuthController;
    public PetraAuthController deeplinkHandler;


    void Start()
    {
        // Add event listeners to the buttons
        try
        {
            ImportAuthButton.onClick.AddListener(OpenImportModal);
            CreateAuthButton.onClick.AddListener(OpenCreateModal);
            ImportModalClose.onClick.AddListener(() =>
            {
                ImportModal.SetActive(false);
            });
            CreateModalClose.onClick.AddListener(() =>
            {
                CreateModal.SetActive(false);
            });
            ImportMnemonicInputButton.onClick.AddListener(ImportMnemonic);
            GoogleAuthButton.onClick.AddListener(() =>
            {
                googleAuthController.HandleAuthClick();
            });
            PetraAuthButton.onClick.AddListener(() =>
            {
                deeplinkHandler.HandleConnectWallet();
            });
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
    }

    private void ImportMnemonic()
    {
        localWalletController.OnImportWalletClicked(MnemonicOutput);
    }

    void OpenImportModal()
    {
        // Close the login modal
        LoginModal.SetActive(false);

        // Open the import modal
        ImportModal.SetActive(true);
        
    }

    void OpenCreateModal()
    {
        // Close the login modal
        LoginModal.SetActive(false);

        // Open the create modal
        CreateModal.SetActive(true);
                localWalletController.OnCreateWalletClicked(MnemonicInput);

    }
}
}