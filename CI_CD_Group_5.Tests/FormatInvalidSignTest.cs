using Xunit;

namespace PersonnummerKontroll
{
    public class PersonnummerValidatorInvalidInputTests
    {
        [Theory]
        [InlineData("12345")]
        [InlineData("ABCDEFGHIJKL")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("!@#$%^&*()")]
        public void Validate_FelaktigtFormatEllerOgiltigaTecken_ReturnerarInvalid_UtanException(string input)
        {
            var exception = Record.Exception(() => PersonnummerValidator.Validate(input));

            Assert.Null(exception);

            var result = PersonnummerValidator.Validate(input);

            Assert.False(result.IsValid);
            Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
        }
    }
}

