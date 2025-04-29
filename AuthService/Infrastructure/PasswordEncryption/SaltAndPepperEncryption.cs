using System.Security.Cryptography;
using AuthService.Properties;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
namespace AuthService.Infrastructure.Encryption
{
    public class SaltAndPepperEncryption : IPasswordEncryption
    {
        private static string _pepperLetters => AppConfig.PepperLetters;
        private static int _pepperLength => AppConfig.PepperLength;

        public (string encryptedPassword, string encryptionKey) EncryptionPassword(string password)
        {
            byte[] generatedSalt = GenerateSalt();
            string generatedPepper = GeneratePepper();
            var encryptedPassword = HashPassword(password + generatedPepper, generatedSalt);
            return (encryptedPassword, Convert.ToBase64String(generatedSalt));
        }

        public bool CheckPasswordValid(string? password, string? encryptedPassword, string? encryptionKey)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(encryptedPassword) || string.IsNullOrEmpty(encryptionKey))
            {
                return false;
            }
            byte[] salt;
            try
            {
                salt = Convert.FromBase64String(encryptionKey);
            }
            catch (FormatException)
            {
                return false; // If decryption fails due to a malformed encryption key
            }
            for (int i = 0; i < Math.Pow(_pepperLetters.Length, _pepperLength); i++)
            {
                int[] currentPepper = ConvertToPepperArray(i);
                var pepper = ConvertIndexArrayToPepperWord(currentPepper);
                var currentEncryptedPassword = HashPassword(password + pepper, salt);
                if (encryptedPassword == currentEncryptedPassword)
                {
                    return true;
                }

            }
            return false;
        }
        private static string HashPassword(string pepperedPassword, byte[] salt)
        {
            // derive a 256-bit subkey (use HMACSHA256 with 100,000 iterations)
            string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: pepperedPassword!,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));
            return hashedPassword;
        }

        private static string GeneratePepper()
        {
            string result = "";

            for (int i = 0; i < _pepperLength; i++)
            {
                int randomIndex = RandomNumberGenerator.GetInt32(_pepperLetters.Length - 1); // Pick a random index from the string
                result += _pepperLetters[randomIndex]; // Add the random character to the result array
            }
            return result;
        }
        private static byte[] GenerateSalt()
        {
            // Generate a 128-bit salt using a sequence of
            // cryptographically strong random bytes.
            return RandomNumberGenerator.GetBytes(128 / 8); // divide by 8 to convert bits to bytes
        }

        private static string ConvertIndexArrayToPepperWord(int[] indexArray)
        {
            string word = "";
            foreach (int i in indexArray)
            {
                word += _pepperLetters[i];
            }
            return word;
        }

        private static int[] ConvertToPepperArray(int value)
        {
            int[] array = new int[_pepperLength];
            int max = _pepperLetters.Length;
            int index = 0;
            while (value > 0 && index < _pepperLength)
            {
                array[index] += value; // Add the value to the current cell
                if (array[index] >= max) // Check if it overflows
                {
                    value = array[index] / max; // Carry over to the next cell
                    array[index] %= max;        // Keep only the remainder in the current cell
                }
                else
                {
                    value = 0; // If no overflow, we're done
                }

                index++; // Move to the next cell
            }
            return array;
        }
    }
}