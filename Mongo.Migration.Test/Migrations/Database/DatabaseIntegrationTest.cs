using System;
using System.Collections.Generic;
using System.Linq;

using Mongo.Migration.Documents;
using Mongo.Migration.Migrations.Database;
using Mongo.Migration.Startup;
using Mongo.Migration.Startup.Static;

using Testcontainers.MongoDb;

using MongoDB.Bson;
using MongoDB.Driver;

namespace Mongo.Migration.Test.Migrations.Database
{
    internal class DatabaseIntegrationTest : IDisposable
    {
        private const string MigrationsCollectionName = "_migrations";

        protected IMongoClient _client;

        protected IComponentRegistry _components;

        protected IMongoDatabase _db;

        protected MongoDbContainer _mongoContainer;

        protected virtual string DatabaseName { get; set; } = "DatabaseMigration";

        protected virtual string CollectionName { get; set; } = "Test";

        public void Dispose()
        {
            this._mongoContainer?.DisposeAsync().AsTask().Wait();
        }

        protected virtual void OnSetUp(DocumentVersion databaseMigrationVersion)
        {
            // Create and start MongoDB container with latest version
            this._mongoContainer = new MongoDbBuilder()
                .WithImage("mongo:latest")
                .Build();

            this._mongoContainer.StartAsync().Wait();

            this._client = new MongoClient(this._mongoContainer.GetConnectionString());
            this._db = this._client.GetDatabase(this.DatabaseName);
            this._db.CreateCollection(this.CollectionName);

            this._components = new ComponentRegistry(
                new MongoMigrationSettings
                {
                    ConnectionString = this._mongoContainer.GetConnectionString(),
                    Database = this.DatabaseName,
                    DatabaseMigrationVersion = databaseMigrationVersion
                });
            this._components.RegisterComponents(this._client);
        }

        protected void InsertMigrations(IEnumerable<DatabaseMigration> migrations)
        {
            var list = migrations.Select(m => new BsonDocument { { "MigrationId", m.GetType().ToString() }, { "Version", m.Version.ToString() } });
            this._db.GetCollection<BsonDocument>(MigrationsCollectionName).InsertManyAsync(list).Wait();
        }

        protected List<MigrationHistory> GetMigrationHistory()
        {
            var migrationHistoryCollection = this._db.GetCollection<MigrationHistory>(MigrationsCollectionName);
            return migrationHistoryCollection.Find(m => true).ToList();
        }
    }
}