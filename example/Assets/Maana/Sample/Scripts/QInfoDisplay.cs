using System;
using JetBrains.Annotations;
using Maana.GraphQL;
using TMPro;
using UnityEngine;

// ReSharper disable InconsistentNaming
#pragma warning disable 649

public class QInfoDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private float rotateAmount = .005f;
    
    [UsedImplicitly]
    private class QInfo
    {
        public string id;
        public string name;
        public string description;
    }
    
    private void Start()
    {
        GraphQLManager.Instance.Query(@"query { info { id name description } }", callback: QueryCallback);
    }

    private void Update()
    {
        text.transform.Rotate(Vector3.up, rotateAmount);
    }

    private void QueryCallback(GraphQLResponse response)
    {
        print(response.Result);
        var info = response.GetValue<QInfo>("info");
        text.text = $"Info: {info.id}\nName: {info.name}\nDescription: {info.description}";
    }
}