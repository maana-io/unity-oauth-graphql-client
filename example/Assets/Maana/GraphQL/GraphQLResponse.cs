using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Maana.GraphQL
{
    public class GraphQLResponse
    {
        public string Raw { get; private set; }
        private readonly JObject _data;
        public string Exception { get; private set; }

        public GraphQLResponse(string text, string ex = null)
        {
            Exception = ex;
            Raw = text;
            _data = string.IsNullOrEmpty(text) ? null : JObject.Parse(text);
        }

        public T Get<T>(string key)
        {
            return GetData()[key].ToObject<T>();
        }

        public List<T> GetList<T>(string key)
        {
            return Get<JArray>(key).ToObject<List<T>>();
        }

        private JObject GetData()
        {
            return _data == null ? null : JObject.Parse(_data["data"].ToString());
        }
    }

}