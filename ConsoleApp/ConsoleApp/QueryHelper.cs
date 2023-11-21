using ConsoleApp.Model;
using ConsoleApp.Model.Enum;
using ConsoleApp.OutputTypes;

namespace ConsoleApp;

public class QueryHelper : IQueryHelper
{
    /// <summary>
    /// Get Deliveries that has payed
    /// </summary>
    public IEnumerable<Delivery> Paid(IEnumerable<Delivery> deliveries) => 
        deliveries.Where(delivery => delivery.Status == DeliveryStatus.Confirmed);

    /// <summary>
    /// Get Deliveries that now processing by system (not Canceled or Done)
    /// </summary>
    
    public IEnumerable<Delivery> NotFinished(IEnumerable<Delivery> deliveries) =>
        deliveries.Where(delivery => delivery.Status != DeliveryStatus.Cancelled && delivery.Status != DeliveryStatus.Done);

    /// <summary>
    /// Get DeliveriesShortInfo from deliveries of specified client
    /// </summary>
    public IEnumerable<DeliveryShortInfo> DeliveryInfosByClient(IEnumerable<Delivery> deliveries, string clientId) =>
        deliveries
            .Where(delivery => delivery.ClientId == clientId)
            .Select(delivery => new DeliveryShortInfo
            {
                Id = delivery.Id,
                StartCity = delivery.Direction.Origin,
                EndCity = delivery.Direction.Destination,
                ClientId = delivery.ClientId,
                Type = delivery.Type,
                LoadingPeriod = delivery.LoadingPeriod,
                ArrivalPeriod = delivery.ArrivalPeriod,
                Status = delivery.Status,
                CargoType = delivery.CargoType
            });
    
    /// <summary>
    /// Get first ten Deliveries that starts at specified city and have specified type
    /// </summary>
    public IEnumerable<Delivery> DeliveriesByCityAndType(IEnumerable<Delivery> deliveries, string cityName, DeliveryType type) => new List<Delivery>();//TODO: Завдання 4
    
    /// <summary>
    /// Order deliveries by status, then by start of loading period
    /// </summary>
    
    public IEnumerable<Delivery> OrderByStatusThenByStartLoading(IEnumerable<Delivery> deliveries) =>
        deliveries
            .OrderBy(delivery => delivery.Status)
            .ThenBy(delivery => delivery.LoadingPeriod);

    /// <summary>
    /// Count unique cargo types
    /// </summary>
    public int CountUniqCargoTypes(IEnumerable<Delivery> deliveries) =>
        deliveries.Select(delivery => delivery.CargoType).Distinct().Count();
    
    /// <summary>
    /// Group deliveries by status and count deliveries in each group
    /// </summary>
    public Dictionary<DeliveryStatus, int> CountsByDeliveryStatus(IEnumerable<Delivery> deliveries) =>
        deliveries
            .GroupBy(delivery => delivery.Status)
            .ToDictionary(group => group.Key, group => group.Count());
    
    /// <summary>
    /// Group deliveries by start-end city pairs and calculate average gap between end of loading period and start of arrival period (calculate in minutes)
    /// </summary>
    public IEnumerable<AverageGapsInfo> AverageTravelTimePerDirection(IEnumerable<Delivery> deliveries) =>
        deliveries.GroupBy(delivery => new { delivery.Direction.Origin, delivery.Direction.Destination })
            .Select(group => new AverageGapsInfo
            {
                StartCity = group.Key.Origin,
                EndCity = group.Key.Destination,
                AverageGap = group.Average(delivery =>
                {
                    DateTime start = delivery.LoadingPeriod.Start.GetValueOrDefault();
                    DateTime end = delivery.ArrivalPeriod.End.GetValueOrDefault();

                    if (start != default && end != default)
                    {
                        return (end - start).TotalMinutes;
                    }

                    return 0.0; 
                })
            });
    
    /// <summary>
    /// Paging helper
    /// </summary>
   public IEnumerable<TElement> Paging<TElement, TOrderingKey>(IEnumerable<TElement> elements,
        Func<TElement, TOrderingKey> ordering,
        Func<TElement, bool>? filter = null,
        int countOnPage = 100,
        int pageNumber = 1)
    {
        var filteredElements = filter != null ? elements.Where(filter) : elements;

        return filteredElements
            .OrderBy(ordering)
            .Skip((pageNumber - 1) * countOnPage)
            .Take(countOnPage);
    }
}
