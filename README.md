# Car Reservation  
## The Task  
>### Car Reservation API  
>The goal of the application is to reserve cars for upcoming rides.  
>
>As a user of the system, you can add cars where each car is represented by its make,
>model and unique identifier following the pattern “C\<number\>”.  
>
>Besides adding a new car, a user can also update one, remove one, or see all the cars
>(no pagination needed).  
>
>The last step in the flow is the ability to reserve a car for a ride at a certain time and
>duration. The reservation can be taken up to 24 hours ahead and the duration can
>take up to 2 hours. The system should find an available car, store the reservation if
>possible and give back a response with the details. The user can also see all the
>upcoming reservations by calling another dedicated endpoint.  
>
>The communication is only via public API in the JSON format and the data can be
>stored just in the memory. The application doesn’t need to implement a concept of
>users (data is shared).  

Additional rules and assumptions made during the implementation:
- The duration cannot be shorter than 5 minutes.
- Only the start time has to be within the 24 hours limitation.
- Car with an ongoing or upcoming reservation cannot be updated or removed.
  - Car with only past reservations can be both removed and updated.
- Reservations can be adjacent to each other - there is no 'break' required.

## Tests  
Since this was meant to be an API project the tests are written as integration tests, excercising the endpoints directly. The API is used to setup the data, to perform the tested action and to obtain data for verification. The time source is the only internal element that was abstracted and replaced with a test implementation for the few tests needing such control.
Overall the suite of 37 tests executes in around a second on subsequent runs, which is satisfactory.  
**!! Important note !!** Finding an available car requires pulling the data first. There is a brief period, a race condition, between the pulling of this data and storing a new reservation, where another request could 'reserve' the car. It could result in violation of the invariant that reservations for one car cannot overlap.  
TO DO:
- [ ] Split larger tests to make them more concise
- [ ] Research simulating the above problem  

## Design  
The API itself is meant to be built on top of two modules - Cars and Reservation. These are inherently coupled - reservation are made for cars and cars with reservations cannot be modified or removed. Still, being two different areas these have been put into two separate folders/namespaces. Cross-reference is allowed, but preferably over the main module entrypoins, i.e. the `CarService` and `ReservationService` classes.
The model itself works, but could be further improved if needed, by turning the `Car` into an aggregate and the `Reservation` into a Value Object. Then all reservations would be attached to cars, not the other way around. Furthermore, the DTOs are used too much across the module, blurring the border between the HTTP, domain and persistence layers.
TO DO:
- [ ] Create a `Car`aggregate and `Reservation` Value Object.
- [ ] Separate out the DTOs and use C# records to implement them.

## Swagger/OpenAPI and Docker  
Support for both tools have been added to the project.
Swagger is only accessible in the Debug build, using the following URL pattern:
```
http:\\{host:port}\swagger
```
In order to build and run the Docker image do the following in the command line:
```
cd {SolutionFolder}
docker build -t car-reservation -f .\CarReservationApi\Dockerfile .
docker run -it --rm -p 5000:80 car-reservation
```