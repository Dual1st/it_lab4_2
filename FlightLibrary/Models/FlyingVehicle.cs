using System;

namespace FlightLibrary.Models
{
    public abstract class FlyingVehicle : IFlyingVehicle
    {
        public event EventHandler<string>? TakeoffStarted;
        public event EventHandler<string>? TakeoffCompleted;
        public event EventHandler<string>? LandingStarted;
        public event EventHandler<string>? LandingCompleted;

        private double _height;

        public double Height
        {
            get => _height;
            protected set
            {
                _height = value;
                OnHeightChanged();
            }
        }

        public string Name { get; set; } = string.Empty;

        public FlyingVehicle()
        {
            Height = 0;
        }

        public abstract bool Takeoff();
        public abstract void Land();

        protected virtual void OnHeightChanged()
        {
        }

        protected void RaiseTakeoffStarted(string message)
        {
            TakeoffStarted?.Invoke(this, message);
        }

        protected void RaiseTakeoffCompleted(string message)
        {
            TakeoffCompleted?.Invoke(this, message);
        }

        protected void RaiseLandingStarted(string message)
        {
            LandingStarted?.Invoke(this, message);
        }

        protected void RaiseLandingCompleted(string message)
        {
            LandingCompleted?.Invoke(this, message);
        }
    }
}