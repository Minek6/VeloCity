using Microsoft.EntityFrameworkCore;
using VeloCity.Api.Models.Enums;

namespace VeloCity.Api.Models.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Line> Lines => Set<Line>();
    public DbSet<Stop> Stops => Set<Stop>();
    public DbSet<RouteStop> RouteStops => Set<RouteStop>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketType> TicketTypes => Set<TicketType>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<Timetable> Timetables => Set<Timetable>(); 
    public DbSet<Route> Routes => Set<Route>();
    public DbSet<ServiceCalendar> ServiceCalendars => Set<ServiceCalendar>();
    public DbSet<ScheduledTrip> ScheduledTrips => Set<ScheduledTrip>();
    public DbSet<ScheduledStopTime> ScheduledStopTimes => Set<ScheduledStopTime>();



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .Property(u => u.Balance)
            .HasPrecision(18, 2);

        modelBuilder.Entity<TicketType>()
            .Property(tt => tt.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Stop>()
            .HasIndex(s => s.ExternalId)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Route>(entity =>
        {
            entity.HasOne(r => r.Line)
                .WithMany()
                .HasForeignKey(r => r.LineId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(150);
        });
        
        modelBuilder.Entity<ServiceCalendar>(entity =>
        {
            entity.Property(sc => sc.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasCheckConstraint(
                "CK_ServiceCalendar_ValidDates",
                "\"ValidFrom\" <= \"ValidTo\""
            );
        });

        modelBuilder.Entity<ScheduledStopTime>(entity =>
        {
            entity.HasOne(sst => sst.ScheduledTrip)
                .WithMany(st => st.StopTimes)
                .HasForeignKey(sst => sst.ScheduledTripId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(sst => sst.RouteStop)
                .WithMany()
                .HasForeignKey(sst => sst.RouteStopId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(sst => new
            {
                sst.ScheduledTripId,
                sst.RouteStopId
            }).IsUnique();
        });

        modelBuilder.Entity<ScheduledTrip>(entity =>
        {
            entity.HasOne(st => st.Route)
                .WithMany()
                .HasForeignKey(st => st.RouteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(st => st.ServiceCalendar)
                .WithMany(sc => sc.ScheduledTrips)
                .HasForeignKey(st => st.ServiceCalendarId)
                .OnDelete(DeleteBehavior.Restrict);
        });


        modelBuilder.Entity<RouteStop>(entity =>
        {
            entity.HasOne(rs => rs.Line)
                .WithMany(l => l.RouteStops)
                .HasForeignKey(rs => rs.LineId);

            entity.HasOne(rs => rs.Stop)
                .WithMany(s => s.RouteStops)
                .HasForeignKey(rs => rs.StopId);

            // Nowa relacja z wariantem trasy
            entity.HasOne(rs => rs.Route)
                .WithMany(r => r.RouteStops)
                .HasForeignKey(rs => rs.RouteId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasIndex(rs => new
            {
                rs.RouteId,
                rs.Sequence
            })
            .IsUnique()
            .HasFilter("\"RouteId\" IS NOT NULL");

        });

        modelBuilder.Entity<Vehicle>()
            .HasIndex(v => v.SideNumber)
            .IsUnique();

        modelBuilder.Entity<Payment>()
            .HasIndex(p => p.TransactionId)
            .IsUnique();

        modelBuilder.Entity<Payment>()
            .HasCheckConstraint("CK_Payment_PaymentMethod", "\"PaymentMethod\" IN (1, 2, 3)");

        SeedUserData(modelBuilder);
        SeedEconomicData(modelBuilder);
        SeedVehiclesData(modelBuilder);
        SeedPaymentsData(modelBuilder);
        SeedTicketsData(modelBuilder);
    }

    private void SeedUserData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Name = "Piotr",
                Surname = "Kierowca",
                Email = "driver@driver.com",
                PasswordHash = "$2a$12$3s7iX0hZX00hn6JFwKJ06elVcg0A5mw9LVx4QHvaZW.Q3MWEyrPA2", //admin123
                Role = UserRole.Driver,
                Balance = 100.00m
            },
            new User
            {
                Id = 2,
                Name = "Piotr",
                Surname = "Bołoz",
                Email = "user@user.com",
                PasswordHash = "$2a$12$3s7iX0hZX00hn6JFwKJ06elVcg0A5mw9LVx4QHvaZW.Q3MWEyrPA2", //user123
                Role = UserRole.Passenger,
                Balance = 150.00m
            },
            new User
            {
                Id = 3,
                Name = "Dominik",
                Surname = "Florek",
                Email = "admin@admin.com",
                PasswordHash = "$2a$12$3s7iX0hZX00hn6JFwKJ06elVcg0A5mw9LVx4QHvaZW.Q3MWEyrPA2", //user123
                Role = UserRole.Admin,
                Balance = 100.00m
            }
        );
    }

    private void SeedPaymentsData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>().HasData(
            new Payment
            {
                Id = 1,
                UserId = 2,
                Amount = 20.00m,
                Currency = Currency.PLN,
                ExchangeRate = 1.00m,
                AmountInBaseCurrency = 20.00m,
                PaymentMethod = PaymentMethod.Card,
                TransactionId = "TNX-20260520-A1B2C3D4",
                Status = PaymentStatus.Completed,
                CreatedAt = new DateTime(2026, 5, 20, 10, 0, 0, DateTimeKind.Utc)
            },
            new Payment
            {
                Id = 2,
                UserId = 2,
                Amount = 30.00m,
                Currency = Currency.PLN,
                ExchangeRate = 1.00m,
                AmountInBaseCurrency = 30.00m,
                PaymentMethod = PaymentMethod.Card,
                TransactionId = "TNX-20260521-E5F6G7H8",
                Status = PaymentStatus.Completed,
                CreatedAt = new DateTime(2026, 5, 21, 14, 30, 0, DateTimeKind.Utc)
            }
        );
    }

    private void SeedEconomicData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TicketType>().HasData(
            new TicketType { Id = 1, Name = "Normalny - Miejski", Price = 4.00m, ZoneLimit = 0, DurationInMinutes = 0 },
            new TicketType { Id = 2, Name = "Ustawowy Ulgowy - Miejski", Price = 2.00m, ZoneLimit = 0, DurationInMinutes = 0 },
            new TicketType { Id = 3, Name = "Lokalny Ulgowy - Miejski", Price = 2.35m, ZoneLimit = 0, DurationInMinutes = 0 },

            new TicketType { Id = 4, Name = "Normalny - Strefa 1", Price = 4.80m, ZoneLimit = 1, DurationInMinutes = 0 },
            new TicketType { Id = 5, Name = "Ustawowy Ulgowy - Strefa 1", Price = 2.40m, ZoneLimit = 1, DurationInMinutes = 0 },
            new TicketType { Id = 6, Name = "Lokalny Ulgowy - Strefa 1", Price = 2.65m, ZoneLimit = 1, DurationInMinutes = 0 },

            new TicketType { Id = 7, Name = "Normalny - Strefa 2", Price = 5.90m, ZoneLimit = 2, DurationInMinutes = 0 },
            new TicketType { Id = 8, Name = "Ustawowy Ulgowy - Strefa 2", Price = 2.95m, ZoneLimit = 2, DurationInMinutes = 0 },
            new TicketType { Id = 9, Name = "Lokalny Ulgowy - Strefa 2", Price = 3.10m, ZoneLimit = 2, DurationInMinutes = 0 },

            new TicketType { Id = 10, Name = "Gminny Normalny", Price = 3.00m, ZoneLimit = 1, DurationInMinutes = 0 },
            new TicketType { Id = 11, Name = "Gminny Ustawowy Ulgowy", Price = 1.50m, ZoneLimit = 1, DurationInMinutes = 0 },
            new TicketType { Id = 12, Name = "Gminny Lokalny Ulgowy", Price = 1.80m, ZoneLimit = 1, DurationInMinutes = 0 },

            new TicketType { Id = 13, Name = "Przesiadkowy 60 min - Normalny", Price = 7.00m, ZoneLimit = 99, DurationInMinutes = 60 },
            new TicketType { Id = 14, Name = "Przesiadkowy 60 min - Ustawowy Ulgowy", Price = 3.50m, ZoneLimit = 99, DurationInMinutes = 60 },
            new TicketType { Id = 15, Name = "Przesiadkowy 60 min - Lokalny Ulgowy", Price = 3.90m, ZoneLimit = 99, DurationInMinutes = 60 },

            new TicketType { Id = 16, Name = "Przesiadkowy 4h - Normalny", Price = 12.60m, ZoneLimit = 99, DurationInMinutes = 240 },
            new TicketType { Id = 17, Name = "Przesiadkowy 4h - Ustawowy Ulgowy", Price = 6.30m, ZoneLimit = 99, DurationInMinutes = 240 },
            new TicketType { Id = 18, Name = "Przesiadkowy 4h - Lokalny Ulgowy", Price = 7.20m, ZoneLimit = 99, DurationInMinutes = 240 }
        );
    }

    private void SeedVehiclesData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Vehicle>().HasData(
            new Vehicle { Id = 1, SideNumber = "101", Model = "Solaris Urbino 12", IsActive = true },
            new Vehicle { Id = 2, SideNumber = "102", Model = "Solaris Urbino 12", IsActive = true },
            new Vehicle { Id = 3, SideNumber = "103", Model = "Solaris Urbino 12", IsActive = true },
            new Vehicle { Id = 4, SideNumber = "104", Model = "Solaris Urbino 12", IsActive = true },
            new Vehicle { Id = 5, SideNumber = "105", Model = "Solaris Urbino 12", IsActive = true },

            new Vehicle { Id = 6, SideNumber = "201", Model = "Solaris Urbino 18", IsActive = true },
            new Vehicle { Id = 7, SideNumber = "202", Model = "Solaris Urbino 18", IsActive = true },

            new Vehicle { Id = 8, SideNumber = "301", Model = "Mercedes-Benz Citaro", IsActive = true },
            new Vehicle { Id = 9, SideNumber = "302", Model = "Mercedes-Benz Citaro", IsActive = true },
            new Vehicle { Id = 10, SideNumber = "303", Model = "Mercedes-Benz Citaro", IsActive = true },

            new Vehicle { Id = 11, SideNumber = "401", Model = "Autosan SanCity 9LE", IsActive = true },
            new Vehicle { Id = 12, SideNumber = "402", Model = "Autosan SanCity 9LE", IsActive = true },
            new Vehicle { Id = 13, SideNumber = "403", Model = "Autosan SanCity 12LF", IsActive = true },

            new Vehicle { Id = 14, SideNumber = "501", Model = "MAN Lion's City", IsActive = true },
            new Vehicle { Id = 15, SideNumber = "502", Model = "MAN Lion's City", IsActive = true },

            new Vehicle { Id = 16, SideNumber = "601", Model = "Iveco Daily 70C", IsActive = true },
            new Vehicle { Id = 17, SideNumber = "602", Model = "Iveco Daily 70C", IsActive = true }
        );
    }
    private void SeedTicketsData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ticket>().HasData(
            // New Ticket
            new Ticket
            {
                Id = 1,
                UserId = 2,
                TicketTypeId = 1, // Normalny - Miejski (4.00 PLN)
                Price = 4.00m,
                PurchasedAt = new DateTime(2026, 5, 30, 15, 0, 0, DateTimeKind.Utc),
                IsValidated = false,
                VehicleId = null,
                ValidFrom = null,
                ValidTo = null
            },

            // Active
            new Ticket
            {
                Id = 2,
                UserId = 2,
                TicketTypeId = 13, // Przesiadkowy 60 min - Normalny (7.00 PLN)
                Price = 7.00m,
                PurchasedAt = new DateTime(2026, 5, 31, 18, 0, 0, DateTimeKind.Utc),
                IsValidated = true,
                VehicleId = 1, // Solaris Urbino 12 (101)
                ValidFrom = new DateTime(2026, 5, 31, 18, 5, 0, DateTimeKind.Utc),
                ValidTo = new DateTime(2026, 5, 31, 19, 5, 0, DateTimeKind.Utc)
            },

            // Expired
            new Ticket
            {
                Id = 3,
                UserId = 2,
                TicketTypeId = 13, // Przesiadkowy 60 min - Normalny (7.00 PLN)
                Price = 7.00m,
                PurchasedAt = new DateTime(2026, 5, 25, 8, 0, 0, DateTimeKind.Utc),
                IsValidated = true,
                VehicleId = 8, // Mercedes-Benz Citaro (301)
                ValidFrom = new DateTime(2026, 5, 25, 8, 10, 0, DateTimeKind.Utc),
                ValidTo = new DateTime(2026, 5, 25, 9, 10, 0, DateTimeKind.Utc)
            }
        );
    }
}
