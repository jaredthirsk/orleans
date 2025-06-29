using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Forkleans.Runtime;
using Forkleans.Transactions.Abstractions;

namespace Forkleans.Transactions
{
    public class TransactionalStateAttributeMapper : TransactionalStateAttributeMapper<TransactionalStateAttribute>
    {
        protected override TransactionalStateConfiguration AttributeToConfig(TransactionalStateAttribute attribute)
        {
            return new TransactionalStateConfiguration(attribute);
        }
    }

    public abstract class TransactionalStateAttributeMapper<TAttribute> : IAttributeToFactoryMapper<TAttribute>
        where TAttribute : IFacetMetadata, ITransactionalStateConfiguration
    {
        private static readonly MethodInfo create = typeof(ITransactionalStateFactory).GetMethod("Create");

        public Factory<IGrainContext, object> GetFactory(ParameterInfo parameter, TAttribute attribute)
        {
            TransactionalStateConfiguration config = AttributeToConfig(attribute);
            // use generic type args to define collection type.
            MethodInfo genericCreate = create.MakeGenericMethod(parameter.ParameterType.GetGenericArguments());
            object[] args = new object[] { config };
            return context => Create(context, genericCreate, args);
        }

        private object Create(IGrainContext context, MethodInfo genericCreate, object[] args)
        {
            ITransactionalStateFactory factory = context.ActivationServices.GetRequiredService<ITransactionalStateFactory>();
            return genericCreate.Invoke(factory, args);
        }

        protected abstract TransactionalStateConfiguration AttributeToConfig(TAttribute attribute);
    }
}
