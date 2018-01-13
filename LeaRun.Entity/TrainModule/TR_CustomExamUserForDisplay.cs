using LeaRun.DataAccess.Attributes;
using LeaRun.Utilities;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LeaRun.Entity
{
    public class TR_CustomExamUserForDisplay
    {
        public string Code { get; set; }
        public string RealName { get; set; }
        public string FullName { get; set; }

        public string UserID { get; set; }
       
    }
}
