﻿using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using SimpleEventStore.Tests.Events;

namespace SimpleEventStore.AzureDocumentDb.Tests
{
    internal static class StorageEngineFactory
    {
        internal static async Task<IStorageEngine> Create(string databaseName)
        {
            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            var documentDbUri = config["Uri"];
            var authKey = config["AuthKey"];
            var consistencyLevel = config["ConsistencyLevel"];
            ConsistencyLevel consistencyLevelEnum;

            if(!Enum.TryParse(consistencyLevel, true, out consistencyLevelEnum))
            {
                throw new Exception($"The ConsistencyLevel value {consistencyLevel} is not supported");
            }

            DocumentClient client = new DocumentClient(new Uri(documentDbUri), authKey);

            return await new AzureDocumentDbStorageEngineBuilder(client, databaseName)
                .UseCollection(o =>
                {
                    o.ConsistencyLevel = consistencyLevelEnum;
                    o.CollectionRequestUnits = 400;
                })
                .UseTypeMap(new ConfigurableSerializationTypeMap()
                    .RegisterTypes(
                        typeof(OrderCreated).GetTypeInfo().Assembly,
                        t => t.Namespace != null && t.Namespace.EndsWith("Events"),
                        t => t.Name))
                .Build()
                .Initialise();
        }
    }
}
