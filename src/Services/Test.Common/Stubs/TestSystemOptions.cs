using Abstractions.Models.Options;
using Microsoft.Extensions.Options;

namespace Test.Common.Stubs;

public class TestSystemOptionsAccessor
{
    public Guid SystemId { get; set; }
}

public class TestSystemOptions(TestSystemOptionsAccessor accessor) : IOptions<SystemOptions>
{
    public SystemOptions Value
    {
        get
        {
            if (accessor.SystemId == Guid.Empty)
                throw new InvalidOperationException("Test system user has not been initialized.");

            return new SystemOptions
            {
                SystemId = accessor.SystemId
            };
        }
    }
}
