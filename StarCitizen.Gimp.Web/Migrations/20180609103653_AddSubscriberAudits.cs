using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace StarCitizen.Gimp.Web.Migrations
{
    public partial class AddSubscriberAudits : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubscriberAudits",
                columns: table => new
                {
                    SubscriberAuditId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    SubscriberId = table.Column<long>(nullable: false),
                    FormVersion = table.Column<string>(maxLength: 16, nullable: true),
                    FormName = table.Column<string>(maxLength: 50, nullable: true),
                    Action = table.Column<string>(maxLength: 200, nullable: true),
                    IpAddress = table.Column<string>(maxLength: 39, nullable: false),
                    Headers = table.Column<string>(maxLength: 4000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriberAudits", x => x.SubscriberAuditId);
                    table.ForeignKey(
                        name: "FK_SubscriberAudits_Subscribers_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Subscribers",
                        principalColumn: "SubscriberId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriberAudits_SubscriberId",
                table: "SubscriberAudits",
                column: "SubscriberId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubscriberAudits");
        }
    }
}
