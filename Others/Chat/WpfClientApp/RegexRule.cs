using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace WpfClientApp
{
    class RegexRule : ValidationRule
    {
        public string Pattern { get; set; }
        public string ErrorMessage { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            var result = Regex.Match((string)value, Pattern);
            if (result.Success)
                return new ValidationResult(true, null);
            else
                return new ValidationResult(false, ErrorMessage);
        }
    }
}
