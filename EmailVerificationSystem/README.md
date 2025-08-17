# Email Verification System

A modern, web-based system built with **ASP.NET Core 8** that allows users to submit their email addresses and receive a verification link via email. The system uses **Amazon SES** to send emails and **Amazon DynamoDB** to store verification tokens, with a clean, maintainable frontend architecture.

## 🧠 Features

- ✅ **Modern Frontend Architecture** - Separated HTML, CSS, and JavaScript files for maintainability
- ✅ **Responsive Design** - Mobile-first approach with modern CSS animations and transitions
- ✅ **Professional UI/UX** - Clean, accessible interface with loading states and real-time validation
- ✅ **ASP.NET Core 8 Web API** backend with comprehensive error handling and logging
- ✅ **Amazon SES integration** for sending verification emails with HTML and plain text support
- ✅ **Amazon DynamoDB** for storing verification tokens with proper expiration handling
- ✅ **Email validation** on both frontend and backend with instant feedback
- ✅ **Token expiration** (5 minutes) with proper handling and security
- ✅ **Modern verification flow** - Redirect-based results using static HTML pages instead of hardcoded responses
- ✅ **Accessibility features** - ARIA attributes, semantic HTML, and keyboard navigation support
- ✅ **Dark mode support** - Automatic theme detection with `prefers-color-scheme`
- ✅ **Service Worker ready** - Offline capabilities and performance optimization
- ✅ **SEO-friendly pages** - Static HTML result pages for better search engine indexing
- ✅ **Performance optimized** - External file caching, preloaded resources, and modern JavaScript patterns

## 🧰 Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- AWS CLI configured (`aws configure`)
- Amazon SES verified email (Sandbox mode is OK for testing)
- DynamoDB table named `EmailVerifications`

## 🚀 Setup Instructions

### 1. AWS Configuration

#### Option 1: Environment Variables (Recommended)
Create a `.env` file in your project root with only AWS credentials:

```bash
# .env file
AWS_ACCESS_KEY_ID=your_access_key_here
AWS_SECRET_ACCESS_KEY=your_secret_key_here
```

**Important Security Notes:**
- Add `.env` to your `.gitignore` file to prevent committing credentials
- Never commit AWS credentials to version control
- Use IAM roles when deploying to production (recommended)

#### Option 2: AWS CLI Configuration
Alternatively, you can use AWS CLI:
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
    --region us-east-1
```

#### Setup Amazon SES
1. Go to AWS SES Console
2. Verify your sender email address
3. If in sandbox mode, also verify recipient email addresses for testing
4. Update `appsettings.Development.json` with your sender email:

```json
{
  "AWS": {
    "Region": "us-east-1",
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

#### Setup Environment File
Create a `.env` file in your project root and add it to `.gitignore`:

```bash
# .gitignore
.env
*.env
.env.local
.env.production
```

This ensures your AWS credentials are never committed to version control.

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
    "Region": "us-east-1",
    "Profile": "default",
    "SenderEmail": "your-verified-email@yourdomain.com"
  },
  "App": {
    "BaseUrl": "https://localhost:7000"
  }
}
```

**Environment Variables:**
The application uses a combination of sources:
- **`.env` file**: Contains only AWS credentials
  - `AWS_ACCESS_KEY_ID` - Your AWS access key
  - `AWS_SECRET_ACCESS_KEY` - Your AWS secret key
- **Configuration files**: For other settings
  - `AWS:Region` - AWS region (e.g., us-east-1)
  - `AWS:SenderEmail` - Verified SES email address
  - `App:BaseUrl` - Base URL for verification links

### 3. Running the Application

#### Start the Application
```bash
dotnet run
```

#### Access the Application
- **Frontend**: https://localhost:7000
- **API Documentation**: https://localhost:7000/openapi
- **Verification endpoint**: https://localhost:7000/api/verification/verify/{token}

## 📋 API Endpoints

### Submit Email for Verification
- **POST** `/api/verification/submit`
- **Body**: `{ "email": "user@example.com" }`
- **Response**: `{ "message": "Verification email sent successfully..." }`

### Verify Email Token
- **GET** `/api/verification/verify/{token}`
- **Response**: Redirects to verification result page with appropriate status

### Health Check
- **GET** `/api/verification/health`
- **Response**: System health status

### Test Endpoint
- **GET** `/api/verification/test`
- **Response**: DynamoDB connection test

## 🏗️ Project Structure

```
Email Verification System/
├── EmailVerificationSystem.sln          # Solution file
├── email-verification-requirements.md   # Project requirements
└── EmailVerificationSystem/
    ├── Controllers/
    │   └── VerificationController.cs    # API endpoints with modern redirects
    ├── Models/
    │   ├── EmailVerification.cs         # DynamoDB model
    │   └── EmailSubmissionRequest.cs    # DTO for API requests
    ├── Repositories/
    │   ├── IEmailVerificationRepository.cs
    │   └── EmailVerificationRepository.cs
    ├── Services/
    │   ├── IEmailService.cs
    │   └── EmailService.cs              # Amazon SES integration
    ├── Infrastructure/
    │   └── DynamoDbTableManager.cs      # DynamoDB table management
    ├── wwwroot/
    │   ├── index.html                   # Main frontend page
    │   ├── styles.css                   # Main stylesheet
    │   ├── app.js                       # Main JavaScript application
    │   ├── verification-result.html     # Verification result page
    │   ├── verification-result.css      # Result page styles
    │   └── verification-result.js       # Result page logic
    ├── Program.cs                       # Application configuration
    ├── appsettings.Development.json     # Development settings
    ├── EmailVerificationSystem.csproj   # Project file
    └── README.md
```

## 🎨 Frontend Architecture

### **Modern Separation of Concerns**
- **HTML** - Semantic structure and accessibility with ARIA attributes
- **CSS** - Responsive design with modern features (CSS Grid, Flexbox, Animations, Dark Mode)
- **JavaScript** - ES6+ classes with modular architecture and modern async/await patterns

### **Key Frontend Features**
- **Responsive Design** - Mobile-first approach with CSS Grid and Flexbox layouts
- **Modern Animations** - Smooth transitions, micro-interactions, and CSS animations (shake, fadeIn, slideIn)
- **Accessibility** - ARIA labels, semantic HTML, keyboard navigation, and screen reader support
- **Dark Mode Support** - Automatic theme detection with `prefers-color-scheme` media query
- **Service Worker Ready** - Offline capabilities and performance optimization
- **Real-time Validation** - Instant feedback on user input with blur events and keyboard shortcuts
- **Performance Optimized** - External file caching, preloaded resources, and modern loading states

### **CSS Features**
- CSS Grid and Flexbox for modern, flexible layouts
- CSS Custom Properties (variables) for maintainable theming
- Modern animations and transitions (shake, fadeIn, slideIn effects)
- Media queries for responsive design and mobile optimization
- Dark mode support with `prefers-color-scheme` media query
- Modern UI elements with hover effects and micro-interactions

### **JavaScript Features**
- ES6+ class-based architecture with modern ES6+ syntax
- Modern async/await patterns for clean asynchronous code
- Event-driven architecture with proper event handling
- Modular code organization with separation of concerns
- Service Worker integration ready for offline capabilities
- Real-time validation with blur events and keyboard shortcuts (Ctrl/Cmd + Enter)
- Modern loading states and user feedback systems

## 🔧 Configuration Options

### AWS Settings
- `AWS:Region` - AWS region (default: us-east-1)
- `AWS:Profile` - AWS profile to use (default: default)
- `AWS:SenderEmail` - Verified email address for sending emails

### Application Settings
- `App:BaseUrl` - Base URL for verification links
- `BaseUrl` - Environment variable for production deployments

## 🔐 Security Features

- **Email validation** on frontend and backend with real-time feedback
- **Token expiration** (5 minutes) with proper handling for enhanced security
- **GUID-based tokens** for security and uniqueness
- **CORS configured** for development and production environments
- **Input sanitization** and validation with proper error handling
- **HTTPS enforcement** in production for secure communications
- **Secure redirects** using proper `Redirect()` method to prevent redirect loops
- **URL parameter encoding** with `Uri.EscapeDataString()` for safe data transmission

## 🧪 Testing

1. **Start the application** with `dotnet run`
2. **Navigate to** https://localhost:7000
3. **Enter a valid email** address
4. **Check your email** for the verification link
5. **Click the verification link** to complete the process
6. **Verify the result page** displays correctly

## 🚢 Deployment

### **AWS Elastic Beanstalk (Recommended)**
This application is optimized for AWS Elastic Beanstalk deployment:

1. **Build the application**:
   ```bash
   dotnet publish -c Release
   ```

2. **Deploy to Elastic Beanstalk**:
   - Use the `aws-beanstalk-tools-defaults.json` configuration
   - Ensure proper IAM roles for AWS services
   - Configure environment variables for production

3. **Production Configuration**:
   - Set environment variables for AWS credentials (or use IAM roles)
   - Set `BaseUrl` environment variable
   - Configure CORS for your production domain
   - Move SES out of sandbox mode if needed
   - Ensure `.env` file is not deployed (use environment variables instead)

### **Other Deployment Options**
- **Azure App Service**
- **Docker containers**
- **Any hosting provider** supporting .NET 8



## 📝 Notes

- **DynamoDB table name** is hardcoded as "EmailVerifications"
- **Verification tokens expire** after 5 minutes for enhanced security
- **AWS SDK default credential chain** for authentication
- **Static files served** from ASP.NET Core wwwroot with proper caching
- **Modern redirect-based flow** using static HTML pages instead of hardcoded responses
- **Production-ready** with proper error handling, logging, and performance optimization
- **Maintainable architecture** with separated frontend concerns (HTML, CSS, JavaScript)
- **SEO-friendly** verification result pages for better search engine indexing
- **Performance optimized** with external file caching and modern loading patterns

## 🆘 Troubleshooting

### **Common Issues**
1. **Redirect Loops** - Fixed with proper `Redirect()` usage instead of `RedirectToPage()`
2. **SES Email Rejection** - Ensure sender email is verified in AWS SES console
3. **DynamoDB Connection** - Check AWS credentials and region configuration
4. **CORS Issues** - Verify CORS configuration for your production domain
5. **Token Expiration** - 5-minute expiration requires quick user action
6. **Frontend Loading** - Ensure all external CSS/JS files are accessible

### **Logs**
- Check `/var/log/web.stdout.log` for application logs
- Check `/var/log/nginx/access.log` for web server logs
- Check `/var/log/nginx/error.log` for error logs

## 🤝 Contributing

This project follows modern web development best practices:
- Clean separation of concerns
- Modern JavaScript (ES6+)
- Responsive CSS design
- Accessibility-first approach
- Performance optimization