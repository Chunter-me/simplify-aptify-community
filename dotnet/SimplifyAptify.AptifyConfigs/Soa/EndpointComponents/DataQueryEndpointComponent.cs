// Ignore Spelling: Ebiz

using Aptify.Framework.DataServices;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PB.Rexies.AptifyBits;
using PB.Rexies.Data;
using SimplifyAptify.AptifyConfigs.Soa.EndpointComponents.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SimplifyAptify.AptifyConfigs.Soa.EndpointComponents
{
    public class DataQueryEndpointComponent : BaseEndpointComponent
    {
        public string DataObjectName
        {
            get => Properties.GetString(nameof(DataObjectName));
            set => Properties[nameof(DataObjectName)] = value;
        }

        protected override string RunCore()
        {
            Logger.LogInformation("DataQueryEndpointComponent");

            // Ensure DataObject
            if (string.IsNullOrWhiteSpace(DataObjectName))
            {
                throw new ArgumentException("DataObjectName is required.");
            }

            var dsEndpointDetails = DataAction.GetDataSetParameterized(
                "spEndpointComponent_DataQueryEndpointComponent_sa",
                CommandType.StoredProcedure,
                DataAction.GetDataParameter("@DataObjectName", SqlDbType.NVarChar, DataObjectName)
            );

            if (dsEndpointDetails.Tables.Count != 2)
            {
                throw new DataException("Unable to retrieved component dataset.");
            }

            if (dsEndpointDetails.Tables[0].Rows.Count != 1
                || dsEndpointDetails.Tables[0].Rows[0][0] == null
                || !dsEndpointDetails.Tables[0].Rows[0][0].ToString().Equals("Stored Procedure", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Only Stored Procedures are supported.");
            }

            Logger.LogInformation("DataQueryEndpointComponent - Sproc: {DataObjectName}", DataObjectName);

            // Build Data Object Params
            var sprocParams = new List<IDataParameter>();
            foreach (DataRow drParam in dsEndpointDetails.Tables[1].Rows)
            {
                var propName = drParam["Name"].ToString();
                var prop = Properties.GetProperty(propName.Replace("@", string.Empty).Trim());
                if (prop != null)
                {
                    if (Enum.TryParse(drParam["Type"].ToString(), out SqlDbType sqlDbType))
                    {
                        sprocParams.Add(DataAction.GetDataParameter(propName, sqlDbType, prop));
                    }
                    else
                    {
                        sprocParams.Add(DataAction.GetDataParameter(propName, prop));
                    }
                }
            }

            Logger.LogInformation("DataQueryEndpointComponent - Params: {SprocParams}", JsonConvert.SerializeObject(sprocParams));

            // Execute Data Object
            var dsDataObject = DataAction.GetDataSetParameterized(
                DataObjectName,
                CommandType.StoredProcedure,
                sprocParams.ToArray()
            );

            // Generate Output Response Object
            var output = new EndpointComponentResponse();

            Logger.LogInformation("DataQueryEndpointComponent - Tbl Count: {Count}", dsDataObject.Tables.Count);
            Logger.LogDebug("DataQueryEndpointComponent - Tbl Data: {Tbl}", JsonConvert.SerializeObject(dsDataObject.Tables));

            // Configured Data Set Output
            if (dsDataObject.Tables.Count > 0 && dsDataObject.Tables[0].Columns[0].ColumnName.Equals("TableName", StringComparison.OrdinalIgnoreCase))
            {
                if (dsDataObject.Tables[0].Rows.Count == 1)
                {
                    var isCollection = (bool)dsDataObject.Tables[0].Rows[0]["IsCollection"];
                    Logger.LogInformation("DataQueryEndpointComponent - Table Name Configuration - Single Table - {IsCollection}", isCollection);

                    output.Data = OutputDataObject(isCollection, dsDataObject.Tables[1]);
                }
                else
                {
                    Logger.LogInformation("DataQueryEndpointComponent - Table Name Configuration - Multiple Table");

                    var outputData = new Dictionary<string, object>();
                    var tableIdx = 1;

                    foreach (DataRow dr in dsDataObject.Tables[0].Rows)
                    {
                        var name = dr["TableName"].ToString();
                        var isCollection = (bool)dr["IsCollection"];

                        Logger.LogInformation("DataQueryEndpointComponent - Table Name Configuration - Multiple Table - {Name}: {IsCollection}", name, isCollection);

                        object data = OutputDataObject(isCollection, dsDataObject.Tables[tableIdx]);
                        outputData.Add(name, data);

                        // Increment Table Index
                        tableIdx += 1;
                    }

                    output.Data = outputData;
                }
            }

            // FOR JSON PATH
            else if (dsDataObject.Tables.Count > 0 && dsDataObject.Tables[0].Columns[0].ColumnName.Equals("JsonOutput", StringComparison.OrdinalIgnoreCase))
            {
                Logger.LogInformation("DataQueryEndpointComponent - Json Output");

                var json = dsDataObject.Tables[0].Rows[0]["JsonOutput"];
                if (json != null)
                {
                    var jsonString = json.ToString();

                    if (!string.IsNullOrWhiteSpace(jsonString) && (jsonString.StartsWith("{") || jsonString.StartsWith("[")))
                    {
                        output.Data = JsonConvert.DeserializeObject<dynamic>(json.ToString());
                    }
                }
            }

            else if (dsDataObject.Tables.Count == 1)
            {
                if (dsDataObject.Tables[0].Rows.Count == 1)
                {
                    Logger.LogInformation("DataQueryEndpointComponent - No Config - Single Row");

                    output.Data = dsDataObject.Tables[0].Columns
                        .Cast<DataColumn>()
                        .ToDictionary(
                            column => column.ColumnName,
                            column => dsDataObject.Tables[0].Rows[0][column]
                        );
                }
                else if (dsDataObject.Tables[0].Rows.Count > 1)
                {
                    Logger.LogInformation("DataQueryEndpointComponent - No Config - Single Table");
                    output.Data = dsDataObject.Tables[0];
                }
            }

            // Raw Data Set Output
            else
            {
                Logger.LogInformation("DataQueryEndpointComponent - No Config - Multiple Tables");
                output.Data = dsDataObject;
            }

            // Set Output Response Message
            EndpointComponentResponseSet(output);

            return SuccessResult;
        }

        private static object OutputDataObject(bool isCollection, DataTable dtResult)
        {
            if (isCollection)
            {
                return dtResult;
            }
            else if (!isCollection && dtResult.Rows.Count > 0)
            {
                return dtResult.Columns.Cast<DataColumn>().ToDictionary(column => column.ColumnName, column => dtResult.Rows[0][column]);
            }

            return null;
        }
    }
}