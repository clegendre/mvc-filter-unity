using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Microsoft.Practices.Unity;

namespace CK.Web.Mvc.Unity
{
    public class UnityFilterProvider : IFilterProvider
    {
        readonly IUnityContainer _container;

        public UnityFilterProvider( IUnityContainer container )
        {
            this._container = container;
        }

        public IEnumerable<Filter> GetFilters( ControllerContext controllerContext, ActionDescriptor actionDescriptor )
        {
            return _container.ResolveAll<IFilterAdapter>().Select( f => f.BuildFilter( controllerContext, actionDescriptor ) );
        }
    }
}
