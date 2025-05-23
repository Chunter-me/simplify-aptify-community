using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PB.Rexies.AptifyBits;
using PB.Rexies.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SimplifyAptify.AptifyConfigs.Soa.EndpointComponents
{
    public class ServiceDataObjectEndpointComponent : PostContentEndpointComponent<Dictionary<string, object>>
    {
        protected override string RunCore()
        {
            Logger.LogInformation("ServiceDataObjectEndpointComponent");

            // Ensure & Get Post Content Data
            EnsureSetPostData();

            // Verify SDO Key
            if (!PostData.ContainsKey("SDO") || string.IsNullOrWhiteSpace(PostData["SDO"].ToString()))
            {
                throw new ArgumentException("SDO key is required.");
            }

            Logger.LogInformation("ServiceDataObjectEndpointComponent - Post Data: {PostData}", JsonConvert.SerializeObject(PostData, Formatting.Indented));

            // Get SDO Details
            var dataObjectName = string.Empty;
            var requiresPrinciple = true;

            var lstProps = new Dictionary<string, bool>();

            using (var reader = DataAction.ExecuteDataReaderParameterized(
                "spEndpointComponent_ServiceDataObjectEndpointComponent_sa",
                CommandType.StoredProcedure,
                DataAction.GetDataParameter("@SDO", SqlDbType.NVarChar, PostData["SDO"])
            ))
            {
                if (reader.Read())
                {
                    dataObjectName = reader.GetNullableString("DataObjectName") ?? string.Empty;
                    requiresPrinciple = reader.GetNullableBoolean("RequiresPrinciple") ?? true;
                }

                reader.NextResult();

                while (reader.Read())
                {
                    lstProps.Add(reader.GetString("Name"), reader.GetBoolean("IsRequired"));
                }
            }

            Logger.LogInformation("BaseEndpointProcessFlowComponent - DBObject: {DataObjectName} - Requires Principle: {RequiresPrinciple}", dataObjectName, requiresPrinciple);

            // Verify SDO Details
            if (string.IsNullOrWhiteSpace(dataObjectName))
            {
                throw new ArgumentException("Invalid SDO.");
            }

            if (requiresPrinciple)
            {
                EnsureAuthenticated();
            }

            if (lstProps.Count > 0)
            {
                var missing = lstProps.Where(p => p.Value)
                  .Select(p => p.Key)
                  .Except(PostData.Keys, StringComparer.OrdinalIgnoreCase);

                if (missing.Any())
                {
                    throw new ArgumentException($"Required properties ({string.Join(", ", missing)}) are missing.");
                }
            }

            // Execute Data Query for SDO
            var dataQueryComponent = new DataQueryEndpointComponent()
            {
                DataObjectName = dataObjectName,
                AuthenticatedPrincipalRecordId = AuthenticatedPrincipalRecordId
            };

            foreach (var propVal in PostData.Where(p => !p.Key.Equals("SDO", StringComparison.OrdinalIgnoreCase)))
            {
                dataQueryComponent.Properties.SetProperty(propVal.Key, propVal.Value);
            }

            dataQueryComponent.Config(Application);
            dataQueryComponent.Run();

            Properties.SetProperty(OutputEndpointComponentResponsePropertyName, dataQueryComponent.Properties.GetProperty(OutputEndpointComponentResponsePropertyName));
            Properties.SetProperty(EndpointComponentResponsePropertyName, dataQueryComponent.Properties.GetProperty(EndpointComponentResponsePropertyName));

            return SuccessResult;
        }
    }
}