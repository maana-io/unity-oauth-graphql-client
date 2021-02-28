using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Maana.GraphQL
{
    public class GraphQLResponse
    {
        public string Raw { get; }
        public JObject Result { get; }
        public JToken Errors { get; }
        public string Exception { get; }

        public GraphQLResponse(string text, string ex = null)
        {
            Exception = ex;
            Raw = text;
            
            var data = string.IsNullOrEmpty(text) ? null : JObject.Parse(text);
            if (data == null) return;
            
            Errors = data["errors"];
            if (Errors != null)
            {
                Debug.Log("GraphQL errors: " + Errors);
                return;
            }
            
            Result = JObject.Parse(data["data"].ToString());
        }

        public T GetValue<T>(string key)
        {
            return Result[key].ToObject<T>();
        }

        public List<T> GetList<T>(string key)
        {
            return GetValue<JArray>(key).ToObject<List<T>>();
        }
    }
}