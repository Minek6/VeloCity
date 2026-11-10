namespace VeloCity.RouteImporter.Models;

public record MpkStop(
    int Id,
    string Number,
    string Name,
    double Latitude,
    double Longitude
);