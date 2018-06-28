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
    [Description("QOS主表")]
    [PrimaryKey("IndexID")]
    public class VP_QOSData : BaseEntity
    {

        [DisplayName("主键")]
        public string IndexID { get; set; }
        public string IndexName { get; set; }
        public string DataProvider { get; set; }
        public string DataAnalyer { get; set; }
        public int IndexYear { get; set; }
        public decimal LastYearData { get; set; }
        public int ControlDirection { get; set; }
        public decimal ThisYearTarget { get; set; }
        public decimal MonthOne { get; set; }
        public decimal MonthTwo { get; set; }
        public decimal MonthThree { get; set; }
        public decimal MonthFour { get; set; }
        public decimal MonthFive { get; set; }
        public decimal MonthSix { get; set; }
        public decimal MonthSeven { get; set; }
        public decimal MonthEight { get; set; }
        public decimal MonthNine { get; set; }
        public decimal MonthTen { get; set; }
        public decimal MonthEleven { get; set; }
        public decimal MonthTwoleve { get; set; }
        public string Formula { get; set; }
        public string InvolveProcess { get; set; }


        #region 扩展操作
        /// <summary>
        /// 新增调用
        /// </summary>
        public override void Create()
        {
            this.IndexID = CommonHelper.GetGuid;

            //this.CreateBy
        }

        public override void Modify(string KeyValue)
        {
            this.IndexID = KeyValue;


        }

        #endregion
    }
}