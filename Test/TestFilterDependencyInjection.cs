using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Web.Mvc;
using Microsoft.Practices.Unity;
using CK.Web.Mvc;
using System.Web;
using System.Security.Principal;
using CK.Web.Mvc.Unity;

namespace CK.Web.Mvc.Tests
{
    [TestFixture]
    class TestFilterDependencyInjection : ControllerTestBase
    {
        class ActionFilterStub : IActionFilter
        {
            private readonly  IDependentService dependentService;

            public ActionFilterStub( IDependentService dependentService )
            {
                this.dependentService = dependentService;
            }

            public void OnActionExecuted( ActionExecutedContext filterContext )
            {
                Assert.That( dependentService, Is.Not.Null );
                filterContext.Result = new ActionFilteredResult();
            }

            public void OnActionExecuting( ActionExecutingContext filterContext )
            {
            }
        }

        class UnityActionFilterStub : UnityFilter<ActionFilterStub>
        {
            public UnityActionFilterStub( IUnityContainer container )
                : base( container, FilterScope.Global, 0 )
            {
            }
        }

        class ControllerStub : Controller
        {
            public ActionResult SimpleAction()
            {
                return new EmptyResult();
            }
        }

        [Test]
        public void Test_Action_Invokation_With_GloablFilter()
        {
            var container = ConfigureUnityContainer();
            GlobalFilters.Filters.Add( new ActionFilterStub( new DependentServiceImpl() ) );

            var controller = container.Resolve<ControllerStub>();
            controller.SetMockControllerContext();

            IActionInvoker a = new ActionInvokerExpecter<ActionFilteredResult>();
            a.InvokeAction( controller.ControllerContext, "SimpleAction" );
        }

        [Test]
        public void Test_Global_Filter_Injection()
        {
            var container = ConfigureUnityContainer();
            var filterProvider = new UnityFilterAttributeFilterProvider( container );
            RegisterFilterProvider( filterProvider );

            container.RegisterType<IDependentService, DependentServiceImpl>();
            container.RegisterType<IFilterAdapter, UnityActionFilterStub>( "Filter" );

            var controller = container.Resolve<ControllerStub>();
            controller.SetMockControllerContext();

            IActionInvoker a = new ActionInvokerExpecter<EmptyResult>();
            a.InvokeAction( controller.ControllerContext, "SimpleAction" );
        }
    }
}
