using VeloCity.Admin.Models;
using VeloCity.Admin.Models.Dto;
using VeloCity.Admin.Models.Forms;
using VeloCity.Admin.Services;

namespace VeloCity.Admin.ViewModels;

public sealed class StopsViewModel(
    StopsApiService stopsApiService)
{
    public PaginatedResponse<StopDto> Result { get; private set; }
        = new();

    public List<StopDto> Stops => Result.Items;

    public bool IsLoading { get; private set; }

    public string? ErrorMessage { get; private set; }

    public string SearchTerm { get; set; } = string.Empty;

    public int PageNumber { get; private set; } = 1;

    public int PageSize { get; private set; } = 20;

    public int TotalCount => Result.TotalCount;

    public int TotalPages => Result.TotalPages;

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;

    public StopFormModel Form { get; private set; } = new();

    public bool IsFormVisible { get; private set; }

    public bool IsEditing { get; private set; }

    public bool IsSaving { get; private set; }

    public string? SuccessMessage { get; private set; }

    public string StatusFilter { get; set; } = "active";

    public bool IsChangingStatus { get; private set; }

    public int? ChangingStopId { get; private set; }

    private bool? SelectedIsActive => StatusFilter switch
    {
        "active" => true,
        "inactive" => false,
        _ => null
    };



    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            Result = await stopsApiService.GetAllAsync(
                SearchTerm,
                PageNumber,
                PageSize,
                SelectedIsActive);
        }
        catch (UnauthorizedAccessException)
        {
            Result = new();
            ErrorMessage = "Sesja administratora wygasła.";
        }
        catch (HttpRequestException ex)
        {
            Result = new();
            ErrorMessage =
                $"Nie udało się pobrać przystanków. {ex.Message}";
        }
        catch (Exception)
        {
            Result = new();
            ErrorMessage = "Wystąpił błąd podczas pobierania przystanków.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task SearchAsync()
    {
        PageNumber = 1;
        await LoadAsync();
    }

    public async Task ClearSearchAsync()
    {
        SearchTerm = string.Empty;
        StatusFilter = "all";
        PageNumber = 1;

        await LoadAsync();
    }

    public async Task NextPageAsync()
    {
        if (!HasNextPage || IsLoading)
            return;

        PageNumber++;
        await LoadAsync();
    }

    public async Task PreviousPageAsync()
    {
        if (!HasPreviousPage || IsLoading)
            return;

        PageNumber--;
        await LoadAsync();
    }

    public void OpenCreateForm()
    {
        Form = new StopFormModel();

        IsEditing = false;
        IsFormVisible = true;

        ErrorMessage = null;
        SuccessMessage = null;
    }

    public void OpenEditForm(StopDto stop)
    {
        Form = new StopFormModel
        {
            Id = stop.Id,
            Name = stop.Name,
            Latitude = stop.Latitude,
            Longitude = stop.Longitude,
            Zone = stop.Zone,
            ExternalId = stop.ExternalId
        };

        IsEditing = true;
        IsFormVisible = true;

        ErrorMessage = null;
        SuccessMessage = null;
    }

    public void CloseForm()
    {
        if (IsSaving)
            return;

        IsFormVisible = false;
    }

    public async Task SaveAsync()
    {
        if (IsSaving || !IsFormVisible)
            return;

        IsSaving = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            var editing = IsEditing;

            if (editing)
            {
                await stopsApiService.UpdateAsync(Form);
            }
            else
            {
                await stopsApiService.CreateAsync(Form);
            }

            IsFormVisible = false;

            if (!editing)
                PageNumber = 1;

            await LoadAsync();

            if (ErrorMessage is null)
            {
                SuccessMessage = editing
                    ? "Przystanek został zaktualizowany."
                    : "Przystanek został dodany.";
            }
        }
        catch (UnauthorizedAccessException)
        {
            ErrorMessage = "Sesja administratora wygasła.";
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage =
                $"Nie udało się zapisać przystanku. {ex.Message}";
        }
        catch (Exception)
        {
            ErrorMessage =
                "Wystąpił błąd podczas zapisywania przystanku.";
        }
        finally
        {
            IsSaving = false;
        }
    }

    public async Task ChangeStatusFilterAsync()
    {
        PageNumber = 1;
        await LoadAsync();
    }

    public async Task ChangeStopStatusAsync(
        int stopId,
        bool activate)
    {
        if (IsChangingStatus)
            return;

        IsChangingStatus = true;
        ChangingStopId = stopId;

        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            if (activate)
                await stopsApiService.ActivateAsync(stopId);
            else
                await stopsApiService.DeactivateAsync(stopId);

            await LoadAsync();

            if (ErrorMessage is null &&
                PageNumber > 1 &&
                PageNumber > TotalPages)
            {
                PageNumber = Math.Max(1, TotalPages);
                await LoadAsync();
            }

            if (ErrorMessage is null)
            {
                SuccessMessage = activate
                    ? "Przystanek został aktywowany."
                    : "Przystanek został dezaktywowany.";
            }
        }
        catch (UnauthorizedAccessException)
        {
            ErrorMessage = "Sesja administratora wygasła.";
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage =
                $"Nie udało się zmienić statusu przystanku. {ex.Message}";
        }
        catch (Exception)
        {
            ErrorMessage =
                "Wystąpił błąd podczas zmiany statusu przystanku.";
        }
        finally
        {
            IsChangingStatus = false;
            ChangingStopId = null;
        }
    }

}
