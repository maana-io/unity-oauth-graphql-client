using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using GraphQL;
using OAuth;

public class GraphQLManager : MonoBehaviour
{
    private static GraphQLManager _instance = null;
    public static GraphQLManager instance => _instance;

    [SerializeField] private string url = null;
    [SerializeField] private TextAsset credentials = null;

    private OAuthFetcher fetcher = null;
    private GraphQLClient client = null;

    private List<QueuedQuery> queuedQueries = new List<QueuedQuery>();

    public bool hasToken => fetcher.token != null;

    protected virtual void Awake()
    {
        if(_instance == null)
            _instance = this;
        else if(_instance != this)
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);

        if(_instance == this)
        {
            if(url != "" && url != null)
                client = new GraphQLClient(url);
            else
                Debug.LogError("No URL specified for GraphQL Manager!");

            if(credentials != null)
            {
                fetcher = new OAuthFetcher(credentials.text);
                fetcher.tokenReceivedEvent.AddListener(TokenReceived);
            }
        }
    }

    private void TokenReceived()
    {
        foreach(QueuedQuery queuedQuery in queuedQueries)
            Query(queuedQuery.query, queuedQuery.variables, callback: queuedQuery.callback);
        
        queuedQueries = new List<QueuedQuery>();
    }

    public void Query(string query, object variables = null, Action<GraphQLResponse> callback = null)
    {
        if(client == null)
            if(hasToken)
                client.Query(query, variables, fetcher.token.access_token, callback: callback);
            else if(fetcher == null)
                client.Query(query, variables, callback: callback);
            else
                queuedQueries.Add(new QueuedQuery(query, variables, callback));
        else
            Debug.LogError("GraphQL Client is null, couldn't send query.");
    }

    private class QueuedQuery
    {
        public string query;
        public object variables;
        public Action<GraphQLResponse> callback;

        public QueuedQuery(string query, object variables = null, Action<GraphQLResponse> callback = null)
        {
            this.query = query;
            this.variables = variables;
            this.callback = callback;
        }
    }
}
