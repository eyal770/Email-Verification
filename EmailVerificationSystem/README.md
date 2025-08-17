# Email Verification System

A web-based system built with **ASP.NET Core 8** that allows users to submit their email addresses and receive a verification link via email. The system uses **Amazon SES** to send emails and **Amazon DynamoDB** to store verification tokens.

## 🧠 Features

- ✅ Simple HTML+JS frontend for email submission
- ✅ ASP.NET Core 8 Web API backend
- ✅ Amazon SES integration for sending verification emails
- ✅ Amazon DynamoDB for storing verification tokens
- ✅ Email validation on both frontend and backend
- ✅ Token expiration (24 hours)
- ✅ Beautiful, responsive UI with loading states
- ✅ Comprehensive error handling

## 🧰 Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- AWS CLI configured (`aws configure`)
- Amazon SES verified email (Sandbox mode is OK for testing)
- DynamoDB table named `EmailVerifications`

## 🚀 Setup Instructions

### 1. AWS Configuration

#### Configure AWS CLI
```bash
aws configure
```
Enter your AWS Access Key ID, Secret Access Key, default region, and output format.

#### Create DynamoDB Table
Create a table named `EmailVerifications` with:
- **Primary Key**: `Token` (String)
- **No sort key needed**

You can create this via AWS Console or CLI:
```bash
aws dynamodb create-table \
    --table-name EmailVerifications \
    --attribute-definitions AttributeName=Token,AttributeType=S \
    --key-schema AttributeName=Token,KeyType=HASH \
    --billing-mode PAY_PER_REQUEST \
    --region eu-central-1
```

#### Setup Amazon SES
1. Go to AWS SES Console
2. Verify your sender email address
3. If in sandbox mode, also verify recipient email addresses for testing
4. Update `appsettings.Development.json` with your sender email:

```json
{
  "AWS": {
    "Region": "eu-central-1",
    "Profile": "default",
    "SenderEmail": "your-verified-email@yourdomain.com"
  }
}
```

### 2. Project Setup

#### Build from Solution
From the root directory:
```bash
dotnet build
```

Or navigate to the project directory:
```bash
cd EmailVerificationSystem
```

#### Install Dependencies
```bash
dotnet restore
```

#### Update Configuration
Edit `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AWS": {
    "Region": "eu-central-1",
    "Profile": "default",
    "SenderEmail": "your-verified-email@yourdomain.com"
  },
  "App": {
    "BaseUrl": "https://localhost:7000"
  }
}
```

### 3. Running the Application

#### Start the Application
```bash
dotnet run
```

#### Access the Application
- Frontend: https://localhost:7000
- API Documentation: https://localhost:7000/openapi
- Verification endpoint: https://localhost:7000/api/verification/verify/{token}

## 📋 API Endpoints

### Submit Email for Verification
- **POST** `/api/verification/submit`
- **Body**: `{ "email": "user@example.com" }`
- **Response**: `{ "message": "Verification email sent successfully..." }`

### Verify Email Token
- **GET** `/api/verification/verify/{token}`
- **Response**: HTML page with verification result

## 🏗️ Project Structure

```
Email Verification System/
├── EmailVerificationSystem.sln          # Solution file
├── email-verification-requirements.md   # Project requirements
└── EmailVerificationSystem/
    ├── Controllers/
    │   └── VerificationController.cs    # API endpoints
    ├── Models/
    │   ├── EmailVerification.cs         # DynamoDB model
    │   └── EmailSubmissionRequest.cs    # DTO for API requests
    ├── Repositories/
    │   ├── IEmailVerificationRepository.cs
    │   └── EmailVerificationRepository.cs
    ├── Services/
    │   ├── IEmailService.cs
    │   └── EmailService.cs              # Amazon SES integration
    ├── wwwroot/
    │   └── index.html                   # Frontend application
    ├── Program.cs                       # Application configuration
    ├── appsettings.Development.json     # Development settings
    ├── EmailVerificationSystem.csproj   # Project file
    └── README.md
```

## 🔧 Configuration Options

### AWS Settings
- `AWS:Region` - AWS region (default: eu-central-1)
- `AWS:Profile` - AWS profile to use (default: default)
- `AWS:SenderEmail` - Verified email address for sending emails

### Application Settings
- `App:BaseUrl` - Base URL for verification links

## 🔐 Security Features

- Email validation on frontend and backend
- Token expiration (24 hours)
- GUID-based tokens for security
- CORS configured for localhost development
- Input sanitization and validation

## 🧪 Testing

1. Start the application with `dotnet run`
2. Navigate to https://localhost:7000
3. Enter a valid email address
4. Check your email for the verification link
5. Click the verification link to complete the process

## 🚢 Deployment

This application is ready for deployment to:
- AWS Elastic Beanstalk
- Azure App Service
- Any hosting provider supporting .NET 8

For production deployment:
1. Update `appsettings.json` with production settings
2. Ensure AWS credentials are configured via IAM roles (recommended) or environment variables
3. Update CORS policy for your production domain
4. Move SES out of sandbox mode if needed

## 📝 Notes

- The DynamoDB table name is hardcoded as "EmailVerifications"
- Verification tokens expire after 24 hours
- The system uses AWS SDK default credential chain for authentication
- Frontend is served as static files from the ASP.NET Core application