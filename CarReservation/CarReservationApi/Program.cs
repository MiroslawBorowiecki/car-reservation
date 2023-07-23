using CarReservationApi;
using CarReservationApi.Cars;
using CarReservationApi.Cars.Persistence;
using CarReservationApi.Reservations;
using CarReservationApi.Reservations.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services
    .AddSingleton<CarService>()
    .AddSingleton<CarRepository>()
    .AddSingleton<ReservationService>()
    .AddSingleton<ReservationRepository>()
    .AddSingleton<IDateTimeProvider, DefaultDateTimeProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }