using System.Net;
using System.Text;
using Abstractions.Models.Options;
using Integrations.Client.Core;
using Integrations.Common;
using Internal.Integration.Core.Models.Common;
using SchemaGeneration.Abstractions.Enums;

namespace Infrastructure.Tests.Integrations;

public sealed class ClientBaseTests
{
    [Fact]
    public async Task ReadResponse_ShouldDeserializeWebContractWithStringEnums()
    {
        const string json = """
                            {
                              "jobs": [
                                {
                                  "systemName": "price-candidate-calculation",
                                  "name": "Price calculation",
                                  "description": "Calculates prices",
                                  "initStateSchema": {
                                    "version": 1,
                                    "fields": [
                                      {
                                        "name": "productId",
                                        "type": "Integer",
                                        "required": true,
                                        "control": "EntitySelector",
                                        "accepts": []
                                      }
                                    ],
                                    "csvSchema": null
                                  }
                                }
                              ]
                            }
                            """;
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };

        var client = new TestClient(new ProjectJsonOptions());
        var result = await client.Read<AvailableJobsResponse>(response);

        Assert.True(result.Success, result.Error);
        var job = Assert.Single(result.ValueOrThrow.Jobs);
        Assert.Equal("price-candidate-calculation", job.SystemName);
        var field = Assert.Single(job.InitStateSchema.Fields);
        Assert.Equal(SchemaValueType.Integer, field.Type);
        Assert.Equal(InputControlType.EntitySelector, field.Control);
    }

    private sealed record AvailableJobsResponse
    {
        public required IReadOnlyList<InternalJobInfo> Jobs { get; init; }
    }

    private sealed class TestClient(ProjectJsonOptions jsonOptions) : ClientBase(jsonOptions)
    {
        public Task<Response<T>> Read<T>(HttpResponseMessage response)
        {
            return ReadResponse<T>(response);
        }
    }
}
