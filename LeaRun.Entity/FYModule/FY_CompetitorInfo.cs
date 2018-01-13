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
    ///		<name>she</name>
    ///		<date>2014.08.11 15:45</date>
    /// </author>
    /// </summary>
    [Description("竞争对手明细表")]
    [PrimaryKey("CompetitorInfoID")]
    public class FY_CompetitorInfo : BaseEntity
    {

        [DisplayName("主键")]
        public string CompetitorInfoID { get; set; }
        public string CompetitorID { get; set; }
        public string CusID { get; set; }
        public string MessageType { get; set; }
        public string Message { get; set; }
        public DateTime? CreateDt { get; set; }

        public string CreateBy { get; set; }
        public DateTime? ModifyDt { get; set; }
        public string ModifyBy { get; set; }





        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.CompetitorInfoID = CommonHelper.GetGuid;

            this.CreateBy = ManageProvider.Provider.Current().UserId;
            this.CreateDt = DateTime.Now;
        }

        public override void Modify(string KeyValue)
        {
            this.CompetitorInfoID = KeyValue;

            this.ModifyBy = ManageProvider.Provider.Current().UserId;
            this.ModifyDt = DateTime.Now;

        }

        #endregion
    }
}