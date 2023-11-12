using System.Globalization;
using System.Reflection;
using System.Text;

namespace AutoBogus.Templating;

internal sealed class Templator<T>
    where T : class
{
    private readonly AutoFaker<T> _autoFaker;

    private string? _propertyNameSpaceDelimiter;
    private bool    _isConfigured;
    private bool    _treatMissingAsNull = true; // if false will treat missing as empty

    private List<Func<Type, string, (bool Handled, object? Result)>> _typeConverters = new();

    public Templator(AutoFaker<T> autoFaker)
    {
        _autoFaker = autoFaker;
    }

    public static TCast Cast<TCast>(object o)
    {
        return (TCast)o;
    }

    public static object? GetDefault(Type type)
    {
        if (type.IsValueType)
        {
            return Activator.CreateInstance(type);
        }

        return null;
    }

    /// <summary>
    ///     Creates a set of test data with number of rows matching that in the dataAsString data set
    ///     With fake values used unless the field is in the template.
    /// </summary>
    /// <param name="template">
    ///     Should be of form
    ///     var testData  =
    ///     "EmployeeId | DateOfBirth     \r\n" +
    ///     "1          | 2000-01-01      \r\n".
    /// </param>
    /// <returns>The generated data.</returns>
    public List<T> GenerateFromTemplate(string template)
    {
        ConfigureTemplator();
        var allRows = ParseTemplate(template);

        // first generate some data
        var generatedData = _autoFaker.Generate(allRows.Count).ToList();

        // now go through and set values as they are in the test data
        for (var i = 0; i < allRows.Count; i++)
        {
            var transaction = generatedData[i];
            var testRow     = allRows[i];

            var properties = transaction.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .ToDictionary(k => k.Name);

            foreach (var fieldName in testRow.Keys)
            {
                var fieldNameTrim      = fieldName.Trim();
                var fieldNameWithDelim = fieldNameTrim;
                if (_propertyNameSpaceDelimiter != null)
                {
                    fieldNameWithDelim = fieldNameTrim
                        .Replace(" ", _propertyNameSpaceDelimiter, StringComparison.CurrentCulture);
                }

                var prop = transaction.GetType()
                    .GetProperty(fieldNameWithDelim, BindingFlags.Public | BindingFlags.Instance);
                if (prop != null && prop.CanWrite)
                {
                    var value = testRow[fieldNameTrim].Trim();
                    // now need to parse value as correct type
                    var concretePropertyType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                    properties.Remove(prop.Name);

                    SetPropertyValue(value, prop, transaction, concretePropertyType);
                }
            }
        }

        return generatedData;
    }

    /// <summary>
    ///     Creates a set of test data with number of rows matching testItems but only using
    ///     the headers in the headers list in the template.
    ///     Properties not in the headers list will use AutoFaker rules.
    /// </summary>
    /// <param name="headers">
    ///     List of headers matching testItems. Defines what properties in testItems we will set the value for.
    /// </param>
    /// <param name="testItems">
    ///     List of items, which properties with names in <paramref name="headers"/> are selected,
    ///     and its values are then used for test data generation as template row values.
    /// </param>
    /// <returns>The generated data.</returns>
    public List<T> GenerateFromTemplate(List<string> headers, List<T> testItems)
    {
        // note: Not the most efficient as it gets converted to a template string and then that gets parsed.
        var headersAsString = string.Join("|", headers);
        var stringBuilder   = new StringBuilder();
        stringBuilder.AppendLine(headersAsString);

        var fieldNames = headersAsString.Split('|');
        foreach (var testItem in testItems)
        {
            var rowBuilder = new StringBuilder();
            for (var i = 0; i < fieldNames.Length; i++)
            {
                var testItemValue = ReflectionHelper.GetPropValue(testItem, fieldNames[i].Trim());
                if (i < fieldNames.Length - 1)
                {
                    rowBuilder.Append(CultureInfo.CurrentCulture, $"{testItemValue}|");
                }
                else
                {
                    rowBuilder.Append(testItemValue);
                }
            }

            // remove trailing |
            var row = rowBuilder.ToString();
            stringBuilder.AppendLine(row);
        }

        var testData = stringBuilder.ToString();
        return GenerateFromTemplate(testData);
    }

    private static List<Dictionary<string, string>> ParseTemplate(string testData)
    {
        // split on | and new line
        using var stringReader = new StringReader(testData.TrimStart());

        var headers = stringReader.ReadLine()!.Split('|');

        var allRows = new List<Dictionary<string, string>>();

        string? line;
        do
        {
            line = stringReader.ReadLine();

            if (line == null)
            {
                continue;
            }

            var thisRowDict = new Dictionary<string, string>();
            var thisLine    = line.Split('|');

            // ignore lines of wrong length
            if (headers.Length != thisLine.Length)
            {
                continue;
            }

            for (var index = 0; index < headers.Length; index++)
            {
                var value = thisLine[index]!.Trim();
                thisRowDict[headers[index].Trim()] = value;
            }

            allRows.Add(thisRowDict);
        }
        while (line != null);

        return allRows;
    }

    /// <summary>
    ///     Configure the templator from the binder.
    /// </summary>
    private void ConfigureTemplator()
    {
        if (_isConfigured)
        {
            return;
        }

        if (_autoFaker.Binder is TemplateBinder templateBinder)
        {
            _typeConverters             = templateBinder.TypeConverters;
            _treatMissingAsNull         = templateBinder.TreatMissingAsNull;
            _propertyNameSpaceDelimiter = templateBinder.PropertyNameSpaceDelimiter;
        }

        _isConfigured = true;
    }

    private void SetPropertyValue(string value, PropertyInfo prop, T transaction, Type concretePropertyType)
    {
        // set nullable to null if string null
        if (string.IsNullOrEmpty(value) && prop.PropertyType.IsGenericType &&
            prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            prop.SetValue(transaction, null, null);
        }
        else if (concretePropertyType == typeof(decimal))
        {
            prop.SetValue(transaction, decimal.Parse(value, CultureInfo.CurrentCulture), null);
        }
        else if (concretePropertyType == typeof(long))
        {
            prop.SetValue(transaction, long.Parse(value, CultureInfo.CurrentCulture), null);
        }
        else if (concretePropertyType == typeof(DateTime))
        {
            prop.SetValue(transaction, DateTime.Parse(value, CultureInfo.CurrentCulture), null);
        }
        else if (concretePropertyType == typeof(bool))
        {
            prop.SetValue(transaction, bool.Parse(value), null);
        }
        else if (concretePropertyType == typeof(int))
        {
            prop.SetValue(transaction, int.Parse(value, CultureInfo.CurrentCulture), null);
        }
        else if (concretePropertyType.IsEnum)
        {
            var v = string.IsNullOrEmpty(value) ? 0 : Enum.Parse(concretePropertyType, value);
            prop.SetValue(transaction, v, null);
        }
        else
        {
            // see if we have custom converts specified for this type
            var handled = false;
            if (_typeConverters.Any())
            {
                foreach (var typeConvert in _typeConverters)
                {
                    var result = typeConvert(concretePropertyType, value);
                    if (result.Handled)
                    {
                        prop.SetValue(transaction, result.Result, null);
                        handled = true;
                    }
                }
            }

            // pretty much has to be a string if not then we may have a complex type with no converter
            if (handled)
            {
                return;
            }

            switch (value)
            {
                case TemplateValues.NullToken:
                    prop.SetValue(transaction, null, null);
                    break;
                case TemplateValues.EmptyToken:
                    prop.SetValue(transaction, string.Empty, null);
                    break;
                default:
                {
                    if (_treatMissingAsNull && string.IsNullOrEmpty(value))
                    {
                        prop.SetValue(transaction, null, null);
                    }
                    else
                    {
                        prop.SetValue(transaction, value, null);
                    }

                    break;
                }
            }
        }
    }
}
