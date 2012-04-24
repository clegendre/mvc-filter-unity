using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace CK.Web.Mvc
{
    public interface IFilterAdapter
    {
        Filter BuildFilter( ControllerContext c, ActionDescriptor actionDescriptor );
    }
}
