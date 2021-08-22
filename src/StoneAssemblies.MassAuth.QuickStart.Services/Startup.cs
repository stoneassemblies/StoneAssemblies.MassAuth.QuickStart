namespace StoneAssemblies.MassAuth.QuickStart.Services
{
    using System;

    using MassTransit;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.OpenApi.Models;

    using StoneAssemblies.MassAuth.Messages;
    using StoneAssemblies.MassAuth.Messages.Extensions;
    using StoneAssemblies.MassAuth.QuickStart.Messages;
    using StoneAssemblies.MassAuth.Services;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(
                    c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "StoneAssemblies.MassAuth.QuickStart.Services v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(
                c =>
                    {
                        c.SwaggerDoc(
                            "v1",
                            new OpenApiInfo
                                {
                                    Title = "StoneAssemblies.MassAuth.QuickStart.Services",
                                    Version = "v1"
                                });
                    });

            services.AddScoped<AuthorizeByRuleFilter>();

            var username = this.Configuration.GetSection("RabbitMQ")?["Username"] ?? "queuedemo";
            var password = this.Configuration.GetSection("RabbitMQ")?["Password"] ?? "queuedemo";
            var messageQueueAddress = this.Configuration.GetSection("RabbitMQ")?["Address"] ?? "rabbitmq://localhost";

            services.AddMassTransit(
                sc =>
                    {
                        sc.AddBus(
                            context => Bus.Factory.CreateUsingRabbitMq(
                                cfg =>
                                    {
                                        cfg.Host(
                                            messageQueueAddress,
                                            configurator =>
                                                {
                                                    configurator.Username(username);
                                                    configurator.Password(password);
                                                });
                                    }));

                        var authorizationRequestQueueName =
                            $"queue:{typeof(AuthorizationRequestMessage<WeatherForecastRequestMessage>).GetFlatName()}";
                        sc.AddRequestClient<AuthorizationRequestMessage<WeatherForecastRequestMessage>>(
                            new Uri(authorizationRequestQueueName));
                    });

            services.AddHostedService<BusHostedService>();
        }
    }
}