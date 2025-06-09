using Bcommerce.Api.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer; // Adicionar usings
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ====================
// CONFIGURAÇÃO DE SERVIÇOS
// ====================
ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

// ====================
// PIPELINE HTTP
// ====================
ConfigureMiddleware(app);

// ====================
// EXECUÇÃO
// ====================
app.Run();


// ====================
// MÉTODOS LOCAIS
// ====================

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    services.AddControllers();
    services.AddInfrastructure(configuration);
    services.AddApplication(configuration);
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
    services.AddCors(options => 
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins("http://localhost:3000")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            }));
    Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
    
    // ADICIONE ESTA SEÇÃO DE AUTENTICAÇÃO
    var key = Encoding.ASCII.GetBytes(configuration["Settings:JwtSettings:SigninKey"]);

    services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false; // Em produção, considere usar true
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = configuration["Settings:JwtSettings:Issuer"],
                ValidateAudience = true,
                ValidAudience = configuration["Settings:JwtSettings:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

    services.AddAuthorization(); // Adiciona o serviço de autorização
}

void ConfigureMiddleware(WebApplication app)
{
    app.UseCors("AllowFrontend");
    
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();
}



// using Bcommerce.Api.Configurations;
//
// var builder = WebApplication.CreateBuilder(args);
//
// // ====================
// // CONFIGURAÇÃO DE SERVIÇOS
// // ====================
//
// // ====================
// // PIPELINE HTTP
// // ====================
//
//
// // ====================
// // MÉTODOS LOCAIS
// // ====================
//
//
// builder.Services.AddControllers();
// builder.Services.AddInfrastructure(builder.Configuration);
// builder.Services.AddApplication(builder.Configuration);
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();
//
// var app = builder.Build();
//
// // Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }
//
// app.UseHttpsRedirection();
//
// app.UseAuthorization();
//
// app.MapControllers();
//
// app.Run();
//
// void ConfigureServices(IServiceCollection services, IConfiguration configuration)
// {
//
// }
//
// void ConfigureMiddleware(WebApplication app)
// {
//
// }
//
