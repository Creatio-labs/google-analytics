using System;
using System.Collections.Generic;
using System.Linq;
using Google.Apis.AnalyticsReporting.v4.Data;

namespace WorkGlbGoogleAnalytics.GA
{
	public class GARequestConfig
	{
		private const string GA_DATE_FORMAT = "yyyy-MM-dd";

		public string ProfileId { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public List<string> Metrics { get; set; }
		public List<string> Dimensions { get; set; }
		public string Filters { get; set; }
		public int PageSize { get; set; } = 1000;
		public string NextPageToken { get; set; }


		public ReportRequest CreateReportRequest()
		{
			return new ReportRequest
			{
				ViewId = ProfileId.StartsWith("ga:") ? ProfileId : $"ga:{ProfileId}",
				DateRanges = new List<DateRange>
				{
					new DateRange
					{
						StartDate = StartDate.ToString(GA_DATE_FORMAT),
						EndDate = EndDate.ToString(GA_DATE_FORMAT)
					}
				},
				Metrics = Metrics
					.Select(metric => new Metric {Expression = metric.StartsWith("ga:") ? metric : $"ga:{metric}"})
					.ToList(),
				Dimensions = Dimensions
					.Select(dimension => new Dimension
						{Name = dimension.StartsWith("ga:") ? dimension : $"ga:{dimension}"})
					.ToList(),
				FiltersExpression = Filters,
				PageSize = PageSize,
				PageToken = NextPageToken
			};
		}
	}
}