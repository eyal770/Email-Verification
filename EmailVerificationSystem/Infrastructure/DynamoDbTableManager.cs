using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace EmailVerificationSystem.Infrastructure;

public static class DynamoDbTableManager
{
    public static async Task EnsureEmailVerificationsTableExistsAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dynamoDb = scope.ServiceProvider.GetRequiredService<IAmazonDynamoDB>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        const string tableName = "EmailVerifications";
        
        try
        {
            // Check if table exists
            var describeResponse = await dynamoDb.DescribeTableAsync(tableName);
            logger.LogInformation("DynamoDB table '{TableName}' already exists with status: {Status}", 
                tableName, describeResponse.Table.TableStatus);
                
            // Wait for table to be active if it's still being created
            if (describeResponse.Table.TableStatus != TableStatus.ACTIVE)
            {
                logger.LogInformation("Waiting for table '{TableName}' to become active...", tableName);
                await WaitForTableToBeActiveAsync(dynamoDb, tableName, logger);
            }
        }
        catch (ResourceNotFoundException)
        {
            logger.LogInformation("DynamoDB table '{TableName}' does not exist. Creating it now...", tableName);
            
            var request = new CreateTableRequest
            {
                TableName = tableName,
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement("Token", KeyType.HASH)
                },
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition("Token", ScalarAttributeType.S)
                },
                BillingMode = BillingMode.PAY_PER_REQUEST
            };
            
            try
            {
                await dynamoDb.CreateTableAsync(request);
                logger.LogInformation("DynamoDB table '{TableName}' creation initiated.", tableName);
                
                // Wait for table to be active
                await WaitForTableToBeActiveAsync(dynamoDb, tableName, logger);
                logger.LogInformation("DynamoDB table '{TableName}' created successfully and is now active.", tableName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create DynamoDB table '{TableName}'", tableName);
                throw;
            }
        }
        catch (Exception ex)
        {
            if (ex is AmazonDynamoDBException dynamoEx)
            {
                if (dynamoEx.ErrorCode == "AccessDenied" || dynamoEx.ErrorCode == "UnauthorizedOperation")
                {
                    logger.LogError("Access denied to DynamoDB. Please check IAM permissions for the EC2 instance role. Error: {ErrorCode} - {ErrorMessage}", 
                        dynamoEx.ErrorCode, dynamoEx.Message);
                }
                else
                {
                    logger.LogError(dynamoEx, "DynamoDB error while checking table '{TableName}': {ErrorCode} - {ErrorMessage}", 
                        tableName, dynamoEx.ErrorCode, dynamoEx.Message);
                }
            }
            else
            {
                logger.LogError(ex, "Unexpected error checking DynamoDB table '{TableName}'", tableName);
            }
            throw;
        }
    }

    private static async Task WaitForTableToBeActiveAsync(IAmazonDynamoDB dynamoDb, string tableName, ILogger logger)
    {
        var maxAttempts = 30; // Maximum 5 minutes (30 * 10 seconds)
        var attempt = 0;
        
        while (attempt < maxAttempts)
        {
            try
            {
                var response = await dynamoDb.DescribeTableAsync(tableName);
                
                if (response.Table.TableStatus == TableStatus.ACTIVE)
                {
                    logger.LogInformation("Table '{TableName}' is now active.", tableName);
                    return;
                }
                
                logger.LogInformation("Table '{TableName}' status: {Status}. Waiting...", 
                    tableName, response.Table.TableStatus);
                    
                await Task.Delay(10000); // Wait 10 seconds
                attempt++;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error while waiting for table '{TableName}' to become active. Attempt {Attempt}/{MaxAttempts}", 
                    tableName, attempt + 1, maxAttempts);
                
                if (attempt >= maxAttempts - 1)
                    throw;
                    
                await Task.Delay(10000);
                attempt++;
            }
        }
        
        throw new TimeoutException($"Table '{tableName}' did not become active within the expected time.");
    }
}