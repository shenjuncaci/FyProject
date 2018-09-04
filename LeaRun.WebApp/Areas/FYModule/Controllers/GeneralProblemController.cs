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
    public class GeneralProblemController : Controller
    {
        //
        // GET: /FYModule/GeneralProblem/

        RepositoryFactory<FY_GeneralProblem> repositoryfactory = new RepositoryFactory<FY_GeneralProblem>();
        FY_GeneralProblemBll GeneralProblemBll = new FY_GeneralProblemBll();
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
                DataTable ListData = GeneralProblemBll.GetPageList(keywords, ref jqgridparam, ParameterJson);
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
            string sql = " select FullName from Base_User a left join  [Base_ObjectUserRelation] b on a.UserId=b.UserId left join Base_Roles c on b.ObjectId=c.RoleId where a.UserId='" + ManageProvider.Provider.Current().UserId + "' and FullName='质保部审批' ";
            DataTable dt = GeneralProblemBll.GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                ViewData["dt"] = dt.Rows[0][0].ToString();
            }
            string UserName = ManageProvider.Provider.Current().Code;
            ViewData["UserName"] = UserName;
            //ViewData["UserName"] = ManageProvider.Provider.Current().UserName;
            return View();
        }

        [HttpPost]
        public ActionResult SubmitForm(string KeyValue, FY_GeneralProblem entity, string BuildFormJson, HttpPostedFileBase Filedata)
        {
            string ModuleId = DESEncrypt.Decrypt(CookieHelper.GetCookie("ModuleId"));
            IDatabase database = DataFactory.Database();
            DbTransaction isOpenTrans = database.BeginTrans();
            try
            {
                string Message = KeyValue == "" ? "新增成功。" : "编辑成功。";
                if (!string.IsNullOrEmpty(KeyValue))
                {
                    //if (KeyValue == ManageProvider.Provider.Current().UserId)
                    //{
                    //    throw new Exception("无权限编辑信息");
                    //}


                    entity.Modify(KeyValue);


                    database.Update(entity, isOpenTrans);

                }
                else
                {
                    //entity.DepartMentID = ManageProvider.Provider.Current().DepartmentId;
                    entity.Create();


                    database.Insert(entity, isOpenTrans);

                    Base_DataScopePermissionBll.Instance.AddScopeDefault(ModuleId, ManageProvider.Provider.Current().UserId, entity.GeneralProblemID, isOpenTrans);
                }
                Base_FormAttributeBll.Instance.SaveBuildForm(BuildFormJson, entity.GeneralProblemID, ModuleId, isOpenTrans);
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
            FY_GeneralProblem entity = DataFactory.Database().FindEntity<FY_GeneralProblem>(KeyValue);
            if (entity == null)
            {
                return Content("");
            }

            string strJson = entity.ToJson();

            strJson = strJson.Insert(1, Base_FormAttributeBll.Instance.GetBuildForm(KeyValue));
            return Content(strJson);
        }

        //public ActionResult CusName()
        //{
        //    string sql = " select fy_cus_id,fy_cus_name from FY_CUS where 1=1 ";
        //    DataTable dt = GeneralProblemBll.GetDataTable(sql);
        //    return Content(dt.ToJson());
        //}


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
            Base_SysLogBll.Instance.WriteLog<FY_GeneralProblem>(array, IsOk.ToString(), Message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="FolderId"></param>
        /// <param name="Filedata"></param>
        /// <param name="type">PD,CA,CM</param>
        /// <returns></returns>
        public ActionResult SubmitUploadify(string FolderId, HttpPostedFileBase Filedata, string type)
        {
            try
            {

                Thread.Sleep(1000);////延迟500毫秒
                Base_NetworkFile entity = new Base_NetworkFile();
                FY_GeneralProblem rapidentity = DataFactory.Database().FindEntity<FY_GeneralProblem>(FolderId);

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

                string virtualPath = string.Format("~/Resource/Document/NetworkDisk/{0}/{1}/{2}{3}", "QSB", uploadDate, fileGuid, FileEextension);
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
                        //文件信息写入数据库
                        //entity.Create();
                        //entity.NetworkFileId = fileGuid;
                        //entity.FolderId = FolderId;
                        //entity.FileName = Filedata.FileName;
                        //entity.FilePath = virtualPath;
                        //entity.FileSize = filesize.ToString();
                        //entity.FileExtensions = FileEextension;
                        //string _FileType = "";
                        //string _Icon = "";
                        //this.DocumentType(FileEextension, ref _FileType, ref _Icon);
                        //entity.Icon = _Icon;
                        //entity.FileType = _FileType;
                        //IsOk = DataFactory.Database().Insert<Base_NetworkFile>(entity).ToString();

                        if (type == "PD")
                        {
                            rapidentity.ProblemAttach = virtualPath;
                        }
                        if (type == "CA")
                        {
                            rapidentity.CauseAnalysisAttach = virtualPath;
                        }
                        if (type == "CM")
                        {
                            rapidentity.MeasureAttach = virtualPath;
                        }
                        if(type== "IR")
                        {
                            rapidentity.ImproveReportAttach = virtualPath;
                        }
                        
                        DataFactory.Database().Update<FY_GeneralProblem>(rapidentity);
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
                    NetworkFile = rapidentity,
                };
                return Content(JsonData.ToJson());


            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        //按需求导出excel
        public void ExcelExport(string condition)
        {
            ExcelHelper ex = new ExcelHelper();
            string sql = @" select a.FinishStatus as 状态,a.ComplaintLevel as 投诉级别,a.ProductArea as 产品区域,a.HappenPlace as 事发地,a.ProblemType2 as 问题类型,a.IsCheck as 是否考核,IsAgain as 是否重复发生,
a.ProblemType as 问题类别,b.realname as 责任人,e.RealName as 跟踪人,c.fullname as 责任部门,a.Customer as 客户,
a.ProblemDescripe as 问题描述,CONVERT(varchar(100),a.HappenDate,23) as 发生日期,a.CauseAnalysis as 根本原因分析,a.CorrectMeasures as 纠正措施,a.ImproveReport as 改善报告,
Requirements as 规范要求,ActualResults as 规范操作,BigType as 问题大类,DetailType as 问题小类
from FY_GeneralProblem a left join Base_user b on a.ResponseBy=b.code 
left join Base_Department c on b.departmentid=c.departmentid
left join Base_User e on a.FollowBy=e.code
where 1=1  ";
            sql = sql + condition;
            DataTable ListData = GeneralProblemBll.GetDataTable(sql);
            //ex.EcportExcel(ListData, "一般问题导出");

            MemoryStream ms = NpoiHelper.RenderDataTableToExcel(ListData) as MemoryStream;

            /*情况1：在Asp.NET中，输出文件流，浏览器自动提示下载*/
            Response.AddHeader("Content-Disposition", string.Format("attachment; filename=download.xls"));
            Response.BinaryWrite(ms.ToArray());
            ms.Close();
            ms.Dispose();

        }

        public string FinishIt(string GeneralProblemID)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(@" update FY_GeneralProblem set finishstatus='已完成',RealFinshDt=GETDATE() where  GeneralProblemID='{0}' and finishstatus!='已完成' ", GeneralProblemID);
            GeneralProblemBll.ExecuteSql(strSql);
            return "0";
        }

        public string RejectIt(string GeneralProblemID)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(@" update FY_GeneralProblem set finishstatus='回退' where  GeneralProblemID='{0}' ", GeneralProblemID);
            GeneralProblemBll.ExecuteSql(strSql);
            return "0";
        }

        //将一般问题再次转换为快速反应
        public string TransToRapid(String ID)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(@"insert into FY_Rapid(res_id,res_area,res_ok,res_again,res_type,res_cpeo,res_kf,res_ms,res_cdate,
res_fxnode,res_csnode,res_msfj,RapidState,PlanTime,IsEmail,CreateDt,
res_yzb,res_fx,res_cs,res_fcf,res_fcsh,res_csgz,res_fmea,res_bzgx,res_jyjx,res_8d,IsCheck,res_jb,res_dd,res_mc,res_cd,Requirements,ActualResults,FollowBy,BigType,DetailType)
select NEWID(),ProductArea,ProblemType,IsAgain,ProblemType2,ResponseBy,Customer,ProblemDescripe,
HappenDate,CauseAnalysis,CorrectMeasures,ProblemAttach,'进行中',DATEADD(DAY,40,HappenDate),1,HappenDate as CreateDt,
'未提交','未提交','未提交','未提交','未提交','未提交','未提交','未提交','未提交','未提交',IsCheck,ComplaintLevel,HappenPlace,ProductName,ImportLevel,Requirements,ActualResults,FollowBy,BigType,DetailType
from FY_GeneralProblem
where GeneralProblemID='{0}' ", ID);
            GeneralProblemBll.ExecuteSql(strSql);

            //删除掉原数据
            //删除原来的记录
            StringBuilder DeleteSql = new StringBuilder();
            DeleteSql.AppendFormat(" delete from  FY_GeneralProblem where GeneralProblemID='{0}' ", ID);
            GeneralProblemBll.ExecuteSql(DeleteSql);

            return "0";
        }

    }
}
