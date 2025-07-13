using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime.Internal;

namespace DotnetDynamodbDuplicatePrevention.ConsoleApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var config = new AmazonDynamoDBConfig
            {
                ServiceURL = "http://localhost:8000"
            };
            var dynamoDbClient = new AmazonDynamoDBClient(config);
            const string tableName = "sample-table";
            const string partitionKeyName = "id";

            // 対象テーブルの初期化
            var tables = await dynamoDbClient.ListTablesAsync();
            if (tables.TableNames.Any(x => x == tableName))
            {
                await dynamoDbClient.DeleteTableAsync(new DeleteTableRequest { TableName = tableName });
            }
            await dynamoDbClient.CreateTableAsync(new CreateTableRequest
            {
                TableName = tableName,
                KeySchema = [new() { AttributeName = partitionKeyName, KeyType = KeyType.HASH }],
                AttributeDefinitions = new AutoConstructedList<AttributeDefinition>
                {
                    new()
                    {
                        AttributeName = partitionKeyName,
                        AttributeType = ScalarAttributeType.S
                    }
                },
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 3,
                    WriteCapacityUnits = 3
                }
            });

            var dynamoDbService = new DynamoDbService(dynamoDbClient);

            // 1回目の登録（成功）
            string id = Guid.NewGuid().ToString();
            var item1 = new Dictionary<string, AttributeValue>
            {
                { partitionKeyName, new AttributeValue { S = id } },
            };

            Console.WriteLine("Attempting to put item1...");
            bool result1 = await dynamoDbService.PutItemIfNotExistsAsync(tableName, partitionKeyName, item1);
            Console.WriteLine($"Item1 put result: [{result1}]");

            // 2回目の登録（キー重複のため失敗）
            var item2 = new Dictionary<string, AttributeValue>
            {
                { partitionKeyName, new AttributeValue { S = id } },
            };
            Console.WriteLine("Attempting to put item2 (duplicate key)...");
            bool result2 = await dynamoDbService.PutItemIfNotExistsAsync(tableName, partitionKeyName, item2);
            Console.WriteLine($"Item2 put result: [{result2}]");
        }
    }
}
