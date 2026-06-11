namespace FlightLibrary.Models
{
    public interface IFlyingVehicle
    {
        string Name { get; set; }
        double Height { get; }
        bool Takeoff();
        void Land();
    }
}