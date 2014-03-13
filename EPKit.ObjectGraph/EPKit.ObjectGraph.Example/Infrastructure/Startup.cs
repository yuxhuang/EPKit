﻿using System;
using Owin;
using System.Web.Http;

namespace EPKit.ObjectGraph.Example.Infrastructure
{
	public class Startup
	{
		public void Configuration(IAppBuilder appBuilder)
		{
			// Configure Web API for self-host. 
			HttpConfiguration config = new HttpConfiguration(); 
			config.Routes.MapHttpRoute( 
				name: "DefaultApi", 
				routeTemplate: "api/{controller}/{id}", 
				defaults: new { id = RouteParameter.Optional } 
			); 

			appBuilder.UseWebApi(config); 
		}
	}
}

