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

namespace LeaRun.WebApp.Areas.Thirdparty.Controllers
{
    public class OutStorageController : Controller
    {
        Repository<ST_OutStorage> re = new Repository<ST_OutStorage>();
        Repository<ST_InOutDetail> red = new Repository<ST_InOutDetail>();
        Repository<ST_DetailDisplay> redisplay = new Repository<ST_DetailDisplay>();
        //

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GridPageListJson(string keywords, string CompanyId, string DepartmentId, JqGridParam jqgridparam, string ParameterJson, string keyword)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                StringBuilder strSql = new StringBuilder();
                List<DbParameter> parameter = new List<DbParameter>();
                strSql.Append(@" select OutID,b.ProductName,b.ProductUnit,a.OutNum,a.remark,a.createdate from ST_OutStorage a left join st_products b on a.productid=b.productid where 1=1  ");
                if (!string.IsNullOrEmpty(keyword))
                {
                    strSql.Append(@" AND (ProductName LIKE @keyword
                                    )");
                    parameter.Add(DbFactory.CreateDbParameter("@keyword", '%' + keyword + '%'));
                }
                if (!string.IsNullOrEmpty(ParameterJson) && ParameterJson.Length > 2)
                {
                    strSql.Append(ConditionBuilder.GetWhereSql(ParameterJson.JonsToList<Condition>(), out parameter));
                }
                DataTable ListData = re.FindTablePageBySql(strSql.ToString(), parameter.ToArray(), ref jqgridparam);
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
        public ActionResult SubmitForm(string KeyValue, ST_OutStorage entity, string BuildFormJson, HttpPostedFileBase Filedata, string DetailForm)
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

                    database.Delete<ST_InOutDetail>("MID", KeyValue, isOpenTrans);
                    List<ST_InOutDetail> DetailList = DetailForm.JonsToList<ST_InOutDetail>();
                    int index = 1;
                    foreach (ST_InOutDetail entityD in DetailList)
                    {
                        if (!string.IsNullOrEmpty(entityD.ProductID))
                        {
                            entityD.Create();
                            entityD.InOut = 1;
                            entityD.MID = KeyValue;
                            database.Insert(entityD, isOpenTrans);
                            index++;
                        }
                    }


                    database.Update(entity, isOpenTrans);

                }
                else
                {

                    entity.Create();

                    List<ST_InOutDetail> DetailList = DetailForm.JonsToList<ST_InOutDetail>();
                    int index = 1;
                    foreach (ST_InOutDetail entityD in DetailList)
                    {
                        if (!string.IsNullOrEmpty(entityD.ProductID))
                        {
                            entityD.Create();
                            entityD.InOut = 1;
                            entityD.MID = entity.OutID;
                            database.Insert(entityD, isOpenTrans);
                            index++;
                        }
                    }
                    database.Insert(entity, isOpenTrans);

                    Base_DataScopePermissionBll.Instance.AddScopeDefault(ModuleId, ManageProvider.Provider.Current().UserId, entity.OutID, isOpenTrans);
                }
                Base_FormAttributeBll.Instance.SaveBuildForm(BuildFormJson, entity.OutID, ModuleId, isOpenTrans);
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
            ST_OutStorage entity = DataFactory.Database().FindEntity<ST_OutStorage>(KeyValue);
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

                IsOk = re.Delete(KeyValue);
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
            Base_SysLogBll.Instance.WriteLog<ST_OutStorage>(array, IsOk.ToString(), Message);
        }


        public ActionResult ChooseProduct()
        {
            return View();
        }

        public ActionResult ChooseDetailProduct()
        {
            return View();
        }

        public ActionResult GetDetailList(string MID)
        {
            try
            {
                StringBuilder strSql = new StringBuilder();
                List<DbParameter> parameter = new List<DbParameter>();
                strSql.Append(@"select b.ProductID,b.ProductName,b.ProductLevel,a.num from st_inoutdetail a left join ST_Products b on a.productid=b.ProductID where MID=@MID  ");
                // strSql.Append(" AND RelationID = @RelationID order by SkillWeight desc ");
                parameter.Add(DbFactory.CreateDbParameter("@MID", MID));


                var JsonData = new
                {
                    rows = redisplay.FindListBySql(strSql.ToString(), parameter.ToArray()),
                };
                return Content(JsonData.ToJson());
            }
            catch (Exception ex)
            {
                Base_SysLogBll.Instance.WriteLog("", OperationType.Query, "-1", "异常错误：" + ex.Message);
                return null;
            }
        }








    }
}
