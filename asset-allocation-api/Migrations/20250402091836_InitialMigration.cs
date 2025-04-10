using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace asset_allocation_api.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssetCheckType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CheckName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ModifiedUserId = table.Column<int>(type: "integer", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__AssetChe__3214EC072F31D91B", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssetType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ReturnHour = table.Column<int>(type: "integer", nullable: true),
                    ModifiedUserId = table.Column<int>(type: "integer", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__AssetTyp__3214EC07219BEE67", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditEntries",
                columns: table => new
                {
                    AuditEntryID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EntitySetName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    EntityTypeName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    State = table.Column<int>(type: "integer", nullable: false),
                    StateName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEntries", x => x.AuditEntryID);
                });

            migrationBuilder.CreateTable(
                name: "Department",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Departme__3214EC07A93BB1DC", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DepartmentPersonnel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DepartmentId = table.Column<int>(type: "integer", nullable: true),
                    PersonnelNo = table.Column<int>(type: "integer", nullable: true),
                    UserGroup = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ModifiedUserId = table.Column<int>(type: "integer", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Departme__3214EC0707139CD3", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Personnel",
                columns: table => new
                {
                    PersonnelNo = table.Column<int>(type: "integer", nullable: false),
                    PersonnelId = table.Column<int>(type: "integer", nullable: true),
                    FirstName = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    LastName = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    FullName = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    HotstampNo = table.Column<int>(type: "integer", nullable: true),
                    CardNum = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    PersonnelSer = table.Column<int>(type: "integer", nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DepartmentDesc = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    PositionDesc = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CompanyDesc = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ContactNumber = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    WorkPhone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    EmploymentStatus = table.Column<int>(type: "integer", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Personne__CAFBB3FC60B0E010", x => x.PersonnelNo);
                });

            migrationBuilder.CreateTable(
                name: "AssetCheckTypeSetting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AssetTypeId = table.Column<int>(type: "integer", nullable: true),
                    CheckTypeId = table.Column<int>(type: "integer", nullable: true),
                    CheckPeriod = table.Column<int>(type: "integer", nullable: true),
                    Expirelimit = table.Column<int>(type: "integer", nullable: true),
                    CheckNear = table.Column<string>(type: "text", nullable: true),
                    ExpireNear = table.Column<string>(type: "text", nullable: true),
                    CreatedUserId = table.Column<int>(type: "integer", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    Enabled = table.Column<int>(type: "integer", nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetCheckTypeSetting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetCheckTypeSetting_AssetCheckType_CheckTypeId",
                        column: x => x.CheckTypeId,
                        principalTable: "AssetCheckType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AssetCheckTypeSetting_AssetTypeId",
                        column: x => x.AssetTypeId,
                        principalTable: "AssetType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AuditEntryProperties",
                columns: table => new
                {
                    AuditEntryPropertyID = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AuditEntryID = table.Column<int>(type: "integer", nullable: false),
                    RelationName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PropertyName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    OldValue = table.Column<string>(type: "text", nullable: true),
                    NewValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditEntryProperties", x => x.AuditEntryPropertyID);
                    table.ForeignKey(
                        name: "FK_AuditEntryProperties_AuditEntries_AuditEntryID",
                        column: x => x.AuditEntryID,
                        principalTable: "AuditEntries",
                        principalColumn: "AuditEntryID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Configuration",
                columns: table => new
                {
                    ConfigId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DepartmentId = table.Column<int>(type: "integer", nullable: true),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ConfigDesc = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ConfigValue = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    isEnabled = table.Column<int>(type: "integer", nullable: true),
                    ModifiedUserId = table.Column<int>(type: "integer", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Configur__C3BC335CA70BD68D", x => x.ConfigId);
                    table.ForeignKey(
                        name: "Configuration_Department_Id_fk",
                        column: x => x.DepartmentId,
                        principalTable: "Department",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DepartmentAssetType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DepartmentId = table.Column<int>(type: "integer", nullable: true),
                    AssetTypeId = table.Column<int>(type: "integer", nullable: true),
                    ModifiedUserId = table.Column<int>(type: "integer", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Departme__3214EC07B0548A20", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepartmentAssetType_AssetType",
                        column: x => x.AssetTypeId,
                        principalTable: "AssetType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DepartmentAssetType_Department",
                        column: x => x.DepartmentId,
                        principalTable: "Department",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NonReturnableAssetType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IconName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Limit = table.Column<int>(type: "integer", nullable: true),
                    CooldownDays = table.Column<int>(type: "integer", nullable: true),
                    Stock = table.Column<int>(type: "integer", nullable: true),
                    ModifiedUserId = table.Column<int>(type: "integer", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    DepartmentId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NonReturnableAssetType", x => x.Id);
                    table.ForeignKey(
                        name: "NonReturnableAssetType_Department_Id_fk",
                        column: x => x.DepartmentId,
                        principalTable: "Department",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NonReturnableAssetField",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AssetTypeId = table.Column<int>(type: "integer", nullable: true),
                    FieldName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ValueType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NonReturnableAssetField", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NonReturnableAssetField_AssetType",
                        column: x => x.AssetTypeId,
                        principalTable: "NonReturnableAssetType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NonReturnableAssetPersonnelPermit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PersonnelNo = table.Column<int>(type: "integer", nullable: true),
                    AssetTypeId = table.Column<int>(type: "integer", nullable: true),
                    Enabled = table.Column<short>(type: "smallint", nullable: true),
                    ModifiedUserId = table.Column<int>(type: "integer", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    PersonnelId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NonReturnableAssetPersonnelPermit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NonReturnableAssetPersonnelPermit_AssetType",
                        column: x => x.AssetTypeId,
                        principalTable: "NonReturnableAssetType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TypeTraining",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AssetTypeId = table.Column<int>(type: "integer", nullable: true),
                    TrainingId = table.Column<int>(type: "integer", nullable: false),
                    NonReturnableAssetTypeId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypeTraining", x => x.Id);
                    table.ForeignKey(
                        name: "TypeTraining_AssetType_Id_fk",
                        column: x => x.AssetTypeId,
                        principalTable: "AssetType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "TypeTraining_NonReturnableAssetType_Id_fk",
                        column: x => x.NonReturnableAssetTypeId,
                        principalTable: "NonReturnableAssetType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NonReturnableAssetPersonnelSetting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PersonnelNo = table.Column<int>(type: "integer", nullable: true),
                    FieldId = table.Column<int>(type: "integer", nullable: true),
                    Value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    PersonnelId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NonReturnableAssetPersonnelSetting", x => x.Id);
                    table.ForeignKey(
                        name: "NonReturnableAssetPersonnelSetting_NonReturnableAssetField_Id_fk",
                        column: x => x.FieldId,
                        principalTable: "NonReturnableAssetField",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "NonReturnableAssetAllocation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PermitId = table.Column<int>(type: "integer", nullable: true),
                    AssignedUserId = table.Column<int>(type: "integer", nullable: true),
                    AssignedDate = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NonReturnableAssetAllocation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NonReturnableAssetAllocation_NonReturnableAssetPersonnelPer~",
                        column: x => x.PermitId,
                        principalTable: "NonReturnableAssetPersonnelPermit",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Asset",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Rfid = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Serial = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Mac2 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Mac3 = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AssetTypeId = table.Column<int>(type: "integer", nullable: true),
                    DepartmentId = table.Column<int>(type: "integer", nullable: true),
                    Country = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    RegisteredDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    ManufacturedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    StartedUsingDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ExpireDate = table.Column<DateOnly>(type: "date", nullable: true),
                    LastMaintenanceId = table.Column<int>(type: "integer", nullable: true),
                    LastAllocationId = table.Column<int>(type: "integer", nullable: true),
                    ModifiedUserId = table.Column<int>(type: "integer", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    Description = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Asset__3214EC0736A337F2", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asset_AssetType",
                        column: x => x.AssetTypeId,
                        principalTable: "AssetType",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Asset_Department",
                        column: x => x.DepartmentId,
                        principalTable: "Department",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AssetAllocation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AssetId = table.Column<int>(type: "integer", nullable: true),
                    PersonnelNo = table.Column<int>(type: "integer", nullable: true),
                    AssignedUserId = table.Column<int>(type: "integer", nullable: true),
                    AssignedDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    ReturnedUserId = table.Column<int>(type: "integer", nullable: true),
                    ReturnedDate = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__AssetAll__3214EC07E69F292A", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asset_Allocation_Asset",
                        column: x => x.AssetId,
                        principalTable: "Asset",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AssetAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AssetId = table.Column<int>(type: "integer", nullable: false),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ModifiedUserId = table.Column<int>(type: "integer", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__AssetAtt__3214EC0710F2A989", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asset_Attachments_Asset",
                        column: x => x.AssetId,
                        principalTable: "Asset",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AssetCheckHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AssetId = table.Column<int>(type: "integer", nullable: true),
                    AssetCheckTypeId = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CheckedUserId = table.Column<int>(type: "integer", nullable: true),
                    CheckedDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__AssetChe__3214EC07AD5F7CCD", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetCheckHistory_Asset",
                        column: x => x.AssetId,
                        principalTable: "Asset",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AssetCheckType_Asset",
                        column: x => x.AssetCheckTypeId,
                        principalTable: "AssetCheckType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AssetInspectionHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AssetId = table.Column<int>(type: "integer", nullable: true),
                    CheckTypeId = table.Column<int>(type: "integer", nullable: true),
                    CheckStatus = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CheckedDateTime = table.Column<DateTime>(type: "timestamp", nullable: true),
                    CheckedUserId = table.Column<int>(type: "integer", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    CreatedUserId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetInspectionHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetInspectionHistory_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Asset",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AssetInspectionHistory_CheckTypeId",
                        column: x => x.CheckTypeId,
                        principalTable: "AssetCheckType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "assetIndx",
                table: "Asset",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Asset_AssetTypeId",
                table: "Asset",
                column: "AssetTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Asset_LastAllocationId",
                table: "Asset",
                column: "LastAllocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Asset_LastMaintenanceId",
                table: "Asset",
                column: "LastMaintenanceId");

            migrationBuilder.CreateIndex(
                name: "NonClusteredIndex-20240927-092258",
                table: "Asset",
                column: "Rfid");

            migrationBuilder.CreateIndex(
                name: "UQ_Asset_Rfid",
                table: "Asset",
                column: "Rfid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Asset_Serial",
                table: "Asset",
                column: "Serial",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetAllocation_AssetId",
                table: "AssetAllocation",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "NonClusteredIndex-20241003-093424",
                table: "AssetAllocation",
                columns: new[] { "PersonnelNo", "ReturnedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AssetAttachments_AssetId",
                table: "AssetAttachments",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetCheckHistory_AssetCheckTypeId",
                table: "AssetCheckHistory",
                column: "AssetCheckTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetCheckHistory_AssetId",
                table: "AssetCheckHistory",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetCheckTypeSetting_AssetTypeId",
                table: "AssetCheckTypeSetting",
                column: "AssetTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetCheckTypeSetting_CheckTypeId",
                table: "AssetCheckTypeSetting",
                column: "CheckTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetInspectionHistory_AssetId",
                table: "AssetInspectionHistory",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetInspectionHistory_CheckTypeId",
                table: "AssetInspectionHistory",
                column: "CheckTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEntryProperties_AuditEntryID",
                table: "AuditEntryProperties",
                column: "AuditEntryID");

            migrationBuilder.CreateIndex(
                name: "IX_Configuration_DepartmentId",
                table: "Configuration",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentAssetType_AssetTypeId",
                table: "DepartmentAssetType",
                column: "AssetTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentAssetType_DepartmentId",
                table: "DepartmentAssetType",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_NonReturnableAssetAllocation_PermitId",
                table: "NonReturnableAssetAllocation",
                column: "PermitId");

            migrationBuilder.CreateIndex(
                name: "IX_NonReturnableAssetField_AssetTypeId",
                table: "NonReturnableAssetField",
                column: "AssetTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_NonReturnableAssetPersonnelPermit_AssetTypeId",
                table: "NonReturnableAssetPersonnelPermit",
                column: "AssetTypeId");

            migrationBuilder.CreateIndex(
                name: "NonClusteredIndex-20240927-090804",
                table: "NonReturnableAssetPersonnelSetting",
                columns: new[] { "FieldId", "PersonnelId" });

            migrationBuilder.CreateIndex(
                name: "IX_NonReturnableAssetType_DepartmentId",
                table: "NonReturnableAssetType",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "NonClusteredIndex-20240927-085449",
                table: "Personnel",
                column: "PersonnelId");

            migrationBuilder.CreateIndex(
                name: "NonClusteredIndex-20241003-093352",
                table: "Personnel",
                column: "CardNum");

            migrationBuilder.CreateIndex(
                name: "IX_TypeTraining_AssetTypeId",
                table: "TypeTraining",
                column: "AssetTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TypeTraining_NonReturnableAssetTypeId",
                table: "TypeTraining",
                column: "NonReturnableAssetTypeId");

            migrationBuilder.AddForeignKey(
                name: "Asset_AssetAllocation_Id_fk",
                table: "Asset",
                column: "LastAllocationId",
                principalTable: "AssetAllocation",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "Asset_AssetCheckHistory_Id_fk",
                table: "Asset",
                column: "LastMaintenanceId",
                principalTable: "AssetCheckHistory",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "Asset_AssetAllocation_Id_fk",
                table: "Asset");

            migrationBuilder.DropForeignKey(
                name: "Asset_AssetCheckHistory_Id_fk",
                table: "Asset");

            migrationBuilder.DropTable(
                name: "AssetAttachments");

            migrationBuilder.DropTable(
                name: "AssetCheckTypeSetting");

            migrationBuilder.DropTable(
                name: "AssetInspectionHistory");

            migrationBuilder.DropTable(
                name: "AuditEntryProperties");

            migrationBuilder.DropTable(
                name: "Configuration");

            migrationBuilder.DropTable(
                name: "DepartmentAssetType");

            migrationBuilder.DropTable(
                name: "DepartmentPersonnel");

            migrationBuilder.DropTable(
                name: "NonReturnableAssetAllocation");

            migrationBuilder.DropTable(
                name: "NonReturnableAssetPersonnelSetting");

            migrationBuilder.DropTable(
                name: "Personnel");

            migrationBuilder.DropTable(
                name: "TypeTraining");

            migrationBuilder.DropTable(
                name: "AuditEntries");

            migrationBuilder.DropTable(
                name: "NonReturnableAssetPersonnelPermit");

            migrationBuilder.DropTable(
                name: "NonReturnableAssetField");

            migrationBuilder.DropTable(
                name: "NonReturnableAssetType");

            migrationBuilder.DropTable(
                name: "AssetAllocation");

            migrationBuilder.DropTable(
                name: "AssetCheckHistory");

            migrationBuilder.DropTable(
                name: "Asset");

            migrationBuilder.DropTable(
                name: "AssetCheckType");

            migrationBuilder.DropTable(
                name: "AssetType");

            migrationBuilder.DropTable(
                name: "Department");
        }
    }
}
