namespace Application.Common.Interfaces;

/// <summary>
/// Unit of Work interface to manage repositories and commit transactions.
/// </summary>
public interface IUnitOfWork
{
    IRepository<T> Repository<T>() where T : class;
    IHotelBookingRepository hotelBookingRepository { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
