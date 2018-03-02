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
    public class RoomController : Controller
    {
        RepositoryFactory<DM_Room> repositoryfactory = new RepositoryFactory<DM_Room>();
        RepositoryFactory<DM_Assets> detailrepository = new RepositoryFactory<DM_Assets>();
        DM_RoomBll RoomBll = new DM_RoomBll();
        //

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GridPageListJson(string keywords, string CompanyId, string DepartmentId, 
            JqGridParam jqgridparam, string ParameterJson,string IsEmpty)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = RoomBll.GetPageList(keywords, ref jqgridparam, ParameterJson,IsEmpty);
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

        public ActionResult GetAssetsList(string KeyValue)
        {
            try
            {
                var JsonData = new
                {
                    rows = RoomBll.GetAssetsList(KeyValue),
                };
                return Content(JsonData.ToJson());
            }
            catch (Exception ex)
            {
                Base_SysLogBll.Instance.WriteLog("", OperationType.Query, "-1", "异常错误：" + ex.Message);
                return null;
            }
        }

        [HttpPost]
        public ActionResult SubmitForm(string KeyValue, DM_Room entity, string BuildFormJson, 
            HttpPostedFileBase Filedata,string DetailForm)
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

                    int index = 1;
                    List<DM_Assets> DetailList = DetailForm.JonsToList<DM_Assets>();
                    foreach (DM_Assets entityD in DetailList)
                    {
                        if (!string.IsNullOrEmpty(entityD.AssetName))
                        {
                            if (!string.IsNullOrEmpty(entityD.AssetID))
                            {
                                entityD.SortNO = index;
                                entityD.Modify(entityD.AssetID);
                                
                                database.Update(entityD, isOpenTrans);
                                index++;
                            }
                            else
                            {
                                entityD.SortNO = index;
                                entityD.Create();
                                entityD.RoomID = entity.RoomID;

                                database.Insert(entityD, isOpenTrans);
                                index++;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(entityD.AssetID))
                            {
                                detailrepository.Repository().Delete(entityD.AssetID);

                            }
                        }
                    }

                    database.Update(entity, isOpenTrans);

                }
                else
                {

                    entity.Create();
                    int index = 1;
                    List<DM_Assets> DetailList = DetailForm.JonsToList<DM_Assets>();
                    foreach (DM_Assets entityD in DetailList)
                    {
                        if (!string.IsNullOrEmpty(entityD.AssetName))
                        {
                            if (!string.IsNullOrEmpty(entityD.AssetID))
                            {
                                entityD.SortNO = index;
                                entityD.Modify(entityD.AssetID);

                                database.Update(entityD, isOpenTrans);
                                index++;
                            }
                            else
                            {
                                entityD.SortNO = index;
                                entityD.Create();
                                entityD.RoomID = entity.RoomID;

                                database.Insert(entityD, isOpenTrans);
                                index++;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(entityD.AssetID))
                            {
                                detailrepository.Repository().Delete(entityD.AssetID);

                            }
                        }
                    }

                    database.Insert(entity, isOpenTrans);

                    Base_DataScopePermissionBll.Instance.AddScopeDefault(ModuleId, ManageProvider.Provider.Current().UserId, entity.RoomID, isOpenTrans);
                }
                Base_FormAttributeBll.Instance.SaveBuildForm(BuildFormJson, entity.RoomID, ModuleId, isOpenTrans);
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
            DM_Room entity = DataFactory.Database().FindEntity<DM_Room>(KeyValue);
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
            Base_SysLogBll.Instance.WriteLog<DM_Room>(array, IsOk.ToString(), Message);
        }

        public ActionResult DormList()
        {
            string sql = " select DormID,DormName from dm_dorm ";
            DataTable dt = RoomBll.GetDataTable(sql);
            return Content(dt.ToJson());
        }


        public ActionResult CheckInForm()
        {
            return View();
        }

        

        public ActionResult GetList(string KeyValue)
        {
            try
            {
                var JsonData = new
                {
                    rows = RoomBll.GetDetailList(KeyValue),
                };
                return Content(JsonData.ToJson());
            }
            catch (Exception ex)
            {
                Base_SysLogBll.Instance.WriteLog("", OperationType.Query, "-1", "异常错误：" + ex.Message);
                return null;
            }
        }
        [HttpPost]
        public ActionResult SubmitCheckInForm(string KeyValue,string CurrentForm)
        {
            string Message = KeyValue == "" ? "新增成功。" : "编辑成功。";
            IDatabase database = DataFactory.Database();
            DbTransaction isOpenTrans = database.BeginTrans();

            List<DM_CheckIn> CheckInList = CurrentForm.JonsToList<DM_CheckIn>();
            int index = 1;
            foreach (DM_CheckIn entityD in CheckInList)
            {
                if (!string.IsNullOrEmpty(entityD.PersonCode))
                {
                    if (string.IsNullOrEmpty(entityD.CheckInID))
                    {
                        entityD.Create();
                        entityD.RoomID = KeyValue;
                        database.Insert(entityD, isOpenTrans);
                        index++;
                    }
                    else
                    {
                        entityD.Modify(entityD.CheckInID);
                        entityD.RoomID = KeyValue;
                        database.Update(entityD, isOpenTrans);
                        index++;
                    }

                }
            }
            database.Commit();
            return Content(new JsonMessage { Success = true, Code = "1", Message = Message }.ToString());
        }

        public ActionResult CheckOut(string CheckInID,string Date)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat("update DM_CheckIn set IsLeave=1,checkoutdate='{1}' where CheckInID='{0}' ", CheckInID,Date);
            RoomBll.ExecuteSql(strSql);
            return Content(new JsonMessage { Success = true, Code = "1", Message = "退宿成功" }.ToString());

        }

        public ActionResult AssetsForm()
        {
            return View();
        }

        public ActionResult ChooseDate()
        {
            return View();
        }








    }
}
