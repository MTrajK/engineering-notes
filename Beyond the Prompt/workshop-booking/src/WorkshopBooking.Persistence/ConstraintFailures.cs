using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WorkshopBooking.Application.Outcomes;
using WorkshopBooking.Domain;

namespace WorkshopBooking.Persistence;

public static class ConstraintFailures
{
    public static ApplicationFailure? Recognize(DbUpdateException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        // Each mapped entity has exactly one non-primary unique index.
        if (exception.InnerException is not SqliteException { SqliteExtendedErrorCode: 2067 })
        {
            return null;
        }

        if (exception.Entries.Count == 1 && exception.Entries[0].Entity is Participant)
        {
            return ApplicationFailure.Conflict("participant_email_conflict");
        }

        if (exception.Entries.Count == 1 && exception.Entries[0].Entity is Registration)
        {
            return ApplicationFailure.Conflict("registration_duplicate");
        }

        return null;
    }
}
