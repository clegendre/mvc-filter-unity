using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using System.ComponentModel;
using Microsoft.Practices.Unity.ObjectBuilder;
using System.Web.Mvc;

namespace CK.Web.Mvc.Unity
{
    public class FilterAttributePropertyUnityContainerExtension : UnityContainerExtension
    {
        protected override void Initialize()
        {
            Context.Strategies.Add( new FilterAttributePropertyInjectionBuilderStrategy( Container ), UnityBuildStage.Initialization );
        }
    }

    public class FilterAttributePropertyInjectionBuilderStrategy : BuilderStrategy
    {
        private readonly IUnityContainer _unityContainer;

        public FilterAttributePropertyInjectionBuilderStrategy( IUnityContainer unityContainer )
        {
            _unityContainer = unityContainer;
        }

        public override void PreBuildUp( IBuilderContext context )
        {
            if( typeof( FilterAttribute ).IsAssignableFrom( context.BuildKey.Type ) )
            {
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties( context.BuildKey.Type );

                foreach( PropertyDescriptor property in properties )
                {
                    if( _unityContainer.IsRegistered( property.PropertyType ) )
                    {
                        property.SetValue( context.Existing, _unityContainer.Resolve( property.PropertyType ) );
                    }
                }
            }
        }
    }
}
