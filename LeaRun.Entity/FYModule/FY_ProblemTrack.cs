﻿//=====================================================================================
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
    ///		<name>she</name>
    ///		<date>2014.08.11 15:45</date>
    /// </author>
    /// </summary>
    [Description("问题跟踪主表")]
    [PrimaryKey("ProblemID")]
    public class FY_ProblemTrack : BaseEntity
    {

        [DisplayName("主键")]
        public string ProblemID { get; set; }

        public string ProblemDescripe { get; set; }

        public string ProblemType { get; set; }

        public string CreateBy { get; set; }
        public DateTime? CreateDt { get; set; }
        public string Remark { get; set; }
        public string ResponseBy { get; set; }

        public string Status { get; set; }
        public string AgentBy { get; set; }
        public string ImprovementMeasures { get; set; }

        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.ProblemID = CommonHelper.GetGuid;
            this.CreateDt = DateTime.Now;
            this.CreateBy = ManageProvider.Provider.Current().UserId;
            this.Status = "进行中";
            //this.CreateBy
        }

        public override void Modify(string KeyValue)
        {
            this.ProblemID = KeyValue;

        }

        #endregion
    }
}