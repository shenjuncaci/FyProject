using LeaRun.DataAccess.Attributes;
using LeaRun.Utilities;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LeaRun.Entity
{
    [Description("底涂产品")]
    [PrimaryKey("ProductID")]
    public class FY_DTProduct:BaseEntity
    {
        [DisplayName("主键")]
        public string ProductID { get; set; }
        public string ProductNO { get; set; }
        public string BracketNO { get; set; }
        public DateTime? CreateDt { get; set; }
        public string PartNO { get; set; }

        public override void Create()
        {
            this.ProductID = CommonHelper.GetGuid;

            //this.CreateBy
        }

        public override void Modify(string KeyValue)
        {
            this.ProductID = KeyValue;

        }
    }
}
