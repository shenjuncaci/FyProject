using LeaRun.Business;
using LeaRun.DataAccess;
using LeaRun.Entity;
using LeaRun.Repository;
using LeaRun.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace LeaRun.WebApp.Areas.ExampleModule.Controllers
{
    public class TestTableController : Controller
    {

        TestTableBll testtablebll = new TestTableBll();
        //
        // GET: /ExampleModule/Test/

        public ActionResult Index()
        {
            return View();
        }


        public ActionResult GridPageListJson(string keywords, string CompanyId, string DepartmentId, JqGridParam jqgridparam)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = testtablebll.GetPageList(keywords,ref jqgridparam);
                var JsonData = new
                {
                    total = jqgridparam.total,
                    page = jqgridparam.page,
                    records = jqgridparam.records,
                    costtime = CommonHelper.TimerEnd(watch),
                    rows = ListData,
                };
                return Content(JsonData.ToJson());
            }
            catch (Exception ex)
            {
                Base_SysLogBll.Instance.WriteLog("", OperationType.Query, "-1", "异常错误：" + ex.Message);
                return null;
            }
        }

        public ActionResult Form()
        {
            return View();
        }


        [HttpPost]
        public ActionResult SubmitTestTableForm(string KeyValue, TestTable testtable, string BuildFormJson)
        {
            string ModuleId = DESEncrypt.Decrypt(CookieHelper.GetCookie("ModuleId"));
            IDatabase database = DataFactory.Database();
            DbTransaction isOpenTrans = database.BeginTrans();
            try
            {
                string Message = KeyValue == "" ? "新增成功。" : "编辑成功。";
                if (!string.IsNullOrEmpty(KeyValue))
                {
                    if (KeyValue == ManageProvider.Provider.Current().UserId)
                    {
                        throw new Exception("无权限编辑信息");
                    }
                    //base_user.Modify(KeyValue);
                    testtable.Modify(KeyValue);
                    database.Update(testtable, isOpenTrans);
                    
                }
                else
                {
                    testtable.Create();

                    database.Insert(testtable, isOpenTrans);
                    //database.Insert(base_employee, isOpenTrans);
                    Base_DataScopePermissionBll.Instance.AddScopeDefault(ModuleId, ManageProvider.Provider.Current().UserId, testtable.TestId, isOpenTrans);
                }
                Base_FormAttributeBll.Instance.SaveBuildForm(BuildFormJson, testtable.TestId, ModuleId, isOpenTrans);
                database.Commit();
                return Content(new JsonMessage { Success = true, Code = "1", Message = Message }.ToString());
            }
            catch (Exception ex)
            {
                database.Rollback();
                database.Close();
                return Content(new JsonMessage { Success = false, Code = "-1", Message = "操作失败：" + ex.Message }.ToString());
            }
        }


        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SetTestForm(string KeyValue)
        {
            TestTable base_user = DataFactory.Database().FindEntity<TestTable>(KeyValue);
            if (base_user == null)
            {
                return Content("");
            }
            //Base_Employee base_employee = DataFactory.Database().FindEntity<Base_Employee>(KeyValue);
            //Base_Company base_company = DataFactory.Database().FindEntity<Base_Company>(base_user.CompanyId);
            string strJson = base_user.ToJson();
            //公司
            //strJson = strJson.Insert(1, "\"CompanyName\":\"" + base_company.FullName + "\",");
            //员工信息
            //strJson = strJson.Insert(1, base_employee.ToJson().Replace("{", "").Replace("}", "") + ",");
            //自定义
            strJson = strJson.Insert(1, Base_FormAttributeBll.Instance.GetBuildForm(KeyValue));
            return Content(strJson);
        }


        public ActionResult Flexgrid()
        {
            return View();
        }

        public JsonResult GetGridList3(string query, string qtype, string sortname, string sortorder, string page, string rp, string condition, string infoPatch)
        {
            //ARTEA.BAM.BLL.IUserAgent userAgent = ARTEA.BAM.BLL.TopMaster.Instance.GetUserAgent();
            //ARTEA.BAM.DTO.UserDTO currentUser = userAgent.CurrentUser;
            //string sql;
            //sql = "exec Report_ManufactureList " + currentUser.Comp.ToString();

            //_sqlManager.GetDataSetBySQL(sql);
            //string[] sqlPrd = new string[2];
            //if (condition == "undefined")
            //{
            //    sqlPrd[0] = " select distinct * from ##ManufactureList where 1=1";
            //    sqlPrd[1] = " select Count(*) from ##ManufactureList where 1=1";
            //}
            //else
            //{
            //    sqlPrd[0] = " select distinct * from ##ManufactureList where 1=1" + condition;
            //    sqlPrd[1] = " select count(*) from ##ManufactureList where 1=1" + condition;


            //}
            
            //var dt = _sqlManager.GetDataSetBySQL(sql).Tables[0];
            page = "1";
            rp = "15";
            sortname = "ID";
            sortorder = "desc";

            
            DataTable dt=new DataTable();
            int recordCount = 0;
            recordCount = 1;

            var retVal = new BuildGridJson<string, int>(
                dt,
                Convert.ToInt32(page),
                Convert.ToInt32(rp),
                recordCount,
                entity => "11",
                entity => "22",
                entity => "33",
                entity => "44",
                entity => "55"
                
                ).Build();
            return Json(retVal, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Gantee()
        {
            return View();
        }

        public ActionResult PuJiaGantee()
        {
            return View();
        }

        public ActionResult taskTree()
        {
            List<GantJsonData> listnew = new List<GantJsonData>();
            GantJsonData test = new GantJsonData();
            GantJsonDataChildren testd = new GantJsonDataChildren();
            GantJsonDataChildren testd2 = new GantJsonDataChildren();
            List<GantJsonDataChildren> list = new List<GantJsonDataChildren>();

            PredecessorLink pretest = new PredecessorLink();
            List<PredecessorLink> pretestlist = new List<PredecessorLink>();
            
            test.UID = "1";
            test.Name = "项目范围规划";
            test.Duration = 8;
            test.Start = Convert.ToDateTime("2018-7-1");
            test.Finish= Convert.ToDateTime("2018-7-10");
            test.PercentComplete = 0;
            test.Summary = 1;
            test.Critical = 0;
            test.Mileston = 0;

            testd2.UID = "3";
            testd2.Name = "子项测试";
            testd2.Duration = 8;
            testd2.Start = Convert.ToDateTime("2018-7-5");
            testd2.Finish = Convert.ToDateTime("2018-7-10");
            testd2.PercentComplete = 0;
            testd2.Summary = 1;
            testd2.Critical = 0;
            testd2.Mileston = 0;
            

            testd.UID = "2";
            testd.Name = "子项测试";
            testd.Duration = 8;
            testd.Start = Convert.ToDateTime("2018-7-1");
            testd.Finish = Convert.ToDateTime("2018-7-5");
            testd.PercentComplete = 0;
            testd.Summary = 1;
            testd.Critical = 0;
            testd.Mileston = 0;

            pretest.Type = 1;
            pretest.PredecessorUID = "2";
            pretestlist.Add(pretest);
            testd2.PredecessorLink = pretestlist;

            list.Add(testd);
            list.Add(testd2);
            test.children = list;

            listnew.Add(test);

            return Content(listnew.ToJson());
        }


        public class GantJsonData
        {
            public string UID { get; set; }
            public string Name { get; set; }
            public int Duration { get; set; }
            public DateTime? Start { get; set; }
            public DateTime? Finish { get; set; }
            public int PercentComplete { get; set; }
            public int Summary { get; set; }
            public int Critical { get; set; }
            public int Mileston { get; set; }
            public List<GantJsonDataChildren> children { get; set; }

            public List<PredecessorLink> PredecessorLink { get; set; }

        }

        public class GantJsonDataChildren
        {
            public string UID { get; set; }
            public string Name { get; set; }
            public int Duration { get; set; }
            public DateTime? Start { get; set; }
            public DateTime? Finish { get; set; }
            public int PercentComplete { get; set; }
            public int Summary { get; set; }
            public int Critical { get; set; }
            public int Mileston { get; set; }
            public List<PredecessorLink> PredecessorLink { get; set; }
        }

        public class PredecessorLink
        {
            public int Type { get; set; }
            public string PredecessorUID { get; set; }
        }




        public class GantJsonDataNew
        {
            public string UID { get; set; }
            public string Name { get; set; }
            public int Duration { get; set; }
            public DateTime? Start { get; set; }
            public DateTime? Finish { get; set; }
            public int PercentComplete { get; set; }
            public int Summary { get; set; }
            public int Critical { get; set; }
            public int Mileston { get; set; }
            public string Items { get; set; }
            public List<GantJsonDataNew> children { get; set; }

            public List<PredecessorLink> PredecessorLink { get; set; }

        }


        public ActionResult GetPujiaGantedataNew()
        {
            string sql1 = @"select AutoID as UID,YmSoftName as Name,PlanContinueHour as Duration,PlanStartRq as Start,
PlanEndRq as Finish,0 as PercentComplete,1 as Summary,0 as Critical,
0 as Milestone,Items
 from genyeedata.[dbo].[G_PLM_ProjectSchedule]
 where sLevel=0 and AutoID=1";
            DataTable dt1 = testtablebll.GetDataTable(sql1);
            if (dt1.Rows.Count > 0)
            {
                
                string sql2 = "";
                List<GantJsonDataNew> list1 = DtConvertHelper.ConvertToModelListNew<GantJsonDataNew>(dt1);
                foreach(GantJsonDataNew gjd in list1)
                {
                    sql2 = @"select AutoID as UID,YmSoftName as Name,PlanContinueHour as Duration,PlanStartRq as Start,
PlanEndRq as Finish,0 as PercentComplete,1 as Summary,0 as Critical,
0 as Milestone,Items
 from genyeedata.[dbo].[G_PLM_ProjectSchedule]
 where sLevel=1 order by PlanStartRq ";
                    DataTable dt2= testtablebll.GetDataTable(sql2);
                    List<GantJsonDataNew> list2 = DtConvertHelper.ConvertToModelListNew<GantJsonDataNew>(dt2);
                    PredecessorLink pretest = new PredecessorLink();
                    List<PredecessorLink> prelist = new List<PredecessorLink>();

                    pretest.Type = 1;
                   // pretest.PredecessorUID = gjd.UID;
                    prelist.Add(pretest);
                    string temp = "";
                    //foreach (GantJsonDataNew gjd2 in list2)
                    //{
                    //    if(temp!="")
                    //    {
                    //        pretest.PredecessorUID = temp;
                    //    }
                    //    prelist.Clear();
                    //    prelist.Add(pretest);
                    //    temp = gjd2.UID;
                    //    gjd2.PredecessorLink = prelist;
                    //}
                    //for(int i=0;i<list2.Count;i++)
                    //{
                    //    if (temp != "")
                    //    {
                    //        pretest.PredecessorUID = temp;
                    //    }
                    //    list2[i].PredecessorLink = new List<PredecessorLink>();
                    //    list2[i].PredecessorLink.Add(pretest);
                    //    temp = list2[i].UID;
                    //    //if (temp!="")
                    //    //{
                    //    //    list2[i].PredecessorLink[0].PredecessorUID = temp;

                    //    //}

                    //    //if(i!=0)
                    //    //{
                    //    //    list2[i].PredecessorLink[0].PredecessorUID = list2[i-1].UID;
                    //    //}
                    //}
                    pretest.PredecessorUID = "13";
                    list2[2].PredecessorLink = new List<PredecessorLink>();
                    list2[2].PredecessorLink.Add(pretest);
                    pretest.PredecessorUID = "44";
                    list2[3].PredecessorLink = new List<PredecessorLink>();
                    list2[3].PredecessorLink.Add(pretest);

                    string sql3 = "";
                    for (int i = 0; i < list2.Count; i++)
                    {
                        sql3 = @" select AutoID as UID,YmSoftName as Name,PlanContinueHour as Duration,PlanStartRq as Start,
PlanEndRq as Finish,0 as PercentComplete,1 as Summary,0 as Critical,
0 as Milestone,Items
 from  genyeedata.[dbo].[G_PLM_ProjectSchedule]
 where sLevel=2 and Items like '%" + list2[i].Items + "' order by PlanStartRq  ";
                        DataTable dt3= testtablebll.GetDataTable(sql3);
                        List<GantJsonDataNew> list3 = DtConvertHelper.ConvertToModelListNew<GantJsonDataNew>(dt3);
                        list2[i].children = list3;
                    }


                    gjd.children = list2;
                }
                return Content(list1.ToJson());
            }
            else
            {
                return Content("出错了");
            }





            
        }




    }
}
