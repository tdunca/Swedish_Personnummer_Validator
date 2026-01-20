using System;
using System.Globalization;
using System.Linq;

namespace CI_CD_Group_8 // Viktigt: samma namespace som resten av projektet
{
    /// <summary>
    /// Entry point för konsolapplikationen.
    /// Ansvarar endast för in-/utmatning (UI-logik).
    /// Affärslogiken ligger i PersonnummerValidator.
    /// </summary>
    internal static class Program
    {
        static void Main()
        {
            // Säkerställer att svenska tecken visas korrekt i konsolen
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("Personnummerkontroll – Grupp 8");
            Console.WriteLine("Giltiga format:");
            Console.WriteLine("YYMMDD-XXXX, YYMMDDXXXX, YYYYMMDD-XXXX, YYYYMMDDXXXX (+ tillåts)");
            Console.WriteLine();

            // Kör tills användaren själv avslutar
            while (true)
            {
                Console.Write("Ange personnummer (eller 'q' för att avsluta): ");
                var input = Console.ReadLine()?.Trim();

                // Avsluta programmet
                if (string.Equals(input, "q", StringComparison.OrdinalIgnoreCase))
                    return;

                // Tom inmatning = fel
                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Fel: Tom inmatning.\n");
                    continue;
                }

                // Validera personnumret
                var result = PersonnummerValidator.Validate(input);

                if (result.IsValid)
                {
                    Console.WriteLine("✔ Personnumret är giltigt");
                    Console.WriteLine($"Normaliserat format: {result.Normalized}");
                    Console.WriteLine($"Födelsedatum: {result.BirthDate:yyyy-MM-dd}");
                    Console.WriteLine($"Kön (heuristik): {result.GenderHint}");
                }
                else
                {
                    Console.WriteLine("✖ Personnumret är ogiltigt");
                    Console.WriteLine(result.ErrorMessage);
                }

                Console.WriteLine();
            }
        }
    }

    /// <summary>
    /// Innehåller all affärslogik för validering av svenska personnummer.
    /// </summary>
    public static class PersonnummerValidator
    {
        public static ValidationResult Validate(string input)
        {
            var trimmed = input.Trim();

            // Identifiera eventuell separator (+ eller -)
            char? separator =
                trimmed.Contains('+') ? '+' :
                trimmed.Contains('-') ? '-' :
                (char?)null;

            // Ta bort allt utom siffror
            var digits = new string(trimmed.Where(char.IsDigit).ToArray());

            // Svenskt personnummer måste ha 10 eller 12 siffror
            if (digits.Length != 10 && digits.Length != 12)
            {
                return ValidationResult.Invalid(
                    "Fel format: Personnumret måste bestå av 10 eller 12 siffror.");
            }

            // De sista 10 siffrorna används för Luhn-kontrollen
            var last10 = digits.Length == 12
                ? digits.Substring(2, 10)
                : digits;

            // Validera födelsedatum
            DateTime birthDate;

            if (digits.Length == 12)
            {
                // YYYYMMDD
                if (!TryParseDateExact(digits.Substring(0, 8), "yyyyMMdd", out birthDate))
                {
                    return ValidationResult.Invalid(
                        "Ogiltigt datum: Datumdelen (YYYYMMDD) är inte giltig.");
                }
            }
            else
            {
                // YYMMDD – kräver sekeltolkning
                if (!TryParseDateExact(digits.Substring(0, 6), "yyMMdd", out var parsed))
                {
                    return ValidationResult.Invalid(
                        "Ogiltigt datum: Datumdelen (YYMMDD) är inte giltig.");
                }

                birthDate = ResolveCentury(parsed, separator);
            }

            // Kontrollsiffra (Luhn-algoritmen)
            if (!IsValidLuhnForPersonnummer(last10))
            {
                return ValidationResult.Invalid(
                    "Ogiltig kontrollsiffra: Luhn-kontrollen misslyckades.");
            }

            // Normalisera till standardformat
            var normalized = FormatNormalized(birthDate, last10, separator);

            return ValidationResult.Valid(
                normalized,
                birthDate,
                GenderHintFromSerial(last10));
        }

        /// <summary>
        /// Försöker tolka ett datum exakt enligt angivet format.
        /// </summary>
        private static bool TryParseDateExact(string value, string format, out DateTime date)
        {
            return DateTime.TryParseExact(
                value,
                format,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out date);
        }

        /// <summary>
        /// Avgör rätt sekel baserat på separator och dagens datum.
        /// </summary>
        private static DateTime ResolveCentury(DateTime yyDate, char? separator)
        {
            var today = DateTime.Today;
            int yy = yyDate.Year % 100;

            var candidates = new[]
            {
                new DateTime(2000 + yy, yyDate.Month, yyDate.Day),
                new DateTime(1900 + yy, yyDate.Month, yyDate.Day),
                new DateTime(1800 + yy, yyDate.Month, yyDate.Day)
            };

            if (separator == '+')
                return candidates.First(d => (today - d).TotalDays >= 365.25 * 100);

            if (separator == '-')
                return candidates.Where(d => d <= today).Max();

            // Ingen separator → välj rimligast datum i dåtid
            return candidates.Where(d => d <= today).Max();
        }

        /// <summary>
        /// Luhn-algoritm anpassad för personnummer.
        /// </summary>
        private static bool IsValidLuhnForPersonnummer(string last10)
        {
            int sum = 0;

            for (int i = 0; i < 9; i++)
            {
                int digit = last10[i] - '0';
                int factor = (i % 2 == 0) ? 2 : 1;
                int product = digit * factor;
                sum += (product > 9) ? product - 9 : product;
            }

            int control = last10[9] - '0';
            return (10 - (sum % 10)) % 10 == control;
        }

        /// <summary>
        /// Skapar ett normaliserat personnummer i format YYYYMMDD-XXXX.
        /// </summary>
        private static string FormatNormalized(DateTime birthDate, string last10, char? separator)
        {
            char sep = separator ?? '-';
            return $"{birthDate:yyyyMMdd}{sep}{last10.Substring(6, 4)}";
        }

        /// <summary>
        /// Enkel könsindikering baserad på näst sista siffran.
        /// </summary>
        private static string GenderHintFromSerial(string last10)
        {
            int digit = last10[8] - '0';
            return (digit % 2 == 0) ? "Kvinna" : "Man";
        }
    }

    /// <summary>
    /// Resultatobjekt för validering – används för tydlig felhantering.
    /// </summary>
    public sealed class ValidationResult
    {
        public bool IsValid { get; }
        public string Normalized { get; }
        public DateTime BirthDate { get; }
        public string GenderHint { get; }
        public string ErrorMessage { get; }

        private ValidationResult(bool valid, string normalized, DateTime date, string gender, string error)
        {
            IsValid = valid;
            Normalized = normalized;
            BirthDate = date;
            GenderHint = gender;
            ErrorMessage = error;
        }

        public static ValidationResult Valid(string normalized, DateTime date, string gender)
            => new ValidationResult(true, normalized, date, gender, "");

        public static ValidationResult Invalid(string message)
            => new ValidationResult(false, "", default, "", message);
    }
}
