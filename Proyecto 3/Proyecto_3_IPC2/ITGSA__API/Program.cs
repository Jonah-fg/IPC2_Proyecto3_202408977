using ITGSA.API.Services;
using ITGSA__API.Servicios;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddXmlSerializerFormatters().ConfigureApiBehaviorOptions(options =>
{
        options.SuppressMapClientErrors=true;
});

builder.Services.Configure<MvcOptions>(options =>
{
    options.Filters.Add(new ProducesAttribute("application/xml"));
    options.Filters.Add(new ConsumesAttribute("application/xml"));
});

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton<AlmacenamientoXml>();
builder.Services.AddTransient<AplicadorPagos>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
