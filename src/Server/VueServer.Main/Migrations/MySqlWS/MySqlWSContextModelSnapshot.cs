﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VueServer.Modules.Core.Context;

#nullable disable

namespace VueServer.Migrations.MySqlWS
{
    [DbContext(typeof(MySqlWSContext))]
    partial class MySqlWSContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("VueServer.Modules.Core.Models.Modules.ModuleAddOn", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Name")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Modules");

                    b.HasData(
                        new
                        {
                            Id = "documentation",
                            Name = "Documentation"
                        },
                        new
                        {
                            Id = "notes",
                            Name = "Notes"
                        });
                });

            modelBuilder.Entity("VueServer.Modules.Core.Models.Modules.ModuleFeature", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ModuleAddOnId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Name")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("ModuleAddOnId");

                    b.ToTable("Features");
                });

            modelBuilder.Entity("VueServer.Modules.Core.Models.Modules.UserHasModuleAddOn", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("varchar(64)");

                    b.Property<string>("ModuleAddOnId")
                        .HasColumnType("varchar(255)");

                    b.HasKey("UserId", "ModuleAddOnId");

                    b.HasIndex("ModuleAddOnId");

                    b.ToTable("UserHasModule");
                });

            modelBuilder.Entity("VueServer.Modules.Core.Models.Modules.UserHasModuleFeature", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("varchar(64)");

                    b.Property<string>("ModuleFeatureId")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("ModuleAddOnId")
                        .HasColumnType("varchar(255)");

                    b.HasKey("UserId", "ModuleFeatureId");

                    b.HasIndex("ModuleAddOnId");

                    b.HasIndex("ModuleFeatureId");

                    b.ToTable("UserHasFeature");
                });

            modelBuilder.Entity("VueServer.Modules.Core.Models.Notes", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Color")
                        .HasColumnType("longtext");

                    b.Property<DateTimeOffset>("Created")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("Priority")
                        .HasColumnType("int");

                    b.Property<string>("Text")
                        .HasColumnType("longtext");

                    b.Property<string>("Title")
                        .HasColumnType("longtext");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset>("Updated")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("UserId")
                        .HasColumnType("varchar(64)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Notes");
                });

            modelBuilder.Entity("VueServer.Modules.Core.Models.ServerSettings", b =>
                {
                    b.Property<string>("Key")
                        .HasColumnType("varchar(255)");

                    b.Property<string>("Value")
                        .HasColumnType("longtext");

                    b.HasKey("Key");

                    b.ToTable("ServerSettings");
                });

            modelBuilder.Entity("VueServer.Modules.Core.Models.User.WSGuestLogin", b =>
                {
                    b.Property<string>("IPAddress")
                        .HasMaxLength(45)
                        .HasColumnType("varchar(45)");

                    b.Property<bool>("Blocked")
                        .HasColumnType("tinyint(1)");

                    b.Property<int>("FailedLogins")
                        .HasColumnType("int");

                    b.HasKey("IPAddress");

                    b.ToTable("GuestLogin");
                });

            modelBuilder.Entity("VueServer.Modules.Core.Models.User.WSRole", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<string>("DisplayName")
                        .HasColumnType("longtext");

                    b.Property<string>("NormalizedName")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Roles");

                    b.HasData(
                        new
                        {
                            Id = "user",
                            DisplayName = "User",
                            NormalizedName = "USER"
                        },
                        new
                        {
                            Id = "elevated",
                            DisplayName = "Elevated",
                            NormalizedName = "ELEVATED"
                        },
                        new
                        {
                            Id = "administrator",
                            DisplayName = "Administrator",
                            NormalizedName = "ADMINISTRATOR"
                        });
                });

            modelBuilder.Entity("VueServer.Modules.Core.Models.User.WSUser", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.Property<bool>("Active")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("DisplayName")
                        .HasColumnType("longtext");

                    b.Property<string>("NormalizedUserName")
                        .HasColumnType("longtext");

                    b.Property<bool>("PasswordExpired")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Users");

                    b.HasData(
                        new
                        {
                            Id = "admin",
                            Active = false,
                            DisplayName = "Admin",
                            NormalizedUserName = "ADMIN",
                            PasswordExpired = true,
                            PasswordHash = "AQAAAAIAAYagAAAAEFImDvICDOKpo/t5FCvSUlj0lL9aI3L5yztlmlCCAqo+gu7CKIpPng41JiHeVZ5b2g=="
                        });
                });

            modelBuilder.Entity("VueServer.Modules.Core.Models.User.WSUserInRoles", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<string>("RoleId")
                        .HasColumnType("varchar(64)");

                    b.Property<string>("UserId")
                        .HasColumnType("varchar(64)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.HasIndex("UserId");

                    b.ToTable("UserRoles");

                    b.HasData(
                        new
                        {
                            Id = 1L,
                            RoleId = "administrator",
                            UserId = "admin"
                        });
                });

            modelBuilder.Entity("VueServer.Modules.Core.Models.User.WSUserLogin", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    b.Property<string>("IPAddress")
                        .HasMaxLength(45)
                        .HasColumnType("varchar(45)");

                    b.Property<bool>("Success")
                        .HasColumnType("tinyint(1)");

                    b.Property<long>("Timestamp")
                        .HasColumnType("bigint");

                    b.Property<string>("Username")
                        .HasMaxLength(64)
                        .HasColumnType("varchar(64)");

                    b.HasKey("Id");

                    b.ToTable("UserLogin");
                });

            modelBuilder.Entity("VueServer.Modules.Core.Models.User.WSUserProfile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("AvatarPath")
                        .HasColumnType("longtext");

                    b.Property<string>("UserId")
                        .HasColumnType("varchar(64)");

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("UserProfile");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            UserId = "admin"
                        });
                });

            modelBuilder.Entity("VueServer.Modules.Core.Models.User.WSUserTokens", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("ClientId")
                        .HasColumnType("longtext");

                    b.Property<string>("IPAddress")
                        .HasMaxLength(45)
                        .HasColumnType("varchar(45)");

                    b.Property<DateTime>("Issued")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("varchar(64)");

                    b.Property<bool>("Valid")
                        .HasColumnType("tinyint(1)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("UserTokens");
                });

            modelBuilder.Entity("VueServer.Modules.Core.Models.Modules.ModuleFeature", b =>
                {
                    b.HasOne("VueServer.Modules.Core.Models.Modules.ModuleAddOn", "ModuleAddOn")
                        .WithMany("Features")
                        .HasForeignKey("ModuleAddOnId");

                    b.Navigation("ModuleAddOn");
                });

            modelBuilder.Entity("VueServer.Modules.Core.Models.Modules.UserHasModuleAddOn", b =>
                {
                    b.HasOne("VueServer.Modules.Core.Models.Modules.ModuleAddOn", "ModuleAddOn")
                        .WithMany("UserModuleAddOns")
                        .HasForeignKey("ModuleAddOnId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("VueServer.Modules.Core.Models.User.WSUser", "User")
                        .WithMany("UserModuleAddOns")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ModuleAddOn");

                    b.Navigation("User");
                });

            modelBuilder.Entity("VueServer.Modules.Core.Models.Modules.UserHasModuleFeature", b =>
                {
                    b.HasOne("VueServer.Modules.Core.Models.Modules.ModuleAddOn", null)
                        .WithMany("UserModuleFeatures")
                        .HasForeignKey("ModuleAddOnId");

                    b.HasOne("VueServer.Modules.Core.Models.Modules.ModuleFeature", "ModuleFeature")
                        .WithMany("UserModuleFeatures")
                        .HasForeignKey("ModuleFeatureId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("VueServer.Modules.Core.Models.User.WSUser", "User")
                        .WithMany("UserModuleFeatures")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ModuleFeature");

                    b.Navigation("User");
                });

            modelBuilder.Entity("VueServer.Modules.Core.Models.Notes", b =>
                {
                    b.HasOne("VueServer.Modules.Core.Models.User.WSUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("User");
                });

            modelBuilder.Entity("VueServer.Modules.Core.Models.User.WSUserInRoles", b =>
                {
                    b.HasOne("VueServer.Modules.Core.Models.User.WSRole", "Role")
                        .WithMany()
                        .HasForeignKey("RoleId");

                    b.HasOne("VueServer.Modules.Core.Models.User.WSUser", "User")
                        .WithMany("Roles")
                        .HasForeignKey("UserId");

                    b.Navigation("Role");

                    b.Navigation("User");
                });

            modelBuilder.Entity("VueServer.Modules.Core.Models.User.WSUserProfile", b =>
                {
                    b.HasOne("VueServer.Modules.Core.Models.User.WSUser", "User")
                        .WithOne("UserProfile")
                        .HasForeignKey("VueServer.Modules.Core.Models.User.WSUserProfile", "UserId");

                    b.Navigation("User");
                });

            modelBuilder.Entity("VueServer.Modules.Core.Models.User.WSUserTokens", b =>
                {
                    b.HasOne("VueServer.Modules.Core.Models.User.WSUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("VueServer.Modules.Core.Models.Modules.ModuleAddOn", b =>
                {
                    b.Navigation("Features");

                    b.Navigation("UserModuleAddOns");

                    b.Navigation("UserModuleFeatures");
                });

            modelBuilder.Entity("VueServer.Modules.Core.Models.Modules.ModuleFeature", b =>
                {
                    b.Navigation("UserModuleFeatures");
                });

            modelBuilder.Entity("VueServer.Modules.Core.Models.User.WSUser", b =>
                {
                    b.Navigation("Roles");

                    b.Navigation("UserModuleAddOns");

                    b.Navigation("UserModuleFeatures");

                    b.Navigation("UserProfile");
                });
#pragma warning restore 612, 618
        }
    }
}
