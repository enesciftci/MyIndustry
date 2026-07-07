using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace MyIndustry.Container.Logging;

public static class SensitiveDataMasker
{
    private const string RedactedValue = "***";
    private const int MaxDepth = 2;
    private const int MaxProperties = 50;

    private static readonly HashSet<string> RedactPropertyNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "password", "pwd", "secret", "token", "refreshToken", "accessToken",
        "apiKey", "authorization", "creditCard", "cvv", "ssn", "pin", "otp",
        "verificationCode", "otpCode", "resetCode"
    };

    private static readonly HashSet<string> PartialEmailPropertyNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "email", "senderEmail", "userEmail", "mail"
    };

    private static readonly HashSet<string> PartialPhonePropertyNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "phone", "mobile", "gsm", "phoneNumber", "mobileNumber"
    };

    private static readonly HashSet<string> TruncatePropertyNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "content", "description", "body", "message", "html", "notes", "userMessage"
    };

    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new();

    public static Dictionary<string, object?> Sanitize(object? value, MediatRLoggingOptions options) =>
        SanitizeObject(value, options, depth: 0, propertyCount: 0).Properties;

    private static (Dictionary<string, object?> Properties, int PropertyCount) SanitizeObject(
        object? value,
        MediatRLoggingOptions options,
        int depth,
        int propertyCount)
    {
        if (value is null)
            return (new Dictionary<string, object?>(), propertyCount);

        if (depth > MaxDepth)
            return (new Dictionary<string, object?> { ["_truncated"] = "[MaxDepthReached]" }, propertyCount);

        var result = new Dictionary<string, object?>();
        var type = value.GetType();

        if (IsSimpleType(type))
        {
            result["_value"] = MaskStringValue(type.Name, null, value.ToString(), options, null);
            return (result, propertyCount + 1);
        }

        if (value is IFormFile file)
        {
            result["fileName"] = file.FileName;
            result["length"] = file.Length;
            result["contentType"] = file.ContentType;
            return (result, propertyCount + 1);
        }

        if (value is IEnumerable<IFormFile> files)
        {
            result["files"] = files.Select(f => new Dictionary<string, object?>
            {
                ["fileName"] = f.FileName,
                ["length"] = f.Length,
                ["contentType"] = f.ContentType
            }).ToList();
            return (result, propertyCount + 1);
        }

        if (value is byte[] bytes)
        {
            result["_value"] = $"[Binary:{bytes.Length} bytes]";
            return (result, propertyCount + 1);
        }

        if (value is Stream)
        {
            result["_value"] = "[Stream]";
            return (result, propertyCount + 1);
        }

        if (value is IEnumerable enumerable and not string)
        {
            var items = new List<object?>();
            foreach (var item in enumerable)
            {
                if (propertyCount >= MaxProperties)
                {
                    items.Add("[MaxPropertiesReached]");
                    break;
                }

                if (item is null)
                {
                    items.Add(null);
                    continue;
                }

                if (IsSimpleType(item.GetType()))
                {
                    items.Add(item);
                    propertyCount++;
                    continue;
                }

                var nested = SanitizeObject(item, options, depth + 1, propertyCount);
                items.Add(nested.Properties);
                propertyCount = nested.PropertyCount;
            }

            result["_items"] = items;
            return (result, propertyCount);
        }

        var properties = PropertyCache.GetOrAdd(type, t =>
            t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
                .ToArray());

        foreach (var property in properties)
        {
            if (propertyCount >= MaxProperties)
            {
                result["_truncated"] = "[MaxPropertiesReached]";
                break;
            }

            object? propertyValue;
            try
            {
                propertyValue = property.GetValue(value);
            }
            catch
            {
                result[property.Name] = "[Unreadable]";
                propertyCount++;
                continue;
            }

            var sensitiveAttr = property.GetCustomAttribute<LogSensitiveAttribute>();
            if (sensitiveAttr?.Mode == LogMaskMode.Ignore)
                continue;

            result[property.Name] = SanitizePropertyValue(
                property.Name,
                propertyValue,
                property.PropertyType,
                options,
                sensitiveAttr,
                depth,
                ref propertyCount);
        }

        return (result, propertyCount);
    }

    private static object? SanitizePropertyValue(
        string propertyName,
        object? value,
        Type propertyType,
        MediatRLoggingOptions options,
        LogSensitiveAttribute? sensitiveAttr,
        int depth,
        ref int propertyCount)
    {
        propertyCount++;

        if (value is null)
            return null;

        if (sensitiveAttr is not null)
            return ApplyMaskMode(sensitiveAttr.Mode, propertyName, value, options, sensitiveAttr.MaxLength);

        if (RedactPropertyNames.Contains(propertyName))
            return RedactedValue;

        if (options.MaskEmails && PartialEmailPropertyNames.Contains(propertyName) && value is string email)
            return MaskEmail(email);

        if (options.MaskPhones && PartialPhonePropertyNames.Contains(propertyName) && value is string phone)
            return MaskPhone(phone);

        if (TruncatePropertyNames.Contains(propertyName) && value is string text)
            return TruncateString(text, options.MaxStringLength);

        if (value is IFormFile file)
            return new Dictionary<string, object?> { ["fileName"] = file.FileName, ["length"] = file.Length, ["contentType"] = file.ContentType };

        if (value is byte[] bytes)
            return $"[Binary:{bytes.Length} bytes]";

        if (value is Stream)
            return "[Stream]";

        if (IsSimpleType(propertyType))
            return MaskStringValue(propertyName, sensitiveAttr, value.ToString(), options, null);

        var nested = SanitizeObject(value, options, depth + 1, propertyCount);
        propertyCount = nested.PropertyCount;
        return nested.Properties;
    }

    private static object? ApplyMaskMode(
        LogMaskMode mode,
        string propertyName,
        object value,
        MediatRLoggingOptions options,
        int maxLength)
    {
        return mode switch
        {
            LogMaskMode.Redact => RedactedValue,
            LogMaskMode.Partial when value is string s && PartialEmailPropertyNames.Contains(propertyName) => MaskEmail(s),
            LogMaskMode.Partial when value is string s && PartialPhonePropertyNames.Contains(propertyName) => MaskPhone(s),
            LogMaskMode.Partial when value is string s => MaskGenericPartial(s),
            LogMaskMode.Truncate when value is string s => TruncateString(s, maxLength > 0 ? maxLength : options.MaxStringLength),
            LogMaskMode.Ignore => null,
            _ => value.ToString()
        };
    }

    private static object? MaskStringValue(
        string propertyName,
        LogSensitiveAttribute? sensitiveAttr,
        string? value,
        MediatRLoggingOptions options,
        int? maxLength)
    {
        if (value is null)
            return null;

        if (sensitiveAttr is not null)
            return ApplyMaskMode(sensitiveAttr.Mode, propertyName, value, options, sensitiveAttr.MaxLength);

        if (TruncatePropertyNames.Contains(propertyName))
            return TruncateString(value, options.MaxStringLength);

        return value;
    }

    private static string MaskEmail(string email)
    {
        var atIndex = email.IndexOf('@');
        if (atIndex <= 0)
            return RedactedValue;

        var local = email[..atIndex];
        var domain = email[atIndex..];
        var visible = local.Length > 0 ? local[0].ToString() : "";
        return $"{visible}***{domain}";
    }

    private static string MaskPhone(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.Length < 4)
            return RedactedValue;

        return $"{digits[..Math.Min(3, digits.Length)]}***{digits[^4..]}";
    }

    private static string MaskGenericPartial(string value) =>
        value.Length <= 2 ? RedactedValue : $"{value[0]}***{value[^1]}";

    private static string TruncateString(string value, int maxLength)
    {
        if (maxLength <= 0 || value.Length <= maxLength)
            return value;

        return value[..maxLength] + "...";
    }

    private static bool IsSimpleType(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        return underlying.IsPrimitive
               || underlying.IsEnum
               || underlying == typeof(string)
               || underlying == typeof(decimal)
               || underlying == typeof(DateTime)
               || underlying == typeof(DateTimeOffset)
               || underlying == typeof(TimeSpan)
               || underlying == typeof(Guid);
    }
}
