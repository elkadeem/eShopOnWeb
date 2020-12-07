using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.eShopWeb.Infrastructure.ApplicationInsights
{
    public class RoleNameTelemetryIntializar : ITelemetryInitializer
    {
        public RoleNameTelemetryIntializar(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentException($"'{nameof(roleName)}' cannot be null or whitespace", nameof(roleName));
            }

            RoleName = roleName;
        }

        public string RoleName { get; }

        public void Initialize(ITelemetry telemetry)
        {
            telemetry.Context.Cloud.RoleName = RoleName;
        }
    }
}
