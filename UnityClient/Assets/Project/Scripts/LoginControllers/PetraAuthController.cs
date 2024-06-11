using System;
using System.Text;
using System.Web;
using UnityEngine;
using Chaos.NaCl;
using Newtonsoft.Json;
using NaCl;
using System.Security.Cryptography;
using NBitcoin.DataEncoders;
using Org.BouncyCastle.Utilities.Encoders;
using UnityEngine.SceneManagement;

namespace AptosUnityLogin.AuthController
{
    public class PetraAuthController : MonoBehaviour
    {
        private byte[] dappPublicKey;
        private byte[] dappPrivateKey;
        private byte[] sharedSecret;
        private byte[] nonce;
        private string walletUrl;

        void Start()
        {
            GenerateKeyPair();
            InitializeDeepLinkListener();
        }

        private void GenerateKeyPair()
        {
            using var rng = RandomNumberGenerator.Create();

            Curve25519XSalsa20Poly1305.KeyPair(out var secretKey, out var publicKey);
            nonce = new byte[Curve25519XSalsa20Poly1305.NonceLength];
            rng.GetBytes(nonce);
            dappPublicKey = publicKey;
            dappPrivateKey = secretKey;

            Debug.Log("DApp Public Key: " + Convert.ToBase64String(dappPublicKey));
            Debug.Log("DApp Private Key: " + Convert.ToBase64String(dappPrivateKey));
        }

        private void InitializeDeepLinkListener()
        {
            Application.deepLinkActivated += OnDeepLinkActivated;
        }

        void OnDeepLinkActivated(string url)
        {
            walletUrl = url;
            HandleDeepLink();
        }

        public void HandleConnectWallet()
        {
            try
            {
                var connectData = new
                {
                    appInfo = new { domain = "aptosvictors.xyz" },
                    redirectLink = "https://applinktest-git-main-sikka2x2.vercel.app/?nonce=" + Convert.ToBase64String(nonce),
                    dappEncryptionPublicKey = Encoding.UTF8.GetString(Hex.Encode(dappPublicKey))
                };

                var encodedConnectData = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(connectData)));
                var url = $"https://petra.app/api/v1/connect?data={encodedConnectData}&nonce={Convert.ToBase64String(nonce)}";

                Debug.Log("Connect Data: " + JsonConvert.SerializeObject(connectData));
                Debug.Log("Connect URL: " + url);

                Application.OpenURL(url);
            }
            catch (Exception error)
            {
                Debug.LogError("Failed to connect wallet: " + error);
            }
        }

        private void HandleDeepLink()
        {
            if (string.IsNullOrEmpty(walletUrl)) return;

            var uri = new Uri(walletUrl);
            var queryParams = HttpUtility.ParseQueryString(uri.Query);

            if (queryParams["data"] != null)
            {
                string data = queryParams["data"];
                string decodedData = Encoding.UTF8.GetString(Convert.FromBase64String(data));

                try
                {
                    var walletData = JsonConvert.DeserializeObject<WalletData>(decodedData);

                    PlayerPrefs.SetString("walletAddress", walletData.address);
                    PlayerPrefs.SetString("walletPublicKey", walletData.publicKey);
                    PlayerPrefs.SetString("petraPublicEncryptedKey", walletData.petraPublicEncryptedKey);

                    // Debug the values to see if they are correctly stored.
                    Debug.Log($"Wallet Address: {walletData.address}");
                    Debug.Log($"Public Key: {walletData.publicKey}");
                    Debug.Log($"Petra Public Encrypted Key: {walletData.petraPublicEncryptedKey}");

                    OnLoginSuccess();
                }
                catch (JsonException ex)
                {
                    Debug.LogError($"Failed to decode wallet data: {ex.Message}");
                }
            }
        }

        private void OnLoginSuccess()
        {
            SceneManager.LoadSceneAsync(1);
        }

        // Updated structures to match the expected format
        [Serializable]
        private class WalletData
        {
            public string address;
            public string publicKey;
            public string petraPublicEncryptedKey;
        }
    }
}
