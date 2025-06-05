using Bcommerce.Api.Configurations;

var builder = WebApplication.CreateBuilder(args);

// ====================
// CONFIGURAÇÃO DE SERVIÇOS
// ====================


builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // // Serviços da camada de infraestrutura e aplicação
    // services.AddMvc(options => options.Filters.Add(typeof(ExceptionFilter)));
    // services.AddInfrastructure(builder.Configuration);
    // services.AddApplication(builder.Configuration);
    // services.AddScoped<ITokenProvider, HttpContextTokenValue>();
    // services.AddHttpContextAccessor();
    // // Serviços MVC e Swagger
    // services.AddControllers();
    // services.AddEndpointsApiExplorer();
    // services.AddSwaggerGen();
    // // Configuração de CORS
    // services.AddCors(options =>
    // {
    //     options.AddPolicy("AllowFrontend", policy =>
    //     {
    //         policy.WithOrigins("http://localhost:3000")
    //             .AllowAnyHeader()
    //             .AllowAnyMethod();
    //         // .AllowCredentials(); // Descomente se usar autenticação via cookies
    //     });
    // });
    // // Configuração global do Dapper
    // Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
}

void ConfigureMiddleware(WebApplication app)
{
    // // Aplicação da política de CORS
    // app.UseCors("AllowFrontend");
    //
    // if (app.Environment.IsDevelopment())
    // {
    //     app.UseSwagger();
    //     app.UseSwaggerUI();
    // }
    //
    // app.UseHttpsRedirection();
    // app.UseAuthorization();
    // app.MapControllers();
}

