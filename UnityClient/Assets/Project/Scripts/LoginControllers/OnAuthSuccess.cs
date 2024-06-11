using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AptosUnityLogin.AuthController
{
    public class OnAuthSuccess : MonoBehaviour
    {
        public TMP_Text accountInfo;

        void OnEnable()
        {
            // Check for Google keyless account
            CheckGoogleLogin();

            // Check for Petra wallet account
            CheckPetraLogin();

            // If neither found, go to Login Scene
            NavigateToLoginIfNoAuth();
        }

        private void CheckGoogleLogin()
        {
            string keylessAccountAddress = PlayerPrefs.GetString("keylessAccountAddress");
            string idToken = PlayerPrefs.GetString("id_token");
            Debug.Log("Google keylessAccountAddress: " + keylessAccountAddress);
            Debug.Log("Google idToken: " + idToken);

            if (!string.IsNullOrEmpty(keylessAccountAddress) && !string.IsNullOrEmpty(idToken))
            {
                accountInfo.text = "Google Account: " + keylessAccountAddress;
            }
        }

        private void CheckPetraLogin()
        {
            string walletAddress = PlayerPrefs.GetString("walletAddress");
            string walletPublicKey = PlayerPrefs.GetString("walletPublicKey");
            Debug.Log("Petra walletAddress: " + walletAddress);
            Debug.Log("Petra walletPublicKey: " + walletPublicKey);

            if (!string.IsNullOrEmpty(walletAddress) && !string.IsNullOrEmpty(walletPublicKey))
            {
                accountInfo.text = "Petra Wallet Address: " + walletAddress;
            }
        }

        private void NavigateToLoginIfNoAuth()
        {
            if (string.IsNullOrEmpty(accountInfo.text))
            {
                SceneManager.LoadScene("LoginScene");
            }
        }
    }
}
