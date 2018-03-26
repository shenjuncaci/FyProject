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
    [Description("公共文件表")]
    [PrimaryKey("FileID")]
    public class PM_PublicFile : BaseEntity
    {

        [DisplayName("主键")]
        public string FileID { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string Icon { get; set; }
        public string FileType { get; set; }
        public string FileExtensions { get; set; }
        public string FileSize { get; set; }


        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.FileID = CommonHelper.GetGuid;

            //this.CreateBy
        }

        public override void Modify(string KeyValue)
        {
            this.FileID = KeyValue;

        }

        #endregion
    }
}
