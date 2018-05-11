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

namespace LeaRun.WebApp.Areas.DormModule.Controllers
{
    public class RoomCostController : Controller
    {
        RepositoryFactory<DM_RoomCost> repositoryfactory = new RepositoryFactory<DM_RoomCost>();
        DM_RoomCostBll RoomCostBll = new DM_RoomCostBll();
        //

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GridPageListJson(string keywords, string CompanyId, string DepartmentId, JqGridParam jqgridparam, string ParameterJson)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = RoomCostBll.GetPageList(keywords, ref jqgridparam, ParameterJson);
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
        public ActionResult SubmitForm(string KeyValue, DM_RoomCost entity, string BuildFormJson, HttpPostedFileBase Filedata)
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
                    entity.ElectricPriceAll = entity.ElectricPrice * (entity.NowElectric - entity.PreElectric);
                    entity.WaterPriceAll = entity.WaterPrice * (entity.NowWater - entity.PreWater);

                    database.Update(entity, isOpenTrans);

                }
                else
                {

                    entity.Create();

                    entity.ElectricPriceAll = entity.ElectricPrice * (entity.NowElectric - entity.PreElectric);
                    entity.WaterPriceAll = entity.WaterPrice * (entity.NowWater - entity.PreWater);


                    database.Insert(entity, isOpenTrans);

                    Base_DataScopePermissionBll.Instance.AddScopeDefault(ModuleId, ManageProvider.Provider.Current().UserId, entity.RoomCostID, isOpenTrans);
                }
                Base_FormAttributeBll.Instance.SaveBuildForm(BuildFormJson, entity.RoomCostID, ModuleId, isOpenTrans);
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
            DM_RoomCost entity = DataFactory.Database().FindEntity<DM_RoomCost>(KeyValue);
            if (entity == null)
            {
                return Content("");
            }

            string strJson = entity.ToJson();

            strJson = strJson.Insert(1, Base_FormAttributeBll.Instance.GetBuildForm(KeyValue));
            return Content(strJson);
        }


        [HttpPost]
        public ActionResult Delete(string KeyValue)
        {
            try
            {
                var Message = "删除失败。";
                int IsOk = 0;

                IsOk = repositoryfactory.Repository().Delete(KeyValue);
                if (IsOk > 0)
                {
                    Message = "删除成功。";
                }

                WriteLog(IsOk, KeyValue, Message);
                return Content(new JsonMessage { Success = true, Code = IsOk.ToString(), Message = Message }.ToString());
            }
            catch (Exception ex)
            {
                WriteLog(-1, KeyValue, "操作失败：" + ex.Message);
                return Content(new JsonMessage { Success = false, Code = "-1", Message = "操作失败：" + ex.Message }.ToString());
            }
        }

        public void WriteLog(int IsOk, string KeyValue, string Message = "")
        {
            string[] array = KeyValue.Split(',');
            Base_SysLogBll.Instance.WriteLog<DM_RoomCost>(array, IsOk.ToString(), Message);
        }

        //核算费用到每个人
        public ActionResult RoomCostEveryOne()
        {
            return View();
        }

        public ActionResult GetRoomCostJson(string keywords, JqGridParam jqgridparam)
        {
            try
            {
                keywords = keywords.Replace("-0", "-");
                if (keywords == null || keywords == "undefined" || keywords == "")
                {
                    keywords = DateTime.Now.Year.ToString()+"-"+DateTime.Now.Month.ToString();
                }
                
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = RoomCostBll.GetRoomCostJson(keywords);
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

        public void RoomCostExcelExport(string keywords)
        {
            if (keywords == null || keywords == "undefined" || keywords == "")
            {
                keywords = DateTime.Now.Month.ToString() + "-" + DateTime.Now.Month.ToString();
            }

            keywords = keywords.Replace("-0", "-");
            DataTable ListData = RoomCostBll.GetRoomCostJson(keywords);
            ExcelHelper ex = new ExcelHelper();
            ex.EcportExcel(ListData, "个人房费导出");
        }





    }
}
