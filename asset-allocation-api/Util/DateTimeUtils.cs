using asset_allocation_api.Config;

namespace asset_allocation_api.Util
{
    public class DateTimeUtils
    {
        public static DateTime ShiftEndingTime(DateTime input)
        {
            if (input.Hour >= AssetAllocationConfig.assetAllocShiftStart &&
                input.Hour < AssetAllocationConfig.assetAllocShiftEnd)
            {
                input.AddHours((-1 * input.Hour) + AssetAllocationConfig.assetAllocShiftEnd);
                input.AddMinutes(-1 * input.Minute);
                input.AddSeconds(-1 * input.Second);
            }
            else
            {
                input.AddHours((-1 * input.Hour) + AssetAllocationConfig.assetAllocShiftStart);
                input.AddMinutes(-1 * input.Minute);
                input.AddSeconds(-1 * input.Second);
            }

            return input;
        }

        public static DateTime ShiftStartingTime(DateTime input)
        {
            if (input.Hour >= AssetAllocationConfig.assetAllocShiftStart &&
                input.Hour < AssetAllocationConfig.assetAllocShiftEnd)
            {
                input.AddHours((-1 * input.Hour) + AssetAllocationConfig.assetAllocShiftStart);
                input.AddMinutes(-1 * input.Minute);
                input.AddSeconds(-1 * input.Second);
            }
            else
            {
                input.AddHours((-1 * input.Hour) + AssetAllocationConfig.assetAllocShiftEnd);
                input.AddMinutes(-1 * input.Minute);
                input.AddSeconds(-1 * input.Second);
            }

            return input;
        }

        public static bool IsPreviousShift(DateTime input)
        {
            string AllocatedShift =
                input.Hour >= AssetAllocationConfig.assetAllocShiftStart &&
                input.Hour < AssetAllocationConfig.assetAllocShiftEnd
                    ? "DayShift"
                    : "NightShift";
            string CurrentShift =
                DateTime.Now.Hour >= AssetAllocationConfig.assetAllocShiftStart &&
                DateTime.Now.Hour < AssetAllocationConfig.assetAllocShiftEnd
                    ? "DayShift"
                    : "NightShift";

            return AllocatedShift != CurrentShift;
        }

        public static bool IsNowShift(DateTime allocatedDateTime)
        {
            var now = DateTime.Now;
            DateTime nowShiftStart, nowShiftEnd;

            if (now.Hour >= AssetAllocationConfig.assetAllocShiftStart && now.Hour < AssetAllocationConfig.assetAllocShiftEnd) /** Is day shift? **/
            {
                nowShiftStart = new DateTime(now.Year, now.Month, now.Day, AssetAllocationConfig.assetAllocShiftStart, 0, 0);
                nowShiftEnd = new DateTime(now.Year, now.Month, now.Day, AssetAllocationConfig.assetAllocShiftEnd, 0, 0);
            }
            else /** Is night shift? **/
            {
                var tom= DateTime.Now.AddDays(1);
                nowShiftStart = new DateTime(now.Year, now.Month, now.Day, AssetAllocationConfig.assetAllocShiftEnd, 0, 0);
                nowShiftEnd = new DateTime(tom.Year, tom.Month, tom.Day, AssetAllocationConfig.assetAllocShiftStart, 0, 0);
            }

            return allocatedDateTime >= nowShiftStart && allocatedDateTime <= nowShiftEnd;
        }

        public static DateTime GetNowShiftStartDateTime()
        {
            var now = DateTime.Now;

            if (now.Hour >= AssetAllocationConfig.assetAllocShiftStart &&
                now.Hour <= AssetAllocationConfig.assetAllocShiftEnd)
            {
                return new DateTime(now.Year, now.Month, now.Day, AssetAllocationConfig.assetAllocShiftStart, 0, 0);
            }

            return new DateTime(now.Year, now.Month, now.Day, AssetAllocationConfig.assetAllocShiftEnd, 0, 0);
        }
        
        public static DateTime GetNowShiftEndDateTime()
        {
            var now = DateTime.Now;

            if (now.Hour >= AssetAllocationConfig.assetAllocShiftStart &&
                now.Hour <= AssetAllocationConfig.assetAllocShiftEnd)
            {
                return new DateTime(now.Year, now.Month, now.Day, AssetAllocationConfig.assetAllocShiftEnd, 0, 0);
            }

            var tom= DateTime.Now.AddDays(1);
            return new DateTime(tom.Year, tom.Month, tom.Day, AssetAllocationConfig.assetAllocShiftStart, 0, 0);
        }
    }
}