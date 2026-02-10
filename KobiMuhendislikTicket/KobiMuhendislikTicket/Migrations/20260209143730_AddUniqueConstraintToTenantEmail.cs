using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KobiMuhendislikTicket.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintToTenantEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Tenants",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "PasswordHash",
                value: "$2a$11$xvgkP7pc5NGGjgGCT3EKIus8Sd3uT/ZEoPAaYEjMibijPYed6P8mK");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "PasswordHash",
                value: "$2a$11$TDvyNoKaAH.L6dq8s0yYF.8cJHju8zlR468aO3jejkGf7YdVPziNq");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "PasswordHash",
                value: "$2a$11$m3jFMiCWUByBlh.KbiCBSO65MiWXJaBhgWvnx2d9K039o8jKUz4OK");

            // Duplicate email'leri kaldır (related kayıtlar da silinir)
            migrationBuilder.Sql(
                @"DECLARE @DuplicateTenantId UNIQUEIDENTIFIER;
                  SELECT @DuplicateTenantId = MAX(Id) FROM Tenants WHERE LOWER(Email) IN (
                      SELECT LOWER(Email) FROM Tenants 
                      GROUP BY LOWER(Email) 
                      HAVING COUNT(*) > 1
                  );
                  
                  IF @DuplicateTenantId IS NOT NULL
                  BEGIN
                      DELETE FROM Notifications WHERE TicketId IN (SELECT Id FROM Tickets WHERE TenantId = @DuplicateTenantId);
                      DELETE FROM TicketComments WHERE TicketId IN (SELECT Id FROM Tickets WHERE TenantId = @DuplicateTenantId);
                      DELETE FROM TicketHistories WHERE TicketId IN (SELECT Id FROM Tickets WHERE TenantId = @DuplicateTenantId);
                      DELETE FROM Tickets WHERE TenantId = @DuplicateTenantId;
                      DELETE FROM Assets WHERE TenantId = @DuplicateTenantId;
                      DELETE FROM Tenants WHERE Id = @DuplicateTenantId;
                  END");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Email",
                table: "Tenants",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tenants_Email",
                table: "Tenants");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Tenants",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"),
                column: "PasswordHash",
                value: "$2a$11$paHE2QpiAo6f.dj1qEI2busB8imS.0rIPGRQKT82zewAqHEYeWqqy");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"),
                column: "PasswordHash",
                value: "$2a$11$jxvoD4vQpT8nyqteHubrN.9arLWrtHeY3PIwGiRWUoBJlE7LSoRLu");

            migrationBuilder.UpdateData(
                table: "Staffs",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "PasswordHash",
                value: "$2a$11$20sHmj7Vpdyy8OBjeNBPrebKuk5YEqzcsuPKCMxkk3Eyg0WWeMK6i");
        }
    }
}
