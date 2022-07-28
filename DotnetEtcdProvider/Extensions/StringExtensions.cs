namespace DotnetEtcdProvider.Extensions
{
    public static class StringExtensions
    {
        public static bool HasData(this string str)
        {
            return str is not null && str.Length > 0;
        }

        public static bool IsEmpty(this string str)
        {
            return str is null || str.Length <= 0;
        }

        public static bool IsUrlWithProxy(this string str)
        {
            return str.Contains("http", StringComparison.OrdinalIgnoreCase) || str.Contains("https", StringComparison.OrdinalIgnoreCase);
        }
    }
}
