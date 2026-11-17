
using VeloCity.Admin.Models;
using VeloCity.Admin.Services;

namespace VeloCity.Admin.ViewModels;

public sealed class VehiclesViewModel(
    VehiclesApiService vehiclesService)
{
    public List<VehicleDto> Vehicles { get; private set; } = [];

    public VehicleFormModel Form { get; private set; } = new();

    public bool IsLoading { get; private set; }
    public bool IsSaving { get; private set; }
    public bool IsFormVisible { get; private set; }
    public bool IsEditing { get; private set; }
    public bool IsDeactivating { get; private set; }
    public bool IsActivating { get; private set; }

    public string? ErrorMessage { get; private set; }
    public string? SuccessMessage { get; private set; }

    public int TotalVehicles => Vehicles.Count;
    public int ActiveVehicles => Vehicles.Count(v => v.IsActive);
    public int InactiveVehicles => Vehicles.Count(v => !v.IsActive);

    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            Vehicles = await vehiclesService.GetAllAsync();
        }
        catch (Exception)
        {
            ErrorMessage = "Nie udało się pobrać listy pojazdów.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void OpenCreateForm()
    {
        Form = new VehicleFormModel();
        IsEditing = false;
        IsFormVisible = true;
        ErrorMessage = null;
        SuccessMessage = null;
    }

    public void OpenEditForm(VehicleDto vehicle)
    {
        Form = new VehicleFormModel
        {
            Id = vehicle.Id,
            SideNumber = vehicle.SideNumber,
            Model = vehicle.Model
        };

        IsEditing = true;
        IsFormVisible = true;
        ErrorMessage = null;
        SuccessMessage = null;
    }

    public void CloseForm()
    {
        IsFormVisible = false;
    }

    public async Task SaveAsync()
    {
        if (IsSaving)
            return;

        IsSaving = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            var editing = IsEditing;

            if (editing)
                await vehiclesService.UpdateAsync(Form);
            else
                await vehiclesService.CreateAsync(Form);

            IsFormVisible = false;

            await LoadAsync();

            if (ErrorMessage is null)
            {
                SuccessMessage = editing
                    ? "Pojazd został zaktualizowany."
                    : "Pojazd został dodany.";
            }
        }
        catch (UnauthorizedAccessException)
        {
            ErrorMessage = "Sesja administratora wygasła.";
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage =
                $"Nie udało się zapisać pojazdu. Status API: {ex.StatusCode?.ToString() ?? "brak odpowiedzi"}.";
        }
        catch (Exception)
        {
            ErrorMessage = "Wystąpił błąd podczas zapisywania pojazdu.";
        }
        finally
        {
            IsSaving = false;
        }
    }

    public async Task DeactivateAsync(int id)
    {
        if (IsDeactivating)
            return;

        IsDeactivating = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            await vehiclesService.DeactivateAsync(id);

            await LoadAsync();

            if (ErrorMessage is null)
            {
                SuccessMessage = "Pojazd został dezaktywowany.";
            }
        }
        catch (UnauthorizedAccessException)
        {
            ErrorMessage = "Sesja administratora wygasła.";
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage =
                $"Nie udało się dezaktywować pojazdu. Status API: {ex.StatusCode?.ToString() ?? "brak odpowiedzi"}.";
        }
        catch (Exception)
        {
            ErrorMessage = "Wystąpił błąd podczas dezaktywacji pojazdu.";
        }
        finally
        {
            IsDeactivating = false;
        }
    }

    public async Task ActivateAsync(int id)
    {
        if (IsActivating || IsDeactivating)
            return;

        IsActivating = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            await vehiclesService.ActivateAsync(id);

            await LoadAsync();

            if (ErrorMessage is null)
            {
                SuccessMessage = "Pojazd został aktywowany.";
            }
        }
        catch (UnauthorizedAccessException)
        {
            ErrorMessage = "Sesja administratora wygasła.";
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage =
                $"Nie udało się aktywować pojazdu. Status API: {ex.StatusCode?.ToString() ?? "brak odpowiedzi"}.";
        }
        catch (Exception)
        {
            ErrorMessage = "Wystąpił błąd podczas aktywacji pojazdu.";
        }
        finally
        {
            IsActivating = false;
        }
    }
}
