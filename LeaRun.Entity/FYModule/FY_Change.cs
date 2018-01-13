//=====================================================================================
// created by shenjun 20170803
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
    [Description("变更系统主表")]
    [PrimaryKey("ChangeID")]
    public class FY_Change : BaseEntity
    {

        [DisplayName("主键")]
        public string ChangeID { get; set; }

        public string ChangeNO { get; set; }
        public string ChangeSource { get; set; }
        public string ProjectName { get; set; }
        public string ChangeType { get; set; }
        public string RelationFactory { get; set; }
        public string ProduceStage { get; set; }
        public string TransitDepot { get; set; }
        public string CreateBy { get; set; }
        public string CreateByID { get; set; }
        public string CreateDept { get; set; }
        public DateTime? CreateDt{get;set;}
        public string ChangeReson { get; set; }
        public string ChangeBasis { get; set; }
        public string BeforePicture { get; set; }
        public string AfterPicture { get; set; }
        public string BeforeContent { get; set; }
        public string AfterContent { get; set; }
        public DateTime? ChangeStartDt { get; set; }
        public DateTime? ChangePutDt { get; set; }
        public DateTime? NewProduceDt { get; set; }
        public DateTime? BreakPointDt { get; set; }
        public string OldProductState { get; set; }
        public DateTime? TermDt { get; set; }
        public string ComponentNO { get; set; }

        public string CarType { get; set; }
        public string ChangeState { get; set; }

        public string TopManagerRemark { get; set; }
        public DateTime? ApproveDt { get; set; }
        public string ApproveBy { get; set; }
        public string ScrappeGlass { get; set; }
        public string ScrappeMaterial { get; set; }
        public string ScrappeAll { get; set; }
        public string FiRemark { get; set; }
        public string FiBy { get; set; }
        public DateTime? FyDt { get; set; }

        public string CreateRemark { get; set; }
        public string CreateState { get; set; }
        public string ManagerRemark { get; set; }
        public DateTime? ManagerDt { get; set; }
        public string Manager { get; set; }
        public string IsChangeOver { get; set; }
        public string TopManager1Remark { get; set; }
        public string Approve1By { get; set; }
        public DateTime? Approve1Dt { get; set; }
        public string ProjectLeaderStatus { get; set; }
        public string CreateOpinion { get; set; }

        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.ChangeID = CommonHelper.GetGuid;
            this.CreateByID = ManageProvider.Provider.Current().UserId;
            this.CreateBy= ManageProvider.Provider.Current().UserName;
            this.CreateDept = ManageProvider.Provider.Current().DepartmentId;
            this.CreateDt = DateTime.Now;
            this.ChangeState = "等待项目主管评审";
            //目前下面这个状态可能用不到
            this.ProjectLeaderStatus = "等待项目主管评审";
            //this.CreateBy
        }

        public override void Modify(string KeyValue)
        {
            this.ChangeID = KeyValue;

        }

        #endregion
    }
}