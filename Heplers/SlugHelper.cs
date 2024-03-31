using System.Text.RegularExpressions;
using System.Text;

namespace OnlineJwellery_Shopping.Heplers
{
    public static class SlugHelper
    {
        public static string GenerateSlug(string productName, int productId)
        {
            string uniqueIdentifier = Guid.NewGuid().ToString().Substring(0, 8); // Tạo một chuỗi ngẫu nhiên duy nhất
            string str = RemoveAccent(productName).ToLower() + "-" + uniqueIdentifier + "-" + productId.ToString();
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = Regex.Replace(str, @"\s+", " ").Trim();
            str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
            str = Regex.Replace(str, @"\s", "-");
            return str;
        }


        public static string RemoveAccent(string text)
        {
            byte[] bytes = Encoding.GetEncoding("UTF-8").GetBytes(text);
            return Encoding.ASCII.GetString(bytes);
        }
    }
}
