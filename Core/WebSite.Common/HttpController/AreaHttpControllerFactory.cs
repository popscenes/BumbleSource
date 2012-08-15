using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace WebSite.Common.HttpController
{
    //not supported anymore
//    public class AreaHttpControllerFactory : IHttpControllerFactory
//    {
//        private const string ControllerSuffix = "Controller";
//        private const string AreaRouteVariableName = "area";
//
//        private readonly IHttpControllerFactory _defaultFactory;
//        private readonly HttpConfiguration _configuration;
//        private Dictionary<string, Type> _apiControllerTypes;
//        
//        public AreaHttpControllerFactory(HttpConfiguration configuration)
//        {
//            _configuration = configuration;
//            _defaultFactory = new DefaultHttpControllerFactory(configuration);
//        }
//
//        public IHttpController CreateController(HttpControllerContext controllerContext, string controllerName)
//        {
//            var controller = GetApiController(controllerContext, controllerName);
//            return controller ?? _defaultFactory.CreateController(controllerContext, controllerName);
//        }
//
//        public void ReleaseController(IHttpController controller)
//        {
//            _defaultFactory.ReleaseController(controller); 
//        }
//
//        private Dictionary<string, Type> ApiControllerTypes
//        {
//            get
//            {
//                if (_apiControllerTypes != null)
//                {
//                    return _apiControllerTypes;
//                }
//
//                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
//
//                _apiControllerTypes = assemblies.SelectMany(a => a.GetTypes().Where(t => !t.IsAbstract && t.Name.EndsWith(ControllerSuffix) && typeof(IHttpController).IsAssignableFrom(t))).ToDictionary(t => t.FullName, t => t);
//
//                return _apiControllerTypes;
//            }
//        }
//
//        private IHttpController GetApiController(HttpControllerContext controllerContext, string controllerName)
//        {
//            if (!controllerContext.RouteData.Values.ContainsKey(AreaRouteVariableName))
//            {
//                return null;
//            }
//
//            var areaName = controllerContext.RouteData.Values[AreaRouteVariableName].ToString().ToLower();
//            if (string.IsNullOrEmpty(areaName))
//            {
//                return null;
//            }
//
//            var type = ApiControllerTypes.Where(t => t.Key.ToLower().Contains(string.Format(".{0}.", areaName)) && t.Key.EndsWith(string.Format(".{0}{1}", controllerName, ControllerSuffix), StringComparison.OrdinalIgnoreCase)).Select(t => t.Value).FirstOrDefault();
//            if (type == null)
//            {
//                return null;
//            }
//
//            return CreateControllerInstance(controllerContext, controllerName, type);
//        }
//
//        private IHttpController CreateControllerInstance(HttpControllerContext controllerContext, string controllerName, Type controllerType)
//        {
//            var descriptor = new HttpControllerDescriptor(_configuration, controllerName, controllerType);
//            controllerContext.ControllerDescriptor = descriptor;
//            var controller = descriptor.HttpControllerActivator.Create(controllerContext, controllerType);
//            controllerContext.Controller = controller;
//            return controller;
//        }
//    }
}
