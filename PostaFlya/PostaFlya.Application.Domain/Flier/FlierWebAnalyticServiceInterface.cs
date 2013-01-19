using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PostaFlya.Domain.Flier.Analytic;
using PostaFlya.Domain.Flier.Command;
using Website.Domain.Location;

namespace PostaFlya.Application.Domain.Flier
{
    public interface FlierWebAnalyticServiceInterface
    {
        void RecordVisit(string flierId, FlierAnalyticSourceAction context, Location location = null);
    }
}
