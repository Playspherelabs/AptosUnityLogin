using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using AptosUnityLogin.DeepLink;

namespace AptosUnityLogin.AuthController{
public class GoogleAuthController : MonoBehaviour
{


    private string clientId = LoginConstants.GoogleClientID; // Replace with your Google client ID
    private string redirectUri = LoginConstants.GoogleDeepLink; // Deep link to your game

    private void OnEnable(){
        DeepLinkManager.OnLoginActivated+= OnGoogleAuthActivated;
    }

private void OnGoogleAuthActivated(string url)
{
    if (string.IsNullOrEmpty(url))
    {
        Debug.LogError("Deep link URL is null or empty");
        return;
    }

    try
    {
        // Parse the id_token from the URL fragment
        var uri = new Uri(url);  // Check if this throws an exception
        var fragment = uri.Fragment;
        var parameters = new Dictionary<string, string>();

        if (!string.IsNullOrEmpty(fragment))
        {
            fragment = fragment.Substring(1); // Remove the leading '#'
            foreach (var pair in fragment.Split('&'))
            {
                var parts = pair.Split('=');
                if (parts.Length == 2)
                {
                    parameters.Add(parts[0], Uri.UnescapeDataString(parts[1]));
                }
            }
        }

        if (parameters.ContainsKey("id_token"))
        {
            string idToken = parameters["id_token"];
            PlayerPrefs.SetString("id_token", idToken);
            StartCoroutine(DeriveKeylessAccountAddress(idToken));
        }
        else
        {
            Debug.LogError("No id_token found in URL fragment");
        }
    }
    catch (Exception ex)
    {
        Debug.LogError($"Exception occurred while processing deep link URL: {ex.Message}");
    }
}

    private void Start()
    {

        string jwt = PlayerPrefs.GetString("id_token");
        if (!string.IsNullOrEmpty(jwt))
        {
            StartCoroutine(DeriveKeylessAccountAddress(jwt));
        }
    }

    public void HandleAuthClick()
    {
        StartCoroutine(GenerateKeys());
    }

    private IEnumerator GenerateKeys()
    {
        string url = LoginConstants.KeylessURL + "/generate-keys";
        using (UnityWebRequest www = UnityWebRequest.Post(url, "","application/json"))
        {
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error generating keys: " + www.error);
            }
            else
            {
                string responseJson = www.downloadHandler.text;
                GenerateKeysResponse response = JsonConvert.DeserializeObject<GenerateKeysResponse>(responseJson);

                PlayerPrefs.SetString("ephemeralKeyPair", JsonConvert.SerializeObject(response.ephemeralKeyPair));
                string nonce = response.ephemeralKeyPair.nonce;

                string authURL = $"https://accounts.google.com/o/oauth2/v2/auth?response_type=id_token&scope=openid+email+profile&nonce={nonce}&redirect_uri={redirectUri}&client_id={clientId}";
                Application.OpenURL(authURL);
            }
        }
    }

    private IEnumerator DeriveKeylessAccountAddress(string jwt)
    {
        string storedEphemeralKeyPair = PlayerPrefs.GetString("ephemeralKeyPair");
        if (string.IsNullOrEmpty(storedEphemeralKeyPair))
        {
            Debug.LogError("No ephemeral key pair found in PlayerPrefs");
            yield break;
        }

        EphemeralKeyPair ephemeralKeyPair = JsonConvert.DeserializeObject<EphemeralKeyPair>(storedEphemeralKeyPair);

        string url = LoginConstants.KeylessURL + "/derive-keyless-account";
        using (UnityWebRequest www = UnityWebRequest.Post(url, "","application/json"))
        {
            www.SetRequestHeader("Content-Type", "application/json");

            DeriveKeylessAccountRequest request = new DeriveKeylessAccountRequest
            {
                nonce = ephemeralKeyPair.nonce,
                jwtToken = jwt
            };
            string requestJson = JsonConvert.SerializeObject(request);
            www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(requestJson));

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error deriving keyless account address: " + www.error);
            }
            else
            {
                string responseJson = www.downloadHandler.text;
                DeriveKeylessAccountResponse response = JsonConvert.DeserializeObject<DeriveKeylessAccountResponse>(responseJson);
                Debug.Log("Keyless Account Address: " + response.accountAddress);
                PlayerPrefs.SetString("keylessAccountAddress", response.accountAddress);
                // keylessAccountAddressText.text = "Keyless Account Address: " + response.accountAddress;
                OnLoginSuccess();
            }
        }
    }

    private void OnLoginSuccess()
    {
        // PlayerPerfs get id token or keyless account
        SceneManager.LoadSceneAsync(1);
    }

    [Serializable]
    private class GenerateKeysResponse
    {
        public EphemeralKeyPair ephemeralKeyPair;
    }

    [Serializable]
    private class EphemeralKeyPair
    {
        public string nonce;
        // Add other properties as needed
    }

    [Serializable]
    private class DeriveKeylessAccountRequest
    {
        public string nonce;
        public string jwtToken;
    }

    [Serializable]
    private class DeriveKeylessAccountResponse
    {
        public string accountAddress;
    }
}

}