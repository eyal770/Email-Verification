using Amazon.DynamoDBv2.DataModel;
using EmailVerificationSystem.Models;

namespace EmailVerificationSystem.Repositories;

public class EmailVerificationRepository : IEmailVerificationRepository
{
    private readonly IDynamoDBContext _context;

    public EmailVerificationRepository(IDynamoDBContext context)
    {
        _context = context;
    }

    public async Task SaveVerificationAsync(EmailVerification verification)
    {
        await _context.SaveAsync(verification);
    }

    public async Task<EmailVerification?> GetVerificationAsync(string token)
    {
        return await _context.LoadAsync<EmailVerification>(token);
    }

    public async Task UpdateVerificationAsync(EmailVerification verification)
    {
        await _context.SaveAsync(verification);
    }
}