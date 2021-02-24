using System;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

// ReSharper disable InconsistentNaming

namespace Maana.GraphQL
{
    [Serializable]
    public class OAuthToken
    {
        [UsedImplicitly] public string access_token;
        [UsedImplicitly] public string token_type;
        [UsedImplicitly] public string session_state;
        [UsedImplicitly] public string scope;
    }

    [Serializable]
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
        private readonly string AUTH_CLIENT_ID;
        private readonly string AUTH_CLIENT_SECRET;

        private readonly string AUTH_DOMAIN;
        private readonly string AUTH_IDENTIFIER;
        private readonly float REFRESH_MINUTES;

        public OAuthFetcher(string authDomain, string authClientId, string authClientSecret, string authIdentifier,
            float refreshMinutes = 5f)
        {
            AUTH_DOMAIN = authDomain;
            AUTH_CLIENT_ID = authClientId;
            AUTH_CLIENT_SECRET = authClientSecret;
            AUTH_IDENTIFIER = authIdentifier;
            REFRESH_MINUTES = refreshMinutes;
            GetOAuthToken();
        }

        public OAuthFetcher(string authCredentialsJson)
        {
            var credentials = JsonUtility.FromJson<OAuthCredentials>(authCredentialsJson);
            AUTH_DOMAIN = credentials.AUTH_DOMAIN;
            AUTH_CLIENT_ID = credentials.AUTH_CLIENT_ID;
            AUTH_CLIENT_SECRET = credentials.AUTH_CLIENT_SECRET;
            AUTH_IDENTIFIER = credentials.AUTH_IDENTIFIER;
            REFRESH_MINUTES = credentials.REFRESH_MINUTES;
            GetOAuthToken();
        }

        public OAuthToken Token { get; private set; }

        public UnityEvent TokenReceivedEvent { get; } = new UnityEvent();

        private string StripCredentials(string str)
        {
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
            if (AUTH_IDENTIFIER == null)
                throw new Exception(
                    "OAuth: No auth identifier detected in environment variables: proceeding WITHOUT authentication!");

            try
            {
                var url = $"https://{AUTH_DOMAIN}/auth/realms/{AUTH_IDENTIFIER}/protocol/openid-connect/token";

                var formData = new WWWForm();

                formData.AddField("client_id", AUTH_CLIENT_ID);
                formData.AddField("client_secret", AUTH_CLIENT_SECRET);

                formData.AddField("grant_type", "client_credentials");
                formData.AddField("audience", AUTH_IDENTIFIER);

                var request = UnityWebRequest.Post(url, formData);

                request.SetRequestHeader("Accept", "application/json");
                request.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

                return request;
            }
            catch (Exception ex)
            {
                throw new Exception($"OAuth: Error obtaining OAuth token: {StripCredentials(ex.Message)}");
            }
        }

        private IEnumerator SendRequest()
        {
            var request = TokenRequest();

            using (var www = request)
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                    throw new Exception("Could not authenticate: " + www.error);

                Token = JsonUtility.FromJson<OAuthToken>(www.downloadHandler.text);

                TokenReceivedEvent.Invoke();
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