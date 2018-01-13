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
    ///		<name>shenjun</name>
    ///		<date>2017.09.07 15:45</date>
    /// </author>
    /// </summary>
    [Description("竞争对手表")]
    [PrimaryKey("CompetitorID")]
    public class FY_Competitor : BaseEntity
    {

        [DisplayName("主键")]
        public string CompetitorID { get; set; }

        public string CompetitorName { get; set; }
        public DateTime? Createdt { get; set; }

        public string CreateBy { get; set; }
        public DateTime? Modifydt { get; set; }
        public string ModifyBy { get; set; }





        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.CompetitorID = CommonHelper.GetGuid;

            this.CreateBy = ManageProvider.Provider.Current().UserId;
            this.Createdt = DateTime.Now;
        }

        public override void Modify(string KeyValue)
        {
            this.CompetitorID = KeyValue;

            this.ModifyBy = ManageProvider.Provider.Current().UserId;
            this.Modifydt = DateTime.Now;

        }

        #endregion
    }
}