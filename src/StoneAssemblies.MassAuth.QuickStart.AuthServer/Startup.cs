namespace StoneAssemblies.MassAuth.QuickStart.AuthServer
{
    using GreenPipes;

    using MassTransit;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    using Serilog;

    using StoneAssemblies.Extensibility.Extensions;
    using StoneAssemblies.MassAuth.Hosting.Extensions;
    using StoneAssemblies.MassAuth.Messages.Extensions;
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
            }

            app.UseRouting();

            app.UseEndpoints(
                endpoints => { endpoints.MapGet("/", async context => { await context.Response.WriteAsync("Hello World!"); }); });
        }

        public void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddExtensions(this.Configuration);
            serviceCollection.AddRules();

            var username = this.Configuration.GetSection("RabbitMQ")?["Username"] ?? "queuedemo";
            var password = this.Configuration.GetSection("RabbitMQ")?["Password"] ?? "queuedemo";
            var messageQueueAddress = this.Configuration.GetSection("RabbitMQ")?["Address"] ?? "rabbitmq://localhost";

            serviceCollection.AddMassTransit(
                sc =>
                    {
                        sc.AddAuthorizationRequestConsumers();

                        Log.Information("Connecting to message queue server with address '{ServiceAddress}'", messageQueueAddress);

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

                                        sc.ConfigureAuthorizationRequestConsumers(
                                            (messagesType, consumerType) =>
                                                {
                                                    cfg.DefaultReceiveEndpoint(
                                                        messagesType,
                                                        e =>
                                                            {
                                                                e.PrefetchCount = 16;
                                                                e.UseMessageRetry(x => x.Interval(2, 100));
                                                                e.ConfigureConsumer(context, consumerType);
                                                            });
                                                });
                                    }));
                    });

            serviceCollection.AddHostedService<BusHostedService>();
        }
    }
}