using ConfidoSoft.Data.Domain.DBModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConfidoSoft.Data.Domain.Database
{

    public class ApplicationDbContext : IdentityDbContext<User, Role, long>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Setting> Settings { get; set; }
        public virtual DbSet<UserProfile> UserProfiles { get; set; }
        public virtual DbSet<RolePermission> RolePermissions { get; set; }
        public virtual DbSet<UserRefreshToken> UserRefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //One to one relation with PK as FK...
            modelBuilder.Entity<User>()
                .HasOne(u => u.UserProfile)
                .WithOne(p => p.User)
                .HasForeignKey<UserProfile>(pe => pe.Id)
                .OnDelete(DeleteBehavior.Cascade);

            //User and UserRefreshToken PK and FK..
            modelBuilder.Entity<UserRefreshToken>()
              .HasOne(d => d.User)
              .WithMany(p => p.UserRefreshTokens)
              .HasForeignKey(d => d.UserId)
              .OnDelete(DeleteBehavior.Cascade);
            //Must be unique..
            modelBuilder.Entity<UserRefreshToken>()
            .HasIndex(b => b.RefreshToken)
            .IsUnique();

            //User Role navigation property
            modelBuilder.Entity<User>()
                .HasMany(e => e.UserRoles)
                .WithOne()
                .HasForeignKey(e => e.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Role>()
                .HasMany(e => e.UserRoles)
                .WithOne()
                .HasForeignKey(e => e.RoleId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            //Role and Permissions
            modelBuilder.Entity<RolePermission>()
              .HasOne(d => d.Role)
              .WithMany(p => p.RolePermissions)
              .HasForeignKey(d => d.RoleId)
              .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
