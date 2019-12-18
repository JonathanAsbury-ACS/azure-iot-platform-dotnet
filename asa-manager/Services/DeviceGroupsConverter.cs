using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Mmm.Platform.IoT.AsaManager.Services.External.BlobStorage;
using Mmm.Platform.IoT.AsaManager.Services.External.IotHubManager;
using Mmm.Platform.IoT.AsaManager.Services.Models;
using Mmm.Platform.IoT.AsaManager.Services.Models.DeviceGroups;
using Mmm.Platform.IoT.Common.Services.External.StorageAdapter;

namespace Mmm.Platform.IoT.AsaManager.Services
{
    public class DeviceGroupsConverter : Converter, IConverter
    {
        private const string CSV_HEADER = "DeviceId,GroupId";

        public override string Entity { get { return "devicegroups"; } }
        public override string FileExtension { get { return "csv"; } }

        private readonly IIotHubManagerClient _iotHubManager;

        public DeviceGroupsConverter(
            IIotHubManagerClient iotHubManager,
            IBlobStorageClient blobClient,
            IStorageAdapterClient storageAdapterClient,
            ILogger<DeviceGroupsConverter> log) : base(blobClient, storageAdapterClient, log)
        {
            this._iotHubManager = iotHubManager;
        }

        public override async Task<ConversionApiModel> ConvertAsync(string tenantId, string operationId = null)
        {
            ValueListApiModel deviceGroups = null;
            try
            {
                deviceGroups = await this._storageAdapterClient.GetAllAsync(this.Entity);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to query {entity} using storage adapter. OperationId: {operationId}. TenantId: {tenantId}", this.Entity, operationId, tenantId);
                throw e;
            }

            DeviceGroupListModel deviceGroupModels = null;
            try
            {
                deviceGroupModels = new DeviceGroupListModel
                {
                    Items = deviceGroups.Items.Select(group => 
                    {
                        DeviceGroupDataModel dataModel = JsonConvert.DeserializeObject<DeviceGroupDataModel>(group.Data);
                        DeviceGroupModel individualModel = new DeviceGroupModel(group.Key, group.ETag, dataModel);
                        return individualModel;
                    })
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to convert {entity} queried from storage adapter to appropriate data model. OperationId: {operationId}. TenantId: {tenantId}", this.Entity, operationId, tenantId);
                throw e;
            }

            Dictionary<DeviceGroupModel, DeviceListModel> deviceMapping = new Dictionary<DeviceGroupModel, DeviceListModel>();
            foreach (DeviceGroupModel deviceGroup in deviceGroupModels.Items)
            {
                try
                {
                    DeviceListModel devicesList = await this._iotHubManager.GetListAsync(deviceGroup.Conditions, tenantId);
                    if (devicesList.Items.Count() > 0)
                    {
                        deviceMapping.Add(deviceGroup, devicesList);
                    }
                }
                catch (Exception e)
                {
                    // Do not throw an exception here, attempt to query other device groups instead to get as much data as possible
                    // Log all device groups that could not be retreived
                    _logger.LogError(e, "Unable to get list of devices for devicegroup {deviceGroup} from IotHubManager. OperationId: {operationId}. TenantId: {tenantId}", deviceGroup.Id, operationId, tenantId);
                }
            }
            if (deviceMapping.Count() == 0)
            {
                string groups = $"[{String.Join(", ", deviceGroupModels.Items.Select(group => group.Id))}]";
                _logger.LogError("No Devices were found for any {entity}. OperationId: {operationId}. TenantId: {tenantId}\n{deviceGroups}", this.Entity, operationId, tenantId, groups);
                throw new Exception($"No Devices were found for any {this.Entity}.");
            }

            string fileContent = null;
            try
            {
                // Write a file in csv format:
                // deviceId,groupId
                // mapping contains devices groups, and a list model of all devices within each device group
                // create a new csv row for each device and device group combination
                string fileContentRows = String.Join("\n", deviceMapping.Select(mapping =>
                {
                    return String.Join("\n", mapping.Value.Items.Select(device => $"{device.Id},{mapping.Key.Id}"));
                }));
                // Add the rows and the header together to complete the csv file content
                fileContent = $"{CSV_HEADER}\n{fileContentRows}";
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to serialize the {entity} data models for the temporary file content. OperationId: {operationId}. TenantId: {tenantId}", this.Entity, operationId, tenantId);
                throw e;
            }

            string blobFilePath = await this.WriteFileContentToBlobAsync(fileContent, tenantId, operationId);

            ConversionApiModel conversionResponse = new ConversionApiModel
            {
                TenantId = tenantId,
                BlobFilePath = blobFilePath,
                Entities = deviceGroups,
                OperationId = operationId
            };
            _logger.LogInformation("Successfully Completed {entity} conversion\n{model}", this.Entity, JsonConvert.SerializeObject(conversionResponse));
            return conversionResponse;
        }
    }
}