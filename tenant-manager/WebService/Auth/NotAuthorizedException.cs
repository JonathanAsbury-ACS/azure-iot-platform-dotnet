// Copyright (c) Microsoft. All rights reserved.

using System;

namespace MMM.Azure.IoTSolutions.TenantManager.WebService
{
    /// <summary>
    /// This exception is thrown when the user is not authorized to perform the action.
    /// </summary>
    public class NotAuthorizedException : Exception
    {
        public NotAuthorizedException() : base()
        {
        }

        public NotAuthorizedException(string message) : base(message)
        {
        }

        public NotAuthorizedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}