using System;
using System.Threading.Tasks;

using Mongo.Migration.Startup;
using Mongo.Migration.Startup.Static;

using Testcontainers.MongoDb;

using MongoDB.Driver;

namespace Mongo.Migration.Test
{
    public class IntegrationTest : IDisposable
    {
        protected IMongoClient _client;

        protected IComponentRegistry _components;

        protected MongoDbContainer _mongoContainer;

        public void Dispose()
        {
            this._mongoContainer?.DisposeAsync().AsTask().Wait();
        }

        protected void OnSetUp()
        {
            // Create and start MongoDB container with latest version
            this._mongoContainer = new MongoDbBuilder()
                .WithImage("mongo:latest")
                .Build();

            this._mongoContainer.StartAsync().Wait();

            this._client = new MongoClient(this._mongoContainer.GetConnectionString());

            this._client.GetDatabase("PerformanceTest").CreateCollection("Test");

            this._components = new ComponentRegistry(
                new MongoMigrationSettings
                    { ConnectionString = this._mongoContainer.GetConnectionString(), Database = "PerformanceTest" });
            this._components.RegisterComponents(this._client);
        }
    }
}