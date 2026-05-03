using System.Text.Json;
using FluentAssertions;
using Main.Entities.Event;
using Main.Entities.Storage;
using Main.Enums;

namespace Main.Tests.Domain.Events;

public class StorageMovementEventTests
{
    [Fact]
    public void Create_FromData_SerializesCorrectly()
    {
        var data = new StorageMovementEventData
        {
            ProductId = 1,
            StorageName = "Main",
            CurrencyId = 10,
            Count = 5,
            BuyPrice = 100m,
            MovementType = StorageMovementType.Purchase
        };

        var ev = StorageMovementEvent.Create(data);

        ev.Json.Should().NotBeNullOrWhiteSpace();

        var deserialized = JsonSerializer.Deserialize<StorageMovementEventData>(ev.Json);

        deserialized.Should().BeEquivalentTo(data);
    }

    [Fact]
    public void Create_FromStorageContent_MapsCorrectly()
    {
        var content = StorageContent.Create(
            "Main", 
            1, 
            5, 
            100m, 
            10, 
            DateTime.UtcNow);

        var ev = StorageMovementEvent.Create(content, StorageMovementType.Purchase);

        var data = JsonSerializer.Deserialize<StorageMovementEventData>(ev.Json);

        data.Should().NotBeNull();
        
        data.ProductId.Should().Be(content.ProductId);
        data.StorageName.Should().Be(content.StorageName);
        data.CurrencyId.Should().Be(content.CurrencyId);
        data.Count.Should().Be(content.Count);
        data.BuyPrice.Should().Be(content.BuyPrice);
        data.MovementType.Should().Be(StorageMovementType.Purchase);
    }

    [Fact]
    public void Data_LazyDeserialization_Works()
    {
        var data = new StorageMovementEventData
        {
            ProductId = 1,
            StorageName = "Main",
            CurrencyId = 10,
            Count = 5,
            BuyPrice = 100m,
            MovementType = StorageMovementType.Purchase
        };

        var ev = StorageMovementEvent.Create(data);

        var extracted = ev.Data;

        extracted.Should().BeEquivalentTo(data);
    }
}