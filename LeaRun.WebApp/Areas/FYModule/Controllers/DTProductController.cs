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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Net.Mail;
using System.Net;

namespace LeaRun.WebApp.Areas.FYModule.Controllers
{
    public class DTProductController : Controller
    {
        RepositoryFactory<FY_DTProduct> repositoryfactory = new RepositoryFactory<FY_DTProduct>();
        FY_DTProductBll ProductBll = new FY_DTProductBll();
        //
        // GET: /FYModule/Process/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GridPageListJson(string keywords, string CompanyId, string DepartmentId, JqGridParam jqgridparam, string ParameterJson)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = ProductBll.GetPageList(keywords, ref jqgridparam, ParameterJson);
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
            //ViewData["UserName"] = ManageProvider.Provider.Current().UserName;
            return View();
        }

        [HttpPost]
        public ActionResult SubmitForm(string KeyValue, FY_DTProduct entity, string BuildFormJson, HttpPostedFileBase Filedata)
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


                    entity.Modify(KeyValue);


                    database.Update(entity, isOpenTrans);

                }
                else
                {
                    //entity.DepartmentID = ManageProvider.Provider.Current().DepartmentId;
                    entity.Create();
                    entity.CreateDt = DateTime.Now;

                    database.Insert(entity, isOpenTrans);

                    Base_DataScopePermissionBll.Instance.AddScopeDefault(ModuleId, ManageProvider.Provider.Current().UserId, entity.ProductID, isOpenTrans);
                }
                Base_FormAttributeBll.Instance.SaveBuildForm(BuildFormJson, entity.ProductID, ModuleId, isOpenTrans);
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
        public ActionResult SetForm(string KeyValue)
        {
            FY_DTProduct entity = DataFactory.Database().FindEntity<FY_DTProduct>(KeyValue);
            if (entity == null)
            {
                return Content("");
            }

            string strJson = entity.ToJson();

            strJson = strJson.Insert(1, Base_FormAttributeBll.Instance.GetBuildForm(KeyValue));
            return Content(strJson);
        }

        public ActionResult CodeGen()
        {
            return View();
        }

        public string GetCode(string OriginCode)
        {
            string sqlUpdate = "";
            string result = "";
            int currentNO = 0;

            string zeroNO = "";

            string[] temp;
            temp = OriginCode.Split('@');


            string sql = " select PartNO from FY_DTProduct where ProductNO='{0}' ";
            sql = string.Format(sql, temp[1]);
            DataTable dt = ProductBll.GetDataTable(sql);
            if(dt.Rows.Count>0)
            {
                result += dt.Rows[0][0].ToString();


                result = result + " EXP S" + DateTime.Now.AddMonths(3).ToString("yyyyMMdd")+"";

                //第一步
                string sql1 = " select NowNumber from FY_Serialno where cast(today as date)=cast(getdate() as date)  ";

                DataTable dt1 = ProductBll.GetDataTable(sql1);
                if(dt1.Rows.Count>0)
                {
                    currentNO = Convert.ToInt32(dt1.Rows[0][0].ToString())+1;
                    sqlUpdate = " update FY_Serialno set NowNumber=NowNumber+1 where cast(today as date)=cast(getdate() as date) ";
                }
                else
                {
                    sqlUpdate = " insert into FY_Serialno values (GETDATE(),0) ";
                }
                StringBuilder strsql = new StringBuilder();
                strsql = strsql.Append(sqlUpdate);
                ProductBll.ExecuteSql(strsql);

                int aa = 0;
                if (currentNO==0)
                {
                    aa = 0;
                }
                else
                {
                    aa = currentNO.ToString().Length;

                }
                 
                if(aa<4)
                {
                    int temp1 = 4 - aa;
                    for(int i=0;i<temp1;i++)
                    {
                        zeroNO = zeroNO + "0";
                    }
                }

                result = result + zeroNO + currentNO.ToString();

            }

            return result;
        }




    }
}
