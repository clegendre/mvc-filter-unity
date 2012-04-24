using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using NUnit.Framework;
using Microsoft.Practices.Unity;
using CK.Web.Mvc;
using CK.Web.Mvc.Unity;
using System.Threading.Tasks;

namespace CK.Web.Mvc.Tests
{
    [TestFixture]
    public class TestAttributeFilterDependencyInjection : ControllerTestBase
    {
        class ActionFilterWithDependencyAttribute : FilterAttribute, IActionFilter
        {
            IDependentService _serviceRef;
            IDependentService _dependentService;

            [Dependency]
            public IDependentService DependentService
            {
                get { return _dependentService; }
                set
                {
                    _dependentService = value;
                    if( _serviceRef == null ) _serviceRef = value;
                }
            }

            public void OnActionExecuted( ActionExecutedContext filterContext )
            {
                Assert.That( DependentService, Is.Not.Null );
                Assert.That( DependentService is DependentServiceImpl );
                Assert.That( _dependentService, Is.SameAs( _serviceRef ) );
                var impl = DependentService as DependentServiceImpl;
                Assert.That( impl.Trace == 1 );

                filterContext.Result = new ActionFilteredResult();
            }

            public void OnActionExecuting( ActionExecutingContext filterContext )
            {
            }
        }

        class ActionFilterAttribute : FilterAttribute, IActionFilter
        {
            public IDependentService DependentService { get; set; }

            public void OnActionExecuted( ActionExecutedContext filterContext )
            {
                Assert.That( DependentService, Is.Not.Null );
                filterContext.Result = new ActionFilteredResult();
            }

            public void OnActionExecuting( ActionExecutingContext filterContext )
            {
            }
        }

        class ControllerStub : Controller
        {
            [ActionFilterWithDependency]
            public ActionResult SimpleActionUnityDependent()
            {
                return new EmptyResult();
            }

            [ActionFilter]
            public ActionResult SimpleAction()
            {
                return new EmptyResult();
            }
        }

        [Test]
        public void Test_Unity_Filter_Attribute()
        {
            var container = ConfigureUnityContainer();
            container.RegisterType<IDependentService, DependentServiceImpl>();

            var filterProvider = new UnityFilterAttributeFilterProvider( container );

            var controller = container.Resolve<ControllerStub>();
            controller.SetMockControllerContext();

            IActionInvoker a = new ActionInvokerExpecter<ActionFilteredResult>();

            RegisterFilterAttributeProvider( filterProvider );
            a.InvokeAction( controller.ControllerContext, "SimpleActionUnityDependent" );

            Assert.Throws<AssertionException>( () => a.InvokeAction( controller.ControllerContext, "SimpleAction" ), "Unity cannot injects dependencies without Dependency Attribute" );

            container.AddNewExtension<FilterAttributePropertyUnityContainerExtension>();
            a.InvokeAction( controller.ControllerContext, "SimpleAction" );
        }

        [Test]
        public void ParallelInvokationShouldReuseTheSameAttribute()
        {
            var container = ConfigureUnityContainer();
            container.RegisterType<IDependentService, DependentServiceImpl>( new PerResolveLifetimeManager() );

            var filterProvider = new UnityFilterAttributeFilterProvider( container );


            RegisterFilterAttributeProvider( filterProvider );
            IActionInvoker a = new ActionInvokerExpecter<ActionFilteredResult>();
            ParallelLoopResult result = Parallel.For( 0, 2, ( cpt ) =>
            {
                var controller = container.Resolve<ControllerStub>();
                controller.SetMockControllerContext();
                a.InvokeAction( controller.ControllerContext, "SimpleActionUnityDependent" );
            } );
            Assert.That( result.IsCompleted );
        }
    }
}
