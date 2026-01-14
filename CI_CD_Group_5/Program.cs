using System;
using System.Globalization;
using System.Linq;

namespace PersonnummerKontroll
{
    internal static class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("Personnummerkontroll (Sverige)");
            Console.WriteLine("Accepterade format: YYMMDD-XXXX, YYMMDDXXXX, YYYYMMDD-XXXX, YYYYMMDDXXXX (även +)");
            Console.WriteLine();

            while (true)
            {
                Console.Write("Ange personnummer (eller 'q' för att avsluta): ");
                var input = Console.ReadLine()?.Trim();

                if (string.Equals(input, "q", StringComparison.OrdinalIgnoreCase))
                    return;

                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Fel: Tom inmatning.\n");
                    continue;
                }

                var result = PersonnummerValidator.Validate(input);

                if (result.IsValid)
                {
                    Console.WriteLine("OK: Personnumret är giltigt.");
                    Console.WriteLine($"Normaliserat: {result.Normalized}");
                    Console.WriteLine($"Datum: {result.BirthDate:yyyy-MM-dd}");
                    Console.WriteLine($"Kön (heuristik på näst sista siffran): {result.GenderHint}");
                }
                else
                {
                    Console.WriteLine("Fel: Personnumret är ogiltigt.");
                    Console.WriteLine(result.ErrorMessage);
                }

                Console.WriteLine();
            }
        }
    }

    public static class PersonnummerValidator
    {
        public static ValidationResult Validate(string input)
        {
            var trimmed = input.Trim();

            char? separator = trimmed.Contains('+') ? '+' : trimmed.Contains('-') ? '-' : (char?)null;

            var digits = new string(trimmed.Where(char.IsDigit).ToArray());

            if (digits.Length != 10 && digits.Length != 12)
                return ValidationResult.Invalid("Fel format: Personnumret måste innehålla 10 eller 12 siffror (exklusive '-' eller '+').");

            var last10 = digits.Length == 12 ? digits.Substring(2, 10) : digits;

            DateTime birthDate;
            if (digits.Length == 12)
            {
                var yyyyMMdd = digits.Substring(0, 8);
                if (!TryParseDateExact(yyyyMMdd, "yyyyMMdd", out birthDate))
                    return ValidationResult.Invalid("Ogiltigt datum: Datumdelen (YYYYMMDD) är inte ett giltigt datum.");
            }
            else
            {
                var yyMMdd = digits.Substring(0, 6);
                if (!TryParseDateExact(yyMMdd, "yyMMdd", out var parsed))
                    return ValidationResult.Invalid("Ogiltigt datum: Datumdelen (YYMMDD) är inte ett giltigt datum.");

                birthDate = ResolveCentury(parsed, separator);
            }
            if (!IsValidLuhnForPersonnummer(last10))
                return ValidationResult.Invalid("Ogiltig kontrollsiffra: Luhn-kontrollen (mod 10) stämmer inte.");

            var normalized = FormatNormalized(birthDate, last10, separator);

            return ValidationResult.Valid(normalized, birthDate, GenderHintFromSerial(last10));
        }

        private static bool TryParseDateExact(string value, string format, out DateTime date)
        {
            return DateTime.TryParseExact(
                value,
                format,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out date
            );
        }

        private static DateTime ResolveCentury(DateTime yyDate, char? separator)
        {
            var today = DateTime.Today;

            int yy = yyDate.Year % 100;
            var candidate2000 = new DateTime(2000 + yy, yyDate.Month, yyDate.Day);
            var candidate1900 = new DateTime(1900 + yy, yyDate.Month, yyDate.Day);
            var candidate1800 = new DateTime(1800 + yy, yyDate.Month, yyDate.Day);

            if (separator == '+')
            {
                var options = new[] { candidate2000, candidate1900, candidate1800 }
                    .Where(d => (today - d).TotalDays >= 365.25 * 100)
                    .OrderByDescending(d => d); 
                return options.FirstOrDefault() == default ? candidate1900 : options.First();
            }

            if (separator == '-')
            {
                var options = new[] { candidate2000, candidate1900, candidate1800 }
                    .Where(d => d <= today && (today - d).TotalDays < 365.25 * 100)
                    .OrderByDescending(d => d);
                return options.FirstOrDefault() == default ? (candidate2000 <= today ? candidate2000 : candidate1900) : options.First();
            }

            var nonFuture = new[] { candidate2000, candidate1900, candidate1800 }
                .Where(d => d <= today)
                .OrderByDescending(d => d)
                .ToArray();

            return nonFuture.Length > 0 ? nonFuture[0] : candidate1900;
        }

        private static bool IsValidLuhnForPersonnummer(string last10Digits)
        {
            if (last10Digits.Length != 10 || !last10Digits.All(char.IsDigit))
                return false;

            int sum = 0;

            for (int i = 0; i < 9; i++)
            {
                int digit = last10Digits[i] - '0';
                int factor = (i % 2 == 0) ? 2 : 1;
                int product = digit * factor;
                sum += (product / 10) + (product % 10);
            }

            int controlDigit = last10Digits[9] - '0';
            int computed = (10 - (sum % 10)) % 10;

            return computed == controlDigit;
        }

        private static string FormatNormalized(DateTime birthDate, string last10, char? separator)
        {
            var sep = separator ?? '-';
            var serial = last10.Substring(6, 4); 
            return $"{birthDate:yyyyMMdd}{sep}{serial}";
        }

        private static string GenderHintFromSerial(string last10)
        {
            int penultimate = last10[8] - '0';
            return (penultimate % 2 == 0) ? "Kvinna (jämn)" : "Man (udda)";
        }
    }

    public sealed class ValidationResult
    {
        public bool IsValid { get; }
        public string Normalized { get; }
        public DateTime BirthDate { get; }
        public string GenderHint { get; }
        public string ErrorMessage { get; }

        private ValidationResult(bool isValid, string normalized, DateTime birthDate, string genderHint, string errorMessage)
        {
            IsValid = isValid;
            Normalized = normalized;
            BirthDate = birthDate;
            GenderHint = genderHint;
            ErrorMessage = errorMessage;
        }

        public static ValidationResult Valid(string normalized, DateTime birthDate, string genderHint)
            => new ValidationResult(true, normalized, birthDate, genderHint, "");

        public static ValidationResult Invalid(string message)
            => new ValidationResult(false, "", default, "", message);
    }
}
