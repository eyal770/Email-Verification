using Amazon.DynamoDBv2.DataModel;

namespace EmailVerificationSystem.Models;

[DynamoDBTable("EmailVerifications")]
public class EmailVerification
{
    [DynamoDBHashKey]
    public string Token { get; set; } = string.Empty;

    [DynamoDBProperty]
    public string Email { get; set; } = string.Empty;

    [DynamoDBProperty]
    public bool IsVerified { get; set; } = false;

    [DynamoDBProperty]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}