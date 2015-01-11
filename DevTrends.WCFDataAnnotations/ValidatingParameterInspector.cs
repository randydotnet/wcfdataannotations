﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;

namespace DevTrends.WCFDataAnnotations
{
    /// <summary>
    /// 
    /// </summary>
    public class ValidatingParameterInspector : IParameterInspector
    {
        private readonly IEnumerable<IObjectValidator> _validators;
        private readonly IErrorMessageGenerator _errorMessageGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidatingParameterInspector"/> class.
        /// </summary>
        /// <param name="validators">The validators.</param>
        /// <param name="errorMessageGenerator">The error message generator.</param>
        public ValidatingParameterInspector(IEnumerable<IObjectValidator> validators, IErrorMessageGenerator errorMessageGenerator)
        {
            if (validators == null)
            {
                throw new ArgumentNullException("validators");
            }

            if (!validators.Any())
            {
                throw new ArgumentException("At least one validator is required.");
            }

            if (errorMessageGenerator == null)
            {
                throw new ArgumentNullException("errorMessageGenerator");
            }

            _validators = validators;
            _errorMessageGenerator = errorMessageGenerator;
        }

        /// <summary>
        /// Called after client calls are returned and before service responses are sent.
        /// </summary>
        /// <param name="operationName">The name of the invoked operation.</param>
        /// <param name="outputs">Any output objects.</param>
        /// <param name="returnValue">The return value of the operation.</param>
        /// <param name="correlationState">Any correlation state returned from the 
        /// <see cref="M:System.ServiceModel.Dispatcher.IParameterInspector.BeforeCall(System.String,System.Object[])" /> method, 
        /// or null.</param>
        public void AfterCall(string operationName, object[] outputs, object returnValue, object correlationState)
        {            
        }

        /// <summary>
        /// Called before client calls are sent and after service responses are returned.
        /// </summary>
        /// <param name="operationName">The name of the operation.</param>
        /// <param name="inputs">The objects passed to the method by the client.</param>
        /// <returns>
        /// The correlation state that is returned as the <paramref name="correlationState" /> 
        /// parameter in <see cref="M:System.ServiceModel.Dispatcher.IParameterInspector.AfterCall(System.String,System.Object[],System.Object,System.Object)" />. 
        /// Return null if you do not intend to use correlation state.
        /// </returns>
        /// <exception cref="System.ServiceModel.FaultException"></exception>
        public object BeforeCall(string operationName, object[] inputs)
        {
            var validationResults = new List<ValidationResult>();

            foreach (var input in inputs)
            {
                foreach (var validator in _validators)
                {
                    var results = validator.Validate(input);
                    validationResults.AddRange(results);                    
                }                                
            }

            if (validationResults.Count > 0)
            {
                throw new FaultException(_errorMessageGenerator.GenerateErrorMessage(operationName, validationResults));
            }

            return null;
        }
    }
}