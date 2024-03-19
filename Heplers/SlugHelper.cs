using System.Text.RegularExpressions;
using System.Text;

namespace OnlineJwellery_Shopping.Heplers
{
    public static class SlugHelper
    {
        public static string GenerateSlug(string phrase)
        {
            string str = RemoveAccent(phrase).ToLower();
            str = Regex.Replace(str, @"[^a-z0-9\s-]", ""); // Remove các ký tự không phải chữ cái, số, dấu cách, dấu gạch ngang
            str = Regex.Replace(str, @"\s+", " ").Trim(); // Thay thế nhiều dấu cách liên tiếp bằng một dấu cách và loại bỏ dấu cách ở đầu và cuối chuỗi
            str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim(); // Giới hạn chiều dài của slug
            str = Regex.Replace(str, @"\s", "-"); // Thay thế dấu cách bằng dấu gạch ngang
            return str;
        }

        public static string RemoveAccent(string text)
        {
            byte[] bytes = Encoding.GetEncoding("UTF-8").GetBytes(text);
            return Encoding.ASCII.GetString(bytes);
        }
    }
}
