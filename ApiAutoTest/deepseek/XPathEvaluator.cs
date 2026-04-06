// XPathEvaluator.cs
using System.Xml;
using System.Xml.XPath;
using System.IO;
using System.Xml.Linq;

namespace TestAutomationEngine.Core
{
    public static class XPathEvaluator
    {
        public static object? Evaluate(object? data, string xpath)
        {
            if (data == null || string.IsNullOrEmpty(xpath))
                return null;

            try
            {
                XPathDocument doc;
                if (data is string xmlString)
                {
                    using var sr = new StringReader(xmlString);
                    doc = new XPathDocument(sr);
                }
                else if (data is XElement elem)
                {
                    doc = new XPathDocument(elem.CreateReader());
                }
                else
                {
                    return null;
                }

                var nav = doc.CreateNavigator();
                var result = nav.Evaluate(xpath);
                return result;
            }
            catch
            {
                return null;
            }
        }
    }
}