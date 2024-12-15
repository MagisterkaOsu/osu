using System.Text;

namespace Cipher.Helpers
{
    public class StringHelper
    {
        public static string ParseBitString(string bitString)
        {
            var result = new StringBuilder();

            for (int i = 0; i < bitString.Length; i += 8)
            {
                if (bitString.Length >= i + 7)
                {
                    string byteString = bitString.Substring(i, 8);
                    char character = (char)Convert.ToInt32(byteString, 2);
                    result.Append(character);
                }
            }

            return result.ToString();
        }
    }
}
