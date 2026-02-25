using FluentAssertions;
using MyIndustry.ApplicationService.Handler.Contract.CreateContractCommand;

namespace MyIndustry.Tests.Unit.Contract;

public class CreateContractCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Throw_NotImplementedException()
    {
        var handler = new CreateContractCommandHandler();

        var act = () => handler.Handle(new CreateContractCommand(), CancellationToken.None);

        await act.Should().ThrowAsync<NotImplementedException>();
    }
}
