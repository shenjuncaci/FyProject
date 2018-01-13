﻿using LeaRun.Business;
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

namespace LeaRun.WebApp.Areas.TCModule.Controllers
{
    public class AttachContractController : Controller
    {
        RepositoryFactory<TC_AttachContract> repositoryfactory = new RepositoryFactory<TC_AttachContract>();
        TC_AttachContractBll ContractBll = new TC_AttachContractBll();
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
                DataTable ListData = ContractBll.GetPageList(keywords, ref jqgridparam, ParameterJson);
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
        public ActionResult SubmitForm(string KeyValue, TC_AttachContract entity, 
            string BuildFormJson, HttpPostedFileBase Filedata,string DetailForm,string IsNew)
        {
            string ModuleId = DESEncrypt.Decrypt(CookieHelper.GetCookie("ModuleId"));
            IDatabase database = DataFactory.Database();
            DbTransaction isOpenTrans = database.BeginTrans();

            StringBuilder sql = new StringBuilder();

            try
            {
                string Message = KeyValue == "" ? "新增成功。" : "编辑成功。";
                if (!string.IsNullOrEmpty(KeyValue))
                {
                    if (IsNew == "1")
                    {
                        entity.Create();

                        //database.Delete<TC_AttachContractDetail>("ContractID", KeyValue, isOpenTrans);
                        List<TC_AttachContractDetail> DetailList = DetailForm.JonsToList<TC_AttachContractDetail>();
                        int index = 1;
                        decimal AttachNum = 0;
                        decimal AttachCost = 0;
                        decimal PlanCost = 0;
                        foreach (TC_AttachContractDetail entityD in DetailList)
                        {
                            if (!string.IsNullOrEmpty(entityD.InstallType))
                            {
                                AttachNum += entityD.AttachNum;
                                AttachCost += entityD.AttachCost;
                                PlanCost += entityD.PlanCost;

                                entityD.Create();
                                entityD.ContractID = entity.ContractID;
                                entityD.PlanCost = entityD.AttachNum*entityD.AttachCost;
                                entityD.SortNO = index;
                                database.Insert(entityD, isOpenTrans);
                                index++;
                            }
                        }
                        entity.AttachNum = AttachNum;
                        entity.AttachCost = AttachCost;
                        entity.PlanCost = PlanCost;
                        entity.EndCost = PlanCost * (1 + entity.LossRate);
                        entity.LaborCost1 = entity.Wages1 * entity.Employes1 / ZeroToOne(entity.AttendanceDays1) / ZeroToOne(entity.Shift1);
                        entity.LaborCost2 = entity.Wages2 * entity.Employes2 / ZeroToOne(entity.AttendanceDays2) / ZeroToOne(entity.Shift2);
                        entity.LaborCost3 = entity.Wages3 * entity.Employes3 / ZeroToOne(entity.AttendanceDays3) / ZeroToOne(entity.Shift3);
                        entity.LaborCostAll = entity.LaborCost1 + entity.LaborCost2 + entity.LaborCost3;
                        entity.Subsidy = entity.Areas * entity.Power;
                        entity.ManageExpense = (entity.LaborCostAll + entity.Subsidy) * (decimal)0.05;
                        entity.SingleEndCost = entity.EndCost + entity.LaborCostAll + entity.Subsidy + entity.ManageExpense;

                        database.Insert(entity, isOpenTrans);


                        
                        Base_DataScopePermissionBll.Instance.AddScopeDefault(ModuleId, ManageProvider.Provider.Current().UserId, entity.ContractID, isOpenTrans);
                    }
                    else
                    {
                        if (KeyValue == ManageProvider.Provider.Current().UserId)
                        {
                            throw new Exception("无权限编辑信息");
                        }


                        entity.Modify(KeyValue);

                        database.Delete<TC_AttachContractDetail>("ContractID", KeyValue, isOpenTrans);
                        List<TC_AttachContractDetail> DetailList = DetailForm.JonsToList<TC_AttachContractDetail>();
                        int index = 1;
                        decimal AttachNum = 0;
                        decimal AttachCost = 0;
                        decimal PlanCost = 0;
                        foreach (TC_AttachContractDetail entityD in DetailList)
                        {
                            if (!string.IsNullOrEmpty(entityD.InstallType))
                            {
                                AttachNum += entityD.AttachNum;
                                AttachCost += entityD.AttachCost;
                                PlanCost += entityD.PlanCost;

                                entityD.Create();
                                entityD.ContractID = KeyValue;
                                entityD.PlanCost = entityD.AttachNum * entityD.AttachCost;
                                entityD.SortNO = index;
                                database.Insert(entityD, isOpenTrans);
                                index++;
                            }
                        }
                        entity.AttachNum = AttachNum;
                        entity.AttachCost = AttachCost;
                        entity.PlanCost = PlanCost;

                        entity.EndCost = PlanCost * (1 + entity.LossRate);
                        entity.LaborCost1 = entity.Wages1 * entity.Employes1 / ZeroToOne(entity.AttendanceDays1) / ZeroToOne(entity.Shift1);
                        entity.LaborCost2 = entity.Wages2 * entity.Employes2 / ZeroToOne(entity.AttendanceDays2) / ZeroToOne(entity.Shift2);
                        entity.LaborCost3 = entity.Wages3 * entity.Employes3 / ZeroToOne(entity.AttendanceDays3) / ZeroToOne(entity.Shift3);

                        entity.LaborCostAll = entity.LaborCost1 + entity.LaborCost2 + entity.LaborCost3;
                        entity.Subsidy = entity.Areas * entity.Power;
                        entity.ManageExpense = (entity.LaborCostAll + entity.Subsidy) * (decimal)0.05;
                        entity.SingleEndCost = entity.EndCost + entity.LaborCostAll + entity.Subsidy + entity.ManageExpense;

                        database.Update(entity, isOpenTrans);

                    }

                }
                else
                {

                    entity.Create();

                    database.Delete<TC_AttachContractDetail>("ContractID", KeyValue, isOpenTrans);
                    List<TC_AttachContractDetail> DetailList = DetailForm.JonsToList<TC_AttachContractDetail>();
                    int index = 1;
                    decimal AttachNum = 0;
                    decimal AttachCost = 0;
                    decimal PlanCost = 0;
                    foreach (TC_AttachContractDetail entityD in DetailList)
                    {
                        if (!string.IsNullOrEmpty(entityD.InstallType))
                        {
                            AttachNum += entityD.AttachNum;
                            AttachCost += entityD.AttachCost;
                            PlanCost += entityD.PlanCost;

                            entityD.Create();
                            entityD.ContractID = entity.ContractID;
                            entityD.PlanCost = entityD.AttachNum * entityD.AttachCost;
                            entityD.SortNO = index;
                            database.Insert(entityD, isOpenTrans);
                            index++;
                        }
                    }
                    entity.AttachNum = AttachNum;
                    entity.AttachCost = AttachCost;
                    entity.PlanCost = PlanCost;
                    entity.EndCost = PlanCost * (1 + entity.LossRate);
                    entity.LaborCost1 = entity.Wages1 * entity.Employes1 / ZeroToOne(entity.AttendanceDays1) / ZeroToOne(entity.Shift1);
                    entity.LaborCost2 = entity.Wages2 * entity.Employes2 / ZeroToOne(entity.AttendanceDays2) / ZeroToOne(entity.Shift2);
                    entity.LaborCost3 = entity.Wages3 * entity.Employes3 / ZeroToOne(entity.AttendanceDays3) / ZeroToOne(entity.Shift3);
                    entity.LaborCostAll = entity.LaborCost1 + entity.LaborCost2 + entity.LaborCost3;
                    entity.Subsidy = entity.Areas * entity.Power;
                    entity.ManageExpense = (entity.LaborCostAll + entity.Subsidy) * (decimal)0.05;
                    entity.SingleEndCost = entity.EndCost + entity.LaborCostAll + entity.Subsidy + entity.ManageExpense;

                    database.Insert(entity, isOpenTrans);


                    


                    Base_DataScopePermissionBll.Instance.AddScopeDefault(ModuleId, ManageProvider.Provider.Current().UserId, entity.ContractID, isOpenTrans);
                }
                Base_FormAttributeBll.Instance.SaveBuildForm(BuildFormJson, entity.ContractID, ModuleId, isOpenTrans);
                database.Commit();
                
                if(IsNew=="1"&& !string.IsNullOrEmpty(KeyValue))
                {
                    sql.AppendFormat("update TC_AttachContract set Enable=0,NewContractID='{1}' where NewContractID='{0}'", KeyValue, entity.ContractID);
                    sql.AppendFormat("update TC_AttachContract set Enable=0,NewContractID='{1}' where ContractID='{0}'", KeyValue, entity.ContractID);

                    //database.ExecuteBySql(sql);
                    ContractBll.ExecuteSql(sql);
                }

                return Content(new JsonMessage { Success = true, Code = "1", Message = Message }.ToString());
            }
            catch (Exception ex)
            {
                database.Rollback();
                database.Close();
                return Content(new JsonMessage { Success = false, Code = "-1", Message = "操作失败：" + ex.Message }.ToString());
            }
        }

        public decimal ZeroToOne(decimal Num)
        {
            if(Num==0)
            {
                return 1;
            }
            else
            {
                return Num;
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SetForm(string KeyValue)
        {
            TC_AttachContract entity = DataFactory.Database().FindEntity<TC_AttachContract>(KeyValue);
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
            Base_SysLogBll.Instance.WriteLog<TC_AttachContract>(array, IsOk.ToString(), Message);
        }

        public ActionResult GetDetailList(string KeyValue)
        {
            try
            {
                var JsonData = new
                {
                    rows = ContractBll.GetDetailList(KeyValue),
                };
                return Content(JsonData.ToJson());
            }
            catch (Exception ex)
            {
                Base_SysLogBll.Instance.WriteLog("", OperationType.Query, "-1", "异常错误：" + ex.Message);
                return null;
            }
        }

        public ActionResult Uploadify()
        {
            return View();
        }


        public ActionResult SubmitUploadify(string FolderId, HttpPostedFileBase Filedata, string type)
        {
            try
            {

                Thread.Sleep(1000);////延迟500毫秒
                Base_NetworkFile entity = new Base_NetworkFile();
                TC_AttachContract PAentity = DataFactory.Database().FindEntity<TC_AttachContract>(FolderId);

                string IsOk = "";
                //没有文件上传，直接返回
                if (Filedata == null || string.IsNullOrEmpty(Filedata.FileName) || Filedata.ContentLength == 0)
                {
                    return HttpNotFound();
                }
                //获取文件完整文件名(包含绝对路径)
                //文件存放路径格式：/Resource/Document/NetworkDisk/{日期}/{guid}.{后缀名}
                //例如：/Resource/Document/Email/20130913/43CA215D947F8C1F1DDFCED383C4D706.jpg
                string fileGuid = CommonHelper.GetGuid;
                long filesize = Filedata.ContentLength;
                string FileEextension = Path.GetExtension(Filedata.FileName);
                string uploadDate = DateTime.Now.ToString("yyyyMMdd");
                //string UserId = ManageProvider.Provider.Current().UserId;

                string virtualPath = string.Format("~/Resource/Document/NetworkDisk/{0}/{1}/{2}{3}", "TransferContract", uploadDate, fileGuid, FileEextension);
                //rapidentity.res_msfj = virtualPath;

                string fullFileName = this.Server.MapPath(virtualPath);
                //创建文件夹，保存文件
                string path = Path.GetDirectoryName(fullFileName);
                Directory.CreateDirectory(path);
                if (!System.IO.File.Exists(fullFileName))
                {
                    Filedata.SaveAs(fullFileName);
                    try
                    {
                        PAentity.Attachment = virtualPath;


                        DataFactory.Database().Update<TC_AttachContract>(PAentity);
                    }
                    catch (Exception ex)
                    {
                        //IsOk = ex.Message;
                        //System.IO.File.Delete(virtualPath);
                    }
                }
                var JsonData = new
                {
                    Status = IsOk,
                    NetworkFile = PAentity,
                };
                return Content(JsonData.ToJson());


            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        public ActionResult HistoryList(string NewContractID)
        {
            string result = "";
            string sql = " select * from TC_AttachContract where NewContractID='"+NewContractID+ "' order by createdt ";
            DataTable dt = ContractBll.GetDataTable(sql);
            if(dt.Rows.Count>0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    result += dt.Rows[i]["ContractID"].ToString() + ","+dt.Rows[i]["CreateDt"].ToString()+"|";
                }
            }
            result = result.Substring(0, result.Length - 1);
            ViewData["Result"] = result;
            return View();
        }

        public ActionResult PrintPage(string KeyValue)
        {
            string sql = " select * from TC_AttachContract where contractid='"+KeyValue+"' ";
            DataTable dt = ContractBll.GetDataTable(sql);
            string sqlDetail = " select * from TC_AttachContractDetail where contractid='"+KeyValue+"' ";
            DataTable dtDetail = ContractBll.GetDataTable(sqlDetail);

            ViewData["dt"] = dt;
            ViewData["dtDetail"] = dtDetail;

            return View();
        }

    }
}
