//=====================================================================================
// All Rights Reserved , Copyright @ Learun 2014
// Software Developers @ Learun 2014
//=====================================================================================

using LeaRun.DataAccess.Attributes;
using LeaRun.Utilities;
using System;
using System.ComponentModel;

using System.Text;

namespace LeaRun.Entity
{
    ////虚拟类，实际没有数据表对应，用于临时存放显示到页面上的数据
    public class FlowDetailDisplay
    {
        #region 

        
        public string FlowDID { get; set; }

        public string FlowID { get; set; }
        public string ApproveBy { get; set; }
        public string ApprovePost { get; set; }
        public int StepNO { get; set; }
        public int IsFinish { get; set; }
        public string Approvestatus { get; set; }


        public string FullName { get; set; }

        public string RealName { get; set; }

        #endregion

       
    }
}