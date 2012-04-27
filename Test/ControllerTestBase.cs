using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using System.Web.Mvc;
using CK.Web.Mvc;
using CK.Web.Mvc.Unity;

namespace CK.Web.Mvc.Tests
{
    public abstract class ControllerTestBase
    {
        protected interface IDependentService
        {
        }

        protected class DependentServiceImpl : IDependentService
        {
            public int Trace { get; set; }

            public static int TraceStatic { get; set; }

            public DependentServiceImpl()
            {
                Trace++;
                TraceStatic++;
            }
        }

        protected class ActionFilteredResult : ActionResult
        {
            public override void ExecuteResult( ControllerContext context )
            {
            }
        }

        public IUnityContainer ConfigureUnityContainer()
        {
            var u = new UnityContainer();

            u.RegisterType<UnityFilterProvider>( new ContainerControlledLifetimeManager() );
            u.RegisterType<UnityFilterAttributeFilterProvider>( new ContainerControlledLifetimeManager() );

            return u;
        }

        protected void RegisterFilterProvider( IFilterProvider filterProvider )
        {
            var oldProviders = FilterProviders.Providers.Where( f => f is IFilterProvider ).ToList();
            if( oldProviders != null ) foreach( var fp in oldProviders ) FilterProviders.Providers.Remove( fp );

            FilterProviders.Providers.Add( filterProvider );
        }

        protected void RegisterFilterAttributeProvider( IFilterProvider filterProvider )
        {
            var oldProvider = FilterProviders.Providers.SingleOrDefault( f => f is FilterAttributeFilterProvider );
            if( oldProvider != null ) FilterProviders.Providers.Remove( oldProvider );

            FilterProviders.Providers.Add( filterProvider );
        }
    }
}
