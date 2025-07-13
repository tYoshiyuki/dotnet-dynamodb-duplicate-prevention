# dotnet-dynamodb-duplicate-prevention
.NET で DynamoDB の重複キー登録をチェックするサンプル

## Feature
- .NET8
- AWS SDK
- DynamoDB

## Note
- 動作確認のためには Docker Desktop が必要です。
  - 以下コマンドを実行し、`dynamodb-local` `dynamodb-admin` を起動してください。

```
docker compose up -d
```

- `ConditionExpression` を用いて重複登録をチェックしています。

```cs
            var request = new PutItemRequest
            {
                TableName = tableName,
                Item = itemAttributes,
                ConditionExpression = "attribute_not_exists(id)", // 条件式: キーが存在しない場合にのみPutItemを実行
            };
```