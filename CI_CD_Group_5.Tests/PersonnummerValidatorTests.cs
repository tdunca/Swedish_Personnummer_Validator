using PersonnummerKontroll;

namespace CI_CD_Group_5.Tests
{
    public class PersonnummerValidatorTests
    {
        [Fact]
        public void Validate_InvalidCheckDigit_ReturnsFalse()
        {
            // Arrange - Använder ett personnummer där allt är korrekt utom kontrollsiffran
            // Giltigt personnummer: 19811218-9876
            // Ogiltigt personnummer: 19811218-9870 (ändrad sista siffra)
            var invalidPersonnummer = "19811218-9870";

            // Act
            var result = PersonnummerValidator.Validate(invalidPersonnummer);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("kontrollsiffra", result.ErrorMessage.ToLower());
            Assert.Contains("luhn", result.ErrorMessage.ToLower());
        }

        [Fact]
        public void Validate_ValidCheckDigit_ReturnsTrue()
        {
            // Arrange - Använder ett giltigt personnummer för jämförelse
            var validPersonnummer = "19811218-9876";

            // Act
            var result = PersonnummerValidator.Validate(validPersonnummer);

            // Assert
            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_InvalidCheckDigit_DifferentFormats_ReturnsFalse()
        {
            // Arrange - Testar olika format med ogiltig kontrollsiffra
            var invalidPersonnummer1 = "811218-9870";  // Kort format
            var invalidPersonnummer2 = "198112189870"; // Utan separator
            var invalidPersonnummer3 = "8112189870";   // Kort format utan separator

            // Act
            var result1 = PersonnummerValidator.Validate(invalidPersonnummer1);
            var result2 = PersonnummerValidator.Validate(invalidPersonnummer2);
            var result3 = PersonnummerValidator.Validate(invalidPersonnummer3);

            // Assert
            Assert.False(result1.IsValid);
            Assert.False(result2.IsValid);
            Assert.False(result3.IsValid);
        }

        [Theory]
        [InlineData("19811218-9870")] // Ändrad sista siffran från 6 till 0
        [InlineData("19811218-9871")] // Ändrad sista siffran från 6 till 1
        [InlineData("19811218-9872")] // Ändrad sista siffran från 6 till 2
        [InlineData("19811218-9873")] // Ändrad sista siffran från 6 till 3
        [InlineData("19811218-9874")] // Ändrad sista siffran från 6 till 4
        [InlineData("19811218-9875")] // Ändrad sista siffran från 6 till 5
        [InlineData("19811218-9877")] // Ändrad sista siffran från 6 till 7
        [InlineData("19811218-9878")] // Ändrad sista siffran från 6 till 8
        [InlineData("19811218-9879")] // Ändrad sista siffran från 6 till 9
        public void Validate_InvalidCheckDigit_MultipleVariations_ReturnsFalse(string invalidPersonnummer)
        {
            // Act
            var result = PersonnummerValidator.Validate(invalidPersonnummer);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("kontrollsiffra", result.ErrorMessage.ToLower());
        }

        [Theory]
        [InlineData("19900101-1231")] // Ogiltig kontrollsiffra
        [InlineData("20000229-1112")] // Ogiltig kontrollsiffra
        [InlineData("19851015-5555")] // Ogiltig kontrollsiffra
        [InlineData("19750512-9998")] // Ogiltig kontrollsiffra
        public void Validate_InvalidCheckDigit_DifferentDates_ReturnsFalse(string invalidPersonnummer)
        {
            // Act
            var result = PersonnummerValidator.Validate(invalidPersonnummer);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains("kontrollsiffra", result.ErrorMessage.ToLower());
        }

        [Fact]
        public void Validate_ValidCheckDigit_OriginalNumber_ReturnsTrue()
        {
            // Arrange - Det korrekta personnumret som vi ändrat i andra tester
            var validPersonnummer = "19811218-9876";

            // Act
            var result = PersonnummerValidator.Validate(validPersonnummer);

            // Assert
            Assert.True(result.IsValid);
            Assert.Empty(result.ErrorMessage);
        }
    }
}
