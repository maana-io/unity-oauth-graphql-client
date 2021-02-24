﻿using System;
using System.Collections;
using System.Text;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Maana.GraphQL
{
    public class GraphQLClient
    {
        private readonly string _url;

        public GraphQLClient(string url)
        {
            this._url = url;
            Debug.Log("GraphQL endpoint: " + url);
        }

        private class GraphQLQuery
        {
            [UsedImplicitly] public string Query;
            [UsedImplicitly] public object Variables;
        }

        private UnityWebRequest QueryRequest(string query, object variables, string token = null)
        {
            var fullQuery = new GraphQLQuery()
            {
                Query = query,
                Variables = variables,
            };

            var json = JsonConvert.SerializeObject(fullQuery);

            var request = UnityWebRequest.Post(_url, UnityWebRequest.kHttpVerbPOST);

            var payload = Encoding.UTF8.GetBytes(json);

            request.uploadHandler = new UploadHandlerRaw(payload);
            request.SetRequestHeader("Content-Type", "application/json");
            if(token != null) request.SetRequestHeader("Authorization", "Bearer " + token);

            return request;
        }

        private IEnumerator SendRequest(string query, object variables = null,
            Action<GraphQLResponse> callback = null,
            string token = null)
        {
            var request = QueryRequest(query, variables, token);

            using (var www = request)
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    callback?.Invoke(new GraphQLResponse("", www.error));
                    yield break;
                }

                string responseString = www.downloadHandler.text;

                var result = new GraphQLResponse(responseString);

                callback?.Invoke(result);
            }

            request.Dispose();
        }

        public void Query(
            string query,
            object variables = null,
            string sToken = null,
            Action<GraphQLResponse> callback = null)
        {
            Coroutiner.StartCoroutine(SendRequest(query, variables, callback, sToken));
        }
    }
}