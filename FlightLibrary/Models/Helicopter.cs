using System;
using System.Threading;

namespace FlightLibrary.Models
{
    public class Helicopter : FlyingVehicle
    {

        public Helicopter() : base()
        {
        }

        public override bool Takeoff()
        {
            RaiseTakeoffStarted($"[{Name}] Запуск винтов...");

            Thread.Sleep(1000);
            Height = 200;

            RaiseTakeoffCompleted($"[{Name}] Зависание на высоте {Height}м.");

            return Height > 0;
        }

        public override void Land()
        {
            RaiseLandingStarted($"[{Name}] Снижение...");

            Thread.Sleep(1000);
            Height = 0;

            RaiseLandingCompleted($"[{Name}] Приземлился на площадку.");
        }
    }
}