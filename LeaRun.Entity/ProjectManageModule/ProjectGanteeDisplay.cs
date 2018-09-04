//=====================================================================================
// All Rights Reserved , Copyright @ Learun 2014
// Software Developers @ Learun 2014
//=====================================================================================

using LeaRun.DataAccess.Attributes;
using LeaRun.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;

using System.Text;

namespace LeaRun.Entity
{
    ////虚拟类，实际没有数据表对应，用于临时存放显示到页面上的数据
    public class ProjectGanteeDisplay
    {
        public string UID { get; set; }

        public string Name { get; set; }

        public DateTime? Start { get; set; }
        public DateTime? Finish { get; set; }
        public int Duration { get; set; }
        public int PercentComplete { get; set; }
        public int Summary { get; set; }
        public int Critical { get; set; }
        public int Milestone { get; set; }
        public string ParentID { get; set; }
        public List<G_PLM_ProjectPreLink> PredecessorLink { get; set; }
        public List<ProjectGanteeDisplay> children { get; set; }

        public int StructureForm { get; set; }
    }
}