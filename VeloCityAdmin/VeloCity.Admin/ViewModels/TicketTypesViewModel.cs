using VeloCity.Admin.Models.Dto;
using VeloCity.Admin.Services;
using VeloCity.Admin.Models.Forms;

namespace VeloCity.Admin.ViewModels;

public sealed class TicketTypesViewModel(
    TicketTypesApiService apiService)
{
    public List<TicketTypeDto> TicketTypes { get; private set; } = [];

    public bool IsLoading { get; private set; }

    public string? ErrorMessage { get; private set; }

    public CreateTicketTypeFormModel CreateForm { get; private set; } = new();

    public bool IsCreateFormVisible { get; private set; }

    public bool IsCreating { get; private set; }

    public string? SuccessMessage { get; private set; }

    public bool IsEditing { get; private set; }

    public int? EditingTicketTypeId { get; private set; }

    public bool IsActivating { get; private set; }

    public bool IsDeactivating { get; private set; }


    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            TicketTypes = await apiService.GetAllAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Nie udało się pobrać rodzajów biletów.";
            Console.WriteLine($"[TicketTypesViewModel] {ex}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void OpenCreateForm()
    {
        if (IsCreating)
            return;

        CreateForm = new CreateTicketTypeFormModel();

        IsEditing = false;
        EditingTicketTypeId = null;
        IsCreateFormVisible = true;

        ErrorMessage = null;
        SuccessMessage = null;
    }

    public void OpenEditForm(TicketTypeDto ticket)
    {
        if (IsCreating)
            return;

        CreateForm = new CreateTicketTypeFormModel
        {
            Name = ticket.Name,
            Price = ticket.Price,
            DurationInMinutes = ticket.DurationInMinutes,
            ZoneLimit = ticket.ZoneLimit
        };

        EditingTicketTypeId = ticket.Id;
        IsEditing = true;
        IsCreateFormVisible = true;

        ErrorMessage = null;
        SuccessMessage = null;
    }

    public void CloseCreateForm()
    {
        if (IsCreating)
            return;

        IsCreateFormVisible = false;
        IsEditing = false;
        EditingTicketTypeId = null;
        CreateForm = new CreateTicketTypeFormModel();
    }

    public async Task SaveAsync()
    {
        if (IsCreating)
            return;

        IsCreating = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            var wasEditing = IsEditing;

            if (wasEditing)
            {
                if (EditingTicketTypeId is null)
                    throw new InvalidOperationException(
                        "Nie wybrano rodzaju biletu do edycji.");

                await apiService.UpdateAsync(
                    EditingTicketTypeId.Value,
                    CreateForm);
            }
            else
            {
                await apiService.CreateAsync(CreateForm);
            }

            await LoadAsync();

            if (ErrorMessage is not null)
                return;

            IsCreateFormVisible = false;
            IsEditing = false;
            EditingTicketTypeId = null;
            CreateForm = new CreateTicketTypeFormModel();

            SuccessMessage = wasEditing
                ? "Rodzaj biletu został zaktualizowany."
                : "Rodzaj biletu został dodany.";
        }
        catch (Exception ex)
        {
            ErrorMessage = IsEditing
                ? "Nie udało się zaktualizować rodzaju biletu."
                : "Nie udało się dodać rodzaju biletu.";

            Console.WriteLine($"[TicketTypesViewModel] {ex}");
        }
        finally
        {
            IsCreating = false;
        }
    }

    public async Task ActivateAsync(int id)
    {
        if (IsActivating || IsDeactivating || IsCreating)
            return;

        IsActivating = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            await apiService.ActivateAsync(id);
            await LoadAsync();

            if (ErrorMessage is null)
            {
                SuccessMessage =
                    "Rodzaj biletu został aktywowany.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage =
                "Nie udało się aktywować rodzaju biletu.";

            Console.WriteLine($"[TicketTypesViewModel] {ex}");
        }
        finally
        {
            IsActivating = false;
        }
    }

    public async Task DeactivateAsync(int id)
    {
        if (IsDeactivating || IsActivating || IsCreating)
            return;

        IsDeactivating = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            await apiService.DeactivateAsync(id);
            await LoadAsync();

            if (ErrorMessage is null)
            {
                SuccessMessage =
                    "Rodzaj biletu został dezaktywowany.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage =
                "Nie udało się dezaktywować rodzaju biletu.";

            Console.WriteLine($"[TicketTypesViewModel] {ex}");
        }
        finally
        {
            IsDeactivating = false;
        }
    }


}
