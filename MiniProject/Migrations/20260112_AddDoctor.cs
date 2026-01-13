using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniProject.Migrations
{
    public partial class AddDoctor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ensure schema exists
            migrationBuilder.Sql(@"IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Healthcare') EXEC('CREATE SCHEMA Healthcare');");

            // Create Doctor table
            migrationBuilder.CreateTable(
                name: "Doctor",
                schema: "Healthcare",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Specialization = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doctor", x => x.Id);
                });

            // Add DoctorId column to Appointment
            migrationBuilder.AddColumn<int>(
                name: "DoctorId",
                schema: "Healthcare",
                table: "Appointment",
                type: "int",
                nullable: true);

            // Create foreign key
            migrationBuilder.AddForeignKey(
                name: "FK_Appointment_Doctor_DoctorId",
                schema: "Healthcare",
                table: "Appointment",
                column: "DoctorId",
                principalSchema: "Healthcare",
                principalTable: "Doctor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop FK and column
            migrationBuilder.DropForeignKey(
                name: "FK_Appointment_Doctor_DoctorId",
                schema: "Healthcare",
                table: "Appointment");

            migrationBuilder.DropColumn(
                name: "DoctorId",
                schema: "Healthcare",
                table: "Appointment");

            migrationBuilder.DropTable(
                name: "Doctor",
                schema: "Healthcare");
        }
    }
}
