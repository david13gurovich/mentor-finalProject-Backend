using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.In
{
    public class JointMissions
    {
        public ScheduleSetting? Setting { get; set; }
        public List<int>? MissionsId { get; set; }
    }
}
