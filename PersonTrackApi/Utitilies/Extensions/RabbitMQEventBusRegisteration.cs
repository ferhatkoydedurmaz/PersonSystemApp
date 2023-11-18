using PersonTrackApi.Consumer;
using System.Diagnostics.CodeAnalysis;

namespace PersonTrackApi.Utitilies.Extensions;
public static class RabbitMQEventBusRegisteration
{
    public static PersonMovementReportEventBusConsumer Listener { get; set; }

    public static IApplicationBuilder UseEventBusListener(this IApplicationBuilder app)
    {
        Listener = app.ApplicationServices.GetService<PersonMovementReportEventBusConsumer>();
        var life = app.ApplicationServices.GetService<IHostApplicationLifetime>();

        life.ApplicationStarted.Register(OnStarted);
        life.ApplicationStopping.Register(OnStopping);
        return app;
    }

    private static void OnStarted()
    {
        Listener.Consume();
    }
    private static void OnStopping()
    {
        Listener.Disconnect();
    }
}
