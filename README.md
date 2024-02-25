# SegregatedStorage

A small library for providing segregated storage abstraction for Web API's and similar projects.

Comes with built-in In-Memory support for ease of testing, as well as packages for Azure Blob storage for the actual files and MongoDB for metadata.

# Installation

I recommend using the NuGet package: [SegregatedStorage](https://www.nuget.org/packages/SegregatedStorage) however feel free to clone the source instead if that suits your needs
better.

For Azure Blob storage support, add this NuGet package: [SegregatedStorage.AzureStorageProvider](https://www.nuget.org/packages/SegregatedStorage.AzureStorageProvider)
For Mongo File repository support, add this NuGet package: [SegregatedStorage.MongoFileRepository](https://www.nuget.org/packages/SegregatedStorage.MongoFileRepository)

# Usage

First inject the core functionality via Dependency Injection.
The type of key (here it's `int`), as well as the naming of collection, databases and containers can be whatever you prefer.

```
builder.Services.AddMongoFileRepository<int>("connectionString", "files", key => $"db-{key}");
builder.Services.AddAzureStorageProvider<int>("connectionString", key => $"container-{key}");
builder.Services.AddStorageService<int>();
```

Secondly, if you want to utilize the built-in API endpoints, add those to your app:

```
var app = builder.Build();

app.MapStorageApi(); // This has an optional configuration you can adjust if need be, e.g. for AntiForgery needs
```

With all this done you can both inject the `IStorageService<TKey>` and use it directly, as well as simply use the built-in API endpoints:

```
- GET /file/{key}/{id}
- DELETE /file/{key}/{id}
- POST /file/{key}
```

# Compatibility

Currently Azure Blob storage is supported for cloud storage of files, and MongoDB is supported as metadata container.
Both are configured with a key, to allow for easy segregation of data e.g. between different customers.

~~~~You can however quite easily build your own `IStorageProvider<TKey>` or `IFileRepository<TKey>` if you want to use different technologies.

Feel free to submit a PR if you add a provider/repository you want to share with everyone else.