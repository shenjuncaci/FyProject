using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LeaRun.Entity
{
    //虚拟类，实际没有数据表对应，用于临时存放显示到页面上的数据
    public class FlowDisplay
    {
        public string FlowID { get; set; }
        public string FlowNo { get; set; }
        public string NoteID { get; set; }
        public int Approvestatus { get; set; }
        public string CurrentPost { get; set; }
        public string CurrentPerson { get; set; }

        public string FullName { get; set; }

        public string RealName { get; set; }
        public string ApprovestatusCN { get; set; }

        public string FlowName { get; set; }

        public IList<FlowDetailDisplay> LogList { get; set; }

        public IList<Base_flowApprove> ApproveList { get; set; }
    }
}
