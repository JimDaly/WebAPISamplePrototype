using Microsoft.Cds.Metadata.Query;
using Microsoft.Cds.QueryTypes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace WebAPISamplePrototype
{
    class EntityMetadataQuery
    {

        public static void Run(CDSWebApiService svc)
        {
            var entityFilter = new MetadataFilterExpression
            {
                FilterOperator = LogicalOperator.And,
                Conditions = new List<MetadataConditionExpression>
                {
                    new MetadataConditionExpression()
                    {
                        ConditionOperator = MetadataConditionOperator.Equals,
                        PropertyName = "SchemaName",
                        Value = new Microsoft.Cds.Metadata.Query.Object() { Type = "string", Value = "Account" }
                    }
                }
            };

            var entityProperties = new MetadataPropertiesExpression()
            {
                AllProperties = true
            };

            var query = new EntityKeyQueryExpression()
            {
                Criteria = entityFilter,
                Properties = entityProperties
            };
            var jsonQuery = JsonConvert.SerializeObject(query);
            var jsonDeletedMetadataFilters = JsonConvert.SerializeObject(DeletedMetadataFilters.Default);

            //BUG: 1563435 This returns an HTML Bad Request error
            //svc.Get($"RetrieveMetadataChanges(Query={jsonQuery},DeletedMetadataFilters={jsonDeletedMetadataFilters})");

            //This returns all metadata without any filter
            var response = svc.Get("RetrieveMetadataChanges");

            var results = (RetrieveMetadataChangesResponse)JsonConvert.DeserializeObject(response.ToString(), typeof(RetrieveMetadataChangesResponse));

            Console.WriteLine($"ServerVersionStamp: {results.ServerVersionStamp}\n");
            Console.WriteLine($"Entities returned: {results.EntityMetadata.Count}\n");

            var accountMetadata = results.EntityMetadata.Find(x => x.SchemaName.Equals("Account"));

            accountMetadata.Attributes.Sort((x, y) => x.SchemaName.CompareTo(y.SchemaName));

            accountMetadata.Attributes.ForEach(x => {
                Console.WriteLine($"{x.SchemaName} {x.AttributeTypeName.Value}");
            });
            Console.WriteLine();


        }
    }
}
