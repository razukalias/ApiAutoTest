// ExtractionRule.cs (complete)
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace TestAutomationEngine.Core
{
    public class ExtractionRule: INotifyPropertyChanged
    {
        private string _source = string.Empty;
        private string _targetVariable = string.Empty;
        private ExtractionType _type = ExtractionType.JsonPath;
        private string _sourceType = string.Empty;

        public string Source
        {
            get => _source;
            set { _source = value; OnPropertyChanged(); }
        }

        public string TargetVariable
        {
            get => _targetVariable;
            set { _targetVariable = value; OnPropertyChanged(); }
        }

        public ExtractionType Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(); }
        }

        // NEW – tells where the extraction came from
        public string SourceType
        {
            get => _sourceType;
            set { _sourceType = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

     

       

        public object? Extract(object data)
        {
            return Type switch
            {
                ExtractionType.JsonPath => JsonPathEvaluator.Evaluate(data, Source),
                ExtractionType.XPath => XPathEvaluator.Evaluate(data, Source),
                ExtractionType.Regex => Regex.Match(data?.ToString() ?? "", Source).Value,
                _ => null
            };
        }
        

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum ExtractionType { JsonPath, XPath, Regex }
}