﻿using CarReservationApi.Cars;
using CarReservationApi.Cars.Persistence;
using CarReservationApi.Http;
using CarReservationApi.Reservations.Persistence;

namespace CarReservationApi.Reservations;

public class ReservationService
{
    private readonly ReservationRepository _reservationRepository;
    private readonly CarRepository _carRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ReservationService(
        ReservationRepository reservationRepository,
        CarRepository carRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _reservationRepository = reservationRepository;
        _carRepository = carRepository;
        _dateTimeProvider = dateTimeProvider;
    }
    
    public ReservationResponse? ReserveCar(ReservationRequest request)
    {
        ReservationValidator.Validate(request, _dateTimeProvider.Now);

        if (_carRepository.Count == 0) return null;

        var availableCar = FindAvailableCar(request);

        if (availableCar == null) return null;

        var response = ReservationResponse.Create(request, availableCar);
        _reservationRepository.Add(response);
        return response;
    }

    public List<ReservationResponse> GetAll() 
        => _reservationRepository.Where(r => r.Time > _dateTimeProvider.Now).ToList();

    internal bool CarHasUpcomingOrOngoingReservation(string carId) => _reservationRepository
        .Any(r => r.Time + r.Duration > _dateTimeProvider.Now && r.Car.Id == carId);

    private Car? FindAvailableCar(ReservationRequest request)
    {
        DateTime? requestEnd = request.Time + request.Duration;

        // Assuming no 'break' between reservations is needed. Consult domain experts.
        var conflictingCarReservations =
            from reservation in _reservationRepository
            let reservationEnd = reservation.Time + reservation.Duration
            // Requested reservation would end during an existing one.
            where (reservation.Time < requestEnd && reservationEnd >= requestEnd)
            // Requested reservation would encompass an existing one.
            || (reservation.Time >= request.Time && reservationEnd <= requestEnd)
            // Requested reservation would start during an existing one.
            || (reservation.Time <= request.Time && reservationEnd > request.Time)
            select reservation.Car;

        return _carRepository.Values
            .Except(conflictingCarReservations, CarComparer.That)
            .FirstOrDefault();
    }
}
