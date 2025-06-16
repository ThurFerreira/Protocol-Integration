using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace integra_dados.Models;

public class Registry
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [JsonIgnore]
    public string? _id { get; set; }
    public string? Id { get; set; }
    [JsonIgnore]
    public string? Uri { get; set; }
    public int Status { get; set; }
    [JsonIgnore]
    public string? IdSistema { get; set; }
    public string? Nome { get; set; }
    public string? Fonte { get; set; }
    public string? TipoDado { get; set; }
    public string? DescricaoPontoGeografico { get; set; }
    public string? Observacao { get; set; }
    public DateTime UltimaAtualizacao { get; set; }
    public string? TopicoBroker { get; set; }
    public bool OtimizarPublicacaoBroker { get; set; }

    [System.Text.Json.Serialization.JsonIgnore] // Ignora ao serializar para JSON, equivalente ao @Transient
    public long LastRead { get; set; } = 0;

    [System.Text.Json.Serialization.JsonIgnore]
    public object? LastRegisteredValue { get; set; }

    public void SetIdSistema()
    {
        IdSistema = Util.Util.GenerateRandomNumber().ToString();
        Id = IdSistema;
    }

    public bool IsTimeToSendMessage(int freqLeitura)
    {
        long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        if (LastRead == 0 || now >= LastRead + (freqLeitura * 1000))
        {
            LastRead = now;
            return true;
        }

        return false;
    }

    public void UpdateRegistry(int onOffSwitchValue)
    {
        const int VALUE_NOT_VALID = -1;
        UpdateDate();

        if (onOffSwitchValue == VALUE_NOT_VALID)
        {
            UpgradeStatusToUnavailable();
        }
        else
        {
            UpgradeStatusToAvailable();
        }
    }

    private void UpdateDate()
    {
        UltimaAtualizacao = DateTime.Now;
    }

    public void UpgradeStatusToAvailable()
    {
        Status = (int)StatusVariable.AVAILABLE;
    }

    public void UpgradeStatusToUnavailable()
    {
        Status = (int)StatusVariable.UNAVAILABLE;
    }

    public bool ShouldSendToBroker(Object actualRegisteredValue)
    {
        if (OtimizarPublicacaoBroker)
        {
            if (actualRegisteredValue.Equals(LastRegisteredValue))
            {
                return false;
            }

            LastRegisteredValue = actualRegisteredValue;
        }

        return true;
    }
}