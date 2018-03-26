//=====================================================================================
// created by shenjun 20180315
//=====================================================================================

using LeaRun.DataAccess.Attributes;
using LeaRun.Utilities;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LeaRun.Entity
{
    [Description("项目管理通知表")]
    [PrimaryKey("NoticeID")]
    public class PM_Notice : BaseEntity
    {

        [DisplayName("主键")]
        public string NoticeID { get; set; }

        public string NoticeContent { get; set; }

        public string ProjectID { get; set; }

        public string CreateBy { get; set; }

        public DateTime? CreateDate { get; set; }


        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.NoticeID = CommonHelper.GetGuid;
            this.CreateBy = ManageProvider.Provider.Current().UserId;
            this.CreateDate = DateTime.Now;
            //this.CreateBy
        }

        public override void Modify(string KeyValue)
        {
            this.NoticeID = KeyValue;

        }

        #endregion
    }
}
