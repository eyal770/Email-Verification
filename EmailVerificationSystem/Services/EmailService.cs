using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;

namespace EmailVerificationSystem.Services;

public class EmailService : IEmailService
{
    private readonly IAmazonSimpleEmailService _sesClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IAmazonSimpleEmailService sesClient, IConfiguration configuration, ILogger<EmailService> logger)
    {
        _sesClient = sesClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendVerificationEmailAsync(string email, string verificationLink)
    {
        try
        {
            // Log all possible configuration sources
            _logger.LogInformation("=== EMAIL SERVICE CONFIGURATION DEBUG ===");
            _logger.LogInformation("Reading AWS:SenderEmail from configuration...");
            
            var senderEmail = _configuration["AWS:SenderEmail"] ?? "noreply@yourdomain.com";
            
            _logger.LogInformation("Configuration value for AWS:SenderEmail: '{SenderEmail}'", senderEmail);
            _logger.LogInformation("Recipient email: '{RecipientEmail}'", email);
            _logger.LogInformation("Verification link: '{VerificationLink}'", verificationLink);
            
            // Log all AWS-related configuration keys
            _logger.LogInformation("All AWS configuration keys:");
            foreach (var key in _configuration.GetSection("AWS").GetChildren())
            {
                _logger.LogInformation("  {Key}: {Value}", key.Key, key.Value);
            }
            
            // Log environment variables
            _logger.LogInformation("Environment variables:");
            _logger.LogInformation("  SenderEmail: {SenderEmail}", Environment.GetEnvironmentVariable("SenderEmail"));
            _logger.LogInformation("  AWS_SenderEmail: {AWSSenderEmail}", Environment.GetEnvironmentVariable("AWS_SenderEmail"));
            _logger.LogInformation("  ASPNETCORE_ENVIRONMENT: {Environment}", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
            
            _logger.LogInformation("=== END CONFIGURATION DEBUG ===");
            
            // Validate sender email is not null or empty
            if (string.IsNullOrWhiteSpace(senderEmail))
            {
                throw new InvalidOperationException("Sender email is not configured. Please set AWS:SenderEmail in configuration.");
            }

            var subject = "Email Verification Required";
            var htmlBody = $@"
                <html>
                <body>
                    <h2>Email Verification</h2>
                    <p>Thank you for submitting your email address. Please click the link below to verify your email:</p>
                    <p><a href='{verificationLink}' style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 4px;'>Verify Email</a></p>
                    <p>If the button doesn't work, copy and paste this link into your browser:</p>
                    <p>{verificationLink}</p>
                    <p>This link will expire after 24 hours for security reasons.</p>
                </body>
                </html>";

            var textBody = $@"
                Email Verification
                
                Thank you for submitting your email address. Please visit the following link to verify your email:
                
                {verificationLink}
                
                This link will expire after 24 hours for security reasons.";

            var sendRequest = new SendEmailRequest
            {
                Source = senderEmail,
                Destination = new Destination
                {
                    ToAddresses = new List<string> { email }
                },
                Message = new Message
                {
                    Subject = new Content(subject),
                    Body = new Body
                    {
                        Html = new Content
                        {
                            Charset = "UTF-8",
                            Data = htmlBody
                        },
                        Text = new Content
                        {
                            Charset = "UTF-8",
                            Data = textBody
                        }
                    }
                }
            };

            _logger.LogInformation("Sending email request to SES for recipient: {RecipientEmail}", email);
            _logger.LogInformation("Using sender email: {SenderEmail}", senderEmail);
            await _sesClient.SendEmailAsync(sendRequest);
            _logger.LogInformation("Email sent successfully to: {RecipientEmail}", email);
        }
        catch (Amazon.SimpleEmail.Model.MessageRejectedException ex)
        {
            _logger.LogError(ex, "SES rejected the email for recipient: {RecipientEmail}. Error: {ErrorMessage}", email, ex.Message);
            
            if (ex.Message.Contains("Email address is not verified"))
            {
                var senderEmail = _configuration["AWS:SenderEmail"] ?? "noreply@yourdomain.com";
                throw new InvalidOperationException(
                    $"SES rejected the email: {ex.Message}. " +
                    $"The sender email '{senderEmail}' is not verified in AWS SES. " +
                    $"Please verify this email address in the AWS SES console before sending emails.", ex);
            }
            
            throw new InvalidOperationException($"SES rejected the email: {ex.Message}. This usually means the sender email is not verified in SES.", ex);
        }
        catch (Amazon.SimpleEmail.Model.MailFromDomainNotVerifiedException ex)
        {
            _logger.LogError(ex, "SES domain not verified for recipient: {RecipientEmail}. Error: {ErrorMessage}", email, ex.Message);
            throw new InvalidOperationException($"SES domain not verified: {ex.Message}. Please verify your sender domain in SES.", ex);
        }
        catch (Amazon.SimpleEmail.Model.ConfigurationSetDoesNotExistException ex)
        {
            _logger.LogError(ex, "SES configuration set not found for recipient: {RecipientEmail}. Error: {ErrorMessage}", email, ex.Message);
            throw new InvalidOperationException($"SES configuration set not found: {ex.Message}. Please check your SES configuration.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error sending email to recipient: {RecipientEmail}. Error: {ErrorMessage}", email, ex.Message);
            throw new InvalidOperationException($"Failed to send email via SES: {ex.Message}", ex);
        }
    }
}