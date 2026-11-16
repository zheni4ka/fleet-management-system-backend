using business_logic.Constants;
using System.Security.Cryptography;

namespace business_logic.Helpers
{
    public static class TemporaryPasswordGenerator
    {
        public static string GenerateTemporaryPassword()
        {
            var allTemporaryPasswordCharacters = AccountConstraints.TemporaryPasswordUppercaseLetters
                + AccountConstraints.TemporaryPasswordLowercaseLetters
                + AccountConstraints.TemporaryPasswordDigits;

            var temporaryPasswordCharacters = new List<char>
            {
                PickRandomCharacter(AccountConstraints.TemporaryPasswordUppercaseLetters),
                PickRandomCharacter(AccountConstraints.TemporaryPasswordLowercaseLetters),
                PickRandomCharacter(AccountConstraints.TemporaryPasswordDigits)
            };

            while (temporaryPasswordCharacters.Count < AccountConstraints.TemporaryPasswordLength)
            {
                temporaryPasswordCharacters.Add(PickRandomCharacter(allTemporaryPasswordCharacters));
            }

            var shuffledTemporaryPasswordCharacters = temporaryPasswordCharacters.ToArray();
            RandomNumberGenerator.Shuffle(shuffledTemporaryPasswordCharacters.AsSpan());
            return new string(shuffledTemporaryPasswordCharacters);
        }

        private static char PickRandomCharacter(string characterSet)
        {
            return characterSet[RandomNumberGenerator.GetInt32(characterSet.Length)];
        }
    }
}
