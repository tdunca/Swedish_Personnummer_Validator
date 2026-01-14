using Xunit;
using PersonnummerKontroll;

namespace CI_CD_Group_5.Tests
{
    public class OgiltigaDatumTests
    {
        [Fact]
        public void Validate_InvalidDate_ReturnsFalse()
        {
            // Personnummer med ogiltiga datum
            string[] invalidDates = {
                "19990230-1234", // 30 februari
                "19991301-1234", // månad 13
                "20000230-1234"  // 30 februari skottår
            };

            foreach (var input in invalidDates)
            {
                var result = PersonnummerValidator.Validate(input);
                Assert.False(result.IsValid, $"Numret {input} borde vara ogiltigt.");
                Assert.Contains("datum", result.ErrorMessage.ToLower());
            }
        }
    }
}
