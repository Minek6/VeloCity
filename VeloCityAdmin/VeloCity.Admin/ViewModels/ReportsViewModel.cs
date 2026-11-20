using VeloCity.Admin.Models.Dto;
using VeloCity.Admin.Services;

namespace VeloCity.Admin.ViewModels;

public sealed class ReportsViewModel(
    DashboardApiService apiService,
    TicketTypesApiService ticketTypesApiService)
{
    public List<DailyTicketSalesDto> TicketSales
        { get; private set; } = [];

    public int SelectedPeriod { get; private set; } = 7;

    public bool IsLoading { get; private set; }

    public string? ErrorMessage { get; private set; }

    public decimal TotalSales =>
        TicketSales.Sum(x => x.TotalAmount);

    public int TicketsSold =>
        TicketSales.Sum(x => x.TicketsSold);

    public decimal AverageTicketPrice =>
        TicketsSold > 0
            ? TotalSales / TicketsSold
            : 0m;

    private static DateOnly WarsawToday()
        {
            var zone = TimeZoneInfo.FindSystemTimeZoneById(
                "Europe/Warsaw");

            var now = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow, zone);

            return DateOnly.FromDateTime(now);
        }

    public int? TicketTypeIdFilter { get; set; }

    public string ValidationStatusFilter { get; set; } = "all";

    public bool IsCustomPeriod { get; private set; }

    private int? _appliedTicketTypeId;
    private bool? _appliedIsValidated;

    public DateOnly FromDate { get; set; } = WarsawToday().AddDays(-6);
    public DateOnly ToDate { get; set; } = WarsawToday();

    private DateOnly _appliedFrom = WarsawToday().AddDays(-6);
    private DateOnly _appliedTo = WarsawToday();

    public List<TicketTypeDto> TicketTypes { get; private set; } = [];

    public bool IsLoadingTicketTypes { get; private set; }

    public string? TicketTypesErrorMessage { get; private set; }

    public List<TicketTypeSalesDto> TicketTypeRanking
        { get; private set; } = [];

    public bool IsLoadingRanking { get; private set; }

    public string? RankingErrorMessage { get; private set; }

    
    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            TicketSales = await apiService.GetTicketSalesReportAsync(
                _appliedFrom,
                _appliedTo,
                _appliedTicketTypeId,
                _appliedIsValidated);
        }
        catch (Exception ex)
        {
            TicketSales = [];

            ErrorMessage =
                "Nie udało się pobrać raportu sprzedaży.";

            Console.WriteLine($"[ReportsViewModel] {ex}");
        }
        finally
        {
            IsLoading = false;
        }
        await LoadRankingAsync();
    }

    public async Task ChangePeriodAsync(int days)
    {
        if (days is not (7 or 30))
            return;

        SelectedPeriod = days;
        IsCustomPeriod = false;

        ToDate = WarsawToday();
        FromDate = ToDate.AddDays(-(days - 1));

        await ApplyFiltersAsync();
    }

    public void SelectCustomPeriod()
    {
        IsCustomPeriod = true;
    }

    public async Task ApplyFiltersAsync()
    {
        ErrorMessage = null;

        if (FromDate > ToDate)
        {
            ErrorMessage =
                "Data początkowa nie może być późniejsza niż końcowa.";
            return;
        }

        if (ToDate.DayNumber - FromDate.DayNumber > 365)
        {
            ErrorMessage =
                "Maksymalny zakres raportu to 366 dni.";
            return;
        }

        if (ToDate == DateOnly.MaxValue)
        {
            ErrorMessage =
                "Nieprawidłowa data końcowa.";
            return;
        }

        if (TicketTypeIdFilter is <= 0)
        {
            ErrorMessage =
                "ID rodzaju biletu musi być większe od zera.";
            return;
        }

        _appliedFrom = FromDate;
        _appliedTo = ToDate;

        _appliedTicketTypeId = TicketTypeIdFilter;

        _appliedIsValidated = ValidationStatusFilter switch
        {
            "validated" => true,
            "not-validated" => false,
            _ => null
        };

        await LoadAsync();
    }

    public async Task LoadTicketTypesAsync()
    {
        IsLoadingTicketTypes = true;
        TicketTypesErrorMessage = null;

        try
        {
            TicketTypes = await ticketTypesApiService.GetAllAsync();
        }
        catch (Exception ex)
        {
            TicketTypes = [];

            TicketTypesErrorMessage =
                "Nie udało się pobrać rodzajów biletów.";

            Console.WriteLine($"[ReportsViewModel] {ex}");
        }
        finally
        {
            IsLoadingTicketTypes = false;
        }
    }

    public async Task ClearFiltersAsync()
    {
        SelectedPeriod = 7;
        IsCustomPeriod = false;

        ToDate = WarsawToday();
        FromDate = ToDate.AddDays(-6);

        TicketTypeIdFilter = null;
        ValidationStatusFilter = "all";

        await ApplyFiltersAsync();
    }

    public async Task LoadRankingAsync()
    {
        IsLoadingRanking = true;
        RankingErrorMessage = null;

        try
        {
            TicketTypeRanking =
                await apiService.GetTicketTypeRankingAsync(
                    _appliedFrom,
                    _appliedTo,
                    _appliedTicketTypeId,
                    _appliedIsValidated);
        }
        catch (Exception ex)
        {
            TicketTypeRanking = [];
            RankingErrorMessage =
                "Nie udało się pobrać rankingu biletów.";

            Console.WriteLine($"[ReportsViewModel] {ex}");
        }
        finally
        {
            IsLoadingRanking = false;
        }
    }

}
