using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace OAuth
{
    [System.Serializable]
    public class OAuthToken
    {
        public string access_token;
        public string token_type;
        public string session_state;
        public string scope;
    }

    [System.Serializable]
    public class OAuthCredentials
    {
        public string AUTH_DOMAIN;
        public string AUTH_CLIENT_ID;
        public string AUTH_CLIENT_SECRET;
        public string AUTH_IDENTIFIER;
        public float REFRESH_MINUTES = 5f;
    }

    public class OAuthFetcher
    {
        public OAuthToken token { get; private set; }

        public UnityEvent tokenReceivedEvent { get; private set; } = new UnityEvent();

        private string AUTH_DOMAIN;
        private string AUTH_CLIENT_ID;
        private string AUTH_CLIENT_SECRET;
        private string AUTH_IDENTIFIER;
        private float REFRESH_MINUTES;

        public OAuthFetcher(string authDomain, string authClientId, string authClientSecret, string authIdentifier, float refreshMinutes = 5f)
        {
            AUTH_DOMAIN = authDomain;
            AUTH_CLIENT_ID = authClientId;
            AUTH_CLIENT_SECRET = authClientSecret;
            AUTH_IDENTIFIER = authIdentifier;
            REFRESH_MINUTES = refreshMinutes;
            GetOAuthToken();
        }

        public OAuthFetcher(string authCredentials)
        {
            OAuthCredentials credentials = JsonUtility.FromJson<OAuthCredentials>(authCredentials);
            AUTH_DOMAIN = credentials.AUTH_DOMAIN;
            AUTH_CLIENT_ID = credentials.AUTH_CLIENT_ID;
            AUTH_CLIENT_SECRET = credentials.AUTH_CLIENT_SECRET;
            AUTH_IDENTIFIER = credentials.AUTH_IDENTIFIER;
            REFRESH_MINUTES = credentials.REFRESH_MINUTES;
            GetOAuthToken();
        }

        public string StripCredentials(string str){
            return str
                .Replace(AUTH_IDENTIFIER, "<chunk redacted>")
                .Replace(AUTH_CLIENT_SECRET, "<chunk redacted>");
        }

        private void BeginTokenUpdater()
        {
            Coroutiner.StartCoroutine(UpdateToken());
        }

        private IEnumerator UpdateToken()
        {
            yield return new WaitForSeconds(REFRESH_MINUTES * 60f);
            GetOAuthToken();
        }

        private UnityWebRequest TokenRequest()
        {
            if(AUTH_IDENTIFIER != null)
            {
                try
                {
                    string URL = $"https://{AUTH_DOMAIN}/auth/realms/{AUTH_IDENTIFIER}/protocol/openid-connect/token";

                    WWWForm formData = new WWWForm();

                    formData.AddField("client_id", AUTH_CLIENT_ID);
                    formData.AddField("client_secret", AUTH_CLIENT_SECRET);

                    formData.AddField("grant_type", "client_credentials");
                    formData.AddField("audience", AUTH_IDENTIFIER);

                    UnityWebRequest request = UnityWebRequest.Post(URL, formData);

                    request.SetRequestHeader("Accept", "application/json");
                    request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

                    return request;
                } catch(Exception ex)
                {
                    throw new Exception($"OAuth: Error obtaining OAuth token: {StripCredentials(ex.Message)}");
                }
            } else
            {
                Debug.Log("OAuth: No auth identifier detected in environment variables: proceeding WITHOUT authentication!");
                return null; 
            }
        }

        private IEnumerator SendRequest()
        {
            var request = TokenRequest();

            using(UnityWebRequest www = request)
            {
                yield return www.SendWebRequest();

                if(www.isNetworkError)
                {
                    Debug.Log(www.error);
                    yield break;
                }

                token = JsonUtility.FromJson<OAuthToken>(www.downloadHandler.text);

                Debug.Log("OAuth Token Received");

                tokenReceivedEvent.Invoke();
            }

            request.Dispose();

            BeginTokenUpdater();
        }

        private void GetOAuthToken()
        {
            Coroutiner.StartCoroutine(SendRequest());
        }
    }
}