using System.Web.Http;

namespace Parrot {

	/// <summary>
	/// Web APIコンフィグ
	/// </summary>
	public static class WebApiConfig {

		public static void Register( HttpConfiguration config ) {

			// Web API の構成およびサービス

			// Web API ルート
			config.MapHttpAttributeRoutes();

			config.Routes.MapHttpRoute(
				name : "DefaultApi" ,
				routeTemplate : "api/{controller}/{id}" ,
				defaults : new { id = RouteParameter.Optional }
			);

		}

	}

}
