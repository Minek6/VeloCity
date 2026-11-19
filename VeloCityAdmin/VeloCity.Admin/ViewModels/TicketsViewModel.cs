using VeloCity.Admin.Models.Dto;
using VeloCity.Admin.Services;
using VeloCity.Admin.Models.Forms;

namespace VeloCity.Admin.ViewModels;

public sealed class TicketsViewModel(
    TicketsApiService apiService)
{
    public List<AdminTicketDto> Tickets { get; private set; } = [];

    public bool IsLoading { get; private set; }

    public string? ErrorMessage { get; private set; }

    public int PageNumber { get; private set; } = 1;

    public int PageSize { get; private set; } = 10;

    public int TotalCount { get; private set; }

    public int TotalPages { get; private set; }

    public string UserIdFilter { get; set; } = "";
    public string VehicleIdFilter { get; set; } = "";
    public string StatusFilter { get; set; } = "all";

    public string SortBy { get; set; } = "date";
    public string SortDirection { get; set; } = "desc";

    private int? _appliedUserId;
    private int? _appliedVehicleId;
    private bool? _appliedIsValidated;

    private string _appliedSortBy = "date";
    private string _appliedSortDirection = "desc";

    public AdminTicketDto? SelectedTicket { get; private set; }

    public List<TicketValidityAdjustmentDto> ValidityAdjustments
        { get; private set; } = [];

    public bool IsLoadingAdjustments { get; private set; }

    public string? AdjustmentsErrorMessage { get; private set; }
    
    public bool IsValidityEditVisible { get; private set; }

    public bool IsSavingValidity { get; private set; }

    public string? ValidityEditErrorMessage { get; private set; }

    public string? ValidityEditSuccessMessage { get; private set; }

    public UpdateTicketValidityFormModel ValidityForm
        { get; private set; } = new();



    public async Task OpenDetailsAsync(AdminTicketDto ticket)
    {
        SelectedTicket = ticket;
        ValidityAdjustments = [];
        AdjustmentsErrorMessage = null;
        IsLoadingAdjustments = true;
        IsValidityEditVisible = false;
        ValidityEditErrorMessage = null;
        ValidityEditSuccessMessage = null;

        try
        {
            var adjustments = await apiService
                .GetValidityAdjustmentsAsync(ticket.Id);

            if (SelectedTicket?.Id == ticket.Id)
            {
                ValidityAdjustments = adjustments;
            }
        }
        catch (Exception ex)
        {
            if (SelectedTicket?.Id == ticket.Id)
            {
                AdjustmentsErrorMessage =
                    "Nie udało się pobrać historii korekt.";
            }

            Console.WriteLine($"[TicketsViewModel] {ex}");
        }
        finally
        {
            if (SelectedTicket?.Id == ticket.Id)
            {
                IsLoadingAdjustments = false;
            }
        }
    }

    public void CloseDetails()
    {
        SelectedTicket = null;
        ValidityAdjustments = [];
        AdjustmentsErrorMessage = null;
        IsLoadingAdjustments = false;
        IsValidityEditVisible = false;
        ValidityEditErrorMessage = null;
        ValidityEditSuccessMessage = null;
    }

    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var response = await apiService.GetAllAsync(
                pageNumber: PageNumber,
                pageSize: PageSize,
                userId: _appliedUserId,
                vehicleId: _appliedVehicleId,
                isValidated: _appliedIsValidated,
                sortBy: _appliedSortBy,
                sortDirection: _appliedSortDirection);

            Tickets = response.Items;
            TotalCount = response.TotalCount;
            TotalPages = response.TotalPages;
        }
        catch (Exception ex)
        {
            ErrorMessage =
                "Nie udało się pobrać listy sprzedanych biletów.";

            Console.WriteLine($"[TicketsViewModel] {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void OpenValidityEdit()
    {
        if (SelectedTicket is null ||
            !SelectedTicket.IsValidated ||
            SelectedTicket.ValidFrom is null ||
            SelectedTicket.ValidTo is null)
        {
            ValidityEditErrorMessage =
                "Można edytować tylko ważność aktywowanego biletu z datą końcową.";
            return;
        }

        var warsawZone =
            TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw");

        ValidityForm = new UpdateTicketValidityFormModel
        {
            ValidToLocal = TimeZoneInfo.ConvertTimeFromUtc(
                SelectedTicket.ValidTo.Value.ToUniversalTime(),
                warsawZone),
            Reason = string.Empty
        };

        ValidityEditErrorMessage = null;
        ValidityEditSuccessMessage = null;
        IsValidityEditVisible = true;
    }

    public void CloseValidityEdit()
    {
        if (IsSavingValidity)
            return;

        IsValidityEditVisible = false;
        ValidityEditErrorMessage = null;
    }

    public async Task SaveValidityAsync()
    {
        if (SelectedTicket is null ||
            ValidityForm.ValidToLocal is null ||
            IsSavingValidity)
            return;

        var ticket = SelectedTicket;
        IsSavingValidity = true;
        ValidityEditErrorMessage = null;
        ValidityEditSuccessMessage = null;

        try
        {
            var warsawZone =
                TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw");

            var localDate = DateTime.SpecifyKind(
                ValidityForm.ValidToLocal.Value,
                DateTimeKind.Unspecified);

            if (warsawZone.IsInvalidTime(localDate) ||
                warsawZone.IsAmbiguousTime(localDate))
            {
                ValidityEditErrorMessage =
                    "Wybrana godzina jest nieprawidłowa lub niejednoznaczna " +
                    "z powodu zmiany czasu. Wybierz inną godzinę.";
                return;
            }

            var validToUtc = TimeZoneInfo.ConvertTimeToUtc(
                localDate,
                warsawZone);

            if (ticket.ValidFrom is not null &&
                validToUtc <= ticket.ValidFrom.Value.ToUniversalTime())
            {
                ValidityEditErrorMessage =
                    "Nowa data ważności musi być późniejsza " +
                    "od daty aktywacji biletu.";
                return;
            }

            if (ticket.ValidTo is not null &&
                validToUtc == ticket.ValidTo.Value.ToUniversalTime())
            {
                ValidityEditErrorMessage =
                    "Nowa data ważności jest taka sama jak poprzednia.";
                return;
            }

            await apiService.UpdateValidityAsync(
                ticket.Id,
                validToUtc,
                ValidityForm.Reason);

            // Aktualizacja danych widocznych w szczegółach.
            ticket.ValidTo = validToUtc;

            IsValidityEditVisible = false;
            ValidityEditSuccessMessage =
                "Data ważności biletu została zaktualizowana.";
        }
        catch (Exception ex)
        {
            ValidityEditErrorMessage =
                "Nie udało się zapisać korekty ważności biletu.";

            Console.WriteLine($"[TicketsViewModel] {ex}");
            return;
        }
        finally
        {
            IsSavingValidity = false;
        }

        // Zapis już się udał. Osobno odświeżamy historię.
        try
        {
            IsLoadingAdjustments = true;

            var adjustments = await apiService
                .GetValidityAdjustmentsAsync(ticket.Id);

            if (SelectedTicket?.Id == ticket.Id)
            {
                ValidityAdjustments = adjustments;
                AdjustmentsErrorMessage = null;
            }
        }
        catch (Exception ex)
        {
            AdjustmentsErrorMessage =
                "Korekta została zapisana, ale nie udało się odświeżyć historii.";

            Console.WriteLine($"[TicketsViewModel] {ex}");
        }
        finally
        {
            IsLoadingAdjustments = false;
        }
    }


    public async Task ChangePageAsync(int page)
    {
        if (page < 1 || page > TotalPages)
            return;

        PageNumber = page;
        SelectedTicket = null;
        await LoadAsync();
    }

    public async Task ChangePageSizeAsync(int pageSize)
    {
        if (pageSize < 1)
            return;

        PageSize = pageSize;
        PageNumber = 1;
        SelectedTicket = null;
        await LoadAsync();
    }

    public async Task ApplyFiltersAsync()
    {
        ErrorMessage = null;

        int? userId = null;
        int? vehicleId = null;

        if (!string.IsNullOrWhiteSpace(UserIdFilter))
        {
            if (!int.TryParse(UserIdFilter, out var parsedUserId)
                || parsedUserId <= 0)
            {
                ErrorMessage = "ID użytkownika musi być liczbą większą od zera.";
                return;
            }

            userId = parsedUserId;
        }

        if (!string.IsNullOrWhiteSpace(VehicleIdFilter))
        {
            if (!int.TryParse(VehicleIdFilter, out var parsedVehicleId)
                || parsedVehicleId <= 0)
            {
                ErrorMessage = "ID pojazdu musi być liczbą większą od zera.";
                return;
            }

            vehicleId = parsedVehicleId;
        }

        _appliedUserId = userId;
        _appliedVehicleId = vehicleId;

        _appliedIsValidated = StatusFilter switch
        {
            "validated" => true,
            "not-validated" => false,
            _ => null
        };

        _appliedSortBy = SortBy;
        _appliedSortDirection = SortDirection;

        PageNumber = 1;

        await LoadAsync();
    }

    public async Task ClearFiltersAsync()
    {
        UserIdFilter = "";
        VehicleIdFilter = "";
        StatusFilter = "all";

        SortBy = "date";
        SortDirection = "desc";

        _appliedUserId = null;
        _appliedVehicleId = null;
        _appliedIsValidated = null;

        _appliedSortBy = "date";
        _appliedSortDirection = "desc";

        PageNumber = 1;

        await LoadAsync();
    }

}
