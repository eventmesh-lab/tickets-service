using System.Threading.Tasks;
using Xunit;
using Moq;
using tickets_service.Domain.Ports;
using tickets_service.Application.UseCases;
using tickets_service.Domain.Entities;
using System.Threading;

namespace tickets_service.Application.Tests
{
    public class CreateExampleUseCaseTests
    {
        [Fact]
        public async Task Handle_WithValidRequest_CallsRepoAndReturnsId()
        {
            var repoMock = new Mock<IExampleRepository>();
            repoMock.Setup(r => r.AddAsync(It.IsAny<ExampleAggregate>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var usecase = new CreateExampleUseCase(repoMock.Object);
            var req = new CreateExampleRequest { Name = "abc" };

            var res = await usecase.Handle(req);

            Assert.NotEqual(System.Guid.Empty, res.Id);
            repoMock.Verify(r => r.AddAsync(It.IsAny<ExampleAggregate>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
