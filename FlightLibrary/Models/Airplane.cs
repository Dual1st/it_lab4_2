using System;
using System.Threading;

namespace FlightLibrary.Models
{
    public class Airplane : FlyingVehicle
    {
        public double RunwayLength { get; set; } // длина взлет полосы

        public Airplane() : base()
        {
            RunwayLength = 0;
        }

        public override bool Takeoff()
        {
            RaiseTakeoffStarted($"[{Name}] Начинаем разбег по полосе {RunwayLength}м...");

            Thread.Sleep(1000);
            Height = 500;

            RaiseTakeoffCompleted($"[{Name}] Взлёт успешен! Высота: {Height}м.");

            return Height > 0;
        }

        public override void Land()
        {
            RaiseLandingStarted($"[{Name}] Заходим на посадку...");

            Thread.Sleep(1000);
            Height = 0;

            RaiseLandingCompleted($"[{Name}] Посадка завершена. Полоса свободна.");
        }

        public void TestMethod(string message)
        {

        }
    }
}