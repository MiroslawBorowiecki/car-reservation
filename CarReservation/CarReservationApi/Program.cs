using CarReservationApi.Cars;
using CarReservationApi.Cars.Persistence;
using CarReservationApi.Reservations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services
    .AddSingleton<CarService>()
    .AddSingleton<CarRepository>()
    .AddSingleton<ReservationRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }