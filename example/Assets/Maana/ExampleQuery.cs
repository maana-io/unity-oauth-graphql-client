using Maana.GraphQL;
using UnityEngine;

namespace Maana
{
    public class ExampleQuery : MonoBehaviour
    {
        private void Start()
        {
            //Sends a query using credentials and will queue the query until the token is gained
            GraphQLManager.Instance.Query(@"query { test }", callback: QueryCallback);

            //Returns true or false depending on whether the manager has an access token
            print(GraphQLManager.Instance.HasToken);
        }

        private static void QueryCallback(GraphQLResponse response)
        {
            //Raw data from response
            print(response.Raw);

            //Any exceptions possibly thrown by response
            print(response.Exception);

            var s = response.Get<string>("Info");
            print(s);
            
            /* Other Functions from GraphQLResponse -
            public T Get<T>(string key)
            public List<T> GetList<T>(string key)
            public JObject GetData()
        */
        }
    }
}