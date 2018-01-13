//=====================================================================================
// created by shenjun 201706019
//=====================================================================================

using LeaRun.DataAccess.Attributes;
using LeaRun.Utilities;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LeaRun.Entity
{
    /// <summary>
    /// 用户管理
    /// <author>
    ///		<name>没有对应的数据库表，只是为了显示个人信息</name>
    ///		<date>2014.08.11 15:45</date>
    /// </author>
    /// </summary>

    public class TR_PersonalInfo : BaseEntity
    {

        public string UserCode { get; set; }
        public string UserName { get; set; }

        public string Department { get; set; }

        public string ExamName { get; set; }

        public string ExamTime { get; set; }

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public string Score { get; set; }

       
    }
}