using LeaRun.DataAccess.Attributes;
using LeaRun.Utilities;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LeaRun.Entity
{
    [Description("结构化表1")]
    [PrimaryKey("StructureID")]
    public class G_PLM_ProjectStructure_2 : BaseEntity
    {

        [DisplayName("主键")]
        public string StructureID { get; set; }
        public string StructureContent { get; set; }
        public string UID { get; set; }


        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.StructureID = CommonHelper.GetGuid;
        }

        public override void Modify(string KeyValue)
        {
            this.StructureID = KeyValue;

        }

        #endregion
    }
}
