using System.Text.Json.Serialization;
using Abstractions.Interfaces.Persistence;
using Application.Common.Interfaces.Repositories;
using Application.Common.LRT;
using Application.Common.NamedObject;
using Domain.CommonEntities;

namespace Main.Application.Lrts;

public class ProducerImportLrt(IRepository<Job, Guid> jobRepository, IUnitOfWork unitOfWork)
    : LrtNamedObjectBase(jobRepository, unitOfWork)
{
    public override string SystemName => nameof(ProducerImportLrt);
    public override string NameLocalizationKey => "lrt.producer.import.name";
    public override string DescriptionLocalizationKey => "lrt.producer.import.description";
    
    protected override Task DoWork()
    {
        throw new NotImplementedException();
    }

    private record CurrentState
    {
        [JsonPropertyName("fileName")]
        public required string FileName { get; init; }
        
        [JsonPropertyName("totalLines")]
        public required int TotalLines { get; init; }
        
        [JsonPropertyName("currentLine")]
        public required int CurrentLine { get; init; }
        
        [JsonPropertyName("errors")]
        public required List<InsertError> Errors { get; init; }
    }

    private record InsertError
    {
        [JsonPropertyName("rowIdx")]
        public int RowIdx { get; init; }
        
        [JsonPropertyName("message")]
        public required string Message { get; init; }
    }
}