using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EventRegistrar.Backend.Infrastructure.DataAccess.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccessToEventRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    EventId = table.Column<Guid>(nullable: false),
                    Identifier = table.Column<string>(nullable: true),
                    IdentityProvider = table.Column<string>(nullable: true),
                    RequestReceived = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessToEventRequest", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    AccountIban = table.Column<string>(nullable: true),
                    Acronym = table.Column<string>(nullable: true),
                    Currency = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    State = table.Column<int>(nullable: false),
                    TwilioAccountSid = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    AccountIban = table.Column<string>(nullable: true),
                    Balance = table.Column<decimal>(nullable: true),
                    BookingsFrom = table.Column<DateTime>(nullable: true),
                    BookingsTo = table.Column<DateTime>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    Currency = table.Column<string>(nullable: true),
                    EventId = table.Column<Guid>(nullable: true),
                    FileId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Registrables",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    CheckinListColumn = table.Column<string>(nullable: true),
                    EventId = table.Column<Guid>(nullable: false),
                    HasWaitingList = table.Column<bool>(nullable: false),
                    IsCore = table.Column<bool>(nullable: false),
                    MaximumAllowedImbalance = table.Column<int>(nullable: true),
                    MaximumDoubleSeats = table.Column<int>(nullable: true),
                    MaximumSingleSeats = table.Column<int>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Price = table.Column<decimal>(nullable: true),
                    ShowInMailListOrder = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Registrables", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    FirstName = table.Column<string>(nullable: true),
                    IdentityProvider = table.Column<int>(nullable: false),
                    IdentityProviderUserIdentifier = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegistrationForms",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    EventId = table.Column<Guid>(nullable: true),
                    ExternalIdentifier = table.Column<string>(nullable: true),
                    Language = table.Column<string>(nullable: true),
                    QuestionId_FirstName = table.Column<Guid>(nullable: true),
                    QuestionId_LastName = table.Column<Guid>(nullable: true),
                    QuestionId_Phone = table.Column<Guid>(nullable: true),
                    QuestionId_Remarks = table.Column<Guid>(nullable: true),
                    State = table.Column<int>(nullable: false),
                    Title = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationForms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistrationForms_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReceivedPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    Amount = table.Column<decimal>(nullable: false),
                    BookingDate = table.Column<DateTime>(nullable: false),
                    Currency = table.Column<string>(nullable: true),
                    Info = table.Column<string>(nullable: true),
                    PaymentFileId = table.Column<Guid>(nullable: false),
                    RecognizedEmail = table.Column<string>(nullable: true),
                    Reference = table.Column<string>(nullable: true),
                    RegistrationId_Payer = table.Column<Guid>(nullable: true),
                    Repaid = table.Column<decimal>(nullable: true),
                    Settled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceivedPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceivedPayments_PaymentFiles_PaymentFileId",
                        column: x => x.PaymentFileId,
                        principalTable: "PaymentFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsersInEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    EventId = table.Column<Guid>(nullable: false),
                    Role = table.Column<int>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersInEvents", x => x.Id);
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
                name: "Registrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    AdmittedAt = table.Column<DateTime>(nullable: true),
                    EventId = table.Column<Guid>(nullable: false),
                    ExternalIdentifier = table.Column<string>(nullable: true),
                    ExternalTimestamp = table.Column<DateTime>(nullable: false),
                    FallbackToPartyPass = table.Column<bool>(nullable: false),
                    IsWaitingList = table.Column<bool>(nullable: true),
                    Language = table.Column<string>(nullable: true),
                    Phone = table.Column<string>(nullable: true),
                    PhoneNormalized = table.Column<string>(nullable: true),
                    Price = table.Column<decimal>(nullable: true),
                    ReceivedAt = table.Column<DateTime>(nullable: false),
                    RegistrationFormId = table.Column<Guid>(nullable: false),
                    Remarks = table.Column<string>(nullable: true),
                    RemarksProcessed = table.Column<bool>(nullable: false),
                    ReminderLevel = table.Column<int>(nullable: false),
                    RespondentEmail = table.Column<string>(nullable: true),
                    RespondentFirstName = table.Column<string>(nullable: true),
                    RespondentLastName = table.Column<string>(nullable: true),
                    SoldOutMessage = table.Column<string>(nullable: true),
                    State = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Registrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Registrations_RegistrationForms_RegistrationFormId",
                        column: x => x.RegistrationFormId,
                        principalTable: "RegistrationForms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    Amount = table.Column<decimal>(nullable: false),
                    ReceivedPaymentId = table.Column<Guid>(nullable: false),
                    RegistrationId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentAssignments_ReceivedPayments_ReceivedPaymentId",
                        column: x => x.ReceivedPaymentId,
                        principalTable: "ReceivedPayments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentAssignments_Registrations_RegistrationId",
                        column: x => x.RegistrationId,
                        principalTable: "Registrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Seats",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    FirstPartnerJoined = table.Column<DateTime>(nullable: false),
                    IsCancelled = table.Column<bool>(nullable: false),
                    IsWaitingList = table.Column<bool>(nullable: false),
                    PartnerEmail = table.Column<string>(nullable: true),
                    RegistrableId = table.Column<Guid>(nullable: false),
                    RegistrationId = table.Column<Guid>(nullable: true),
                    RegistrationId_Follower = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Seats_Registrables_RegistrableId",
                        column: x => x.RegistrableId,
                        principalTable: "Registrables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Seats_Registrations_RegistrationId",
                        column: x => x.RegistrationId,
                        principalTable: "Registrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Seats_Registrations_RegistrationId_Follower",
                        column: x => x.RegistrationId_Follower,
                        principalTable: "Registrations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAssignments_ReceivedPaymentId",
                table: "PaymentAssignments",
                column: "ReceivedPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAssignments_RegistrationId",
                table: "PaymentAssignments",
                column: "RegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivedPayments_PaymentFileId",
                table: "ReceivedPayments",
                column: "PaymentFileId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationForms_EventId",
                table: "RegistrationForms",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_RegistrationFormId",
                table: "Registrations",
                column: "RegistrationFormId");

            migrationBuilder.CreateIndex(
                name: "IX_Seats_RegistrableId",
                table: "Seats",
                column: "RegistrableId");

            migrationBuilder.CreateIndex(
                name: "IX_Seats_RegistrationId",
                table: "Seats",
                column: "RegistrationId");

            migrationBuilder.CreateIndex(
                name: "IX_Seats_RegistrationId_Follower",
                table: "Seats",
                column: "RegistrationId_Follower");

            migrationBuilder.CreateIndex(
                name: "IX_UsersInEvents_EventId",
                table: "UsersInEvents",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_UsersInEvents_UserId",
                table: "UsersInEvents",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessToEventRequest");

            migrationBuilder.DropTable(
                name: "PaymentAssignments");

            migrationBuilder.DropTable(
                name: "Seats");

            migrationBuilder.DropTable(
                name: "UsersInEvents");

            migrationBuilder.DropTable(
                name: "ReceivedPayments");

            migrationBuilder.DropTable(
                name: "Registrables");

            migrationBuilder.DropTable(
                name: "Registrations");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "PaymentFiles");

            migrationBuilder.DropTable(
                name: "RegistrationForms");

            migrationBuilder.DropTable(
                name: "Events");
        }
    }
}
