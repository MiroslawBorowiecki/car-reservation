using System.Diagnostics.CodeAnalysis;

namespace CarReservationApi.Cars;

public class CarComparer : EqualityComparer<Car>
{
    public static readonly CarComparer That = new CarComparer();

    public override bool Equals(Car? x, Car? y)
    {
        if (x == null && y == null) return true;

        if (x == null || y == null) return false;

        return x.Id == y.Id && x.Make == y.Make && x.Model == y.Model;
    }

    public override int GetHashCode([DisallowNull] Car obj)
    {
        return (obj.Id + obj.Make + obj.Model).GetHashCode();
    }
}
