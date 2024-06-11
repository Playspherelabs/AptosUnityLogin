using System;
using System.Collections;
using System.Collections.Generic;
using Aptos.Unity.Rest.Model;
using Aptos.Unity.Sample.UI;
using TMPro;
using UnityEngine;


namespace AptosUnityLogin.AuthController{

public class LocalWalletController : MonoBehaviour
{

    public void OnCreateWalletClicked(TMP_InputField createdMnemonicInputField)
        {
            if (AptosUILink.Instance.CreateNewWallet())
            {
                createdMnemonicInputField.text = PlayerPrefs.GetString(AptosUILink.Instance.mnemonicsKey);
                ToggleEmptyState(false);
                ToggleNotification(ResponseInfo.Status.Success, "Successfully Create the Wallet");
            }
            else
            {
                ToggleEmptyState(true);
                ToggleNotification(ResponseInfo.Status.Failed, "Fail to Create the Wallet");
            }

        }

    private void ToggleNotification(ResponseInfo.Status success, string v)
    {
        Debug.Log(success);
    }

    private void ToggleEmptyState(bool v)
    {
        Debug.Log(v);
    }

    public void OnImportWalletClicked(TMP_InputField _input)
        {
            if (AptosUILink.Instance.RestoreWallet(_input.text))
            {
                ToggleEmptyState(false);
                ToggleNotification(ResponseInfo.Status.Success, "Successfully Import the Wallet");
            }
            else
            {
                ToggleEmptyState(true);
                ToggleNotification(ResponseInfo.Status.Failed, "Fail to Import the Wallet");
            }
        }
}
}