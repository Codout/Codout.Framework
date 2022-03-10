namespace Codout.Framework.Common.Security
{
    public static class RandomString
    {
        public static string GenerateRandomString(int length)
        {
            var random = new byte[length];

            System.Security.Cryptography.RandomNumberGenerator.Fill(random);

            char[] chars = {
                'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
                '0','1','2','3','4','5','6','7','8','9'
            };

            var charsLength = chars.Length;
            var strBuilder = new char[length];

            for (var i = 0; i < length; i++)
            {
                strBuilder[i] = (chars[random[i] % charsLength]);
            }

            return new string(strBuilder);
        }
    }
}
