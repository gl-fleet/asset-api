using System.Reflection;

namespace asset_allocation_api.Util
{
    public class ReflectionUtils
    {
        public static void UpdateForType<T>(T source, T destination)
        {
            FieldInfo[] myObjectFields = typeof(T).GetFields(
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            foreach (FieldInfo fi in myObjectFields)
            {
                fi.SetValue(destination, fi.GetValue(source));
            }
        }
    }
}
