namespace Sustainable.Helpers
{
    public static class IdGenerator
    {
        public static string GenerateId(string prefix, string? lastId)
        {
            if (string.IsNullOrEmpty(lastId)) return $"{prefix}001";
            var n = int.Parse(lastId.Split('_')[1]);
            return $"{prefix}{(n + 1).ToString("D3")}";
        }
    }
}
