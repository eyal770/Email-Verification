using Microsoft.AspNetCore.Mvc;
using EmailVerificationSystem.Models;
using EmailVerificationSystem.Services;
using EmailVerificationSystem.Repositories;

namespace EmailVerificationSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VerificationController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly IEmailVerificationRepository _repository;
    private readonly IConfiguration _configuration;

    public VerificationController(
        IEmailService emailService,
        IEmailVerificationRepository repository,
        IConfiguration configuration)
    {
        _emailService = emailService;
        _repository = repository;
        _configuration = configuration;
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { 
            status = "healthy", 
            timestamp = DateTime.UtcNow,
            message = "Email Verification System is running"
        });
    }

    [HttpGet("test")]
    public async Task<IActionResult> Test()
    {
        try
        {
            // Test DynamoDB connection
            var testVerification = new EmailVerification
            {
                Token = "test-token-" + Guid.NewGuid().ToString(),
                Email = "test@example.com",
                IsVerified = false,
                CreatedAt = DateTime.UtcNow
            };

            // Try to save to DynamoDB
            await _repository.SaveVerificationAsync(testVerification);
            
            // Try to retrieve it
            var retrieved = await _repository.GetVerificationAsync(testVerification.Token);
            
            // Clean up test data
            if (retrieved != null)
            {
                // Note: DynamoDB doesn't have a direct delete method in the context, 
                // but we can mark it as verified to distinguish it
                retrieved.IsVerified = true;
                await _repository.UpdateVerificationAsync(retrieved);
            }

            return Ok(new { 
                message = "Test successful", 
                dynamodb = "working",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                message = "Test failed", 
                error = ex.Message,
                details = ex.GetType().Name
            });
        }
    }

    [HttpPost("submit")]
    public async Task<IActionResult> SubmitEmail([FromBody] EmailSubmissionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Generate unique token
            var token = Guid.NewGuid().ToString();

            // Create verification record
            var verification = new EmailVerification
            {
                Token = token,
                Email = request.Email,
                IsVerified = false,
                CreatedAt = DateTime.UtcNow
            };

            // Save to DynamoDB
            await _repository.SaveVerificationAsync(verification);

            // Generate verification link using proper URL generation
            var verificationLink = GetVerificationUrl(token);

            // Send email
            await _emailService.SendVerificationEmailAsync(request.Email, verificationLink);

            return Ok(new { message = "Verification email sent successfully. Please check your inbox." });
        }
        catch (Exception ex)
        {
            // Log the full exception details for debugging
            var logger = HttpContext.RequestServices.GetRequiredService<ILogger<VerificationController>>();
            logger.LogError(ex, "Error in SubmitEmail for email: {Email}. Exception: {Exception}", request.Email, ex.ToString());
            
            return StatusCode(500, new { 
                message = "An error occurred while processing your request.", 
                error = ex.Message,
                details = ex.GetType().Name
            });
        }
    }

    [HttpGet("verify/{token}")]
    public async Task<IActionResult> VerifyToken(string token)
    {
        try
        {
            // Get verification record
            var verification = await _repository.GetVerificationAsync(token);

            if (verification == null)
            {
                var html = CreateHtmlResponse("Verification Failed", "Invalid verification token. The token may have expired or does not exist.", false);
                return File(System.Text.Encoding.UTF8.GetBytes(html), "text/html");
            }

            if (verification.IsVerified)
            {
                var html = CreateHtmlResponse("Already Verified", "This email has already been verified.", true);
                return File(System.Text.Encoding.UTF8.GetBytes(html), "text/html");
            }

            // Check if token is expired (24 hours)
            if (verification.CreatedAt.AddHours(24) < DateTime.UtcNow)
            {
                var html = CreateHtmlResponse("Verification Failed", "Verification token has expired. Please request a new verification email.", false);
                return File(System.Text.Encoding.UTF8.GetBytes(html), "text/html");
            }

            // Mark as verified
            verification.IsVerified = true;
            await _repository.UpdateVerificationAsync(verification);

            var successHtml = CreateHtmlResponse("Email Verified!", "Your email has been successfully verified. Thank you!", true);
            return File(System.Text.Encoding.UTF8.GetBytes(successHtml), "text/html");
        }
        catch (Exception ex)
        {
            var errorHtml = CreateHtmlResponse("Error", $"An error occurred: {ex.Message}", false);
            return StatusCode(500, File(System.Text.Encoding.UTF8.GetBytes(errorHtml), "text/html"));
        }
    }

    private string CreateHtmlResponse(string title, string message, bool isSuccess)
    {
        var color = isSuccess ? "#4CAF50" : "#f44336";
        var icon = isSuccess ? "✓" : "✗";

        return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <title>{title}</title>
            <meta charset='utf-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1'>
            <style>
                body {{ 
                    font-family: Arial, sans-serif; 
                    margin: 0; 
                    padding: 20px; 
                    background-color: #f5f5f5;
                    display: flex;
                    justify-content: center;
                    align-items: center;
                    min-height: 100vh;
                }}
                .container {{ 
                    background-color: white;
                    padding: 40px;
                    border-radius: 8px;
                    box-shadow: 0 2px 10px rgba(0,0,0,0.1);
                    text-align: center;
                    max-width: 500px;
                    width: 100%;
                }}
                .icon {{ 
                    font-size: 48px; 
                    color: {color}; 
                    margin-bottom: 20px; 
                }}
                h1 {{ 
                    color: {color}; 
                    margin-bottom: 20px; 
                }}
                p {{ 
                    color: #666; 
                    line-height: 1.6; 
                    margin-bottom: 30px; 
                }}
                .btn {{ 
                    background-color: {color}; 
                    color: white; 
                    padding: 12px 24px; 
                    text-decoration: none; 
                    border-radius: 4px; 
                    display: inline-block; 
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='icon'>{icon}</div>
                <h1>{title}</h1>
                <p>{message}</p>
                <a href='/' class='btn'>Back to Home</a>
            </div>
        </body>
        </html>";
    }

    /// <summary>
    /// Gets the verification URL using the current request's base URL
    /// This automatically works in any environment (localhost, Docker, Elastic Beanstalk)
    /// </summary>
    private string GetVerificationUrl(string token)
    {
        // Try to get the base URL from configuration first
        var configuredBaseUrl = Environment.GetEnvironmentVariable("BaseUrl"); // _configuration["App:BaseUrl"];


        if (!string.IsNullOrEmpty(configuredBaseUrl))
        {
            // Use configured base URL if available
            return $"{configuredBaseUrl.TrimEnd('/')}/api/verification/verify/{token}";
        }
        
        // Fallback: Use the current request's base URL
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        return $"{baseUrl}/api/verification/verify/{token}";
    }
}