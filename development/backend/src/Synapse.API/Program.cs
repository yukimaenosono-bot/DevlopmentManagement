using Scalar.AspNetCore;
using Serilog;
using Synapse.Infrastructure;

// ── ロガーの初期設定（起動時エラーを捕捉するため最初に設定）──
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

    // ── Infrastructure（DB等）──
    builder.Services.AddInfrastructure(builder.Configuration);

    // ── API ──
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            // enum を camelCase 文字列で送受信する（数値送信禁止）
            options.JsonSerializerOptions.Converters.Add(
                new System.Text.Json.Serialization.JsonStringEnumConverter(
                    System.Text.Json.JsonNamingPolicy.CamelCase));
        });
    builder.Services.AddOpenApi();

    var app = builder.Build();

    // ── ミドルウェア ──
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    app.UseSerilogRequestLogging();
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
