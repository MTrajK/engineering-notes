using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WorkshopBooking.Application.Persistence;

namespace WorkshopBooking.Persistence;

public static class PersistenceServices
{
    public static IServiceCollection AddBookingPersistence(this IServiceCollection services)
    {
        services.AddScoped<WorkshopBooking.Application.Workshops.IWorkshopStore, WorkshopStore>();
        services.AddScoped<WorkshopBooking.Application.Workshops.WorkshopService>();
        services.AddScoped<WorkshopBooking.Application.Participants.IParticipantStore, ParticipantStore>();
        services.AddScoped<WorkshopBooking.Application.Participants.ParticipantService>();
        services.AddScoped<WorkshopBooking.Application.Registrations.IRegistrationStore, RegistrationStore>();
        services.AddScoped<WorkshopBooking.Application.Registrations.RegistrationService>();
        services.AddSingleton<BookingDatabase>();
        services.AddSingleton(new DatabaseInitializationOptions());
        services.AddSingleton<IMutationCoordinator, MutationCoordinator>();
        services.AddSingleton(TimeProvider.System);
        services.AddDbContext<WorkshopBookingContext>((provider, options) => options.UseSqlite(provider.GetRequiredService<BookingDatabase>().Connection));
        return services;
    }

    public static async Task InitializeBookingDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using AsyncServiceScope scope = services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<BookingDatabase>().InitializeAsync(
            scope.ServiceProvider.GetRequiredService<WorkshopBookingContext>(),
            scope.ServiceProvider.GetRequiredService<TimeProvider>(),
            scope.ServiceProvider.GetRequiredService<DatabaseInitializationOptions>(), cancellationToken);
    }
}
