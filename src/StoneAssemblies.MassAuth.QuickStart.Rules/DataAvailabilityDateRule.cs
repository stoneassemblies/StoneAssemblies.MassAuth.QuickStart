namespace StoneAssemblies.MassAuth.QuickStart.Rules
{
    using System;
    using System.Threading.Tasks;

    using StoneAssemblies.MassAuth.Messages;
    using StoneAssemblies.MassAuth.QuickStart.Messages;
    using StoneAssemblies.MassAuth.Rules.Interfaces;

    public class DataAvailabilityDateRule : IRule<AuthorizationRequestMessage<WeatherForecastRequestMessage>>
    {
        public bool IsEnabled { get; } = true;

        public string Name { get; } = "Data availability date";

        public int Priority { get; }

        public async Task<bool> EvaluateAsync(AuthorizationRequestMessage<WeatherForecastRequestMessage> message)
        {
            var days = message.Payload.StartDate.Subtract(DateTime.Now).TotalDays;
            return days >= 0 && days < 10;
        }
    }
}