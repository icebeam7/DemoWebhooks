var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var gitHubWebhookSecret = builder.Configuration["GITHUB_SECRET"];



var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/api/issues",
    async (HttpContext context, HttpResponse response) =>
    {
        var headers = context.Request.Headers;

        headers.TryGetValue("X-GitHub-Delivery", out StringValues gitHubDeliveryId);
        headers.TryGetValue("X-GitHub-Event", out StringValues gitHubEvent);
        headers.TryGetValue("X-Hub-Signature-256", out StringValues gitHubSignature);

        app.Logger.LogInformation($"Received GitHub delivery {gitHubDeliveryId} for event {gitHubEvent}");

        using (var reader = new StreamReader(context.Request.Body))
        {
            var data = await reader.ReadToEndAsync();

            if (GitHubWebhook.IsGitHubSignatureValid(data, 
                gitHubSignature, 
                gitHubWebhookSecret))
            {
                //return Results.Ok("works with configured secret!");

                var gitHubInfo = System.Text.Json.JsonSerializer.Deserialize<GitHubInfo>(data);

                if (gitHubInfo != null)
                {
                    var gitHubData = $"Action: {gitHubInfo.action} " +
                    $"Issue URL: {gitHubInfo.issue.url} " +
                    $"Issued by: {gitHubInfo.issue.user.login} " +
                    $"Issue Title: {gitHubInfo.issue.title} " +
                    $"Issue Content: {gitHubInfo.issue.body} " +
                    $"Issue State: {gitHubInfo.issue.state} " +
                    $"Issue Creation Date: {gitHubInfo.issue.created_at} " +
                    $"Repository: {gitHubInfo.repository.full_name} " +
                    $"Sender: {gitHubInfo.sender.login} ";

                    return Results.Ok($"{gitHubData}");
                }
            }
        }

        return Results.Unauthorized();
    })
//.Accepts<Book>("application/json")
//.Produces<Book>(StatusCodes.Status201Created)
.WithName("Issues")
.WithTags("GitHub");

app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}