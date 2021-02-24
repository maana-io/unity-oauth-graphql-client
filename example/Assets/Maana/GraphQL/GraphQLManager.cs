using System;
using System.Collections.Generic;
using UnityEngine;

namespace Maana.GraphQL
{
    public sealed class GraphQLManager : MonoBehaviour
    {
        public static GraphQLManager Instance { get; private set; }

        [SerializeField] private string url;
        [SerializeField] private TextAsset credentials;

        private OAuthFetcher _fetcher;
        private GraphQLClient _client;

        private List<QueuedQuery> _queuedQueries = new List<QueuedQuery>();

        public bool HasToken => _fetcher.Token != null;

        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
            }
            else if(Instance != this)
            {
                Destroy(gameObject);
            }

            DontDestroyOnLoad(gameObject);

            if (Instance != this) return;
            if(!string.IsNullOrEmpty(url))
            {
                _client = new GraphQLClient(url);
            }
            else
            {
                Debug.LogError("No URL specified for GraphQL Manager!");
            }

            if (credentials == null) return;
            _fetcher = new OAuthFetcher(credentials.text);
            _fetcher.TokenReceivedEvent.AddListener(TokenReceived);
        }

        private void TokenReceived()
        {
            foreach(var queuedQuery in _queuedQueries)
            {
                Query(queuedQuery.Query, queuedQuery.Variables, callback: queuedQuery.Callback);
            }

            _queuedQueries = new List<QueuedQuery>();
        }

        public void Query(string query, object variables = null, Action<GraphQLResponse> callback = null)
        {
            if (_client == null)
            {
                throw new Exception("GraphQL Client is null, couldn't send query.");
            }

            if (HasToken)
            {
                _client.Query(query, variables, _fetcher.Token.access_token, callback: callback);
            }
            else if (_fetcher == null)
            {
                _client.Query(query, variables, callback: callback);
            }
            else
            {
                _queuedQueries.Add(new QueuedQuery(query, variables, callback));
            }
        }

        private class QueuedQuery
        {
            public readonly string Query;
            public readonly object Variables;
            public readonly Action<GraphQLResponse> Callback;

            public QueuedQuery(string query, object variables = null, Action<GraphQLResponse> callback = null)
            {
                Query = query;
                Variables = variables;
                Callback = callback;
            }
        }
    }
}
