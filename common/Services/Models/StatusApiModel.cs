﻿using System;
using System.Collections.Generic;
using Mmm.Platform.IoT.Common.Services.Models;
using Mmm.Platform.IoT.Common.Services.Runtime;
using Newtonsoft.Json;

namespace Mmm.Platform.IoT.Common.Services.Models
{
    /// <summary>
    /// Model for retrieving the status of API    
    /// /// </summary>
    public sealed class StatusApiModel
    {
        private const string DATE_FORMAT = "yyyy-MM-dd'T'HH:mm:sszzz";

        [JsonProperty(PropertyName = "Name", Order = 10)]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Status", Order = 20)]
        public StatusResultApiModel Status { get; set; }

        [JsonProperty(PropertyName = "CurrentTime", Order = 30)]
        public string CurrentTime => DateTimeOffset.UtcNow.ToString(DATE_FORMAT);

        [JsonProperty(PropertyName = "StartTime", Order = 40)]
        public string StartTime => Uptime.Start.ToString(DATE_FORMAT);

        [JsonProperty(PropertyName = "UpTime", Order = 50)]
        public long UpTime => Convert.ToInt64(Uptime.Duration.TotalSeconds);

        /// <summary>
        /// Value generated at bootstrap by each instance of the service and
        /// used to correlate logs coming from the same instance. The value
        /// changes every time the service starts.
        /// </summary>
        [JsonProperty(PropertyName = "UID", Order = 60)]
        public string UID => Uptime.ProcessId;

        /// <summary>A property bag with details about the service</summary>
        [JsonProperty(PropertyName = "Properties", Order = 70)]
        public StatusServicePropertiesModel Properties { get; set; }

        /// <summary>A property bag with details about the internal dependencies</summary>
        [JsonProperty(PropertyName = "Dependencies", Order = 80)]
        public Dictionary<string, StatusResultApiModel> Dependencies { get; set; }

        [JsonProperty(PropertyName = "$metadata", Order = 1000)]
        public Dictionary<string, string> Metadata => new Dictionary<string, string>
        {
            { "$type", "Status;" + "0" },
            { "$uri", "/status" }
        };

        public StatusApiModel(StatusServiceModel model, string name)
        {
            Status = new StatusResultApiModel(model.Status);
            Dependencies = new Dictionary<string, StatusResultApiModel>();
            Name = name;
            foreach (KeyValuePair<string, StatusResultServiceModel> pair in model.Dependencies)
            {
                this.Dependencies.Add(pair.Key, new StatusResultApiModel(pair.Value));
            }
            this.Properties = model.Properties;
        }
    }
}
