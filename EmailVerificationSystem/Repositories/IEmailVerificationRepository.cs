using EmailVerificationSystem.Models;

namespace EmailVerificationSystem.Repositories;

public interface IEmailVerificationRepository
{
    Task SaveVerificationAsync(EmailVerification verification);
    Task<EmailVerification?> GetVerificationAsync(string token);
    Task UpdateVerificationAsync(EmailVerification verification);
}