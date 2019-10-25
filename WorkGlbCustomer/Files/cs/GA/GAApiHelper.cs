using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography.X509Certificates;
using Google.Apis.AnalyticsReporting.v4;
using Google.Apis.AnalyticsReporting.v4.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Terrasoft.Core;
using Terrasoft.Core.Configuration;

namespace WorkGlbGoogleAnalytics.GA
{
	public class GAApiHelper
	{
		private UserConnection _userConnection;
		private AnalyticsReportingService AnalyticsService { get; }
		
		public GAApiHelper(UserConnection userConnection)
		{
			_userConnection = userConnection;
			AnalyticsService = CreateAnalyticsService();
		}
		
		private AnalyticsReportingService CreateAnalyticsService()
		{
			var keyPath = String.Empty;
			var serviceEmail = (string)SysSettings.GetValue(_userConnection, "GAServiceEmail");
			var certName = (string)SysSettings.GetValue(_userConnection, "GAServiceUniqueId");
			var appName = (string)SysSettings.GetValue(_userConnection, "GAProjectName");
			
			var certificate = new X509Certificate2(keyPath, "notasecret", X509KeyStorageFlags.Exportable);
			var certificate2 = GetCertificateFromStore(certName);

			var credentials = new ServiceAccountCredential(
				new ServiceAccountCredential.Initializer(serviceEmail)
				{
					Scopes = new[] {AnalyticsReportingService.Scope.AnalyticsReadonly}
				}.FromCertificate(certificate2));

			return new AnalyticsReportingService(new BaseClientService.Initializer()
			{
				HttpClientInitializer = credentials,
				ApplicationName = appName
			});
		}

		private X509Certificate2 GetCertificateFromStore(string crtName)
		{
			crtName = crtName.StartsWith("CN=") ? crtName : $"CN={crtName}";
			using (var store = new X509Store(StoreLocation.LocalMachine)) {
				store.Open(OpenFlags.ReadOnly);
				X509Certificate2Collection crtCollection = store.Certificates.Find(X509FindType.FindBySubjectDistinguishedName, crtName, false);
				return crtCollection.Count == 0 ? null : crtCollection[0];
			}
		}

		public DataTable GetData(GARequestConfig requestConfig)
		{
			var reportRequest = requestConfig.CreateReportRequest();
			GetReportsRequest requestContainer = new GetReportsRequest
			{
				ReportRequests = new List<ReportRequest> {reportRequest}
			};
			DataTable dt = new DataTable();
			foreach (var dimension in reportRequest.Dimensions) {
				dt.Columns.Add(dimension.Name);
			}

			foreach (var metric in reportRequest.Metrics) {
				dt.Columns.Add(metric.Alias ?? metric.Expression);
			}
			
			while (requestContainer.ReportRequests.Count > 0) {
				GetReportsResponse response = AnalyticsService.Reports.BatchGet(requestContainer).Execute();
				requestContainer.ReportRequests = new List<ReportRequest>();
				foreach (Report report in response.Reports) {
					
					var dimensionHeaders = report.ColumnHeader.Dimensions;
					var metricHeaders = report.ColumnHeader.MetricHeader.MetricHeaderEntries;

					foreach (ReportRow row in report.Data.Rows) {
						var dimensionValues = row.Dimensions;
						var metricValues = row.Metrics;
						var dataRow = dt.NewRow();

						for (int i = 0; i < dimensionHeaders.Count && i < dimensionValues.Count; i++) {
							dataRow[dimensionHeaders[i]] = dimensionValues[i];
						}
						
						for (int l = 0; l < metricValues.Count; l++) {
							DateRangeValues values = metricValues[l];
							for (int i = 0; i < values.Values.Count && i < metricHeaders.Count; i++) {
								dataRow[metricHeaders[i].Name] = values.Values[i];
							}
						}

						dt.Rows.Add(dataRow);
					}

					if (!String.IsNullOrEmpty(report.NextPageToken)) {
						requestConfig.NextPageToken = report.NextPageToken;
						requestContainer.ReportRequests.Add(requestConfig.CreateReportRequest());
					}
				}
			}
			return dt;
		}
	}
}