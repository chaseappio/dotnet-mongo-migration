using FluentAssertions;

using Mongo.Migration.Documents;
using Mongo.Migration.Services;

using MongoDB.Bson.Serialization;

using NUnit.Framework;

namespace Mongo.Migration.Test.MongoDB
{
    [TestFixture]
    internal class MongoRegistrator_when_registrating : IntegrationTest
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            this.OnSetUp();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            this.Dispose();
        }

        [Test]
        public void Then_serializer_is_registered()
        {
            // Arrange 
            var migrationService = this._components.Get<IMigrationService>();

            // Act
            migrationService.Migrate();

            // Arrange
            BsonSerializer.LookupSerializer<DocumentVersion>().ValueType.Should().Be(typeof(DocumentVersion));
        }
    }
}