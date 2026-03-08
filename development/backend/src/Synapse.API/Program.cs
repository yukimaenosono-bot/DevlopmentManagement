using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using Synapse.Application;
using Synapse.Infrastructure;
using Synapse.Infrastructure.Identity;
using Synapse.Infrastructure.Persistence;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ──
    builder.Host.UseSerilog((ctx, cfg) =>
        cfg.ReadFrom.Configuration(ctx.Configuration)
           .WriteTo.Console());

    // ── Application（MediatRハンドラ等）──
    builder.Services.AddApplication();

    // ── Infrastructure（DB・Identity等）──
    builder.Services.AddInfrastructure(builder.Configuration);

    // ── JWT認証 ──
    var secretKey = builder.Configuration["Jwt:SecretKey"]
        ?? throw new InvalidOperationException("Jwt:SecretKey が設定されていません");

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateIssuer = false,
                ValidateAudience = false,
            };
        });

    builder.Services.AddAuthorization();

    // ── CORS（開発時はフロントエンドからのアクセスを許可）──
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("DevFrontend", policy =>
            policy.WithOrigins("http://localhost:3000")
                  .AllowAnyHeader()
                  .AllowAnyMethod());
    });

    // ── API ──
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(
                new System.Text.Json.Serialization.JsonStringEnumConverter(
                    System.Text.Json.JsonNamingPolicy.CamelCase));
        });
    // ドキュメント名を "openapi" にすることで openapi.json として出力される
    builder.Services.AddOpenApi("openapi");

    var app = builder.Build();

    // ── DBマイグレーション & 初期ユーザー作成 ──
    await InitializeDatabaseAsync(app);

    // ── ミドルウェア ──
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    app.UseCors("DevFrontend");
    app.UseSerilogRequestLogging();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "アプリケーションの起動に失敗しました");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

return 0;

// ── DBの初期化（マイグレーション適用 + 管理者ユーザー作成）──
static async Task InitializeDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
    const string adminUserName = "admin";
    const string adminPassword = "Admin1234";

    if (await userManager.FindByNameAsync(adminUserName) == null)
    {
        var admin = new AppUser { UserName = adminUserName, DisplayName = "管理者" };
        await userManager.CreateAsync(admin, adminPassword);
        Log.Information("初期管理者ユーザーを作成しました: {UserName}", adminUserName);
    }
}
