# 📧 Codout.Mailer

Robust and extensible library for sending emails in .NET 10 applications, with support for Razor templates, multiple providers (SendGrid, AWS SES), and complete observability.

## 🚀 Features

- ✅ **Razor Templates**: HTML email rendering using ASP.NET Core's native Razor engine
- ✅ **Multiple Providers**: Support for SendGrid and AWS SES
- ✅ **Dependency Injection**: Native integration with ASP.NET Core DI
- ✅ **Observability**: Distributed tracing and metrics with OpenTelemetry
- ✅ **Health Checks**: Service health monitoring
- ✅ **Smart Caching**: Compiled template caching for performance
- ✅ **Retry Policy**: Automatic retries with exponential backoff
- ✅ **Flexible Configuration**: Configuration via appsettings.json and code

## 📦 Installation

### Main Package
```
dotnet add package Codout.Mailer
```

### Razor Template Engine
```
dotnet add package Codout.Mailer.Razor
```

### Specific Providers
```
# For SendGrid
dotnet add package Codout.Mailer.SendGrid

# For AWS SES
dotnet add package Codout.Mailer.AWS
```

## ⚙️ Configuration

### 1. **Dependency Injection in Program.cs**

```csharp
var builder = WebApplication.CreateBuilder(args);

// Basic mailer configuration
builder.Services.AddMailer(builder.Configuration);

// Razor template engine (uses ASP.NET Core's native Razor engine)
builder.Services.AddMailerRazor(options =>
{
    options.TemplateAssembly = typeof(Program).Assembly;
    options.RootNamespace = "YourProject.Templates";
});

// Choose email provider
builder.Services.AddMailerWithSendGrid(builder.Configuration);
// OR
// builder.Services.AddMailerWithAws(builder.Configuration);

// Register your custom email service
builder.Services.AddScoped<EmailService>();

var app = builder.Build();
```

### 2. **Settings in appsettings.json**

```json
{
  "MailerSettings": {
    "DefaultFromName": "Email System",
    "DefaultFromEmail": "noreply@example.com"
  },
  "SendGridSettings": {
    "ApiKey": "SG.your-api-key-here",
    "SandboxMode": false
  },
  "AWSSettings": {
    "RegionEndpoint": "us-east-1",
    "AccessKey": "your-access-key",
    "SecretKey": "your-secret-key"
  }
}
```

## 🏗️ Implementation in Your Project

### 1. **Creating Your EmailService Class**

Create a class that inherits from `MailerServiceBase` in your project:

```csharp
// Services/EmailService.cs
using System.Net.Mail;
using System.Threading.Tasks;
using Codout.Mailer.Configuration;
using Codout.Mailer.Interfaces;
using Codout.Mailer.Models;
using Codout.Mailer.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YourProject.Models.Email;

public class EmailService : MailerServiceBase
{
    public EmailService(
        IOptions<MailerSettings> mailerSettings,
        IMailerDispatcher dispatcher,
        ITemplateEngine templateEngine,
        ILogger<EmailService> logger)
        : base(mailerSettings, dispatcher, templateEngine, logger)
    {
    }

    /// <summary>
    /// Sends welcome email to new users
    /// </summary>
    public async Task<MailerResponse> Welcome(WelcomeModel model)
    {
        return await Send("Welcome", model, "Welcome to our platform!");
    }

    /// <summary>
    /// Sends password reset email
    /// </summary>
    public async Task<MailerResponse> PasswordReset(PasswordResetModel model)
    {
        return await Send("PasswordReset", model, "Password Recovery");
    }

    /// <summary>
    /// Sends registration confirmation email
    /// </summary>
    public async Task<MailerResponse> ConfirmRegistration(ConfirmRegistrationModel model)
    {
        return await Send("ConfirmRegistration", model, "Confirm your registration");
    }

    /// <summary>
    /// Sends order notification email
    /// </summary>
    public async Task<MailerResponse> OrderNotification(OrderNotificationModel model)
    {
        return await Send("OrderNotification", model, $"Order #{model.OrderNumber} - Status Updated");
    }

    /// <summary>
    /// Sends monthly report with attachment
    /// </summary>
    public async Task<MailerResponse> MonthlyReport(MonthlyReportModel model, Attachment[] attachments = null)
    {
        return await Send("MonthlyReport", model, $"Monthly Report - {model.Month:MMMM yyyy}", attachments);
    }
}
```

### 2. **Creating Email Models**

Create a `Models/Email` folder and add models for each email type:

```csharp
// Models/Email/WelcomeModel.cs
using System.Net.Mail;
using Codout.Mailer.Models;

namespace YourProject.Models.Email;

public class WelcomeModel : MailerModelBase
{
    public string Name { get; set; }
    public string ActivationLink { get; set; }
    public string CompanyName { get; set; } = "My Company";
    public string SupportEmail { get; set; } = "support@mycompany.com";
}
```

```csharp
// Models/Email/PasswordResetModel.cs
using System.Net.Mail;
using Codout.Mailer.Models;

namespace YourProject.Models.Email;

public class PasswordResetModel : MailerModelBase
{
    public string Name { get; set; }
    public string ResetLink { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string IpAddress { get; set; }
}
```

```csharp
// Models/Email/ConfirmRegistrationModel.cs
using System.Net.Mail;
using Codout.Mailer.Models;

namespace YourProject.Models.Email;

public class ConfirmRegistrationModel : MailerModelBase
{
    public string Name { get; set; }
    public string ConfirmationLink { get; set; }
    public string Username { get; set; }
}
```

```csharp
// Models/Email/OrderNotificationModel.cs
using System.Net.Mail;
using Codout.Mailer.Models;

namespace YourProject.Models.Email;

public class OrderNotificationModel : MailerModelBase
{
    public string CustomerName { get; set; }
    public string OrderNumber { get; set; }
    public string Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
    public List<OrderItemModel> Items { get; set; } = new();
    public string TrackingUrl { get; set; }
}

public class OrderItemModel
{
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
```

```csharp
// Models/Email/MonthlyReportModel.cs
using System.Net.Mail;
using Codout.Mailer.Models;

namespace YourProject.Models.Email;

public class MonthlyReportModel : MailerModelBase
{
    public string Name { get; set; }
    public DateTime Month { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public string ReportType { get; set; }
    public List<string> Highlights { get; set; } = new();
}
```

## 📝 Creating Email Templates

### 1. **Folder Structure**

Create the following structure in your project:

```
YourProject/
├── Templates/
│   ├── _Layout.cshtml
│   ├── Welcome.cshtml
│   ├── PasswordReset.cshtml
│   ├── ConfirmRegistration.cshtml
│   ├── OrderNotification.cshtml
│   └── MonthlyReport.cshtml
```

### 2. **Base Template (_Layout.cshtml)**

```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>@ViewBag.Title</title>
    <style>
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            color: #333;
            background-color: #f4f4f4;
            margin: 0;
            padding: 0;
        }
        .container {
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
        }
        .header {
            text-align: center;
            border-bottom: 2px solid #007bff;
            padding-bottom: 20px;
            margin-bottom: 30px;
        }
        .footer {
            text-align: center;
            margin-top: 40px;
            padding-top: 20px;
            border-top: 1px solid #eee;
            font-size: 12px;
            color: #666;
        }
        .btn {
            display: inline-block;
            padding: 12px 24px;
            background-color: #007bff;
            color: white;
            text-decoration: none;
            border-radius: 4px;
            font-weight: bold;
        }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1 style="color: #007bff; margin: 0;">My Company</h1>
        </div>
        
        @RenderBody()
        
        <div class="footer">
            <p>© 2025 My Company. All rights reserved.</p>
            <p>This email was sent automatically. Please do not reply to this email.</p>
        </div>
    </div>
</body>
</html>
```

### 3. **Welcome Template (Welcome.cshtml)**

```html
@model YourProject.Models.Email.WelcomeModel
@{
    Layout = "_Layout";
    ViewBag.Title = "Welcome";
}

<h2>Welcome, @Model.Name!</h2>

<p>Thank you for signing up for our platform. We're very happy to have you with us!</p>

<p>To start using all features, click the button below to activate your account:</p>

<div style="text-align: center; margin: 30px 0;">
    <a href="@Model.ActivationLink" class="btn">Activate My Account</a>
</div>

<p>If you can't click the button, copy and paste the link below into your browser:</p>
<p style="word-break: break-all; font-size: 12px; color: #666;">@Model.ActivationLink</p>

<hr style="margin: 30px 0; border: 1px solid #eee;">

<p>If you have any questions, our support team is always ready to help:</p>
<p>📧 Email: <a href="mailto:@Model.SupportEmail">@Model.SupportEmail</a></p>

<p>
    Best regards,<br>
    <strong>@Model.CompanyName Team</strong>
</p>
```

### 4. **Password Reset Template (PasswordReset.cshtml)**

```html
@model YourProject.Models.Email.PasswordResetModel
@{
    Layout = "_Layout";
    ViewBag.Title = "Password Recovery";
}

<h2>Hello, @Model.Name!</h2>

<p>We received a request to reset your account password.</p>

<div style="background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 4px; margin: 20px 0;">
    <strong>⚠️ Important information:</strong>
    <ul style="margin: 10px 0; padding-left: 20px;">
        <li>This link expires on: <strong>@Model.ExpiresAt.ToString("MM/dd/yyyy HH:mm")</strong></li>
        <li>Request made from IP: <strong>@Model.IpAddress</strong></li>
    </ul>
</div>

<p>If you made this request, click the button below to reset your password:</p>

<div style="text-align: center; margin: 30px 0;">
    <a href="@Model.ResetLink" class="btn">Reset Password</a>
</div>

<p>If you didn't request this change, you can safely ignore this email. Your password will remain unchanged.</p>

<p style="font-size: 12px; color: #666;">
    For security, this link can only be used once and expires automatically.
</p>
```

### 5. **Order Template (OrderNotification.cshtml)**

```html
@model YourProject.Models.Email.OrderNotificationModel
@{
    Layout = "_Layout";
    ViewBag.Title = "Order Status";
}

<h2>Hello, @Model.CustomerName!</h2>

<p>We have an update about your order:</p>

<div style="background-color: #d4edda; border: 1px solid #c3e6cb; padding: 20px; border-radius: 4px; margin: 20px 0;">
    <h3 style="margin-top: 0; color: #155724;">Order #@Model.OrderNumber</h3>
    <p><strong>Status:</strong> @Model.Status</p>
    <p><strong>Order Date:</strong> @Model.OrderDate.ToString("MM/dd/yyyy")</p>
    <p><strong>Total:</strong> @Model.TotalAmount.ToString("C")</p>
</div>

<h3>Order Items:</h3>
<table style="width: 100%; border-collapse: collapse; margin: 20px 0;">
    <thead>
        <tr style="background-color: #f8f9fa;">
            <th style="border: 1px solid #dee2e6; padding: 8px; text-align: left;">Product</th>
            <th style="border: 1px solid #dee2e6; padding: 8px; text-align: center;">Qty</th>
            <th style="border: 1px solid #dee2e6; padding: 8px; text-align: right;">Price</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var item in Model.Items)
        {
            <tr>
                <td style="border: 1px solid #dee2e6; padding: 8px;">@item.ProductName</td>
                <td style="border: 1px solid #dee2e6; padding: 8px; text-align: center;">@item.Quantity</td>
                <td style="border: 1px solid #dee2e6; padding: 8px; text-align: right;">@item.Price.ToString("C")</td>
            </tr>
        }
    </tbody>
</table>

@if (!string.IsNullOrEmpty(Model.TrackingUrl))
{
    <div style="text-align: center; margin: 30px 0;">
        <a href="@Model.TrackingUrl" class="btn">Track Order</a>
    </div>
}

<p>Thank you for choosing our store!</p>
```

## 🎯 Using in Controllers

### 1. **Controller Injection**

```csharp
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly EmailService _emailService;
    private readonly ILogger<UserController> _logger;

    public UserController(EmailService emailService, ILogger<UserController> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }
}
```

### 2. **Sending Emails**

```csharp
[HttpPost("register")]
public async Task<IActionResult> Register([FromBody] RegisterRequest request)
{
    // User registration logic...
    
    try
    {
        var activationLink = Url.Action("ActivateAccount", "User", 
            new { token = user.ActivationToken }, Request.Scheme);

        var response = await _emailService.Welcome(new WelcomeModel
        {
            Name = request.Name,
            To = new MailAddress(request.Email, request.Name),
            ActivationLink = activationLink,
            CompanyName = "My Company",
            SupportEmail = "support@mycompany.com"
        });

        if (response.Sent)
        {
            _logger.LogInformation("Welcome email sent to {Email}", request.Email);
            return Ok(new { message = "User registered! Check your email to activate your account." });
        }
        else
        {
            _logger.LogWarning("Failed to send email to {Email}: {Errors}", 
                request.Email, string.Join(", ", response.ErrorMessages));
            return Ok(new { message = "User registered, but there was an issue sending the email." });
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error sending welcome email to {Email}", request.Email);
        return Ok(new { message = "User registered successfully!" });
    }
}

[HttpPost("forgot-password")]
public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
{
    // Password recovery logic...
    
    var resetLink = Url.Action("ResetPassword", "User", 
        new { token = resetToken }, Request.Scheme);

    var response = await _emailService.PasswordReset(new PasswordResetModel
    {
        Name = user.Name,
        To = new MailAddress(user.Email, user.Name),
        ResetLink = resetLink,
        ExpiresAt = DateTime.UtcNow.AddHours(2),
        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
    });

    return Ok(new { message = "If the email exists, you will receive instructions to reset your password." });
}
```

## 📋 Configuring Templates as Embedded Resources

For templates to be found by the Razor engine, add to your `.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <EmbeddedResource Include="Templates\*.cshtml" />
  </ItemGroup>

</Project>
```

## 🚀 Complete Usage Example

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Mailer configuration
builder.Services.AddMailer(builder.Configuration);

// Razor template engine (ASP.NET Core native)
builder.Services.AddMailerRazor(options =>
{
    options.TemplateAssembly = typeof(Program).Assembly;
    options.RootNamespace = "YourProject.Templates";
    options.EnableCache = true;
});

// Email provider
builder.Services.AddMailerWithSendGrid(builder.Configuration);

// Your custom service
builder.Services.AddScoped<EmailService>();

// Other services...
builder.Services.AddControllers();

var app = builder.Build();

// Pipeline...
app.MapControllers();
app.Run();
```

## 🔧 Advanced Configurations

### **Razor Template Engine**
```csharp
builder.Services.AddMailerRazor(options =>
{
    options.TemplateAssembly = typeof(Program).Assembly;
    options.RootNamespace = "YourProject.Templates";
    options.EnableCache = true; // Enables compiled template caching (default: true)
});
```

### **Custom Template Engine**

You can implement `ITemplateEngine` to use any template engine of your choice:

```csharp
public class MyCustomTemplateEngine : ITemplateEngine
{
    public async Task<string> RenderAsync<T>(string templateKey, T model)
    {
        // Your custom rendering logic
    }
}

// Register in DI
builder.Services.AddScoped<ITemplateEngine, MyCustomTemplateEngine>();
```

## 📊 Monitoring and Observability

### **Health Checks**
```csharp
// Automatically added when using AddMailer()
app.MapHealthChecks("/health");
```

### **Metrics and Tracing**
The library automatically generates:
- **Traces**: For each email send operation
- **Metrics**: Counters for sent/failed emails
- **Structured logs**: For debugging and auditing

## 🚨 Error Handling

```csharp
try
{
    var response = await _emailService.Welcome(model);
    
    if (!response.Sent)
    {
        // Email was not sent
        _logger.LogWarning("Send failure: {Errors}", 
            string.Join(", ", response.ErrorMessages));
    }
}
catch (TemplateNotFoundException ex)
{
    _logger.LogError("Template not found: {Template}", ex.TemplateName);
}
catch (EmailProviderException ex)
{
    _logger.LogError("Email provider error: {Error}", ex.Message);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Unexpected error sending email");
}
```

## 📋 Available Configurations

### MailerSettings (appsettings.json)

| Configuration | Description | Default |
|-------------|-----------|--------|
| `DefaultFromName` | Default sender name | `null` |
| `DefaultFromEmail` | Default sender email | `null` |

### RazorMailerOptions (Codout.Mailer.Razor)

| Configuration | Description | Default |
|-------------|-----------|--------|
| `TemplateAssembly` | Assembly containing embedded Razor templates | `null` |
| `RootNamespace` | Root namespace of embedded templates | `null` |
| `EnableCache` | Enable compiled template caching | `true` |

## 🔧 Tips and Best Practices

### ✅ **Do's**
- ✅ Always inherit from `MailerServiceBase` for your custom class
- ✅ Create specific models inheriting from `MailerModelBase`
- ✅ Use Razor templates organized in folders
- ✅ Register a template engine via `AddMailerRazor()` or a custom `ITemplateEngine`
- ✅ Handle exceptions when sending emails
- ✅ Use logging for auditing

### ❌ **Don'ts**
- ❌ Don't use `IMailerService` directly in controllers
- ❌ Don't forget to configure templates as Embedded Resources
- ❌ Don't expose send errors to end users
- ❌ Don't send emails without data validation

## 📚 Next Steps

1. **Advanced Templates**: Create responsive layouts
2. **Internationalization**: Support for multiple languages
3. **Email Queue**: Integration with background services
4. **Analytics**: Open and click tracking

---

Now you have everything you need to implement a robust email sending system! 🎉
