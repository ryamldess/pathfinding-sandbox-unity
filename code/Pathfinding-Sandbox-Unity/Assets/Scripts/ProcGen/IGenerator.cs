/** 
 * Copyright (c) 2023-2024 Steve Sedlmayr and Droidknot LLC.
 **/

using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace SnS.ProcGen {
    /// <summary>
    /// An interface defining a generic generator.
    /// </summary>
    /// <typeparam name="GeneratedProductType">The type of the object being generated.</typeparam>
    /// <typeparam name="ParameterObjectType">The type of the value object parameterizing the generation.</typeparam>
    public interface IGenerator<GeneratedProductType, ParameterObjectType> where GeneratedProductType : UnityEngine.Object {
        /// <summary>
        /// Generates a List of GeneratedProductType instances according to the parameters in a ParameterObjectType object.
        /// </summary>
        /// <param name="parameters">The parameters of the generation.</param>
        /// <returns>A list of GeneratedProductType instances.</returns>
        public List<GeneratedProductType> Generate(ParameterObjectType parameters);
    }
}
