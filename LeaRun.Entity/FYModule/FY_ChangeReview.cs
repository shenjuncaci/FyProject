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
    [Description("变更评审表")]
    [PrimaryKey("ChangeReviewID")]
    public class FY_ChangeReview : BaseEntity
    {

        [DisplayName("主键")]
        public string ChangeReviewID { get; set; }

        public string ReviewDepart { get; set; }


        public string ReviewBy { get; set; }
        public string ReviewContent { get; set; }
        public string Measures { get; set; }
        public DateTime? PlanDate { get; set; }
        public string ChangeID { get; set; }

        public string ChangeDataID { get; set; }
        public string ChangeReviewState { get; set; }

        //以下为虚拟属性,如果要实际使用这个entity的话，需要与数据库对应的话还要把下面这几个删掉
        public string RealName { get; set; }
        public string FullName { get; set; }
        public string Result { get; set; }
        public int IsEnd { get; set; }
        public string FllowStatus { get; set; }

        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.ChangeReviewID = CommonHelper.GetGuid;

            //this.CreateBy
        }

        public override void Modify(string KeyValue)
        {
            this.ChangeReviewID = KeyValue;

        }

        #endregion
    }
}