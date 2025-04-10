using asset_allocation_api.Model.CustomModel;

namespace asset_allocation_api.Config
{
    public class AssetAllocationConfig
    {
        private static string GetEnvironmentVariable(string name, string defaultValue)
      => Environment.GetEnvironmentVariable(name) is { Length: > 0 } v ? v : defaultValue;
        public const string NameSpace = "asset_allocation_api";
        public const string Version = "v1";

        public const string ConstResultString = "Result";

        public static readonly string env = "-UNKNOWN";

        public static readonly string assetAllocationServer = "localhost";
        public static readonly string assetAllocationDatabase = "asset_allocation_db";
        public static readonly string assetAllocationUserid = "asset_allocation_user";
        public static readonly string assetAllocationPassword = "Aa@123";

        public static readonly string assetAllocationConnectionString
            = $"Host={assetAllocationServer};Port=5432;Database={assetAllocationDatabase};Username={assetAllocationUserid};Password={assetAllocationPassword};";

        public static readonly string ldapPath = GetEnvironmentVariable("OTUG_LDAP_PATH", "mnoytcorpdc7.corp.riotinto.org");
        public static readonly string ldapDomainName = GetEnvironmentVariable("OTUG_LDAP_DOMAIN_NAME", "CORP");

        public static readonly string JWT_SECRET = GetEnvironmentVariable("OTUG_JWT_SECRET", "8;jAq0}SM]sjDe2T#AZ{j1>M`(j>,.lX~'$C9)A/xCi|wia=h;dXf?gpb-|VF*d0000");

        public static readonly string microServiceAuthUrl = GetEnvironmentVariable("OTUG_MICROSERVICE_AUTH", "http://otauthservice-dev.otapi.corp.riotinto.org/api/v1/authorize");
        public static readonly string microServiceToken = GetEnvironmentVariable("OTUG_MICROSERVICE_TOKEN", "***");

        public static readonly string imsBaseUrl = GetEnvironmentVariable("OTUG_IMS_BASE_URL", "http://ims-dev.otapi.corp.riotinto.org/api/v1/Personnel/");
        public static readonly string imsAllQualification = imsBaseUrl + GetEnvironmentVariable("OTUG_IMS_ALL_QUALIFICATION", "Qualification");
        public static readonly string imsPersonnelQualifications = imsBaseUrl + GetEnvironmentVariable("OTUG_IMS_PERSONNEL_QUALIFICATIONS", "AttendancesByQualifications");

        public static readonly string kafkaPersonnelTopic = GetEnvironmentVariable("OTUG_KAFKA_PERSONNEL_TOPIC", "ims_full_personnel_dev");
        public static readonly string kafkaSocketAllocationTopic = GetEnvironmentVariable("OTUG_KAFKA_SOCKET_ALLOCATION_TOPIC", "asset-allocation-allocations-dev");
        public static readonly string kafkaBootstrapServers = GetEnvironmentVariable("OTUG_KAFKA_SERVER", "MNOYTKFKBRD1.corp.riotinto.org:9092,MNOYTKFKBRD2.corp.riotinto.org:9092,MNOYTKFKBRD3.corp.riotinto.org:9092");
        public static readonly string kafkaConsumerGroupId = GetEnvironmentVariable("OTUG_KAFKA_GROUP", "ot -api-asset-allocation-dev");

        public static readonly int assetAllocShiftStart = Convert.ToInt32(GetEnvironmentVariable("OTUG_ASSET_ALLOC_SHIFT_START", "4"));
        public static readonly int assetAllocShiftEnd = Convert.ToInt32(GetEnvironmentVariable("OTUG_ASSET_ALLOC_SHIFT_END", "16"));

        public static readonly string assetAllocShiftName1 = GetEnvironmentVariable("OTUG_ASSET_ALLOC_SHIFT_NAME1", "Morning");
        public static readonly string assetAllocShiftName2 = GetEnvironmentVariable("OTUG_ASSET_ALLOC_SHIFT_NAME2", "Night");

        public static readonly string caplampURI = GetEnvironmentVariable("CAPLAMPURI", "http://caplamp-assign.otapi.corp.riotinto.org/api/v1/");
        public static readonly string caplampBodyJson = "{{\"personnel\":{{\"id\":{0},\"no\":{1}}},\"tag\":{{\"id\":0,\"mac\":\"{2}\",\"rfid\":\"{3}\"}}}}";

        public static readonly string S3Uri = GetEnvironmentVariable("S3_URI", "http://mnoytpslnasfssz2.corp.riotinto.org:9020");
        public static readonly string S3AccessKey = GetEnvironmentVariable("S3_ACCESS_KEY", "***");
        public static readonly string S3SecretKey = GetEnvironmentVariable("S3_SECRET_KEY", "***");
        public static readonly string S3BucketName = GetEnvironmentVariable("S3_BUCKET_NAME", "s3-gobi-itengineering-assetallocation");

        public static readonly string MINLOG_ASSIGN_URI = GetEnvironmentVariable("MINLOG_ASSIGN_URI", "http://minlog-assign.otapi.corp.riotinto.org");
        public static readonly string PLI_ASSIGN_URI = GetEnvironmentVariable("PLI_ASSIGN_URI", "http://plitag-assign-dev.otapi.corp.riotinto.org");
        public static readonly string MINLOG_ASSIGN_TOKEN = GetEnvironmentVariable("MINLOG_ASSIGN_TOKEN", "***");
        public static readonly string MINLOG_WIFI_CHECK_DETECTION_MINUTE = GetEnvironmentVariable("MINLOG_WIFI_CHECK_DETECTION_MINUTE", "15");

        public static readonly string UG_ACCESS_ALLOCATION_URI = GetEnvironmentVariable("UG_ACCESS_ALLOCATION_URI", "http://ugaccessallocation-dev.otapi.corp.riotinto.org");
        public static readonly string TYCO_AGGREGATOR_URI = GetEnvironmentVariable("TYCO_AGGREGATOR_URI", "http://ims-tyco-aggregator-dev.otapi.corp.riotinto.org");
        public static readonly string WIFI_CHECK_CONFIG_NAME = GetEnvironmentVariable("WIFI_CHECK_CONFIG_NAME", "Caplamp Detection");
        public static readonly string PLI_CAPLAMP_TOGGLE_CONFIG_NAME = GetEnvironmentVariable("PLI_CAPLAMP_TOGGLE_CONFIG_NAME", "PLI assign / Caplamp assign");
        public static readonly string CAPLAMP_WITH_PLI_ASSET_TYPE_NAME = GetEnvironmentVariable("CAPLAMP_WITH_PLI_ASSET_TYPE_NAME", "Caplamp with PLI");
        public static readonly string jwtExpiryDays = GetEnvironmentVariable("JWT_EXPIRE_DAYS", "14");
        public static IEnumerable<AssignmentSetting> AssignmentSettings = new List<AssignmentSetting>
        {
            new AssignmentSetting
            {
                SystemIdentityName = "PLI",
                BaseURL = PLI_ASSIGN_URI + "/api/v1/CaplampAssignments/assign",
                MustSuccess = true,
                Assignable = true,
                Unassignable = true,
                UnAssignMethod = "DELETE",
                AssignMethod = "POST"
            },

            new AssignmentSetting
            {
                SystemIdentityName = "Minlog",
                BaseURL = MINLOG_ASSIGN_URI + "/api/v1/assign",
                MustSuccess = true,
                Assignable = true,
                Unassignable = false,
                AssignMethod = "POST"
            }
        };
    }
}

