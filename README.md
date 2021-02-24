# GraphQL with OAuth Client for Unity

This Unity asset includes a GraphQL prefab that can be used in a Unity project to securely connect and communicate with a Maana Q service.

## Installation

In Unity, from the `Assets` menu, select `Import Package > Custom Packate ...` and choose the `MaanaGraphQL.unitypackage` from this repository.

## Configuration

Once the package is imported. in the `Maana` folder, you will find a `GraphQLManager` prefab that can be added to your hierarchy.  Once added, you will need to configure it using the Inspector:

- `Url`: the Maana Q endpoint (e.g., `https://stable.knowledge.maana.io:8443/service/io.maana.catalog/graphql`)
- `Credentials`: a reference to a Unit TextAsset that is created by adding a JSON file to your project

The `Credentials` JSON file consists of:

```json
{
    "AUTH_DOMAIN" : "example.com",
    "AUTH_CLIENT_ID" : "exampleName",
    "AUTH_CLIENT_SECRET" : "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "AUTH_IDENTIFIER" : "exampleIdentifier",
    "REFRESH_MINUTES" : 20.0
}
```

A template for this file is provided in the package (`credentials_template`).

## Usage
