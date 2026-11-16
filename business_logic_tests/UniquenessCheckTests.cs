using business_logic.DTOs;
using business_logic.Entities;
using business_logic.Exceptions;
using business_logic.Interfaces;
using business_logic_tests.Infrastructure;

namespace business_logic_tests
{
    public sealed class UniquenessCheckTests : IDisposable
    {
        private readonly BusinessLogicTestHost _testHost = new();

        [Fact]
        public async Task AutoUpdateAsync_KeepsOwnNumber_UpdatesAuto()
        {
            var auto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            await _testHost.SeedAsync(auto);

            await _testHost.RunInNewScopeAsync<IAutoService>(autoService => autoService.UpdateAsync(
                CreateUpdateAutoRequestModel(auto, auto.Number, updatedMark: "Scania")));

            var updatedAuto = await _testHost.RunInNewScopeAsync<IAutoService, GetAutoResponseModel>(autoService => autoService.GetAsync(auto.Id));
            Assert.Equal("Scania", updatedAuto.Mark);
        }

        [Fact]
        public async Task AutoUpdateAsync_TakesAnotherAutoNumber_ThrowsBusinessRuleException()
        {
            var updatedAuto = BusinessLogicTestHost.CreateAuto("АА1111АА");
            var otherAuto = BusinessLogicTestHost.CreateAuto("АА2222АА");
            await _testHost.SeedAsync(updatedAuto, otherAuto);

            var uniquenessException = await Assert.ThrowsAsync<BusinessRuleException>(() => _testHost.RunInNewScopeAsync<IAutoService>(
                autoService => autoService.UpdateAsync(CreateUpdateAutoRequestModel(updatedAuto, otherAuto.Number, updatedMark: "Volvo"))));

            Assert.Contains(otherAuto.Number, uniquenessException.Message);
        }

        [Fact]
        public async Task DriverUpdateAsync_KeepsOwnLicenseNumber_UpdatesDriver()
        {
            var driver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            await _testHost.SeedAsync(driver);

            await _testHost.RunInNewScopeAsync<IDriverService>(driverService => driverService.UpdateAsync(
                CreateUpdateDriverRequestModel(driver, driver.LicenseNumber, updatedSurname: "Франко")));

            var updatedDriver = await _testHost.RunInNewScopeAsync<IDriverService, GetDriverResponseModel>(driverService => driverService.GetAsync(driver.Id));
            Assert.Equal("Франко", updatedDriver.Surname);
        }

        [Fact]
        public async Task DriverUpdateAsync_TakesAnotherDriverLicenseNumber_ThrowsBusinessRuleException()
        {
            var updatedDriver = BusinessLogicTestHost.CreateDriver("ABC 111111");
            var otherDriver = BusinessLogicTestHost.CreateDriver("ABC 222222");
            await _testHost.SeedAsync(updatedDriver, otherDriver);

            var uniquenessException = await Assert.ThrowsAsync<BusinessRuleException>(() => _testHost.RunInNewScopeAsync<IDriverService>(
                driverService => driverService.UpdateAsync(CreateUpdateDriverRequestModel(updatedDriver, otherDriver.LicenseNumber, updatedSurname: "Шевченко"))));

            Assert.Contains(otherDriver.LicenseNumber, uniquenessException.Message);
        }

        public void Dispose()
        {
            _testHost.Dispose();
        }

        private static UpdateAutoRequestModel CreateUpdateAutoRequestModel(Auto updatedAuto, string number, string updatedMark)
        {
            return new UpdateAutoRequestModel
            {
                Id = updatedAuto.Id,
                RowVersion = updatedAuto.RowVersion,
                DepotId = updatedAuto.DepotId,
                Mark = updatedMark,
                Model = "FH16",
                Color = "Білий",
                Number = number,
                Capacity = 20
            };
        }

        private static UpdateDriverRequestModel CreateUpdateDriverRequestModel(Driver updatedDriver, string licenseNumber, string updatedSurname)
        {
            return new UpdateDriverRequestModel
            {
                Id = updatedDriver.Id,
                RowVersion = updatedDriver.RowVersion,
                DepotId = updatedDriver.DepotId,
                Name = "Тарас",
                Surname = updatedSurname,
                Patronymic = "Григорович",
                LicenseNumber = licenseNumber
            };
        }
    }
}
