using System.Net.Http;
using VeloCity.Admin.Models.Dto;
using VeloCity.Admin.Services;
using VeloCity.Admin.Models.Enums;
using VeloCity.Admin.Models.Forms;

namespace VeloCity.Admin.ViewModels;

public sealed class UsersViewModel(
    UsersApiService usersApiService)
{
    public List<UserDto> Users { get; private set; } = [];

    public bool IsLoading { get; private set; }
    public string? ErrorMessage { get; private set; }

    public int PageNumber { get; private set; } = 1;
    public int PageSize { get; private set; } = 10;

    public int TotalCount { get; private set; }
    public int TotalPages { get; private set; }

    public string SearchTerm { get; private set; } = "";
    public string StatusFilter { get; private set; } = "all";
    public string SortBy { get; private set; } = "asc";

    public UserDetailsDto? SelectedUser { get; private set; }

    public UserRole SelectedRole { get; set; }

    public bool SelectedIsActive { get; set; }

    public bool IsLoadingDetails { get; private set; }

    public bool IsSaving { get; private set; }

    public string? DetailsErrorMessage { get; private set; }

    public string? SuccessMessage { get; private set; }

    public string EditedName { get; set; } = string.Empty;
    public string EditedSurname { get; set; } = string.Empty;
    public string EditedEmail { get; set; } = string.Empty;

    public decimal AdjustmentAmount { get; set; }

    public string AdjustmentReason { get; set; } = string.Empty;

    public bool IsAdjustingBalance { get; private set; }

    public BalanceAdjustmentDto? LastBalanceAdjustment
    {
        get;
        private set;
    }

    public CreateUserFormModel CreateForm { get; private set; } = new();

    public bool IsCreateFormVisible { get; private set; }

    public bool IsCreating { get; private set; }

    public async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            bool? isActive = StatusFilter switch
            {
                "active" => true,
                "inactive" => false,
                _ => null
            };

            var result = await usersApiService.GetUsersAsync(
                pageNumber: PageNumber,
                pageSize: PageSize,
                search: SearchTerm,
                isActive: isActive,
                sortDirection: SortBy);

            Users = result.Items;
            TotalCount = result.TotalCount;
            TotalPages = result.TotalPages;
        }
        catch (UnauthorizedAccessException)
        {
            Users = [];
            ErrorMessage = "Sesja administratora wygasła.";
        }
        catch (HttpRequestException ex)
        {
            Users = [];
            ErrorMessage =
                $"Nie udało się pobrać użytkowników. Status API: {ex.StatusCode?.ToString() ?? "brak odpowiedzi"}.";
        }
        catch (Exception)
        {
            Users = [];
            ErrorMessage = "Wystąpił błąd podczas pobierania użytkowników.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task ChangePageAsync(int page)
    {
        PageNumber = Math.Max(1, page);
        await LoadAsync();
    }

    public async Task ChangePageSizeAsync(int size)
    {
        PageSize = size;
        PageNumber = 1;
        await LoadAsync();
    }

    public async Task SetSearchAsync(string search)
    {
        SearchTerm = search;
        PageNumber = 1;
        await LoadAsync();
    }

    public async Task SetStatusAsync(string status)
    {
        StatusFilter = status;
        PageNumber = 1;
        await LoadAsync();
    }

    public async Task SetSortAsync(string sort)
    {
        SortBy = sort;
        PageNumber = 1;
        await LoadAsync();
    }

    public async Task ClearFiltersAsync()
    {
        SearchTerm = "";
        StatusFilter = "all";
        SortBy = "asc";
        PageNumber = 1;

        await LoadAsync();
    }

    public async Task OpenDetailsAsync(int id)
    {
        if (IsLoadingDetails || IsSaving)
            return;

        SelectedUser = null;
        IsLoadingDetails = true;
        DetailsErrorMessage = null;
        SuccessMessage = null;

        try
        {
            var user = await usersApiService.GetByIdAsync(id);

            SelectedUser = user;
            SelectedRole = user.Role;
            SelectedIsActive = user.IsActive;

            AdjustmentAmount = 0;
            AdjustmentReason = string.Empty;
            LastBalanceAdjustment = null;

            EditedName = user.Name;
            EditedSurname = user.Surname;
            EditedEmail = user.Email;
        }
        catch (UnauthorizedAccessException)
        {
            DetailsErrorMessage =
                "Sesja administratora wygasła.";
        }
        catch (HttpRequestException ex)
        {
            DetailsErrorMessage =
                $"Nie udało się pobrać szczegółów użytkownika. {ex.Message}";
        }
        catch (Exception ex)
        {
            DetailsErrorMessage =
                "Wystąpił błąd podczas pobierania szczegółów.";
            Console.WriteLine(
                $"[UsersViewModel] {ex}");
        }
        finally
        {
            IsLoadingDetails = false;
        }
    }

    public async Task SaveProfileAsync()
    {
        if (SelectedUser is null || IsSaving)
            return;

        IsSaving = true;
        DetailsErrorMessage = null;
        SuccessMessage = null;

        try
        {
            var userId = SelectedUser.Id;

            await usersApiService.UpdateProfileAsync(
                userId,
                EditedName,
                EditedSurname,
                EditedEmail);

            // Odświeżenie szczegółów użytkownika
            var updatedUser =
                await usersApiService.GetByIdAsync(userId);

            SelectedUser = updatedUser;

            EditedName = updatedUser.Name;
            EditedSurname = updatedUser.Surname;
            EditedEmail = updatedUser.Email;

            // Odświeżenie tabeli
            await LoadAsync();

            if (ErrorMessage is not null)
            {
                DetailsErrorMessage = ErrorMessage;
                return;
            }

            SuccessMessage =
                "Dane użytkownika zostały zaktualizowane.";
        }
        catch (UnauthorizedAccessException)
        {
            DetailsErrorMessage =
                "Sesja administratora wygasła.";
        }
        catch (HttpRequestException ex)
        {
            DetailsErrorMessage =
                $"Nie udało się zapisać profilu. {ex.Message}";
        }
        catch (Exception ex)
        {
            DetailsErrorMessage =
                "Wystąpił błąd podczas aktualizacji profilu.";

            Console.WriteLine($"[UsersViewModel] {ex}");
        }
        finally
        {
            IsSaving = false;
        }
    }

    public void CloseDetails()
    {
        if (IsSaving)
            return;

        SelectedUser = null;
        DetailsErrorMessage = null;
        SuccessMessage = null;
    }

    public async Task SaveUserAsync()
    {
        if (SelectedUser is null || IsSaving)
            return;

        IsSaving = true;
        DetailsErrorMessage = null;
        SuccessMessage = null;

        try
        {
            await usersApiService.UpdateStatusAsync(
                SelectedUser.Id,
                SelectedRole,
                SelectedIsActive);

            var userId = SelectedUser.Id;

            await LoadAsync();

            if (ErrorMessage is not null)
            {
                DetailsErrorMessage = ErrorMessage;
                return;
            }

            await OpenDetailsAfterSaveAsync(userId);

            if (DetailsErrorMessage is null)
            {
                SuccessMessage =
                    "Dane użytkownika zostały zaktualizowane.";
            }
        }
        catch (UnauthorizedAccessException)
        {
            DetailsErrorMessage =
                "Sesja administratora wygasła.";
        }
        catch (HttpRequestException ex)
        {
            DetailsErrorMessage =
                $"Nie udało się zapisać zmian. {ex.Message}";
        }
        catch (Exception ex)
        {
            DetailsErrorMessage =
                "Wystąpił błąd podczas zapisywania zmian.";
            Console.WriteLine(
                $"[UsersViewModel] {ex}");
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task OpenDetailsAfterSaveAsync(int id)
    {
        var user = await usersApiService.GetByIdAsync(id);

        SelectedUser = user;
        SelectedRole = user.Role;
        SelectedIsActive = user.IsActive;
    }


    public async Task AdjustBalanceAsync()
    {
        if (SelectedUser is null ||
            IsAdjustingBalance ||
            IsSaving)
        {
            return;
        }

        DetailsErrorMessage = null;
        SuccessMessage = null;

        if (AdjustmentAmount == 0)
        {
            DetailsErrorMessage =
                "Kwota korekty nie może wynosić 0 zł.";
            return;
        }

        if (string.IsNullOrWhiteSpace(AdjustmentReason) ||
            AdjustmentReason.Trim().Length < 5)
        {
            DetailsErrorMessage =
                "Podaj powód korekty (minimum 5 znaków).";
            return;
        }

        IsAdjustingBalance = true;

        try
        {
            var userId = SelectedUser.Id;

            var adjustment =
                await usersApiService.AdjustBalanceAsync(
                    userId,
                    AdjustmentAmount,
                    AdjustmentReason);

            LastBalanceAdjustment = adjustment;

            // Pobierz aktualny stan konta.
            SelectedUser =
                await usersApiService.GetByIdAsync(userId);

            // Odśwież listę użytkowników.
            await LoadAsync();

            if (ErrorMessage is not null)
            {
                DetailsErrorMessage = ErrorMessage;
                return;
            }

            AdjustmentAmount = 0;
            AdjustmentReason = string.Empty;

            SuccessMessage =
                $"Saldo użytkownika zostało zmienione. " +
                $"Nowe saldo: {adjustment.BalanceAfter:N2} zł.";
        }
        catch (UnauthorizedAccessException)
        {
            DetailsErrorMessage =
                "Sesja administratora wygasła.";
        }
        catch (HttpRequestException ex)
        {
            DetailsErrorMessage =
                $"Nie udało się zmienić salda. Konto użytkownika nie aktywne.";
        }
        catch (Exception ex)
        {
            DetailsErrorMessage =
                "Wystąpił błąd podczas korekty salda.";

            Console.WriteLine($"[UsersViewModel] {ex}");
        }
        finally
        {
            IsAdjustingBalance = false;
        }
    }

    public void OpenCreateForm()
    {
        if (IsCreating)
            return;

        CreateForm = new CreateUserFormModel();
        IsCreateFormVisible = true;

        ErrorMessage = null;
        SuccessMessage = null;
    }

    public void CloseCreateForm()
    {
        if (IsCreating)
            return;

        IsCreateFormVisible = false;
    }

    public async Task CreateUserAsync()
    {
        if (IsCreating)
            return;

        IsCreating = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            await usersApiService.CreateUserAsync(CreateForm);

            // Wracamy na pierwszą stronę listy.
            PageNumber = 1;

            // Odświeżamy dane po utworzeniu konta.
            await LoadAsync();

            if (ErrorMessage is not null)
                return;

            IsCreateFormVisible = false;
            CreateForm = new CreateUserFormModel();

            SuccessMessage = "Użytkownik został utworzony.";
        }
        catch (UnauthorizedAccessException)
        {
            ErrorMessage = "Sesja administratora wygasła.";
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage =
                $"Nie udało się utworzyć użytkownika. {ex.Message}";
        }
        catch (Exception ex)
        {
            ErrorMessage =
                "Wystąpił błąd podczas tworzenia użytkownika.";

            Console.WriteLine($"[UsersViewModel] {ex}");
        }
        finally
        {
            IsCreating = false;
        }
    }

}
