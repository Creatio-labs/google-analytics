using System.Data;
using Terrasoft.Core.Factories;

namespace WorkGlbGoogleAnalytics.GA
{
	using System.ServiceModel;
	using System.ServiceModel.Activation;
	using System.ServiceModel.Web;
	using Terrasoft.Web.Common;

	[ServiceContract]
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
	public class WorkGlbGoogleAnalyticsService : BaseService
	{
		[OperationContract]
		[WebInvoke(Method = "GET", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped,
			ResponseFormat = WebMessageFormat.Json)]
		public void Method1(GARequestConfig requestConfig)
		{
			GAApiHelper helper = ClassFactory.Get<GAApiHelper>(
				new ConstructorArgument("userConnection", UserConnection)
			);
			DataTable result = helper.GetData(requestConfig);
			
		}
	}
}