using Bcommerce.Api.Configurations;

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
}

void ConfigureMiddleware(WebApplication app)
{
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
