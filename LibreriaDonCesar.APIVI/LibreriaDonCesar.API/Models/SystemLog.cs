using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LibreriaDonCesar.API.Models;

[BsonIgnoreExtraElements]
public class SystemLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("entryId")]
    public string EntryId { get; set; } = Guid.NewGuid().ToString();

    [BsonElement("entity")]
    public string Entity { get; set; } = string.Empty;

    [BsonElement("requestType")]
    public string RequestType { get; set; } = string.Empty;

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("status")]
    public string Status { get; set; } = "Pendiente";

    [BsonElement("traceId")]
    public string TraceId { get; set; } = string.Empty;

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [BsonElement("level")]
    public string Level { get; set; } = "Information";

    [BsonElement("environment")]
    public string Environment { get; set; } = "Development";

    [BsonElement("clientIp")]
    public string? ClientIp { get; set; }

    [BsonElement("userId")]
    public string? UserId { get; set; }

    [BsonElement("userName")]
    public string UserName { get; set; } = "Anonimo";

    [BsonElement("performedBy")]
    public string PerformedBy { get; set; } = "Anonimo";

    [BsonElement("request")]
    public RequestDetail Request { get; set; } = new();

    [BsonElement("response")]
    public ResponseDetail Response { get; set; } = new();

    [BsonElement("exception")]
    [BsonIgnoreIfNull]
    public ExceptionDetail? Exception { get; set; }
}

[BsonIgnoreExtraElements]
public class RequestDetail
{
    [BsonElement("method")]
    public string Method { get; set; } = string.Empty;

    [BsonElement("Method")]
    [BsonIgnoreIfNull]
    [JsonIgnore]
    public string? LegacyMethod
    {
        get => null;
        set
        {
            if (string.IsNullOrEmpty(Method))
            {
                Method = value ?? string.Empty;
            }
        }
    }

    [BsonElement("path")]
    public string Path { get; set; } = string.Empty;

    [BsonElement("Path")]
    [BsonIgnoreIfNull]
    [JsonIgnore]
    public string? LegacyPath
    {
        get => null;
        set
        {
            if (string.IsNullOrEmpty(Path))
            {
                Path = value ?? string.Empty;
            }
        }
    }

    [BsonElement("queryString")]
    public string? QueryString { get; set; }

    [BsonElement("QueryString")]
    [BsonIgnoreIfNull]
    [JsonIgnore]
    public string? LegacyQueryString
    {
        get => null;
        set
        {
            if (string.IsNullOrEmpty(QueryString))
            {
                QueryString = value;
            }
        }
    }

    [BsonElement("headers")]
    public Dictionary<string, string> Headers { get; set; } = new();

    [BsonElement("payload")]
    public string? Payload { get; set; }

    [BsonElement("Payload")]
    [BsonIgnoreIfNull]
    [JsonIgnore]
    public string? LegacyPayload
    {
        get => null;
        set
        {
            if (string.IsNullOrEmpty(Payload))
            {
                Payload = value;
            }
        }
    }
}

[BsonIgnoreExtraElements]
public class ResponseDetail
{
    [BsonElement("statusCode")]
    public int StatusCode { get; set; }

    [BsonElement("StatusCode")]
    [BsonIgnoreIfNull]
    [JsonIgnore]
    public int? LegacyStatusCode
    {
        get => null;
        set
        {
            if (value.HasValue)
            {
                StatusCode = value.Value;
            }
        }
    }

    [BsonElement("elapsedMilliseconds")]
    public long ElapsedMilliseconds { get; set; }

    [BsonElement("ElapsedMilliseconds")]
    [BsonIgnoreIfNull]
    [JsonIgnore]
    public long? LegacyElapsedMilliseconds
    {
        get => null;
        set
        {
            if (value.HasValue)
            {
                ElapsedMilliseconds = value.Value;
            }
        }
    }
}

[BsonIgnoreExtraElements]
public class ExceptionDetail
{
    [BsonElement("message")]
    public string Message { get; set; } = string.Empty;

    [BsonElement("Message")]
    [BsonIgnoreIfNull]
    [JsonIgnore]
    public string? LegacyMessage
    {
        get => null;
        set
        {
            if (string.IsNullOrEmpty(Message))
            {
                Message = value ?? string.Empty;
            }
        }
    }

    [BsonElement("stackTrace")]
    public string? StackTrace { get; set; }

    [BsonElement("StackTrace")]
    [BsonIgnoreIfNull]
    [JsonIgnore]
    public string? LegacyStackTrace
    {
        get => null;
        set
        {
            if (string.IsNullOrEmpty(StackTrace))
            {
                StackTrace = value;
            }
        }
    }

    [BsonElement("source")]
    public string Source { get; set; } = string.Empty;

    [BsonElement("Source")]
    [BsonIgnoreIfNull]
    [JsonIgnore]
    public string? LegacySource
    {
        get => null;
        set
        {
            if (string.IsNullOrEmpty(Source))
            {
                Source = value ?? string.Empty;
            }
        }
    }
}
