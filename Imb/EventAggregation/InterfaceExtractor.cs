using System;
using System.Collections.Generic;

namespace Imb.EventAggregation
{
    public class InterfaceExtractor
    {
        /// <summary>
        /// When you need to know if a type implements a specific generic interface, but you don't care about the parameter 
        /// types, this helper will return you an array. For example, if the interface you want is <see cref="IList{T}" />,
        /// and the type you are analysing is MyType:
        /// 
        ///     GetImplementationsOfGenericInterface(typeof(MyType), typeof(IList&lt;&gt;));
        /// 
        /// The output from this call would be an array of <see cref="IList{T}" /> Types with the type arguments intact.
        /// </summary>
        /// <param name="typeToCheck">The type you need to analyse.</param>
        /// <param name="genericInterfaceSought">The generic interface for which you are looking.</param>
        /// <returns></returns>
        public static Type[] GetImplementationsOfGenericInterface(Type typeToCheck, Type genericInterfaceSought)
        {
            return typeToCheck.FindInterfaces(MatchType, genericInterfaceSought);
        }

        private static bool MatchType(Type m, object filtercriteria)
        {
            return m.IsGenericType && ReferenceEquals(m.GetGenericTypeDefinition(), filtercriteria as Type);
        }
    }
}
