using Azure.Identity;
using AzureAIServicesDemo.Components;
using AzureAIServicesDemo.Services;
using Microsoft.Extensions.Azure;

var builder = WebApplication.CreateBuilder(args);

string? cs = Environment.GetEnvironmentVariable("RecExtConfigConnectionString");
builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect(cs)
            .ConfigureKeyVault(kv =>
            {
                kv.SetCredential(new DefaultAzureCredential());
            });
});

builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddImageAnalysisClient(new Uri(builder.Configuration["aiServicesEndpoint"] ?? ""),
        new Azure.AzureKeyCredential(builder.Configuration["aiServicesKey"] ?? ""));
    clientBuilder.AddConversationAnalysisClient(new Uri(builder.Configuration["languageEndpoint"] ?? ""),
        new Azure.AzureKeyCredential(builder.Configuration["languageKey"] ?? ""));
    clientBuilder.AddTextTranslationClient(new Azure.AzureKeyCredential(builder.Configuration["translatorKey"] ?? ""),
        "eastus");
    clientBuilder.AddTextAnalyticsClient(new Uri(builder.Configuration["aiServicesEndpoint"] ?? ""),
        new Azure.AzureKeyCredential(builder.Configuration["aiServicesKey"] ?? ""));
    clientBuilder.AddContentSafetyClient(new Uri(builder.Configuration["contentSafetyEndpoint"] ?? ""),
        new Azure.AzureKeyCredential(builder.Configuration["contentSafetyKey"] ?? ""));
    clientBuilder.AddBlocklistClient(new Uri(builder.Configuration["contentSafetyEndpoint"] ?? ""),
        new Azure.AzureKeyCredential(builder.Configuration["contentSafetyKey"] ?? ""));
    clientBuilder.AddDocumentIntelligenceClient(new Uri(builder.Configuration["aiServicesEndpoint"] ?? ""),
        new Azure.AzureKeyCredential(builder.Configuration["aiServicesKey"] ?? ""));
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddTransient<SpeechService>();
builder.Services.AddTransient<LanguageService>();
builder.Services.AddTransient<ImageService>();
builder.Services.AddTransient<ContentSafetyService>();
builder.Services.AddTransient<TranslatorService>();
builder.Services.AddTransient<DocumentIntelligenceService>();
builder.Services.AddBlazorBootstrap();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
