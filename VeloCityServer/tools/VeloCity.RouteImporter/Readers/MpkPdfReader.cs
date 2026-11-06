
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using VeloCity.RouteImporter.Models;
using System.Text.RegularExpressions;

namespace VeloCity.RouteImporter.Readers;

public static class MpkPdfReader
{
    public static MpkTimetable? Read(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine(
                $"ERROR: PDF file not found: {filePath}");
            return null;
        }

        using var document = PdfDocument.Open(filePath);

        if (document.NumberOfPages == 0)
        {
            Console.WriteLine("ERROR: PDF has no pages.");
            return null;
        }

        var page = document.GetPage(1);

        var text = ContentOrderTextExtractor.GetText(page);

        var lines = text
            .Split('\n', StringSplitOptions.TrimEntries |
                         StringSplitOptions.RemoveEmptyEntries)
            .ToList();

        var timetable = new MpkTimetable();

        // Numer linii występuje po "Linia nr".
        var lineIndex = lines.FindIndex(
            line => line.Equals(
                "Linia nr",
                StringComparison.OrdinalIgnoreCase));

        if (lineIndex >= 0 && lineIndex + 1 < lines.Count)
        {
            timetable.LineNumber = lines[lineIndex + 1];
        }

        // Dane przystanku znajdują się po etykietach.
        var stopIndex = lines.FindIndex(
            line => line.StartsWith(
                "Kierunek:",
                StringComparison.OrdinalIgnoreCase));

        if (stopIndex >= 0 && stopIndex + 3 < lines.Count)
        {
            timetable.StopCode = lines[stopIndex + 1];
            timetable.StopName = lines[stopIndex + 2];
            timetable.Direction = lines[stopIndex + 3];
        }

        
        // Odczytywanie kolejności przystanków.
        var routeStartIndex = lines.FindIndex(
            line => line.Equals(
                "Przystanki na linii:",
                StringComparison.OrdinalIgnoreCase));

        var routeEndIndex = lines.FindIndex(
            line => line.StartsWith(
                "Legenda:",
                StringComparison.OrdinalIgnoreCase));

        if (routeStartIndex >= 0 &&
            routeEndIndex > routeStartIndex)
        {
            timetable.RouteStops = lines
                .Skip(routeStartIndex + 1)
                .Take(routeEndIndex - routeStartIndex - 1)
                .ToList();
        }

        Console.WriteLine();
        Console.WriteLine(
            $"Route stops: {timetable.RouteStops.Count}");

        for (int i = 0; i < timetable.RouteStops.Count; i++)
        {
            Console.WriteLine(
                $"{i + 1}. {timetable.RouteStops[i]}");
        }

        Console.WriteLine();


        // Odczytywanie godzin odjazdów w dni robocze.
        var weekdayIndex = lines.FindIndex(
            line => line.Equals(
                "Dni robocze",
                StringComparison.OrdinalIgnoreCase));

        var saturdayIndex = lines.FindIndex(
            line => line.Equals(
                "Soboty",
                StringComparison.OrdinalIgnoreCase));

        if (weekdayIndex >= 0 &&
            saturdayIndex > weekdayIndex)
        {
            var weekdayLines = lines
                .Skip(weekdayIndex + 1)
                .Take(saturdayIndex - weekdayIndex - 1);

        }

        // Odczytywanie godzin sobotnich.
        var sundayIndex = lines.FindIndex(
            line => line.StartsWith(
                "Niedziele",
                StringComparison.OrdinalIgnoreCase));

        if (saturdayIndex >= 0 &&
            sundayIndex > saturdayIndex)
        {
            var saturdayLines = lines
                .Skip(saturdayIndex + 1)
                .Take(sundayIndex - saturdayIndex - 1);

        }

        // Odczytywanie godzin niedzielnych i świątecznych.
        if (sundayIndex >= 0)
        {
            var sundayLines = lines.Skip(sundayIndex + 1);
        }

        var legendStartIndex = lines.FindIndex(
            line => line.Equals(
                "Legenda:",
                StringComparison.OrdinalIgnoreCase));

        var legendEndIndex = lines.FindIndex(
            line => line.Equals(
                "Dni robocze",
                StringComparison.OrdinalIgnoreCase));

        if (legendStartIndex >= 0 &&
            legendEndIndex > legendStartIndex)
        {
            
            string? currentSymbol = null;

            foreach (var line in lines
                .Skip(legendStartIndex + 1)
                .Take(legendEndIndex - legendStartIndex - 1))
            {
                var match = Regex.Match(
                    line,
                    @"^([A-Z])\s*-\s*(.+)$");

                if (match.Success)
                {
                    currentSymbol = match.Groups[1].Value;

                    timetable.CourseLegend[currentSymbol] =
                        match.Groups[2].Value.Trim();
                }
                else if (currentSymbol != null)
                {
                    timetable.CourseLegend[currentSymbol] +=
                        " " + line.Trim();
                }
            }

        }

        Console.WriteLine();
        Console.WriteLine("=== Course legend ===");

        foreach (var item in timetable.CourseLegend)
        {
            Console.WriteLine($"{item.Key}: {item.Value}");
        }


        Console.WriteLine("=== MPK Timetable ===");
        Console.WriteLine($"Line: {timetable.LineNumber}");
        Console.WriteLine($"Stop code: {timetable.StopCode}");
        Console.WriteLine($"Stop name: {timetable.StopName}");
        Console.WriteLine($"Direction: {timetable.Direction}");

        
        Console.WriteLine();
        Console.WriteLine("=== Raw departure sections ===");

        if (weekdayIndex >= 0)
        {
            foreach (var line in lines.Skip(weekdayIndex))
            {
                Console.WriteLine($"RAW: [{line}]");
            }
        }

        
        Console.WriteLine();
        Console.WriteLine("=== New departure parser ===");

        // Dni robocze
        if (weekdayIndex >= 0 &&
            saturdayIndex > weekdayIndex)
        {
            var weekdayLines = lines
                .Skip(weekdayIndex + 1)
                .Take(saturdayIndex - weekdayIndex - 1);

            timetable.WeekdayCourses =
                MpkDepartureParser.Parse(weekdayLines);
        }

        // Soboty
        if (saturdayIndex >= 0 &&
            sundayIndex > saturdayIndex)
        {
            var saturdayLines = lines
                .Skip(saturdayIndex + 1)
                .Take(sundayIndex - saturdayIndex - 1);

            timetable.SaturdayCourses =
                MpkDepartureParser.Parse(saturdayLines);
        }

        // Niedziele i święta
        if (sundayIndex >= 0)
        {
            var sundayLines = lines.Skip(sundayIndex + 1);

            timetable.SundayCourses =
                MpkDepartureParser.Parse(sundayLines);
        }

        Console.WriteLine(
            $"Weekday courses: {timetable.WeekdayCourses.Count}");

        Console.WriteLine(
            $"Saturday courses: {timetable.SaturdayCourses.Count}");

        Console.WriteLine(
            $"Sunday courses: {timetable.SundayCourses.Count}");

        Console.WriteLine();
        Console.WriteLine("=== Departure details ===");

        foreach (var departure in timetable.WeekdayCourses)
        {
            Console.WriteLine(
                $"{departure.Time:HH:mm} " +
                $"Symbol: {departure.CourseSymbol ?? "-"}");
        }


        return timetable;
    }
}
