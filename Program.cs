using restaurante_web_app.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//añadir la cadena de conexion de la base de datos
builder.Services.AddDbContext<GoeatContext>(opt => opt.UseNpgsql
(builder.Configuration.GetConnectionString("cadenaPgSQL")));

//ignorar las referecias ciclicas de los resultados JSON
builder.Services.AddControllers().AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

//añadir las reglas de CORS para permitir el uso de la API en cualquier dominio
var reglasCors = "ReglasCors";
//builder.Services.AddCors(opt =>
//{
//    opt.AddPolicy(name: reglasCors, builder =>
//    {
//        builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
//    });
//});

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: reglasCors,
    builder =>
    {
        builder.WithOrigins("*")
        .AllowAnyHeader()
        .AllowAnyMethod(); ;
    });
});

//añadir los servicios de JWT Bearer
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"]
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


//activar las reglas CORS que hemos creado
app.UseCors(reglasCors);

//middleware para autenticar a los usuarios segun el rol
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
