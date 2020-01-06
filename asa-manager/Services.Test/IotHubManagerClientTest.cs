
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Mmm.Platform.IoT.AsaManager.Services;
using Mmm.Platform.IoT.AsaManager.Services.Exceptions;
using Mmm.Platform.IoT.AsaManager.Services.External.IotHubManager;
using Mmm.Platform.IoT.AsaManager.Services.Models.DeviceGroups;
using Mmm.Platform.IoT.AsaManager.Services.Test.Helpers;
using Mmm.Platform.IoT.Common.Services.Helpers;
using Mmm.Platform.IoT.Common.TestHelpers;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Mmm.Platform.IoT.AsaManager.Services.Test
{
    public class IothubmanagerclientTest
    {
        private const string MOCK_API_URL = "http://iothub:80/v1";

        private Mock<IIotHubManagerClientConfig> mockConfig;
        private Mock<IExternalRequestHelper> mockRequestHelper;
        private IIotHubManagerClient client;
        private Random rand;
        private CreateEntityHelper entityHelper;

        public IothubmanagerclientTest()
        {
            this.mockConfig = new Mock<IIotHubManagerClientConfig>();
            this.mockConfig
                .Setup(c => c.IotHubManagerApiUrl)
                .Returns(MOCK_API_URL);

            this.mockRequestHelper = new Mock<IExternalRequestHelper>();

            this.client = new IotHubManagerClient(this.mockConfig.Object, this.mockRequestHelper.Object);            
            this.rand = new Random();
            this.entityHelper = new CreateEntityHelper(this.rand);
        }

        [Fact]
        public async Task GetListAsyncReturnsExpectedValue()
        {
            string tenantId = this.rand.NextString();
            List<DeviceGroupConditionModel> conditions = new List<DeviceGroupConditionModel>();
            List<DeviceModel> devices = new List<DeviceModel>
            {
                this.entityHelper.CreateDevice(),
                this.entityHelper.CreateDevice()
            };
            DeviceListModel deviceListModel = new DeviceListModel
            {
                Items = devices
            };

            this.mockRequestHelper
                .Setup(r => r.ProcessRequestAsync<DeviceListModel>(
                    It.Is<HttpMethod>(m => m == HttpMethod.Get),
                    It.Is<String>(url => url.Contains(MOCK_API_URL)),
                    It.Is<String>(s => s == tenantId)))
                .ReturnsAsync(deviceListModel);

            DeviceListModel response = await this.client.GetListAsync(conditions, tenantId);

            this.mockRequestHelper
                .Verify(r => r.ProcessRequestAsync<DeviceListModel>(
                        It.Is<HttpMethod>(m => m == HttpMethod.Get),
                        It.Is<String>(url => url.Contains(MOCK_API_URL)),
                        It.Is<String>(s => s == tenantId)),
                    Times.Once);
            
            Assert.Equal(deviceListModel, response);
        }
    }
}