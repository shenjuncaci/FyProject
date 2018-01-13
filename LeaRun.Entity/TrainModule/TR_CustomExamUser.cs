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
    /// 自定义考试人员表
    /// <author>
    ///		<name>she</name>
    ///		<date>2014.08.11 15:45</date>
    /// </author>
    /// </summary>
    [Description("自定义考试人员表")]
    [PrimaryKey("CustomExamUserID")]
    public class TR_CustomExamUser : BaseEntity
    {

        [DisplayName("主键")]
        public string CustomExamUserID { get; set; }

        public string CustomExamID { get; set; }

        public string UserID { get; set; }

       

        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.CustomExamUserID = CommonHelper.GetGuid;
            
           
        }


        

        public override void Modify(string KeyValue)
        {
            this.CustomExamUserID = KeyValue;


        }

        #endregion
    }
}