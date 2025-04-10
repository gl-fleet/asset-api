namespace asset_allocation_api.Models
{
    public class NonReturnableAssetPersonnelPermitView
    {
        public int Id { get; set; }

        public int? PersonnelNo { get; set; }

        public int? AssetTypeId { get; set; }

        public short? Enabled { get; set; }

        public int? ModifiedUserId { get; set; }

        public DateTime? ModifiedDate { get; set; }

        /*
        public virtual TypePart? Type { get; set; }
        public virtual List<SettingPart>? Settings { get; set; }
        */

        public string? TypeName { get; set; }
        public string? TypeIconName { get; set; }
        public int? TypeLimit { get; set; }
        public int? TypeCooldownDays { get; set; }

        public DateTime? AllocationLastDate { get; set; }
        public string? AllocationColor { get; set; }
        public int AllocatedCount { get; set; }
        public int? SettingsId { get; set; }
        public string? SettingsFieldName { get; set; }
        public string? SettingsValueType { get; set; }
        public string? SettingsValue { get; set; }
    }

    public class TypePart
    {
        public string? Name { get; set; }
        public string? IconName { get; set; }
        public int? Limit { get; set; }
        public int? CooldownDays { get; set; }
    }

    public class SettingPart
    {
        public int Id { get; set; }
        public string? FieldName { get; set; }
        public string? ValueType { get; set; }
        public string? Value { get; set; }
    }
}