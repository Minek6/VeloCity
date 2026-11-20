using VeloCity.Admin.Models;
using VeloCity.Admin.Models.Dto;
using VeloCity.Admin.Services;
using VeloCity.Admin.Models.Forms;

namespace VeloCity.Admin.ViewModels;

public sealed class LinesViewModel(
    LinesApiService linesApiService)
{
    public PaginatedResponse<LineDto> Result { get; private set; }
        = new();

    public List<LineDto> Lines => Result.Items;

    public string SearchTerm { get; set; } = string.Empty;

    public string SortDirection { get; set; } = "asc";

    public int PageNumber { get; private set; } = 1;

    public int PageSize { get; } = 20;

    public int TotalCount => Result.TotalCount;

    public int TotalPages => Result.TotalPages;

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;

    public bool IsLoading { get; private set; }

    public string? ErrorMessage { get; private set; }   

    public LineFormModel Form { get; private set; } = new();

    public bool IsEditing { get; private set; }

    public bool IsEditFormVisible { get; private set; }

    public bool IsSaving { get; private set; }

    public bool IsChangingStatus { get; private set; }

    public string StatusFilter { get; set; } = "active";

    public string? SuccessMessage { get; private set; }

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
            Result = await linesApiService.GetAllAsync(
                SearchTerm,
                PageNumber,
                PageSize,
                SortDirection == "desc",
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
                $"Nie udało się pobrać linii. {ex.Message}";
        }
        catch (Exception)
        {
            Result = new();
            ErrorMessage =
                "Wystąpił błąd podczas pobierania linii.";
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

    public async Task ClearFiltersAsync()
    {
        SearchTerm = string.Empty;
        StatusFilter = "active";
        SortDirection = "asc";
        PageNumber = 1;

        await LoadAsync();
    }

    public async Task ChangeSortAsync()
    {
        PageNumber = 1;
        await LoadAsync();
    }

    public async Task NextPageAsync()
    {
        if (IsLoading || !HasNextPage)
            return;

        PageNumber++;
        await LoadAsync();
    }

    public async Task PreviousPageAsync()
    {
        if (IsLoading || !HasPreviousPage)
            return;

        PageNumber--;
        await LoadAsync();
    }

    public void OpenEditForm(LineDto line)
    {
        if (!line.IsActive)
            return;

        Form = new LineFormModel
        {
            Id = line.Id,
            Name = line.Name
        };
        IsEditing = true;
        IsEditFormVisible = true;
        ErrorMessage = null;
        SuccessMessage = null;
    }

    public void CloseEditForm()
    {
        if (!IsSaving)
            IsEditFormVisible = false;
    }

    public async Task SaveAsync()
    {
        if (IsSaving || !IsEditFormVisible)
            return;

        IsSaving = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            var editing = IsEditing;

            if (editing)
            {
                await linesApiService.UpdateAsync(Form);
            }
            else
            {
                await linesApiService.CreateAsync(Form);
            }

            IsEditFormVisible = false;

            if (!editing)
            {
                PageNumber = 1;
                SearchTerm = string.Empty;
                StatusFilter = "active";
            }

            await LoadAsync();

            if (ErrorMessage is null)
            {
                SuccessMessage = editing
                    ? "Linia została zaktualizowana."
                    : "Nowa linia została dodana.";
            }
        }
        catch (UnauthorizedAccessException)
        {
            ErrorMessage = "Sesja administratora wygasła.";
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage =
                $"Nie udało się zapisać linii. {ex.Message}";
        }
        catch (Exception)
        {
            ErrorMessage =
                "Wystąpił błąd podczas zapisywania linii.";
        }
        finally
        {
            IsSaving = false;
        }
    }

    public async Task ChangeStatusAsync(
        int id,
        bool activate)
    {
        if (IsChangingStatus)
            return;

        IsChangingStatus = true;
        ErrorMessage = null;
        SuccessMessage = null;

        try
        {
            if (activate)
                await linesApiService.ActivateAsync(id);
            else
                await linesApiService.DeactivateAsync(id);

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
                    ? "Linia została aktywowana."
                    : "Linia została dezaktywowana.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage =
                $"Nie udało się zmienić statusu linii. {ex.Message}";
        }
        finally
        {
            IsChangingStatus = false;
        }
    }

    public async Task ChangeStatusFilterAsync()
    {
        PageNumber = 1;
        await LoadAsync();
    }

    public void OpenCreateForm()
    {
        Form = new LineFormModel();

        IsEditing = false;
        IsEditFormVisible = true;

        ErrorMessage = null;
        SuccessMessage = null;
    }

}
