using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bcommerce.Domain.Validation;

    public abstract class Validator
    {
        private readonly IValidationHandler _handler;

        protected Validator(IValidationHandler handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public abstract void Validate();

        protected IValidationHandler ValidationHandler => _handler;
    }
