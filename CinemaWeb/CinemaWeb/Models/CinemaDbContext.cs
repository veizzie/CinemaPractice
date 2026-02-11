using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace CinemaWeb.Models;

public partial class CinemaDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public CinemaDbContext()
    {
    }

    public CinemaDbContext(DbContextOptions<CinemaDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Genre> Genres { get; set; }

    public virtual DbSet<Hall> Halls { get; set; }

    public virtual DbSet<Movie> Movies { get; set; }

    public virtual DbSet<Moviegenre> Moviegenres { get; set; }

    public virtual DbSet<Seat> Seats { get; set; }

    public virtual DbSet<Session> Sessions { get; set; }

    public virtual DbSet<Ticket> Tickets { get; set; }

    // public virtual DbSet<User> Users { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Genre>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__genres__3213E83F5527D2A4");

            entity.ToTable("genres");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Hall>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__halls__3213E83FE2BA369E");

            entity.ToTable("halls");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.Capacity).HasColumnName("capacity");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__movies__3213E83F1B2C4912");

            entity.ToTable("movies");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Cast).HasColumnName("cast");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Director)
                .HasMaxLength(155)
                .HasColumnName("director");
            entity.Property(e => e.Duration).HasColumnName("duration");
            entity.Property(e => e.PosterUrl)
                .HasMaxLength(2048)
                .HasColumnName("poster_url");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price");
            entity.Property(e => e.ReleaseDate).HasColumnName("release_date");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
        });

        modelBuilder.Entity<Moviegenre>(entity =>
        {
            entity.HasKey(e => new { e.MovieId, e.GenreId }).HasName("PK__moviegen__C6D8E3B2D3B8F1E3");

            entity.ToTable("moviegenres");

            entity.Property(e => e.GenreId).HasColumnName("genre_id");
            entity.Property(e => e.MovieId).HasColumnName("movie_id");

            entity.HasOne(d => d.Genre).WithMany(p => p.Moviegenres)
                .HasForeignKey(d => d.GenreId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("moviegenres_fk1");

            entity.HasOne(d => d.Movie).WithMany(p => p.Moviegenres)
                .HasForeignKey(d => d.MovieId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("moviegenres_fk0");
        });

        modelBuilder.Entity<Seat>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__seats__3213E83F9F562DD2");

            entity.ToTable("seats");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.HallId).HasColumnName("hall_id");
            entity.Property(e => e.Number).HasColumnName("number");
            entity.Property(e => e.Row).HasColumnName("row");

            entity.HasOne(d => d.Hall).WithMany(p => p.Seats)
                .HasForeignKey(d => d.HallId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("seats_fk1");
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__sessions__3213E83FD61AA622");

            entity.ToTable("sessions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.HallId).HasColumnName("hall_id");
            entity.Property(e => e.MovieId).HasColumnName("movie_id");
            entity.Property(e => e.StartTime)
                .HasColumnType("datetime")
                .HasColumnName("start_time");

            entity.HasOne(d => d.Hall).WithMany(p => p.Sessions)
                .HasForeignKey(d => d.HallId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("sessions_fk2");

            entity.HasOne(d => d.Movie).WithMany(p => p.Sessions)
                .HasForeignKey(d => d.MovieId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("sessions_fk1");
        });

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tickets__3213E83F5B3F3815");

            entity.ToTable("tickets");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PurchaseDate)
                .HasColumnType("datetime")
                .HasColumnName("purchase_date");
            entity.Property(e => e.SeatId).HasColumnName("seat_id");
            entity.Property(e => e.SessionId).HasColumnName("session_id");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Seat).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.SeatId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("tickets_fk3");

            entity.HasOne(d => d.Session).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("tickets_fk2");

            entity.HasOne(d => d.User).WithMany(p => p.Tickets)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("tickets_fk1");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.FullName)
                .HasMaxLength(155)
                .HasColumnName("full_name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
