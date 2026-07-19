namespace Domain.Enums;

public enum CancellationReasonType
{
    AdminCancelled = 1,
    PassengerCancelled = 2,
    PaymentFailed = 3,
    Weather = 4,
    ScheduleChanged = 5,
    Expired = 6
}
