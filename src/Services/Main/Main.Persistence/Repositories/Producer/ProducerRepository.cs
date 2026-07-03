using Dapper;
using EFCore.BulkExtensions;
using Main.Application.Interfaces.Persistence;
using Main.Entities.Producer;
using Main.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Repository;
using QueryExtensions = Persistence.Interfaces.IQueryableExtensions;

namespace Main.Persistence.Repositories.Producer;

public class ProducerRepository(DContext context, QueryExtensions extensions)
    : LinqRepositoryBase<DContext, Entities.Producer.Producer, int>(context, extensions), IProducerRepository
{
    public Task<bool> ProducerHasAnyArticle(int producerId, CancellationToken cancellationToken = default)
    {
        return Context.Products
            .AsNoTracking()
            .AnyAsync(x => x.ProducerId == producerId, cancellationToken);
    }
    public async Task AddSupplierMappingsOnConflictDoNothingAsync(
        IEnumerable<ProducerSupplierMapping> mappings, 
        CancellationToken cancellationToken = default)
    {
        var items = mappings.ToList();
        if (items.Count == 0) return;

        const string sql = """
                           INSERT INTO producer_supplier_mappings
                               (supplier, producer_id, producer_supplier_name)
                           VALUES
                               (@Supplier, @ProducerId, @SupplierProducerName)
                           ON CONFLICT (producer_id, supplier) DO NOTHING;
                           """;

        await using var connection = Context.Database.GetDbConnection();
        
        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                items.Select(x => new
                {
                    Supplier = x.Supplier.ToString(),
                    x.ProducerId,
                    x.SupplierProducerName
                }),
                cancellationToken: cancellationToken));
    }
}