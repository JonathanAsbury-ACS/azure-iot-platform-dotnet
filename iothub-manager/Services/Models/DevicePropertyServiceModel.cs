// <copyright file="DevicePropertyServiceModel.cs" company="3M">
// Copyright (c) 3M. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace Mmm.Iot.IoTHubManager.Services.Models
{
    public class DevicePropertyServiceModel
    {
        public bool Rebuilding { get; set; } = false;

        public HashSet<string> Tags { get; set; }

        public HashSet<string> Reported { get; set; }

        public bool IsNullOrEmpty() => (Tags == null || Tags.Count == 0) && (Reported == null || Reported.Count == 0);
    }
}