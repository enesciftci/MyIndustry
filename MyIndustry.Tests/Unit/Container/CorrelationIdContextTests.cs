using FluentAssertions;
using MyIndustry.Container.Logging;

namespace MyIndustry.Tests.Unit.Container;

public class CorrelationIdContextTests
{
    [Fact]
    public void BeginScope_Should_Set_Current_Correlation_Id()
    {
        using (CorrelationIdContext.BeginScope("scope-id"))
        {
            CorrelationIdContext.Current.Should().Be("scope-id");
        }
    }

    [Fact]
    public void BeginScope_Should_Restore_Previous_Value_On_Dispose()
    {
        using (CorrelationIdContext.BeginScope("outer"))
        {
            CorrelationIdContext.Current.Should().Be("outer");

            using (CorrelationIdContext.BeginScope("inner"))
            {
                CorrelationIdContext.Current.Should().Be("inner");
            }

            CorrelationIdContext.Current.Should().Be("outer");
        }

        CorrelationIdContext.Current.Should().BeNull();
    }

    [Fact]
    public void Current_Should_Be_Null_By_Default()
    {
        CorrelationIdContext.Current.Should().BeNull();
    }
}
