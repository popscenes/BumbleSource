using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Website.Infrastructure.Domain;

namespace Website.Domain.Payment
{
    public interface ChargableEntityInterface  : EntityIdInterface
    {
        double AccountCredit { get; set; }
    }
}
