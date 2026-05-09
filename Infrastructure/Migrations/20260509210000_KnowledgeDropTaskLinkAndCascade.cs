using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <inheritdoc />
public partial class KnowledgeDropTaskLinkAndCascade : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_KnowledgeArticles_Tasks_ProjectTaskId",
            table: "KnowledgeArticles");

        migrationBuilder.DropIndex(
            name: "IX_KnowledgeArticles_ProjectTaskId",
            table: "KnowledgeArticles");

        migrationBuilder.DropColumn(
            name: "ProjectTaskId",
            table: "KnowledgeArticles");

        migrationBuilder.DropForeignKey(
            name: "FK_KnowledgeArticles_Projects_ProjectId",
            table: "KnowledgeArticles");

        migrationBuilder.AddForeignKey(
            name: "FK_KnowledgeArticles_Projects_ProjectId",
            table: "KnowledgeArticles",
            column: "ProjectId",
            principalTable: "Projects",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_KnowledgeArticles_Projects_ProjectId",
            table: "KnowledgeArticles");

        migrationBuilder.AddForeignKey(
            name: "FK_KnowledgeArticles_Projects_ProjectId",
            table: "KnowledgeArticles",
            column: "ProjectId",
            principalTable: "Projects",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);

        migrationBuilder.AddColumn<Guid>(
            name: "ProjectTaskId",
            table: "KnowledgeArticles",
            type: "uuid",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_KnowledgeArticles_ProjectTaskId",
            table: "KnowledgeArticles",
            column: "ProjectTaskId");

        migrationBuilder.AddForeignKey(
            name: "FK_KnowledgeArticles_Tasks_ProjectTaskId",
            table: "KnowledgeArticles",
            column: "ProjectTaskId",
            principalTable: "Tasks",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);
    }
}
