using System.Text.Json.Serialization;

namespace integra_dados.Services.Notifier;

public class Report : ICloneable
{
    [JsonConverter(typeof(UnixMillisecondsDateTimeConverter))]
    [JsonPropertyName("start")] public DateTime Start { get; } = DateTime.UtcNow;
    [JsonPropertyName("last")]
    [JsonConverter(typeof(UnixMillisecondsDateTimeConverter))]
    public DateTime Last { get; set; } = DateTime.UtcNow;
    [JsonPropertyName("status")]
    public Status Status { get; private set; } = Status.RUNNING;
    [JsonPropertyName("exceptionInfo")]
    public ExceptionInfo ExceptionInfo { get; private set; } = new();

    public string ApplicationName { get; private set; } = "microservice_integra_dados";
    public ResetInfo ResetInfo;

    public object Clone()
    {
        return this.MemberwiseClone(); // shallow copy
    }

    /// Registra uma exceção crítica
    public void CriticalException(Status status)
    {
        if (this.Status < status)
            this.Status = status;

        Last = DateTime.UtcNow;
        ExceptionInfo.IncrementCritical();
    }

    /// Registra uma exceção moderada
    public void ModerateException(Status status)
    {
        if (this.Status < status)
            this.Status = status;

        Last = DateTime.UtcNow;
        ExceptionInfo.IncrementModerate();
    }

    /// Registra uma exceção leve
    /// <param name="running"></param>
    public void LightException(Status running)
    {
        Last = DateTime.UtcNow;
        ExceptionInfo.IncrementLight();
    }

    public void Reset()
    {
        Last = DateTime.UtcNow;
        ExceptionInfo = new ExceptionInfo();
    }

    public Status GetStatus()
    {
        return Status;
    }
}