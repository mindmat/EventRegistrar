using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventRegistrar.Backend.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PredecessorEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    State = table.Column<int>(type: "int", nullable: false),
                    Acronym = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccountIban = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_Events_Events_PredecessorEventId",
                        column: x => x.PredecessorEventId,
                        principalTable: "Events",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RawBankStatementsFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Server = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContractIdentifier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Filename = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Imported = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Processed = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Content = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawBankStatementsFiles", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "RawMailEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Processed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawMailEvents", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "RawRegistrationForms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventAcronym = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FormExternalIdentifier = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Processed = table.Column<bool>(type: "bit", nullable: false),
                    ReceivedMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawRegistrationForms", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "RawRegistrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EventAcronym = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FormExternalIdentifier = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Processed = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceivedMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegistrationExternalIdentifier = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RawRegistrations", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdentityProvider = table.Column<int>(type: "int", nullable: false),
                    IdentityProviderUserIdentifier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateTable(
                name: "DomainEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DomainEventId_Parent = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DomainEvents", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_DomainEvents_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EventConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ValueJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventConfigurations", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_EventConfigurations_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ImportedMails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SenderMail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SenderName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Recipients = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ContentHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentPlainText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Imported = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MessageIdentifier = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SendGridMessageId = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportedMails", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_ImportedMails_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentsFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AccountIban = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    BookingsFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BookingsTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentsFiles", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_PaymentsFiles_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PaymentSlips",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FileBinary = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Filename = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Reference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentSlips", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_PaymentSlips_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Registrables",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameSecondary = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    HasWaitingList = table.Column<bool>(type: "bit", nullable: false),
                    AutomaticPromotionFromWaitingList = table.Column<bool>(type: "bit", nullable: false),
                    IsCore = table.Column<bool>(type: "bit", nullable: false),
                    MaximumAllowedImbalance = table.Column<int>(type: "int", nullable: true),
                    MaximumDoubleSeats = table.Column<int>(type: "int", nullable: true),
                    MaximumSingleSeats = table.Column<int>(type: "int", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ReducedPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ShowInMailListOrder = table.Column<int>(type: "int", nullable: true),
                    CheckinListColumn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Tag = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Registrables", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_Registrables_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegistrableTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tag = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FallbackText = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SortKey = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrableTags", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_RegistrableTags_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegistrationForms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalIdentifier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    State = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationForms", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_RegistrationForms_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccessToEventRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId_Requestor = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserId_Responder = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Identifier = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IdentityProvider = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RequestReceived = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequestText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Response = table.Column<int>(type: "int", nullable: true),
                    ResponseText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessToEventRequests", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_AccessToEventRequests_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccessToEventRequests_Users_UserId_Requestor",
                        column: x => x.UserId_Requestor,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AccessToEventRequests_Users_UserId_Responder",
                        column: x => x.UserId_Responder,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UsersInEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersInEvents", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_UsersInEvents_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsersInEvents_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentsFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Charges = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    BookingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Info = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InstructionIdentification = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RawXml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecognizedEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Reference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Repaid = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Settled = table.Column<bool>(type: "bit", nullable: false),
                    Ignore = table.Column<bool>(type: "bit", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_Payments_PaymentsFiles_PaymentsFileId",
                        column: x => x.PaymentsFileId,
                        principalTable: "PaymentsFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MailTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrableId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BulkMailKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentType = table.Column<int>(type: "int", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MailingAudience = table.Column<int>(type: "int", nullable: true),
                    SenderMail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SenderName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Template = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ReleaseImmediately = table.Column<bool>(type: "bit", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailTemplates", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_MailTemplates_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MailTemplates_Registrables_RegistrableId",
                        column: x => x.RegistrableId,
                        principalTable: "Registrables",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Reductions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrableId1_ReductionActivatedIfCombinedWith = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RegistrableId2_ReductionActivatedIfCombinedWith = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OnlyForRole = table.Column<int>(type: "int", nullable: true),
                    ActivatedByReduction = table.Column<bool>(type: "bit", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reductions", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_Reductions_Registrables_RegistrableId",
                        column: x => x.RegistrableId,
                        principalTable: "Registrables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reductions_Registrables_RegistrableId1_ReductionActivatedIfCombinedWith",
                        column: x => x.RegistrableId1_ReductionActivatedIfCombinedWith,
                        principalTable: "Registrables",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Reductions_Registrables_RegistrableId2_ReductionActivatedIfCombinedWith",
                        column: x => x.RegistrableId2_ReductionActivatedIfCombinedWith,
                        principalTable: "Registrables",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RegistrableCompositions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrableId_Contains = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrableCompositions", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_RegistrableCompositions_Registrables_RegistrableId",
                        column: x => x.RegistrableId,
                        principalTable: "Registrables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistrableCompositions_Registrables_RegistrableId_Contains",
                        column: x => x.RegistrableId_Contains,
                        principalTable: "Registrables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SpotMailLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Text = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpotMailLines", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_SpotMailLines_Registrables_RegistrableId",
                        column: x => x.RegistrableId,
                        principalTable: "Registrables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FormPaths",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationFormId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ConfigurationJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SingleConfiguration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PartnerConfiguration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormPaths", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_FormPaths_RegistrationForms_RegistrationFormId",
                        column: x => x.RegistrationFormId,
                        principalTable: "RegistrationForms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationFormId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalId = table.Column<int>(type: "int", nullable: false),
                    Index = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Section = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mapping = table.Column<int>(type: "int", nullable: true),
                    TemplateKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_Questions_RegistrationForms_RegistrationFormId",
                        column: x => x.RegistrationFormId,
                        principalTable: "RegistrationForms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Registrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationFormId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationId_Partner = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AdmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExternalIdentifier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExternalTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FallbackToPartyPass = table.Column<bool>(type: "bit", nullable: true),
                    IsReduced = table.Column<bool>(type: "bit", nullable: false),
                    IsWaitingList = table.Column<bool>(type: "bit", nullable: true),
                    Language = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    OriginalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PartnerNormalized = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PartnerOriginal = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PhoneNormalized = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RemarksProcessed = table.Column<bool>(type: "bit", nullable: false),
                    ReminderLevel = table.Column<int>(type: "int", nullable: false),
                    RespondentEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RespondentFirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RespondentLastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SoldOutMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    State = table.Column<int>(type: "int", nullable: false),
                    WillPayAtCheckin = table.Column<bool>(type: "bit", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Registrations", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_Registrations_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Registrations_RegistrationForms_RegistrationFormId",
                        column: x => x.RegistrationFormId,
                        principalTable: "RegistrationForms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Registrations_Registrations_RegistrationId_Partner",
                        column: x => x.RegistrationId_Partner,
                        principalTable: "Registrations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "IncomingPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DebitorIban = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DebitorName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PaymentSlipId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomingPayments", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_IncomingPayments_Payments_Id",
                        column: x => x.Id,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IncomingPayments_PaymentSlips_PaymentSlipId",
                        column: x => x.PaymentSlipId,
                        principalTable: "PaymentSlips",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OutgoingPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreditorName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreditorIban = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutgoingPayments", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_OutgoingPayments_Payments_Id",
                        column: x => x.Id,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Mails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MailTemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SenderMail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SenderName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Recipients = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContentPlainText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SendGridMessageId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sent = table.Column<DateTime>(type: "datetime2", nullable: true),
                    State = table.Column<int>(type: "int", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: true),
                    Withhold = table.Column<bool>(type: "bit", nullable: false),
                    Discarded = table.Column<bool>(type: "bit", nullable: false),
                    BulkMailKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataTypeFullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mails", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_Mails_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Mails_MailTemplates_MailTemplateId",
                        column: x => x.MailTemplateId,
                        principalTable: "MailTemplates",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "QuestionOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionOptions", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_QuestionOptions_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ImportedMailsToRegistrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImportedMailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportedMailsToRegistrations", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_ImportedMailsToRegistrations_ImportedMails_ImportedMailId",
                        column: x => x.ImportedMailId,
                        principalTable: "ImportedMails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ImportedMailsToRegistrations_Registrations_RegistrationId",
                        column: x => x.RegistrationId,
                        principalTable: "Registrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IndividualReductions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndividualReductions", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_IndividualReductions_Registrations_RegistrationId",
                        column: x => x.RegistrationId,
                        principalTable: "Registrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IndividualReductions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PayoutRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    State = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayoutRequests", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_PayoutRequests_Registrations_RegistrationId",
                        column: x => x.RegistrationId,
                        principalTable: "Registrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegistrationCancellations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Refund = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RefundPercentage = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Received = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationCancellations", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_RegistrationCancellations_Registrations_RegistrationId",
                        column: x => x.RegistrationId,
                        principalTable: "Registrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AccountSid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Error = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorCode = table.Column<int>(type: "int", nullable: true),
                    From = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RawData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Received = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Sent = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SmsSid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SmsStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    To = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sms", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_Sms_Registrations_RegistrationId",
                        column: x => x.RegistrationId,
                        principalTable: "Registrations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Spots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrableId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RegistrationId_Follower = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FirstPartnerJoined = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCancelled = table.Column<bool>(type: "bit", nullable: false),
                    IsPartnerSpot = table.Column<bool>(type: "bit", nullable: false),
                    IsWaitingList = table.Column<bool>(type: "bit", nullable: false),
                    PartnerEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spots", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_Spots_Registrables_RegistrableId",
                        column: x => x.RegistrableId,
                        principalTable: "Registrables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Spots_Registrations_RegistrationId",
                        column: x => x.RegistrationId,
                        principalTable: "Registrations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Spots_Registrations_RegistrationId_Follower",
                        column: x => x.RegistrationId_Follower,
                        principalTable: "Registrations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MailEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EMail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ExternalIdentifier = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RawEvent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    State = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailEvents", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_MailEvents_Mails_MailId",
                        column: x => x.MailId,
                        principalTable: "Mails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MailsToRegistrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MailId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    State = table.Column<int>(type: "int", nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailsToRegistrations", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_MailsToRegistrations_Mails_MailId",
                        column: x => x.MailId,
                        principalTable: "Mails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MailsToRegistrations_Registrations_RegistrationId",
                        column: x => x.RegistrationId,
                        principalTable: "Registrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestionOptionMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrableId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: true),
                    Language = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionOptionMappings", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_QuestionOptionMappings_QuestionOptions_QuestionOptionId",
                        column: x => x.QuestionOptionId,
                        principalTable: "QuestionOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestionOptionMappings_Registrables_RegistrableId",
                        column: x => x.RegistrableId,
                        principalTable: "Registrables",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Responses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResponseString = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Responses", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_Responses_QuestionOptions_QuestionOptionId",
                        column: x => x.QuestionOptionId,
                        principalTable: "QuestionOptions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Responses_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Responses_Registrations_RegistrationId",
                        column: x => x.RegistrationId,
                        principalTable: "Registrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RegistrationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IncomingPaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OutgoingPaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PaymentAssignmentId_Counter = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PayoutRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentAssignments", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_PaymentAssignments_IncomingPayments_IncomingPaymentId",
                        column: x => x.IncomingPaymentId,
                        principalTable: "IncomingPayments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PaymentAssignments_OutgoingPayments_OutgoingPaymentId",
                        column: x => x.OutgoingPaymentId,
                        principalTable: "OutgoingPayments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PaymentAssignments_PaymentAssignments_PaymentAssignmentId_Counter",
                        column: x => x.PaymentAssignmentId_Counter,
                        principalTable: "PaymentAssignments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PaymentAssignments_PayoutRequests_PayoutRequestId",
                        column: x => x.PayoutRequestId,
                        principalTable: "PayoutRequests",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PaymentAssignments_Registrations_RegistrationId",
                        column: x => x.RegistrationId,
                        principalTable: "Registrations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccessToEventRequests_EventId",
                table: "AccessToEventRequests",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessToEventRequests_Sequence",
                table: "AccessToEventRequests",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_AccessToEventRequests_UserId_Requestor",
                table: "AccessToEventRequests",
                column: "UserId_Requestor");

            migrationBuilder.CreateIndex(
                name: "IX_AccessToEventRequests_UserId_Responder",
                table: "AccessToEventRequests",
                column: "UserId_Responder");

            migrationBuilder.CreateIndex(
                name: "IX_DomainEvents_EventId",
                table: "DomainEvents",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_DomainEvents_Sequence",
                table: "DomainEvents",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_DomainEvents_Timestamp",
                table: "DomainEvents",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_EventConfigurations_EventId",
                table: "EventConfigurations",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_EventConfigurations_Sequence",
                table: "EventConfigurations",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_PredecessorEventId",
                table: "Events",
                column: "PredecessorEventId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_Sequence",
                table: "Events",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_FormPaths_RegistrationFormId",
                table: "FormPaths",
                column: "RegistrationFormId");

            migrationBuilder.CreateIndex(
                name: "IX_FormPaths_Sequence",
                table: "FormPaths",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_ImportedMails_EventId",
                table: "ImportedMails",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportedMails_Sequence",
                table: "ImportedMails",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_ImportedMailsToRegistrations_ImportedMailId",
                table: "ImportedMailsToRegistrations",
                column: "ImportedMailId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportedMailsToRegistrations_RegistrationId",
                table: "ImportedMailsToRegistrations",
                column: "RegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_ImportedMailsToRegistrations_Sequence",
                table: "ImportedMailsToRegistrations",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_IncomingPayments_PaymentSlipId",
                table: "IncomingPayments",
                column: "PaymentSlipId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomingPayments_Sequence",
                table: "IncomingPayments",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_IndividualReductions_RegistrationId",
                table: "IndividualReductions",
                column: "RegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_IndividualReductions_Sequence",
                table: "IndividualReductions",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_IndividualReductions_UserId",
                table: "IndividualReductions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MailEvents_MailId",
                table: "MailEvents",
                column: "MailId");

            migrationBuilder.CreateIndex(
                name: "IX_MailEvents_Sequence",
                table: "MailEvents",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_Mails_EventId",
                table: "Mails",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Mails_MailTemplateId",
                table: "Mails",
                column: "MailTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Mails_Sequence",
                table: "Mails",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_MailsToRegistrations_MailId",
                table: "MailsToRegistrations",
                column: "MailId");

            migrationBuilder.CreateIndex(
                name: "IX_MailsToRegistrations_RegistrationId",
                table: "MailsToRegistrations",
                column: "RegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_MailsToRegistrations_Sequence",
                table: "MailsToRegistrations",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_MailTemplates_EventId",
                table: "MailTemplates",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_MailTemplates_RegistrableId",
                table: "MailTemplates",
                column: "RegistrableId");

            migrationBuilder.CreateIndex(
                name: "IX_MailTemplates_Sequence",
                table: "MailTemplates",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_OutgoingPayments_Sequence",
                table: "OutgoingPayments",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAssignments_IncomingPaymentId",
                table: "PaymentAssignments",
                column: "IncomingPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAssignments_OutgoingPaymentId",
                table: "PaymentAssignments",
                column: "OutgoingPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAssignments_PaymentAssignmentId_Counter",
                table: "PaymentAssignments",
                column: "PaymentAssignmentId_Counter");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAssignments_PayoutRequestId",
                table: "PaymentAssignments",
                column: "PayoutRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAssignments_RegistrationId",
                table: "PaymentAssignments",
                column: "RegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAssignments_Sequence",
                table: "PaymentAssignments",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentsFileId",
                table: "Payments",
                column: "PaymentsFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Sequence",
                table: "Payments",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsFiles_EventId",
                table: "PaymentsFiles",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentsFiles_Sequence",
                table: "PaymentsFiles",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentSlips_EventId",
                table: "PaymentSlips",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentSlips_Sequence",
                table: "PaymentSlips",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_PayoutRequests_RegistrationId",
                table: "PayoutRequests",
                column: "RegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_PayoutRequests_Sequence",
                table: "PayoutRequests",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionOptionMappings_QuestionOptionId",
                table: "QuestionOptionMappings",
                column: "QuestionOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionOptionMappings_RegistrableId",
                table: "QuestionOptionMappings",
                column: "RegistrableId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionOptionMappings_Sequence",
                table: "QuestionOptionMappings",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionOptions_QuestionId",
                table: "QuestionOptions",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionOptions_Sequence",
                table: "QuestionOptions",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_RegistrationFormId",
                table: "Questions",
                column: "RegistrationFormId");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_Sequence",
                table: "Questions",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_RawBankStatementsFiles_Sequence",
                table: "RawBankStatementsFiles",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_RawMailEvents_Sequence",
                table: "RawMailEvents",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_RawRegistrationForms_Sequence",
                table: "RawRegistrationForms",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_RawRegistrations_Sequence",
                table: "RawRegistrations",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_Reductions_RegistrableId",
                table: "Reductions",
                column: "RegistrableId");

            migrationBuilder.CreateIndex(
                name: "IX_Reductions_RegistrableId1_ReductionActivatedIfCombinedWith",
                table: "Reductions",
                column: "RegistrableId1_ReductionActivatedIfCombinedWith");

            migrationBuilder.CreateIndex(
                name: "IX_Reductions_RegistrableId2_ReductionActivatedIfCombinedWith",
                table: "Reductions",
                column: "RegistrableId2_ReductionActivatedIfCombinedWith");

            migrationBuilder.CreateIndex(
                name: "IX_Reductions_Sequence",
                table: "Reductions",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrableCompositions_RegistrableId",
                table: "RegistrableCompositions",
                column: "RegistrableId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrableCompositions_RegistrableId_Contains",
                table: "RegistrableCompositions",
                column: "RegistrableId_Contains");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrableCompositions_Sequence",
                table: "RegistrableCompositions",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_Registrables_EventId",
                table: "Registrables",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Registrables_Sequence",
                table: "Registrables",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrableTags_EventId",
                table: "RegistrableTags",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrableTags_Sequence",
                table: "RegistrableTags",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationCancellations_RegistrationId",
                table: "RegistrationCancellations",
                column: "RegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationCancellations_Sequence",
                table: "RegistrationCancellations",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationForms_EventId",
                table: "RegistrationForms",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationForms_Sequence",
                table: "RegistrationForms",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_EventId",
                table: "Registrations",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_RegistrationFormId",
                table: "Registrations",
                column: "RegistrationFormId");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_RegistrationId_Partner",
                table: "Registrations",
                column: "RegistrationId_Partner");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_Sequence",
                table: "Registrations",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_Responses_QuestionId",
                table: "Responses",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_Responses_QuestionOptionId",
                table: "Responses",
                column: "QuestionOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Responses_RegistrationId",
                table: "Responses",
                column: "RegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_Responses_Sequence",
                table: "Responses",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_Sms_RegistrationId",
                table: "Sms",
                column: "RegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_Sms_Sequence",
                table: "Sms",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_SpotMailLines_RegistrableId",
                table: "SpotMailLines",
                column: "RegistrableId");

            migrationBuilder.CreateIndex(
                name: "IX_SpotMailLines_Sequence",
                table: "SpotMailLines",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_Spots_RegistrableId",
                table: "Spots",
                column: "RegistrableId");

            migrationBuilder.CreateIndex(
                name: "IX_Spots_RegistrationId",
                table: "Spots",
                column: "RegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_Spots_RegistrationId_Follower",
                table: "Spots",
                column: "RegistrationId_Follower");

            migrationBuilder.CreateIndex(
                name: "IX_Spots_Sequence",
                table: "Spots",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Sequence",
                table: "Users",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_UsersInEvents_EventId",
                table: "UsersInEvents",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_UsersInEvents_Sequence",
                table: "UsersInEvents",
                column: "Sequence",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_UsersInEvents_UserId",
                table: "UsersInEvents",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessToEventRequests");

            migrationBuilder.DropTable(
                name: "DomainEvents");

            migrationBuilder.DropTable(
                name: "EventConfigurations");

            migrationBuilder.DropTable(
                name: "FormPaths");

            migrationBuilder.DropTable(
                name: "ImportedMailsToRegistrations");

            migrationBuilder.DropTable(
                name: "IndividualReductions");

            migrationBuilder.DropTable(
                name: "MailEvents");

            migrationBuilder.DropTable(
                name: "MailsToRegistrations");

            migrationBuilder.DropTable(
                name: "PaymentAssignments");

            migrationBuilder.DropTable(
                name: "QuestionOptionMappings");

            migrationBuilder.DropTable(
                name: "RawBankStatementsFiles");

            migrationBuilder.DropTable(
                name: "RawMailEvents");

            migrationBuilder.DropTable(
                name: "RawRegistrationForms");

            migrationBuilder.DropTable(
                name: "RawRegistrations");

            migrationBuilder.DropTable(
                name: "Reductions");

            migrationBuilder.DropTable(
                name: "RegistrableCompositions");

            migrationBuilder.DropTable(
                name: "RegistrableTags");

            migrationBuilder.DropTable(
                name: "RegistrationCancellations");

            migrationBuilder.DropTable(
                name: "Responses");

            migrationBuilder.DropTable(
                name: "Sms");

            migrationBuilder.DropTable(
                name: "SpotMailLines");

            migrationBuilder.DropTable(
                name: "Spots");

            migrationBuilder.DropTable(
                name: "UsersInEvents");

            migrationBuilder.DropTable(
                name: "ImportedMails");

            migrationBuilder.DropTable(
                name: "Mails");

            migrationBuilder.DropTable(
                name: "IncomingPayments");

            migrationBuilder.DropTable(
                name: "OutgoingPayments");

            migrationBuilder.DropTable(
                name: "PayoutRequests");

            migrationBuilder.DropTable(
                name: "QuestionOptions");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "MailTemplates");

            migrationBuilder.DropTable(
                name: "PaymentSlips");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Registrations");

            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "Registrables");

            migrationBuilder.DropTable(
                name: "PaymentsFiles");

            migrationBuilder.DropTable(
                name: "RegistrationForms");

            migrationBuilder.DropTable(
                name: "Events");
        }
    }
}
