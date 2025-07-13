using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace DotnetDynamodbDuplicatePrevention.ConsoleApp
{
    /// <summary>
    /// DynamoDBサービス
    /// </summary>
    public class DynamoDbService
    {
        private readonly IAmazonDynamoDB _dynamoDbClient;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dynamoDbClient"></param>
        public DynamoDbService(AmazonDynamoDBClient dynamoDbClient)
        {
            _dynamoDbClient = dynamoDbClient;
        }

        /// <summary>
        /// データの登録、重複登録の場合失敗とする
        /// </summary>
        /// <param name="tableName">テーブル名</param>
        /// <param name="partitionKey">パーティションキー名</param>
        /// <param name="itemAttributes">登録データ</param>
        /// <returns></returns>
        public async Task<bool> PutItemIfNotExistsAsync(string tableName, string partitionKey, Dictionary<string, AttributeValue> itemAttributes)
        {
            var request = new PutItemRequest
            {
                TableName = tableName,
                Item = itemAttributes,
                ConditionExpression = "attribute_not_exists(id)", // 条件式: キーが存在しない場合にのみPutItemを実行
            };

            try
            {
                _ = await _dynamoDbClient.PutItemAsync(request);
                return true;
            }
            catch (ConditionalCheckFailedException)
            {
                // キー重複のため登録失敗
                return false;
            }
        }
    }
}
