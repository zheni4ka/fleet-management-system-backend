using business_logic.DTOs;
using business_logic.Exceptions;
using business_logic.Interfaces;
using business_logic_tests.Infrastructure;

namespace business_logic_tests
{
    public sealed class OptimisticConcurrencyTests : IDisposable
    {
        private readonly BusinessLogicTestHost _testHost = new();

        [Fact]
        public async Task AutoUpdateAsync_StaleRowVersion_ThrowsConcurrencyConflictWithAutoId()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            await _testHost.SeedAsync(auto);
            var staleRowVersion = auto.RowVersion.Select(rowVersionByte => (byte)~rowVersionByte).ToArray();

            var concurrencyConflictException = await Assert.ThrowsAsync<ConcurrencyConflictException>(() => _testHost.RunInNewScopeAsync<IAutoService>(
                autoService => autoService.UpdateAsync(new UpdateAutoRequestModel
                {
                    Id = auto.Id,
                    RowVersion = staleRowVersion,
                    Mark = "Scania",
                    Model = auto.Model,
                    Color = auto.Color,
                    Number = auto.Number,
                    Capacity = auto.Capacity
                })));

            Assert.Contains($"'{auto.Id}'", concurrencyConflictException.Message);
            var persistedAuto = await _testHost.RunInNewScopeAsync<IAutoService, GetAutoResponseModel>(autoService => autoService.GetAsync(auto.Id));
            Assert.Equal(auto.Mark, persistedAuto.Mark);
        }

        public void Dispose()
        {
            _testHost.Dispose();
        }
    }
}
