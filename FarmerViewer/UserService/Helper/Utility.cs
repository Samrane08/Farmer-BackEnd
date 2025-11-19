namespace UserService.Helper
{
    public class Utility
    {
        public static string GeneratePassword(int length = 10)
        {
            if (length < 4) length = 4; // minimum to satisfy all conditions

            var random = new Random();

            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string special = "!@#$%^&*()-_=+[]{};:<>/?";

            // Ensure at least one of each
            var chars = new List<char>
    {
        upper[random.Next(upper.Length)],
        lower[random.Next(lower.Length)],
        digits[random.Next(digits.Length)],
        special[random.Next(special.Length)]
    };

            // Fill the rest randomly
            string all = upper + lower + digits + special;
            while (chars.Count < length)
            {
                chars.Add(all[random.Next(all.Length)]);
            }

            // Shuffle so guaranteed chars aren’t always at start
            return new string(chars.OrderBy(x => random.Next()).ToArray());
        }

    }
}
