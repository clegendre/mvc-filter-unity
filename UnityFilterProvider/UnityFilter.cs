using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Microsoft.Practices.Unity;

namespace CK.Web.Mvc.Unity
{
    public class UnityFilter<T> : IFilterAdapter
    {
        readonly IUnityContainer _container;

        readonly FilterScope _filterScope;

        readonly int? _order;

        public UnityFilter( IUnityContainer container, FilterScope scope, int? order )
        {
            this._container = container;
            this._filterScope = scope;
            this._order = order;
        }

        public Filter BuildFilter( ControllerContext c, ActionDescriptor actionDescriptor )
        {
            object o = _container.Resolve( typeof( T ) );
            
            return new Filter( o, _filterScope, _order );
        }
    }
}
