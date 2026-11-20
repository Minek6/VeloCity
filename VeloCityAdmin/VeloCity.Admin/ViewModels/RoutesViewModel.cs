using VeloCity.Admin.Models.Dto;
using VeloCity.Admin.Services;

namespace VeloCity.Admin.ViewModels;

public sealed class RoutesViewModel(
    LinesApiService linesApiService)
{
    public LineDetailsDto? Line { get; private set; }

    public int SelectedDirection { get; private set; } = 0;

    public bool IsLoading { get; private set; }

    public string? ErrorMessage { get; private set; }

    public List<LineRouteStopDto> CurrentStops =>
        Line?.Stops
            .Where(s => s.Direction == SelectedDirection)
            .OrderBy(s => s.Sequence)
            .ToList() ?? [];

    public async Task LoadAsync(int lineId)
    {
        IsLoading = true;
        ErrorMessage = null;
        Line = null;
        SelectedDirection = 0;

        try
        {
            Line = await linesApiService.GetByIdAsync(lineId);

            if (Line is null)
                ErrorMessage = "Nie znaleziono linii.";
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage =
                $"Nie udało się pobrać trasy. {ex.Message}";
        }
        catch (UnauthorizedAccessException)
        {
            ErrorMessage = "Sesja administratora wygasła.";
        }
        catch (Exception)
        {
            ErrorMessage = "Wystąpił błąd podczas pobierania trasy.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void ChangeDirection(int direction)
    {
        if (direction is 0 or 1)
            SelectedDirection = direction;
    }
}
