using System.Collections;
using UnityEngine;
using GraphQL;

public class ExampleQuery : MonoBehaviour
{
    void Start()
    {
        //Sends a query using credientials and will queue the query until the token is gained
        GraphQLManager.instance.Query(@"query { test }", callback: QueryCallback);

        //Returns true or false depending on whether the manager has an access token
        Debug.Log(GraphQLManager.instance.hasToken);
    }

    void QueryCallback(GraphQLResponse response)
    {
        //Raw data from response
        Debug.Log(response.raw);

        //Any exceptions possibly thrown by response
        Debug.Log(response.exception);

        /* Other Functions from GraphQLResponse -
            public T Get<T>(string key)
            public List<T> GetList<T>(string key)
            public JObject GetData()
        */
    }
}