namespace StoneAssemblies.MassAuth.QuickStart.Messages
{
    using System;

    using StoneAssemblies.MassAuth.Messages;

    public class WeatherForecastRequestMessage : MessageBase
    {
        public DateTime StartDate { get; set; }
    }
}