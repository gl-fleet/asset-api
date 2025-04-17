using Microsoft.EntityFrameworkCore;
using asset_allocation_api.Config;
using asset_allocation_api.Model.CustomModel;
using Z.EntityFramework.Plus;

namespace asset_allocation_api.Context;

public partial class AppDbContext : DbContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public AppDbContext() {}
    // public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor httpContextAccessor) : base(options) {
        _httpContextAccessor = httpContextAccessor;
        AuditManager.DefaultConfiguration.AutoSavePreAction = (context, auditEntries) =>
        {
            // ADD "Where(x => x.AuditEntryID == 0)" to allow multiple SaveChanges with same Audit
            context.Set<AuditEntry>().AddRange(auditEntries.Entries.Where(x => x.AuditEntryID == 0));
        };
        
        AuditManager.DefaultConfiguration.AutoSavePreAction = (context, audit) =>
        {
            foreach (var entry in audit.Entries)
            {
                entry.CreatedDate = DateTime.UtcNow;
            }

            context.AddRange(audit.Entries);
        };
        
        AuditManager.DefaultConfiguration.Include<Configuration>();
        AuditManager.DefaultConfiguration.Include<AssetType>();
        AuditManager.DefaultConfiguration.Include<Asset>();
        AuditManager.DefaultConfiguration.Include<NonReturnableAssetType>();
        AuditManager.DefaultConfiguration.Include<NonReturnableAssetPersonnelPermit>();
        AuditManager.DefaultConfiguration.Include<NonReturnableAssetPersonnelSetting>();
        AuditManager.DefaultConfiguration.Include<NonReturnableAssetField>();
        AuditManager.DefaultConfiguration.Include<DepartmentAssetType>();
        AuditManager.DefaultConfiguration.Include<AssetCheckTypeSetting>();
        AuditManager.DefaultConfiguration.Include<AssetInspectionHistory>();
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<AuditableEntity>();
        var userName = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";
    
        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.Now;
                    entry.Entity.CreatedBy = userName;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.Now;
                    entry.Entity.UpdatedBy = userName;
                    break;
                case EntityState.Detached:
                    break;
                case EntityState.Unchanged:
                    break;
                case EntityState.Deleted:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    
        return base.SaveChangesAsync(cancellationToken);
    }
    
    public virtual DbSet<Asset> Assets { get; set; }

    public virtual DbSet<AssetAllocation> AssetAllocations { get; set; }

    public virtual DbSet<AssetAttachment> AssetAttachments { get; set; }

    public virtual DbSet<AssetCheckHistory> AssetCheckHistories { get; set; }

    public virtual DbSet<AssetCheckType> AssetCheckTypes { get; set; }

    public virtual DbSet<AssetCheckTypeSetting> AssetCheckTypeSettings { get; set; }

    public virtual DbSet<AssetInspectionHistory> AssetInspectionHistories { get; set; }
    

    public virtual DbSet<AssetType> AssetTypes { get; set; }

    public virtual DbSet<AuditEntry> AuditEntries { get; set; }

    public virtual DbSet<AuditEntryProperty> AuditEntryProperties { get; set; }

    public virtual DbSet<Configuration> Configurations { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<DepartmentAssetType> DepartmentAssetTypes { get; set; }

    public virtual DbSet<DepartmentPersonnel> DepartmentPersonnel { get; set; }

    public virtual DbSet<NonReturnableAssetAllocation> NonReturnableAssetAllocations { get; set; }

    public virtual DbSet<NonReturnableAssetField> NonReturnableAssetFields { get; set; }

    public virtual DbSet<NonReturnableAssetPersonnelPermit> NonReturnableAssetPersonnelPermits { get; set; }

    public virtual DbSet<NonReturnableAssetPersonnelSetting> NonReturnableAssetPersonnelSettings { get; set; }

    public virtual DbSet<NonReturnableAssetType> NonReturnableAssetTypes { get; set; }

    public virtual DbSet<Personnel> Personnel { get; set; }

    public virtual DbSet<TypeTraining> TypeTrainings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) 
        => optionsBuilder.UseNpgsql(AssetAllocationConfig.assetAllocationConnectionString);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Asset__3214EC0736A337F2");

            entity.ToTable("Asset");

            entity.HasIndex(e => e.Rfid, "NonClusteredIndex-20240927-092258");
            entity.HasIndex(e => new { e.Rfid })
                .IsUnique()
                .HasDatabaseName("UQ_Asset_Rfid");
            
            entity.HasIndex(e => new { e.Serial })
                .IsUnique()
                .HasDatabaseName("UQ_Asset_Serial");
            
            entity.HasIndex(e => e.DepartmentId, "assetIndx");

            entity.Property(e => e.Country).HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(50);
            entity.Property(e => e.Mac2).HasMaxLength(50);
            entity.Property(e => e.Mac3).HasMaxLength(50);
            entity.Property(e => e.ModifiedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.RegisteredDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Rfid).HasMaxLength(50);
            entity.Property(e => e.Serial).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp without time zone");
            

            entity.HasOne(d => d.AssetType).WithMany(p => p.Assets)
                .HasForeignKey(d => d.AssetTypeId)
                .HasConstraintName("FK_Asset_AssetType");

            entity.HasOne(d => d.Department).WithMany(p => p.Assets)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK_Asset_Department");

            entity.HasOne(d => d.LastAllocation).WithMany(p => p.Assets)
                .HasForeignKey(d => d.LastAllocationId)
                .HasConstraintName("Asset_AssetAllocation_Id_fk");

            entity.HasOne(d => d.LastMaintenance).WithMany(p => p.Assets)
                .HasForeignKey(d => d.LastMaintenanceId)
                .HasConstraintName("Asset_AssetCheckHistory_Id_fk");
        });

        modelBuilder.Entity<AssetAllocation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AssetAll__3214EC07E69F292A");

            entity.ToTable("AssetAllocation");

            entity.HasIndex(e => new { e.PersonnelNo, e.ReturnedDate }, "NonClusteredIndex-20241003-093424");

            entity.Property(e => e.AssignedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.ReturnedDate).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.Asset).WithMany(p => p.AssetAllocations)
                .HasForeignKey(d => d.AssetId)
                .HasConstraintName("FK_Asset_Allocation_Asset");
        });

        modelBuilder.Entity<AssetAttachment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AssetAtt__3214EC0710F2A989");

            entity.Property(e => e.FilePath).HasMaxLength(500);
            entity.Property(e => e.ModifiedDate).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.Asset).WithMany(p => p.AssetAttachments)
                .HasForeignKey(d => d.AssetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Asset_Attachments_Asset");
        });

        modelBuilder.Entity<AssetCheckHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AssetChe__3214EC07AD5F7CCD");

            entity.ToTable("AssetCheckHistory");

            entity.Property(e => e.CheckedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Status).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.AssetCheckType).WithMany(p => p.AssetCheckHistories)
                .HasForeignKey(d => d.AssetCheckTypeId)
                .HasConstraintName("FK_AssetCheckType_Asset");

            entity.HasOne(d => d.Asset).WithMany(p => p.AssetCheckHistories)
                .HasForeignKey(d => d.AssetId)
                .HasConstraintName("FK_AssetCheckHistory_Asset");
        });

        modelBuilder.Entity<AssetCheckType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AssetChe__3214EC072F31D91B");

            entity.ToTable("AssetCheckType");

            entity.Property(e => e.CheckName).HasMaxLength(255);
            entity.Property(e => e.ModifiedDate).HasColumnType("timestamp");
        });

        modelBuilder.Entity<AssetCheckTypeSetting>(entity =>
        {
            entity.ToTable("AssetCheckTypeSetting");

            entity.Property(e => e.CreatedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.StartDate).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.AssetType).WithMany(p => p.AssetCheckTypeSettings)
                .HasForeignKey(d => d.AssetTypeId)
                .HasConstraintName("FK_AssetCheckTypeSetting_AssetTypeId");
        });

        modelBuilder.Entity<AssetInspectionHistory>(entity =>
        {
            entity.ToTable("AssetInspectionHistory");

            entity.Property(e => e.CheckedDateTime).HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Description).HasMaxLength(1000);

            entity.HasOne(d => d.Asset).WithMany(p => p.AssetInspectionHistories)
                .HasForeignKey(d => d.AssetId)
                .HasConstraintName("FK_AssetInspectionHistory_AssetId");

            entity.HasOne(d => d.CheckType).WithMany(p => p.AssetInspectionHistories)
                .HasForeignKey(d => d.CheckTypeId)
                .HasConstraintName("FK_AssetInspectionHistory_CheckTypeId");
        });

        modelBuilder.Entity<AssetType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__AssetTyp__3214EC07219BEE67");

            entity.ToTable("AssetType");

            entity.Property(e => e.ModifiedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp without time zone");
        });
        
        modelBuilder.Entity<Configuration>(entity =>
        {
            entity.HasKey(e => e.ConfigId).HasName("PK__Configur__C3BC335CA70BD68D");

            entity.ToTable("Configuration");

            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.ConfigDesc).HasMaxLength(50);
            entity.Property(e => e.ConfigValue).HasMaxLength(50);
            entity.Property(e => e.IsEnabled).HasColumnName("isEnabled");
            entity.Property(e => e.ModifiedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.Department).WithMany(p => p.Configurations)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("Configuration_Department_Id_fk");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Departme__3214EC07A93BB1DC");

            entity.ToTable("Department");

            entity.Property(e => e.Name).HasMaxLength(80);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<DepartmentAssetType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Departme__3214EC07B0548A20");

            entity.ToTable("DepartmentAssetType");

            entity.Property(e => e.ModifiedDate).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.AssetType).WithMany(p => p.DepartmentAssetTypes)
                .HasForeignKey(d => d.AssetTypeId)
                .HasConstraintName("FK_DepartmentAssetType_AssetType");

            entity.HasOne(d => d.Department).WithMany(p => p.DepartmentAssetTypes)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK_DepartmentAssetType_Department");
        });

        modelBuilder.Entity<DepartmentPersonnel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Departme__3214EC0707139CD3");

            entity.Property(e => e.ModifiedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.UserGroup).HasMaxLength(255);
        });

        modelBuilder.Entity<NonReturnableAssetAllocation>(entity =>
        {
            entity.ToTable("NonReturnableAssetAllocation");

            entity.Property(e => e.AssignedDate).HasColumnType("timestamp without time zone");
        });

        modelBuilder.Entity<NonReturnableAssetField>(entity =>
        {
            entity.ToTable("NonReturnableAssetField");

            entity.Property(e => e.FieldName).HasMaxLength(255);
            entity.Property(e => e.ValueType).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.AssetType).WithMany(p => p.NonReturnableAssetFields)
                .HasForeignKey(d => d.AssetTypeId)
                .HasConstraintName("FK_NonReturnableAssetField_AssetType");
        });

        modelBuilder.Entity<NonReturnableAssetPersonnelPermit>(entity =>
        {
            entity.ToTable("NonReturnableAssetPersonnelPermit");

            entity.Property(e => e.ModifiedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.AssetType).WithMany(p => p.NonReturnableAssetPersonnelPermits)
                .HasForeignKey(d => d.AssetTypeId)
                .HasConstraintName("FK_NonReturnableAssetPersonnelPermit_AssetType");
        });

        modelBuilder.Entity<NonReturnableAssetPersonnelSetting>(entity =>
        {
            entity.ToTable("NonReturnableAssetPersonnelSetting");

            entity.HasIndex(e => new { e.FieldId, e.PersonnelId }, "NonClusteredIndex-20240927-090804");

            entity.Property(e => e.Value).HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.Field).WithMany(p => p.NonReturnableAssetPersonnelSettings)
                .HasForeignKey(d => d.FieldId)
                .HasConstraintName("NonReturnableAssetPersonnelSetting_NonReturnableAssetField_Id_fk");
        });

        modelBuilder.Entity<NonReturnableAssetType>(entity =>
        {
            entity.ToTable("NonReturnableAssetType");

            entity.Property(e => e.IconName).HasMaxLength(255);
            entity.Property(e => e.ModifiedDate).HasColumnType("timestamp without time zone");
            entity.Property(e => e.Name).HasMaxLength(255);

            entity.HasOne(d => d.Department).WithMany(p => p.NonReturnableAssetTypes)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("NonReturnableAssetType_Department_Id_fk");
        });

        modelBuilder.Entity<Personnel>(entity =>
        {
            entity.HasKey(e => e.PersonnelNo).HasName("PK__Personne__CAFBB3FC60B0E010");

            entity.HasIndex(e => e.PersonnelId, "NonClusteredIndex-20240927-085449");

            entity.HasIndex(e => e.CardNum, "NonClusteredIndex-20241003-093352");

            entity.Property(e => e.PersonnelNo).ValueGeneratedNever();
            entity.Property(e => e.CardNum).HasMaxLength(10);
            entity.Property(e => e.CompanyDesc).HasMaxLength(100);
            entity.Property(e => e.ContactNumber).HasMaxLength(40);
            entity.Property(e => e.DepartmentDesc).HasMaxLength(80);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FirstName).HasMaxLength(40);
            entity.Property(e => e.FullName).HasMaxLength(80);
            entity.Property(e => e.LastName).HasMaxLength(40);
            entity.Property(e => e.ModifiedDate).HasColumnType("timestamp");
            entity.Property(e => e.PositionDesc).HasMaxLength(255);
            entity.Property(e => e.WorkPhone).HasMaxLength(40);
        });

        modelBuilder.Entity<TypeTraining>(entity =>
        {
            entity.ToTable("TypeTraining");

            entity.Property(e => e.Type).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasColumnType("timestamp without time zone");
            entity.Property(e => e.UpdatedAt).HasColumnType("timestamp without time zone");

            entity.HasOne(d => d.AssetType).WithMany(p => p.TypeTrainings)
                .HasForeignKey(d => d.AssetTypeId)
                .HasConstraintName("TypeTraining_AssetType_Id_fk");

            entity.HasOne(d => d.NonReturnableAssetType).WithMany(p => p.TypeTrainings)
                .HasForeignKey(d => d.NonReturnableAssetTypeId)
                .HasConstraintName("TypeTraining_NonReturnableAssetType_Id_fk");
        });

        OnModelCreatingPartial(modelBuilder);
    }
    
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

