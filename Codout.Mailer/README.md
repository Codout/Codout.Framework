# 📧 Codout.Mailer

Biblioteca robusta e extensível para envio de e-mails em aplicações .NET 9, com suporte a templates Razor, multiple providers (SendGrid, AWS SES) e observabilidade completa.

## 🚀 Características

- ✅ **Templates Razor**: Renderização de e-mails HTML usando RazorLight
- ✅ **Multiple Providers**: Suporte a SendGrid e AWS SES
- ✅ **Dependency Injection**: Integração nativa com ASP.NET Core DI
- ✅ **Observabilidade**: Tracing distribuído e métricas com OpenTelemetry
- ✅ **Health Checks**: Monitoramento de saúde dos serviços
- ✅ **Cache Inteligente**: Cache de templates compilados para performance
- ✅ **Retry Policy**: Tentativas automáticas com exponential backoff
- ✅ **Configuração Flexível**: Configuração via appsettings.json e código

## 📦 Instalação

### Pacote Principal
```
dotnet add package Codout.Mailer
```

### Provedores Específicos
```
# Para SendGrid
dotnet add package Codout.Mailer.SendGrid

# Para AWS SES
dotnet add package Codout.Mailer.AWS
```

## ⚙️ Configuração

### 1. **Injeção de Dependência no Program.cs**

```csharp
var builder = WebApplication.CreateBuilder(args);

// Configuração básica do mailer
builder.Services.AddMailer(builder.Configuration, options =>
{
    // Define onde estão os templates embarcados no seu projeto
    options.TemplateRootType = typeof(Program);
});

// Escolha do provedor de e-mail
builder.Services.AddMailerWithSendGrid(builder.Configuration);
// OU
// builder.Services.AddMailerWithAws(builder.Configuration);

// Registra seu serviço de e-mail personalizado
builder.Services.AddScoped<EmailService>();

var app = builder.Build();
```

### 2. **Configurações no appsettings.json**

```json
{
  "MailerSettings": {
    "DefaultFromName": "Sistema de Email",
    "DefaultFromEmail": "noreply@exemplo.com"
  },
  "MailerOptions": {
    "TemplateRenderTimeoutSeconds": 30,
    "EmailSendTimeoutSeconds": 60,
    "EnableTemplateCache": true,
    "TemplateCacheSize": 100,
    "TemplateCacheLifetimeMinutes": 60,
    "EnableDistributedTracing": true,
    "EnableMetrics": true,
    "MaxRetryAttempts": 3,
    "RetryBaseDelayMs": 1000,
    "DevelopmentMode": false,
    "TracingPrefix": "Codout.Mailer",
    "RazorLight": {
      "DefaultNamespace": "Codout.Mailer.Templates",
      "EnableRuntimeCompilation": true,
      "EnableAssemblyCache": true,
      "EnableHotReload": false
    }
  },
  "SendGridSettings": {
    "ApiKey": "SG.sua-api-key-aqui",
    "SandboxMode": false
  },
  "AWSSettings": {
    "RegionEndpoint": "us-east-1",
    "AccessKey": "sua-access-key",
    "SecretKey": "sua-secret-key"
  }
}
```

## 🏗️ Implementação no Seu Projeto

### 1. **Criando Sua Classe EmailService**

Crie uma classe que herda de `MailerService` no seu projeto:

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

public class EmailService : MailerService
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
    /// Envia e-mail de boas-vindas para novos usuários
    /// </summary>
    public async Task<MailerResponse> Welcome(WelcomeModel model)
    {
        return await Send("Welcome", model, "Bem-vindo à nossa plataforma!");
    }

    /// <summary>
    /// Envia e-mail de recuperação de senha
    /// </summary>
    public async Task<MailerResponse> PasswordReset(PasswordResetModel model)
    {
        return await Send("PasswordReset", model, "Recuperação de senha");
    }

    /// <summary>
    /// Envia e-mail de confirmação de cadastro
    /// </summary>
    public async Task<MailerResponse> ConfirmRegistration(ConfirmRegistrationModel model)
    {
        return await Send("ConfirmRegistration", model, "Confirme seu cadastro");
    }

    /// <summary>
    /// Envia e-mail de notificação de pedido
    /// </summary>
    public async Task<MailerResponse> OrderNotification(OrderNotificationModel model)
    {
        return await Send("OrderNotification", model, $"Pedido #{model.OrderNumber} - Status Atualizado");
    }

    /// <summary>
    /// Envia relatório mensal com anexo
    /// </summary>
    public async Task<MailerResponse> MonthlyReport(MonthlyReportModel model, Attachment[] attachments = null)
    {
        return await Send("MonthlyReport", model, $"Relatório Mensal - {model.Month:MMMM yyyy}", attachments);
    }
}
```

### 2. **Criando os Modelos de E-mail**

Crie uma pasta `Models/Email` e adicione os modelos para cada tipo de e-mail:

```csharp
// Models/Email/WelcomeModel.cs
using System.Net.Mail;
using Codout.Mailer.Models;

namespace YourProject.Models.Email;

public class WelcomeModel : MailerModelBase
{
    public string Name { get; set; }
    public string ActivationLink { get; set; }
    public string CompanyName { get; set; } = "Minha Empresa";
    public string SupportEmail { get; set; } = "suporte@minhaempresa.com";
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

## 📝 Criando Templates de E-mail

### 1. **Estrutura de Pastas**

Crie a seguinte estrutura no seu projeto:

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

### 2. **Template Base (_Layout.cshtml)**

```html
<!DOCTYPE html>
<html lang="pt-BR">
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
            <h1 style="color: #007bff; margin: 0;">Minha Empresa</h1>
        </div>
        
        @RenderBody()
        
        <div class="footer">
            <p>© 2025 Minha Empresa. Todos os direitos reservados.</p>
            <p>Este e-mail foi enviado automaticamente. Não responda a este e-mail.</p>
        </div>
    </div>
</body>
</html>
```

### 3. **Template de Boas-vindas (Welcome.cshtml)**

```html
@model YourProject.Models.Email.WelcomeModel
@{
    Layout = "_Layout";
    ViewBag.Title = "Bem-vindo";
}

<h2>Bem-vindo, @Model.Name!</h2>

<p>Obrigado por se cadastrar na nossa plataforma. Estamos muito felizes em tê-lo conosco!</p>

<p>Para começar a usar todos os recursos, clique no botão abaixo para ativar sua conta:</p>

<div style="text-align: center; margin: 30px 0;">
    <a href="@Model.ActivationLink" class="btn">Ativar Minha Conta</a>
</div>

<p>Se você não conseguir clicar no botão, copie e cole o link abaixo no seu navegador:</p>
<p style="word-break: break-all; font-size: 12px; color: #666;">@Model.ActivationLink</p>

<hr style="margin: 30px 0; border: 1px solid #eee;">

<p>Se você tiver alguma dúvida, nossa equipe de suporte está sempre pronta para ajudar:</p>
<p>📧 E-mail: <a href="mailto:@Model.SupportEmail">@Model.SupportEmail</a></p>

<p>
    Atenciosamente,<br>
    <strong>Equipe @Model.CompanyName</strong>
</p>
```

### 4. **Template de Recuperação de Senha (PasswordReset.cshtml)**

```html
@model YourProject.Models.Email.PasswordResetModel
@{
    Layout = "_Layout";
    ViewBag.Title = "Recuperação de Senha";
}

<h2>Olá, @Model.Name!</h2>

<p>Recebemos uma solicitação para redefinir a senha da sua conta.</p>

<div style="background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 4px; margin: 20px 0;">
    <strong>⚠️ Informações importantes:</strong>
    <ul style="margin: 10px 0; padding-left: 20px;">
        <li>Este link expira em: <strong>@Model.ExpiresAt.ToString("dd/MM/yyyy HH:mm")</strong></li>
        <li>Solicitação feita do IP: <strong>@Model.IpAddress</strong></li>
    </ul>
</div>

<p>Se foi você quem solicitou, clique no botão abaixo para redefinir sua senha:</p>

<div style="text-align: center; margin: 30px 0;">
    <a href="@Model.ResetLink" class="btn">Redefinir Senha</a>
</div>

<p>Se você não solicitou esta alteração, pode ignorar este e-mail com segurança. Sua senha permanecerá inalterada.</p>

<p style="font-size: 12px; color: #666;">
    Por segurança, este link só pode ser usado uma vez e expira automaticamente.
</p>
```

### 5. **Template de Pedido (OrderNotification.cshtml)**

```html
@model YourProject.Models.Email.OrderNotificationModel
@{
    Layout = "_Layout";
    ViewBag.Title = "Status do Pedido";
}

<h2>Olá, @Model.CustomerName!</h2>

<p>Temos uma atualização sobre seu pedido:</p>

<div style="background-color: #d4edda; border: 1px solid #c3e6cb; padding: 20px; border-radius: 4px; margin: 20px 0;">
    <h3 style="margin-top: 0; color: #155724;">Pedido #@Model.OrderNumber</h3>
    <p><strong>Status:</strong> @Model.Status</p>
    <p><strong>Data do Pedido:</strong> @Model.OrderDate.ToString("dd/MM/yyyy")</p>
    <p><strong>Total:</strong> @Model.TotalAmount.ToString("C")</p>
</div>

<h3>Itens do Pedido:</h3>
<table style="width: 100%; border-collapse: collapse; margin: 20px 0;">
    <thead>
        <tr style="background-color: #f8f9fa;">
            <th style="border: 1px solid #dee2e6; padding: 8px; text-align: left;">Produto</th>
            <th style="border: 1px solid #dee2e6; padding: 8px; text-align: center;">Qtd</th>
            <th style="border: 1px solid #dee2e6; padding: 8px; text-align: right;">Preço</th>
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
        <a href="@Model.TrackingUrl" class="btn">Rastrear Pedido</a>
    </div>
}

<p>Obrigado por escolher nossa loja!</p>
```

## 🎯 Utilizando no Controller

### 1. **Injeção no Controller**

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

### 2. **Enviando E-mails**

```csharp
[HttpPost("register")]
public async Task<IActionResult> Register([FromBody] RegisterRequest request)
{
    // Lógica de cadastro do usuário...
    
    try
    {
        var activationLink = Url.Action("ActivateAccount", "User", 
            new { token = user.ActivationToken }, Request.Scheme);

        var response = await _emailService.Welcome(new WelcomeModel
        {
            Name = request.Name,
            To = new MailAddress(request.Email, request.Name),
            ActivationLink = activationLink,
            CompanyName = "Minha Empresa",
            SupportEmail = "suporte@minhaempresa.com"
        });

        if (response.Sent)
        {
            _logger.LogInformation("Email de boas-vindas enviado para {Email}", request.Email);
            return Ok(new { message = "Usuário cadastrado! Verifique seu e-mail para ativar a conta." });
        }
        else
        {
            _logger.LogWarning("Falha ao enviar email para {Email}: {Errors}", 
                request.Email, string.Join(", ", response.ErrorMessages));
            return Ok(new { message = "Usuário cadastrado, mas houve problema no envio do email." });
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Erro ao enviar email de boas-vindas para {Email}", request.Email);
        return Ok(new { message = "Usuário cadastrado com sucesso!" });
    }
}

[HttpPost("forgot-password")]
public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
{
    // Lógica de recuperação de senha...
    
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

    return Ok(new { message = "Se o e-mail existir, você receberá instruções para redefinir a senha." });
}
```

## 📋 Configuração dos Templates como Embedded Resources

Para que os templates sejam encontrados pelo RazorLight, adicione no seu `.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <EmbeddedResource Include="Templates\*.cshtml" />
  </ItemGroup>

</Project>
```

## 🚀 Exemplo Completo de Uso

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Configuração do Mailer
builder.Services.AddMailer(builder.Configuration, options =>
{
    options.TemplateRootType = typeof(Program); // Importante: define onde estão os templates
    options.DevelopmentMode = builder.Environment.IsDevelopment();
});

// Provedor de e-mail
builder.Services.AddMailerWithSendGrid(builder.Configuration);

// Seu serviço personalizado
builder.Services.AddScoped<EmailService>();

// Outros serviços...
builder.Services.AddControllers();

var app = builder.Build();

// Pipeline...
app.MapControllers();
app.Run();
```

## 🔧 Configurações Avançadas

### **Performance e Cache**
```csharp
builder.Services.AddMailer(builder.Configuration, options =>
{
    // Cache de templates mais agressivo
    options.TemplateCacheSize = 500;
    options.TemplateCacheLifetimeMinutes = 120;
    
    // Timeouts otimizados
    options.TemplateRenderTimeoutSeconds = 15;
    options.EmailSendTimeoutSeconds = 30;
});
```

### **Ambiente de Desenvolvimento**
```csharp
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddMailer(builder.Configuration, options =>
    {
        options.DevelopmentMode = true;
        options.RazorLight.EnableHotReload = true; // Recarrega templates automaticamente
        options.EnableDistributedTracing = false; // Menos overhead em dev
    });
}
```

### **Configuração de Retry**
```csharp
builder.Services.AddMailer(builder.Configuration, options =>
{
    options.MaxRetryAttempts = 5;
    options.RetryBaseDelayMs = 1000; // 1s, 2s, 4s, 8s, 16s
});
```

## 📊 Monitoramento e Observabilidade

### **Health Checks**
```csharp
// Automaticamente adicionado quando usar AddMailer()
app.MapHealthChecks("/health");
```

### **Métricas e Tracing**
A biblioteca automaticamente gera:
- **Traces**: Para cada envio de email
- **Métricas**: Contadores de emails enviados/falhados
- **Logs estruturados**: Para debugging e auditoria

## 🚨 Tratamento de Erros

```csharp
try
{
    var response = await _emailService.Welcome(model);
    
    if (!response.Sent)
    {
        // Email não foi enviado
        _logger.LogWarning("Falha no envio: {Errors}", 
            string.Join(", ", response.ErrorMessages));
    }
}
catch (TemplateNotFoundException ex)
{
    _logger.LogError("Template não encontrado: {Template}", ex.TemplateName);
}
catch (EmailProviderException ex)
{
    _logger.LogError("Erro do provedor de email: {Error}", ex.Message);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Erro inesperado ao enviar email");
}
```

## 📋 Configurações Disponíveis

| Configuração | Descrição | Padrão |
|-------------|-----------|---------|
| `TemplateRootType` | Tipo raiz para templates embarcados | `null` |
| `TemplateRenderTimeoutSeconds` | Timeout para renderização | `30` |
| `EmailSendTimeoutSeconds` | Timeout para envio | `60` |
| `EnableTemplateCache` | Habilita cache de templates | `true` |
| `TemplateCacheSize` | Tamanho do cache | `100` |
| `MaxRetryAttempts` | Tentativas de reenvio | `3` |
| `RetryBaseDelayMs` | Delay base para retry | `1000` |
| `EnableDistributedTracing` | Habilita tracing | `true` |
| `DevelopmentMode` | Modo de desenvolvimento | `false` |

## 🔧 Dicas e Boas Práticas

### ✅ **Do's (Faça)**
- ✅ Sempre herde de `MailerService` para sua classe personalizada
- ✅ Crie modelos específicos herdando de `MailerModelBase`
- ✅ Use templates Razor organizados em pastas
- ✅ Configure `TemplateRootType` no DI
- ✅ Trate exceções ao enviar e-mails
- ✅ Use logging para auditoria

### ❌ **Don'ts (Não Faça)**
- ❌ Não use `IMailerService` diretamente nos controllers
- ❌ Não esqueça de configurar templates como Embedded Resources
- ❌ Não exponha erros de envio para o usuário final
- ❌ Não envie e-mails sem validação dos dados

## 📚 Próximos Passos

1. **Templates Avançados**: Criar layouts responsivos
2. **Internacionalização**: Suporte a múltiplos idiomas
3. **Fila de E-mails**: Integração com background services
4. **Analytics**: Rastreamento de abertura e cliques

---

Agora você tem tudo o que precisa para implementar um sistema robusto de envio de e-mails! 🎉
