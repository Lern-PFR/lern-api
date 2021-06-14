using System.Collections.Generic;
using System.Linq;
using Lern_API.Models;

namespace Lern_API.DataTransferObjects.Responses
{
    public class SubjectsResponse
    {
        public IEnumerable<Subject> Mine { get; }
        public IEnumerable<Subject> Active { get; }
        public IEnumerable<Subject> Available { get; }

        public SubjectsResponse(IEnumerable<Subject> mine, IEnumerable<Subject> active, IEnumerable<Subject> available)
        {
            Mine = mine.AsEnumerable();
            Active = active.AsEnumerable();
            Available = available.AsEnumerable();
        }
    }
}
