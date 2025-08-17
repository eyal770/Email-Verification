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
                return Redirect($"/verification-result.html?status=invalid&message={Uri.EscapeDataString("Invalid verification token. The token may have expired or does not exist.")}&token={Uri.EscapeDataString(token)}");
            }

            if (verification.IsVerified)
            {
                return Redirect($"/verification-result.html?status=already-verified&message={Uri.EscapeDataString("This email has already been verified.")}&email={Uri.EscapeDataString(verification.Email)}&token={Uri.EscapeDataString(token)}");
            }

                           // Check if token is expired (5 minutes)
               if (verification.CreatedAt.AddMinutes(5) < DateTime.UtcNow)
            {
                return Redirect($"/verification-result.html?status=expired&message={Uri.EscapeDataString("Verification token has expired. Please request a new verification email.")}&email={Uri.EscapeDataString(verification.Email)}&token={Uri.EscapeDataString(token)}");
            }

            // Mark as verified
            verification.IsVerified = true;
            await _repository.UpdateVerificationAsync(verification);

            return Redirect($"/verification-result.html?status=success&message={Uri.EscapeDataString("Your email has been successfully verified. Thank you!")}&email={Uri.EscapeDataString(verification.Email)}&token={Uri.EscapeDataString(token)}");
        }
        catch (Exception ex)
        {
            return Redirect($"/verification-result.html?status=error&message={Uri.EscapeDataString($"An error occurred: {ex.Message}")}&token={Uri.EscapeDataString(token)}");
        }
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