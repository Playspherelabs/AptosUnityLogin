using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;
using UnityEngine;
using UnityEngine.Events;

public class DeepLinkManager : MonoBehaviour
{
    public static DeepLinkManager Instance { get; private set; }
    public string deeplinkURL;
    public static UnityAction<string> OnLoginActivated;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;                
            Application.deepLinkActivated += onDeepLinkActivated;
            if (!string.IsNullOrEmpty(Application.absoluteURL))
            {
                // Cold start and Application.absoluteURL not null so process Deep Link.
                onDeepLinkActivated(Application.absoluteURL);
            }
            // Initialize DeepLink Manager global variable.
            else deeplinkURL = "[none]";
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void onDeepLinkActivated(string url){
 try
        {
            Uri uri = new Uri(url);
            NameValueCollection queryParams = HttpUtility.ParseQueryString(uri.Query);

            print(uri);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing deep link URL: {ex.Message}");
            OnLoginActivated?.Invoke(null);
        }
    }

}
