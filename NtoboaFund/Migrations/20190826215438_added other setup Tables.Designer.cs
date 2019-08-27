﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NtoboaFund.Data.DBContext;

namespace NtoboaFund.Migrations
{
    [DbContext(typeof(NtoboaFundDbContext))]
    [Migration("20190826215438_added other setup Tables")]
    partial class addedothersetupTables
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.8-servicing-32085")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("NtoboaFund.Data.Models.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<string>("FirstName");

                    b.Property<string>("LastName");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("PreferedMoneyReceptionMethod");

                    b.Property<string>("SecurityStamp");

                    b.Property<string>("Token");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("NtoboaFund.Data.Models.BankDetails", b =>
                {
                    b.Property<string>("BankDetailsId");

                    b.Property<string>("AccountNumber");

                    b.Property<string>("BankName");

                    b.Property<string>("SwiftCode");

                    b.HasKey("BankDetailsId");

                    b.ToTable("BankDetails");
                });

            modelBuilder.Entity("NtoboaFund.Data.Models.Business", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<decimal>("Amount");

                    b.Property<decimal>("AmountToWin");

                    b.Property<string>("Date");

                    b.Property<string>("DateDeclared");

                    b.Property<string>("Period");

                    b.Property<string>("Status");

                    b.Property<int>("TransferId");

                    b.Property<string>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Businesses");
                });

            modelBuilder.Entity("NtoboaFund.Data.Models.LuckyMe", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<decimal>("Amount");

                    b.Property<decimal>("AmountToWin");

                    b.Property<string>("Date");

                    b.Property<string>("DateDeclared");

                    b.Property<string>("Period");

                    b.Property<string>("Status");

                    b.Property<int>("TransferId");

                    b.Property<string>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("LuckyMes");
                });

            modelBuilder.Entity("NtoboaFund.Data.Models.MobileMoneyDetails", b =>
                {
                    b.Property<string>("MobileMoneyDetailsId");

                    b.Property<string>("Country");

                    b.Property<string>("Network");

                    b.Property<string>("Number");

                    b.Property<string>("Voucher");

                    b.HasKey("MobileMoneyDetailsId");

                    b.ToTable("MobileMoneyDetails");
                });

            modelBuilder.Entity("NtoboaFund.Data.Models.Scholarship", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<decimal>("Amount");

                    b.Property<decimal>("AmountToWin");

                    b.Property<string>("Date");

                    b.Property<string>("DateDeclared");

                    b.Property<string>("Institution")
                        .IsRequired();

                    b.Property<string>("Period");

                    b.Property<string>("Program")
                        .IsRequired();

                    b.Property<string>("Status");

                    b.Property<string>("StudentId")
                        .IsRequired();

                    b.Property<int>("TransferId");

                    b.Property<string>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Scholarships");
                });

            modelBuilder.Entity("NtoboaFund.Data.Models.Status", b =>
                {
                    b.Property<int>("StatusId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.HasKey("StatusId");

                    b.ToTable("Statuses");
                });

            modelBuilder.Entity("NtoboaFund.Data.Models.Transfer", b =>
                {
                    b.Property<int>("TransferId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("account_number");

                    b.Property<int>("amount");

                    b.Property<string>("bank_code");

                    b.Property<string>("bank_name");

                    b.Property<string>("complete_message");

                    b.Property<string>("currency");

                    b.Property<DateTime>("date_created");

                    b.Property<int>("fee");

                    b.Property<string>("fullname");

                    b.Property<int>("id");

                    b.Property<int>("is_approved");

                    b.Property<string>("message");

                    b.Property<string>("narration");

                    b.Property<string>("reference");

                    b.Property<int>("requires_approval");

                    b.Property<string>("status");

                    b.HasKey("TransferId");

                    b.ToTable("Transfer");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("NtoboaFund.Data.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("NtoboaFund.Data.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("NtoboaFund.Data.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("NtoboaFund.Data.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NtoboaFund.Data.Models.BankDetails", b =>
                {
                    b.HasOne("NtoboaFund.Data.Models.ApplicationUser", "User")
                        .WithOne("BankDetails")
                        .HasForeignKey("NtoboaFund.Data.Models.BankDetails", "BankDetailsId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NtoboaFund.Data.Models.Business", b =>
                {
                    b.HasOne("NtoboaFund.Data.Models.ApplicationUser", "User")
                        .WithMany("Businesses")
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("NtoboaFund.Data.Models.LuckyMe", b =>
                {
                    b.HasOne("NtoboaFund.Data.Models.ApplicationUser", "User")
                        .WithMany("LuckyMes")
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("NtoboaFund.Data.Models.MobileMoneyDetails", b =>
                {
                    b.HasOne("NtoboaFund.Data.Models.ApplicationUser", "User")
                        .WithOne("MomoDetails")
                        .HasForeignKey("NtoboaFund.Data.Models.MobileMoneyDetails", "MobileMoneyDetailsId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("NtoboaFund.Data.Models.Scholarship", b =>
                {
                    b.HasOne("NtoboaFund.Data.Models.ApplicationUser", "User")
                        .WithMany("Scholarships")
                        .HasForeignKey("UserId");
                });
#pragma warning restore 612, 618
        }
    }
}
