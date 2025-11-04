using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ✅ Habilitar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodo", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Controladores
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Cine Grupo 9 API",
        Version = "v1",
        Description = "API para las consultas del proyecto de Cine del Grupo 9"
    });
});

var app = builder.Build();

// ✅ Activar CORS antes de los controladores
app.UseCors("PermitirTodo");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cine Grupo 9 API v1");
        c.RoutePrefix = string.Empty; // Swagger en raíz
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
