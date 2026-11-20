using VeloCity.Admin.Models.Dto;
using VeloCity.Admin.Services;
using VeloCity.Admin.Models.Dashboard;

namespace VeloCity.Admin.ViewModels;

public sealed class DashboardViewModel(
    DashboardApiService apiService)
{
    public DashboardStatsDto? Stats { get; private set; }

    public List<DailyTicketSalesDto> TicketSales
        { get; private set; } = [];

    public int SelectedSalesPeriod { get; private set; } = 7;

    public bool IsLoading { get; private set; }
    public bool IsLoadingSales { get; private set; }

    public string? ErrorMessage { get; private set; }
    public string? SalesErrorMessage { get; private set; }

    public decimal MaxSalesAmount =>
        TicketSales.Count == 0
            ? 0
            : TicketSales.Max(x => x.TotalAmount);

    public decimal SalesPeriodTotal =>
        TicketSales.Sum(x => x.TotalAmount);

    public int SalesPeriodTicketCount =>
        TicketSales.Sum(x => x.TicketsSold);

    public SystemHealthDto? SystemHealth { get; private set; }

    public bool? IsBackendAvailable { get; private set; }

    public long? BackendResponseTimeMs { get; private set; }

    public bool IsLoadingHealth { get; private set; }

    public string? SystemHealthErrorMessage { get; private set; }

    public DateTimeOffset? LastHealthCheck { get; private set; }



    public IReadOnlyList<StatCarouselItem> CarouselItems
    {
        get
        {
            if (Stats is null)
                return [];

            return
            [
                new(
                    "Bilety sprzedane dzisiaj",
                    Stats.TicketsToday.ToString("N0"),
                    "Liczba biletów zakupionych dzisiaj.",
                    "/reports",
                    "Zobacz raport sprzedaży"
                ),

                new(
                    "Łączna wartość sprzedaży",
                    $"{Stats.TotalTicketSales:N2} zł",
                    "Wartość wszystkich zakupionych biletów.",
                    "/reports",
                    "Analizuj sprzedaż"
                ),

                new(
                    "Aktywni pasażerowie",
                    Stats.ActivePassengers.ToString("N0"),
                    "Liczba aktywnych kont pasażerów.",
                    "/users",
                    "Zarządzaj użytkownikami"
                ),

                new(
                    "Aktywne pojazdy",
                    Stats.ActiveVehicles.ToString("N0"),
                    "Pojazdy oznaczone jako aktywne.",
                    "/vehicles",
                    "Zarządzaj pojazdami"
                ),

                new(
                    "Sprzedane bilety",
                    Stats.SoldTickets.ToString("N0"),
                    "Łączna liczba zakupionych biletów.",
                    "/tickets",
                    "Historia sprzedanych biletów"
                ),

                new(
                    "Aktywowane bilety",
                    Stats.ValidatedTickets.ToString("N0"),
                    "Liczba biletów, które zostały aktywowane.",
                    "/tickets",
                    "Przeglądaj bilety"
                )
            ];
        }
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            Stats = await apiService.GetStatsAsync();
        }
        catch (Exception ex)
        {
            Stats = null;
            ErrorMessage = "Nie udało się pobrać statystyk.";

            Console.WriteLine($"[DashboardViewModel] {ex}");
        }
        finally
        {
            IsLoading = false;
        }

        await LoadTicketSalesAsync();
        await LoadSystemHealthAsync();
    }

    public async Task LoadTicketSalesAsync()
    {
        IsLoadingSales = true;
        SalesErrorMessage = null;

        try
        {
            TicketSales = await apiService.GetTicketSalesAsync(
                SelectedSalesPeriod);
        }
        catch (Exception ex)
        {
            TicketSales = [];
            SalesErrorMessage =
                "Nie udało się pobrać danych sprzedaży biletów.";

            Console.WriteLine($"[DashboardViewModel] {ex}");
        }
        finally
        {
            IsLoadingSales = false;
        }
    }

    public async Task ChangeSalesPeriodAsync(int days)
    {
        if (days is not (7 or 30))
            return;

        if (SelectedSalesPeriod == days)
            return;

        SelectedSalesPeriod = days;

        await LoadTicketSalesAsync();
    }

    public async Task LoadSystemHealthAsync()
    {
        if (IsLoadingHealth)
            return;

        IsLoadingHealth = true;
        SystemHealthErrorMessage = null;

        try
        {
            var result = await apiService.GetSystemHealthAsync();

            SystemHealth = result.Data;
            BackendResponseTimeMs = result.ResponseTimeMs;
            IsBackendAvailable = true;
        }
        catch (HttpRequestException ex)
            when (ex.StatusCode is
                System.Net.HttpStatusCode.Unauthorized or
                System.Net.HttpStatusCode.Forbidden)
        {
            IsBackendAvailable = null;
            SystemHealth = null;
            BackendResponseTimeMs = null;

            SystemHealthErrorMessage =
                "Nie można sprawdzić usług: brak autoryzacji administratora.";

            Console.WriteLine($"[DashboardViewModel] {ex}");
        }
        catch (Exception ex)
        {
            IsBackendAvailable = false;
            SystemHealth = null;
            BackendResponseTimeMs = null;

            SystemHealthErrorMessage =
                "Nie udało się nawiązać połączenia z API lub odczytać statusu usług.";

            Console.WriteLine($"[DashboardViewModel] {ex}");
        }
        finally
        {
            LastHealthCheck = DateTimeOffset.UtcNow;
            IsLoadingHealth = false;
        }
    }

}
