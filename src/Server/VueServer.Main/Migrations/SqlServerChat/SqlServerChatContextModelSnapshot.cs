﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VueServer.Modules.Chat.Context;

namespace VueServer.Migrations.SqlServerChat
{
    [DbContext(typeof(SqlServerChatContext))]
    partial class SqlServerChatContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.14")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("VueServer.Modules.Chat.Models.ChatMessage", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<long>("ConversationId")
                        .HasColumnType("bigint");

                    b.Property<string>("Text")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("Timestamp")
                        .HasColumnType("bigint");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(64)");

                    b.HasKey("Id");

                    b.HasIndex("ConversationId");

                    b.HasIndex("UserId");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("VueServer.Modules.Chat.Models.Conversation", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Avatar")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Conversations");
                });

            modelBuilder.Entity("VueServer.Modules.Chat.Models.ConversationHasUser", b =>
                {
                    b.Property<long>("ConversationId")
                        .HasColumnType("bigint");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("Color")
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<bool>("Owner")
                        .HasColumnType("bit");

                    b.HasKey("ConversationId", "UserId");

                    b.HasIndex("UserId");

                    b.ToTable("ConversationHasUser");
                });

            modelBuilder.Entity("VueServer.Modules.Chat.Models.ReadReceipt", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<long>("MessageId")
                        .HasColumnType("bigint");

                    b.Property<long>("Timestamp")
                        .HasColumnType("bigint");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(64)");

                    b.HasKey("Id");

                    b.HasIndex("MessageId");

                    b.HasIndex("UserId");

                    b.ToTable("ReadReceipts");
                });

            modelBuilder.Entity("VueServer.Modules.Core.Models.Modules.ModuleAddOn", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

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
                        },
                        new
                        {
                            Id = "chat",
                            Name = "Chat"
                        });
                });

            modelBuilder.Entity("VueServer.Modules.Core.Models.Modules.ModuleFeature", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ModuleAddOnId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("ModuleAddOnId");

                    b.ToTable("Features");

                    b.HasData(
                        new
                        {
                            Id = "chat-delete-message",
                            ModuleAddOnId = "chat",
                            Name = "Delete Messages"
                        },
                        new
                        {
                            Id = "chat-delete-conversation",
                            ModuleAddOnId = "chat",
                            Name = "Delete Conversations"
                        });
                });

            modelBuilder.Entity("VueServer.Modules.Core.Models.Modules.UserHasModuleAddOn", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("ModuleAddOnId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("UserId", "ModuleAddOnId");

                    b.HasIndex("ModuleAddOnId");

                    b.ToTable("UserHasModule");
                });

            modelBuilder.Entity("VueServer.Modules.Core.Models.Modules.UserHasModuleFeature", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("ModuleFeatureId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ModuleAddOnId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("UserId", "ModuleFeatureId");

                    b.HasIndex("ModuleAddOnId");

                    b.HasIndex("ModuleFeatureId");

                    b.ToTable("UserHasFeature");
                });

            modelBuilder.Entity("VueServer.Modules.Core.Models.Notes", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Color")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset>("Created")
                        .HasColumnType("datetimeoffset");

                    b.Property<int>("Priority")
                        .HasColumnType("int");

                    b.Property<string>("Text")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<DateTimeOffset>("Updated")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(64)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Notes");
                });

            modelBuilder.Entity("VueServer.Modules.Core.Models.ServerSettings", b =>
                {
                    b.Property<string>("Key")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Key");

                    b.ToTable("ServerSettings");
                });

            modelBuilder.Entity("VueServer.Modules.Core.Models.User.WSGuestLogin", b =>
                {
                    b.Property<string>("IPAddress")
                        .HasMaxLength(45)
                        .HasColumnType("nvarchar(45)");

                    b.Property<bool>("Blocked")
                        .HasColumnType("bit");

                    b.Property<long>("ClusterId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("FailedLogins")
                        .HasColumnType("int");

                    b.HasKey("IPAddress")
                        .IsClustered(false);

                    b.HasIndex("ClusterId")
                        .IsUnique()
                        .IsClustered();

                    b.ToTable("GuestLogin");
                });

            modelBuilder.Entity("VueServer.Modules.Core.Models.User.WSRole", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<int>("ClusterId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("DisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NormalizedName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id")
                        .IsClustered(false);

                    b.HasIndex("ClusterId")
                        .IsUnique()
                        .IsClustered();

                    b.ToTable("Roles");

                    b.HasData(
                        new
                        {
                            Id = "user",
                            ClusterId = 1,
                            DisplayName = "User",
                            NormalizedName = "USER"
                        },
                        new
                        {
                            Id = "elevated",
                            ClusterId = 2,
                            DisplayName = "Elevated",
                            NormalizedName = "ELEVATED"
                        },
                        new
                        {
                            Id = "administrator",
                            ClusterId = 3,
                            DisplayName = "Administrator",
                            NormalizedName = "ADMINISTRATOR"
                        });
                });

            modelBuilder.Entity("VueServer.Modules.Core.Models.User.WSUser", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<bool>("Active")
                        .HasColumnType("bit");

                    b.Property<long>("ClusterId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("DisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NormalizedUserName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PasswordExpired")
                        .HasColumnType("bit");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id")
                        .IsClustered(false);

                    b.HasIndex("ClusterId")
                        .IsUnique()
                        .IsClustered();

                    b.ToTable("Users");

                    b.HasData(
                        new
                        {
                            Id = "admin",
                            Active = false,
                            ClusterId = 1L,
                            DisplayName = "Admin",
                            NormalizedUserName = "ADMIN",
                            PasswordExpired = true,
                            PasswordHash = "AQAAAAEAACcQAAAAEOjW3u9tWhKV613KX3PHsfggJluMrmY5hCFohLCY39I83/1aU+9zlbndXQ3a6xA8Cg=="
                        });
                });

            modelBuilder.Entity("VueServer.Modules.Core.Models.User.WSUserInRoles", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("RoleId")
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(64)");

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
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("IPAddress")
                        .HasMaxLength(45)
                        .HasColumnType("nvarchar(45)");

                    b.Property<bool>("Success")
                        .HasColumnType("bit");

                    b.Property<long>("Timestamp")
                        .HasColumnType("bigint");

                    b.Property<string>("Username")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.HasKey("Id");

                    b.ToTable("UserLogin");
                });

            modelBuilder.Entity("VueServer.Modules.Core.Models.User.WSUserProfile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("AvatarPath")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(64)");

                    b.HasKey("Id");

                    b.HasIndex("UserId")
                        .IsUnique()
                        .HasFilter("[UserId] IS NOT NULL");

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
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClientId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("IPAddress")
                        .HasMaxLength(45)
                        .HasColumnType("nvarchar(45)");

                    b.Property<DateTime>("Issued")
                        .HasColumnType("datetime2");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(64)");

                    b.Property<bool>("Valid")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("UserTokens");
                });

            modelBuilder.Entity("VueServer.Modules.Chat.Models.ChatMessage", b =>
                {
                    b.HasOne("VueServer.Modules.Chat.Models.Conversation", "Conversation")
                        .WithMany("Messages")
                        .HasForeignKey("ConversationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("VueServer.Modules.Core.Models.User.WSUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("Conversation");

                    b.Navigation("User");
                });

            modelBuilder.Entity("VueServer.Modules.Chat.Models.ConversationHasUser", b =>
                {
                    b.HasOne("VueServer.Modules.Chat.Models.Conversation", "Conversation")
                        .WithMany("ConversationUsers")
                        .HasForeignKey("ConversationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("VueServer.Modules.Core.Models.User.WSUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Conversation");

                    b.Navigation("User");
                });

            modelBuilder.Entity("VueServer.Modules.Chat.Models.ReadReceipt", b =>
                {
                    b.HasOne("VueServer.Modules.Chat.Models.ChatMessage", "Message")
                        .WithMany("ReadReceipts")
                        .HasForeignKey("MessageId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("VueServer.Modules.Core.Models.User.WSUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId");

                    b.Navigation("Message");

                    b.Navigation("User");
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

            modelBuilder.Entity("VueServer.Modules.Chat.Models.ChatMessage", b =>
                {
                    b.Navigation("ReadReceipts");
                });

            modelBuilder.Entity("VueServer.Modules.Chat.Models.Conversation", b =>
                {
                    b.Navigation("ConversationUsers");

                    b.Navigation("Messages");
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
