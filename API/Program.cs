using API.Extensions;
using API.Helpers.Errors;
using AspNetCoreRateLimit;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

//Configurar Serilog
var logger  = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
#if DEBUG   
    .WriteTo.Console() //IsDevelopment activar para loggear en txt y consola
#endif
    .CreateLogger();

//Desactiva los logger por defecto (no se muestran los logs de la consola)
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

builder.Services.AddAutoMapper(Assembly.GetEntryAssembly()); //Configurar automapper

builder.Services.ConfigureRateLimit(); //Configure RateLimit desde ApplicationServiceExtension

builder.Services.ConfigureApiVersioning(); //Configurar versionado de API desde ApplicationServiceExtension

builder.Services.AddJwt(builder.Configuration); //Configurar JWT desde ApplicationServiceExtension

// Add services to the container.
builder.Services.ConfigureCors(); //agregar la extension Cors desde ApplicationServiceExtension

builder.Services.AddAplicacionServices(); //agregar los servicios/repositorios

//builder.Services.AddControllers();

//Agregar opciones de configuracion (Formatos de salida/respuesta) por defecto es JSON 
builder.Services.AddControllers(options =>
{
    options.RespectBrowserAcceptHeader= true; //Aceptar negociacion de formatos con el header "Accept"
    options.ReturnHttpNotAcceptable = true; //Para retornar un error Http 406 si el formato no esta configurado o es valido
}).AddXmlSerializerFormatters(); //Formato XML

//Agregar el servicio de errores personalizados del modelState
builder.Services.AddValidationErrors();

builder.Services.AddDbContext<TiendaContext>(options =>
{
    var serverVersion = new MySqlServerVersion(new Version(8,0,31));
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), serverVersion);
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//Agregar el middleware de excepcion personalizado despues de build
app.UseMiddleware<ExceptionMiddleware>();

//Agregar paginas de error personalizadas https://andrewlock.net/re-execute-the-middleware-pipeline-with-the-statuscodepages-middleware-to-create-custom-error-pages/
app.UseStatusCodePagesWithReExecute("/errors/{0}"); //ErrorsController

app.UseIpRateLimiting(); //agregar el middleware RateLimit

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Configure migrations automaticamente (Para no estar tecleando el comando update-migration en pmc)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
	
	try
	{
		var context = services.GetRequiredService<TiendaContext>();
		await context.Database.MigrateAsync();
        //TiendaContextSeed
        await TiendaContextSeed.SeedAsync(context,loggerFactory);
        await TiendaContextSeed.SeedRolesAsync(context, loggerFactory);
    }
    catch (Exception ex)
	{
		var _logger = loggerFactory.CreateLogger<Program>();
		_logger.LogError(ex, "Ocurrió un erro durante la migración");
	}
}

app.UseCors("CorsPolicy");

app.UseHttpsRedirection();

//Agregar el moddleware de Autenticacion (siempre debe ir antes del de autorizacion)
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
