# Email Verification System – Project Requirements

## 🧠 Overview
A web-based system built with **ASP.NET Core** that allows users to submit their email addresses and receive a verification link via email. The system uses **Amazon SES** to send emails and **Amazon DynamoDB** to store verification tokens.

---

## ✅ Functional Requirements

### 1. User Email Input
- A simple HTML+JS web page with a form
- One field for email input and a submit button
- Submits the form to a backend API (POST request)

### 2. Sending Verification Email
- Generate a unique token (GUID)
- Save a record in DynamoDB:
  - `Token` (Primary Key)
  - `Email` (string)
  - `IsVerified` (bool, default: false)
  - `CreatedAt` (timestamp)
- Generate a verification link in the format:  
  `https://yourdomain.com/api/verify/{token}`
- Send the email via **Amazon SES** including the link

### 3. Verifying Token on Link Click
- Endpoint: `GET /api/verify/{token}`
- Check if token exists in DynamoDB
- If exists and not yet verified:
  - Mark `IsVerified = true`
  - Return success message/page
- If not found or already verified:
  - Return error message/page

### 4. Backend Implementation
- Written in **ASP.NET Core 8 Web API**
- Use Amazon SDKs:
  - SES (for sending emails)
  - DynamoDB (for storing tokens)
- Create services:
  - `EmailService`
  - `EmailVerificationRepository`
- One controller: `VerificationController`

---

## 🧪 Technical Requirements

| Component         | Technology                  |
|------------------|-----------------------------|
| Backend           | ASP.NET Core 8 Web API       |
| Frontend          | HTML + JavaScript  |
| Email Service     | Amazon SES                  |
| Database          | Amazon DynamoDB             |
| Local Run         | `dotnet run`                |
| SDKs              | AWSSDK.SimpleEmail + AWSSDK.DynamoDBv2 |
| Future Deployment | AWS Elastic Beanstalk       |

---

## 🔐 Security Considerations

- Validate email input on the frontend and backend
- Prevent expired or reused tokens (optionally use DynamoDB TTL)
- Use HTTPS for all requests
- Store AWS keys securely (use environment variables or Secrets Manager)

---

## 🧰 Local Development Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- VS Code
- AWS CLI configured (`aws configure`)
- Amazon SES verified email (Sandbox mode is OK for testing)
- DynamoDB table named `EmailVerifications`
- `appsettings.Development.json` example:
```json
{
  "AWS": {
    "Region": "eu-central-1",
    "Profile": "default"
  }
}
