// Ignore Spelling: Ebiz

using Aptify.Framework.DataServices;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PB.Rexies.AptifyBits;
using PB.Rexies.AptifyBits.ProcessPipeline;
using SimplifyAptify.AptifyConfigs.Soa.EndpointComponents.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Text;

namespace SimplifyAptify.AptifyConfigs.Soa.EndpointComponents
{
    public abstract class BaseEndpointComponent : ProcessComponentBase
    {
        #region Properties

        public int? AuthenticatedPrincipalRecordId
        {
            get => Properties.GetInt32("AuthenticatedPrincipalRecordId");
            set => Properties["AuthenticatedPrincipalRecordId"] = value;
        }

        public const string EndpointComponentResponsePropertyName = "EndpointComponentResponse";
        public static string OutputEndpointComponentResponsePropertyName => $"output{EndpointComponentResponsePropertyName}";

        #endregion


        #region Response Message

        public HttpResponseMessage EndpointComponentResponseMessage
        {
            get {
                var response = Properties.GetProperty(OutputEndpointComponentResponsePropertyName);
                if (response is HttpResponseMessage message)
                {
                    return message;
                }

                return null;
            }
        }

        public HttpResponseMessage EndpointComponentResponseSet(EndpointComponentResponse output)
        {
            if (output == null)
            {
                output = new EndpointComponentResponse() { IsSuccess = false, Message = "An unknown exception has occurred." };
            }

            var outputJson = JsonConvert.SerializeObject(output);
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(outputJson, Encoding.UTF8, "application/json")
            };

            Properties.SetProperty(EndpointComponentResponsePropertyName, output);
            EndpointComponentResponseSet(response);

            return response;
        }

        public void EndpointComponentResponseSet(HttpResponseMessage responseMessage)
        {
            Properties.SetProperty(OutputEndpointComponentResponsePropertyName, responseMessage);
        }

        #endregion

        protected void EnsureAuthenticated()
        {
            if (
                AuthenticatedPrincipalRecordId == null
                || AuthenticatedPrincipalRecordId <= 0
                || (DataAction.ExecuteScalarBooleanParameterized(
                        "spEndpointComponent_IsAnAnonymousPrincipleRecord_sa",
                        System.Data.CommandType.StoredProcedure,
                        DataAction.GetDataParameter(
                            "@AuthenticatedPrincipalRecordId",
                            System.Data.SqlDbType.Int,
                            AuthenticatedPrincipalRecordId
                        ),
                        DataAction.GetDataParameter(
                            "@ServiceApplicationName",
                            System.Data.SqlDbType.NVarChar,
                            DataAction.UserCredentials.Application
                        )
                    ) ?? true
                ))
            {
                throw new UnauthorizedAccessException("Unauthorized");
            }
        }

        protected override string OnHandleException(Exception ex)
        {
            // Log Error
            Logger.LogError(ex, "{Class}.OnHandleException", GetType().Name);

            var output = new EndpointComponentResponse
            {
                IsSuccess = false,
                Message = ex.Message
            };

            EndpointComponentResponseSet(output);

            // eBiz Soa Endpoints need to always return Success so that Aptify doesn't
            // try and take over and do it's weirdness
            return SuccessResult;
        }
    }
}