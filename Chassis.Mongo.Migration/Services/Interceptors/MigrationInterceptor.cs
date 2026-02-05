using System;
using Mongo.Migration.Documents;
using Mongo.Migration.Migrations.Document;

using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Mongo.Migration.Services.Interceptors
{
    internal class MigrationInterceptor<TDocument> : IBsonSerializer<TDocument>
        where TDocument : class, IDocument
    {
        private readonly IDocumentVersionService _documentVersionService;

        private readonly IDocumentMigrationRunner _migrationRunner;

        private readonly IBsonSerializer<TDocument> _innerSerializer;

        public MigrationInterceptor(IDocumentMigrationRunner migrationRunner, IDocumentVersionService documentVersionService)
        {
            this._migrationRunner = migrationRunner;
            this._documentVersionService = documentVersionService;
            this._innerSerializer = new BsonClassMapSerializer<TDocument>(BsonClassMap.LookupClassMap(typeof(TDocument)));
        }

        public Type ValueType => typeof(TDocument);

        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TDocument value)
        {
            this._documentVersionService.DetermineVersion(value);

            this._innerSerializer.Serialize(context, args, value);
        }

        public TDocument Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            // TODO: Performance? LatestVersion, dont do anything
            var document = BsonDocumentSerializer.Instance.Deserialize(context);

            this._migrationRunner.Run(typeof(TDocument), document);

            var migratedContext =
                BsonDeserializationContext.CreateRoot(new BsonDocumentReader(document));

            return this._innerSerializer.Deserialize(migratedContext, args);
        }

        object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            return Deserialize(context, args);
        }

        void IBsonSerializer.Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
        {
            if (value is not TDocument document)
                throw new ArgumentException($"Value must be of type {typeof(TDocument).Name}", nameof(value));
                
            Serialize(context, args, document);
        }
    }
}