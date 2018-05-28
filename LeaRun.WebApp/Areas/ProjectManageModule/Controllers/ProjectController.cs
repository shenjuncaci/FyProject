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

namespace LeaRun.WebApp.Areas.ProjectManageModule.Controllers
{
    public class ProjectController : Controller
    {
        // GET: /ProjectManageModule/Notice/

        RepositoryFactory<PM_Project> repositoryfactory = new RepositoryFactory<PM_Project>();
        PM_ProjectBll ProjectBll = new PM_ProjectBll();
        Base_FlowBll FlowBll = new Base_FlowBll();
        PM_ProjectFileBll ProjectFileBll = new PM_ProjectFileBll();
        PM_ProjectEndFileBll ProjectEndFileBll = new PM_ProjectEndFileBll();
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
                DataTable ListData = ProjectBll.GetPageList(keywords, ref jqgridparam, ParameterJson);
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
            Base_NoteNOBll notenobll = new Base_NoteNOBll();
            string KeyValue = Request["KeyValue"];
            if (string.IsNullOrEmpty(KeyValue))
            {
                //生成单号放到新建保存的地方
                //ViewBag.BillNo = notenobll.CodeByYear("ProjectNO");
                ViewBag.CreateUserName = ManageProvider.Provider.Current().UserName;
            }
            return View();
        }

        [HttpPost]
        public ActionResult SubmitForm(string KeyValue, PM_Project entity, string BuildFormJson, HttpPostedFileBase Filedata, string DetailForm,string PlanForm,string TargetForm)
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

                    database.Delete<PM_ProjectMember>("ProjectID", KeyValue, isOpenTrans);
                    List<PM_ProjectMember> DetailList = DetailForm.JonsToList<PM_ProjectMember>();
                    int index = 1;
                    foreach (PM_ProjectMember entityD in DetailList)
                    {
                        if (!string.IsNullOrEmpty(entityD.UserID))
                        {
                            entityD.Create();
                            entityD.ProjectID = KeyValue;
                            database.Insert(entityD, isOpenTrans);
                            index++;
                        }
                    }

                    database.Delete<PM_ProjectPlan>("ProjectID", KeyValue, isOpenTrans);
                    List<PM_ProjectPlan> PlanList = PlanForm.JonsToList<PM_ProjectPlan>();
                    index = 1;
                    foreach (PM_ProjectPlan entityD in PlanList)
                    {
                        if (!string.IsNullOrEmpty(entityD.ProjectCycle))
                        {
                            entityD.Create();
                            entityD.ProjectID = KeyValue;
                            database.Insert(entityD, isOpenTrans);
                            index++;
                        }
                    }

                    database.Delete<PM_ProjectTarget>("ProjectID", KeyValue, isOpenTrans);
                    List<PM_ProjectTarget> TargetList = TargetForm.JonsToList<PM_ProjectTarget>();
                    index = 1;
                    foreach (PM_ProjectTarget entityD in TargetList)
                    {
                        if (!string.IsNullOrEmpty(entityD.BaseNum))
                        {
                            entityD.Create();
                            entityD.ProjectID = KeyValue;
                            database.Insert(entityD, isOpenTrans);
                            index++;
                        }
                    }



                    database.Update(entity, isOpenTrans);

                }
                else
                {
                    Base_NoteNOBll notenobll = new Base_NoteNOBll();
                    entity.Create();
                    entity.ProjectNO= notenobll.CodeByYear("ProjectNO");
                    //新建单据的时候，注册流程】
                    entity.FlowID = FlowBll.RegistFlow("Sj_ProjectManage", entity.ProjectID, entity.DataProvider);

                    List<PM_ProjectMember> DetailList = DetailForm.JonsToList<PM_ProjectMember>();
                    int index = 1;
                    foreach (PM_ProjectMember entityD in DetailList)
                    {
                        if (!string.IsNullOrEmpty(entityD.UserID))
                        {
                            entityD.Create();
                            entityD.ProjectID = entity.ProjectID;
                            database.Insert(entityD, isOpenTrans);
                            index++;
                        }
                    }


                    database.Delete<PM_ProjectPlan>("ProjectID", KeyValue, isOpenTrans);
                    List<PM_ProjectPlan> PlanList = PlanForm.JonsToList<PM_ProjectPlan>();
                    index = 1;
                    foreach (PM_ProjectPlan entityD in PlanList)
                    {
                        if (!string.IsNullOrEmpty(entityD.ProjectCycle))
                        {
                            entityD.Create();
                            entityD.ProjectID = KeyValue;
                            database.Insert(entityD, isOpenTrans);
                            index++;
                        }
                    }

                    database.Delete<PM_ProjectTarget>("ProjectID", KeyValue, isOpenTrans);
                    List<PM_ProjectTarget> TargetList = TargetForm.JonsToList<PM_ProjectTarget>();
                    index = 1;
                    foreach (PM_ProjectTarget entityD in TargetList)
                    {
                        if (!string.IsNullOrEmpty(entityD.BaseNum))
                        {
                            entityD.Create();
                            entityD.ProjectID = KeyValue;
                            database.Insert(entityD, isOpenTrans);
                            index++;
                        }
                    }



                    database.Insert(entity, isOpenTrans);

                    Base_DataScopePermissionBll.Instance.AddScopeDefault(ModuleId, ManageProvider.Provider.Current().UserId, entity.ProjectID, isOpenTrans);
                }
                Base_FormAttributeBll.Instance.SaveBuildForm(BuildFormJson, entity.ProjectID, ModuleId, isOpenTrans);
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

        //SubmitActivityForm
        [HttpPost]
        public ActionResult SubmitActivityForm(string KeyValue,string BuildFormJson, HttpPostedFileBase Filedata, string DetailForm, string ActivityForm,string ProblemForm)
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

                    database.Delete<PM_ProjectActivity>("ProjectID", KeyValue, isOpenTrans);
                    List<PM_ProjectActivity> ActivityList = ActivityForm.JonsToList<PM_ProjectActivity>();
                    int index = 1;
                    foreach (PM_ProjectActivity entityD in ActivityList)
                    {
                        if (!string.IsNullOrEmpty(entityD.ActivityContent))
                        {
                            database.Delete<PM_ProjectActivityMember>("ProjectActivityID", entityD.ProjectActivityID, isOpenTrans);
                            entityD.Create();
                            entityD.ActivityDate = DateTime.Now;
                            entityD.ProjectID = KeyValue;

                           
                            #region 图片处理，采用base64的方式转码解码
                            string virtualPath = "";
                            //图片上传
                            if (!string.IsNullOrEmpty(entityD.picsrc))
                            {
                                //删除老的图片
                                string FilePath = this.Server.MapPath(entityD.PictureUrl);
                                if (System.IO.File.Exists(FilePath))
                                    System.IO.File.Delete(FilePath);

                                string fileGuid = CommonHelper.GetGuid;
                                //long filesize = Filedata.ContentLength;
                                string FileEextension = ".jpg";
                                string uploadDate = DateTime.Now.ToString("yyyyMMdd");
                                //string UserId = ManageProvider.Provider.Current().UserId;
                                virtualPath = string.Format("~/Resource/Document/NetworkDisk/{0}/{1}{2}", "ProjectActivity", fileGuid, FileEextension);

                                string realPath = string.Format(@"D:\LeaRun\Resource\Document\NetworkDisk\{0}\{1}{2}", "ProjectActivity", fileGuid, FileEextension);

                                //string fullFileName = this.Server.MapPath(virtualPath);
                                ////创建文件夹，保存文件
                                //realPath = Path.GetDirectoryName(fullFileName);
                                //先处理图片文件
                                string temp = entityD.picsrc.Substring(23);
                                byte[] arr2 = Convert.FromBase64String(entityD.picsrc.Substring(23));
                                using (MemoryStream ms2 = new MemoryStream(arr2))
                                {
                                    System.Drawing.Bitmap bmp2 = new System.Drawing.Bitmap(ms2);
                                    bmp2.Save(realPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                                    bmp2.Dispose();
                                    ms2.Close();
                                }
                                entityD.PictureUrl = virtualPath;
                            }
                            #endregion
                            

                            string[] UserArr = entityD.MemberID.Split(',');
                            for (int j=0; j < UserArr.Length - 1;j++)
                            {
                                PM_ProjectActivityMember entityDD = new PM_ProjectActivityMember();
                                entityDD.Create();
                                entityDD.UserID = UserArr[j];
                                entityDD.ProjectActivityID = entityD.ProjectActivityID;
                                database.Insert(entityDD, isOpenTrans);
                            }
                            
                            database.Insert(entityD, isOpenTrans);
                            index++;
                        }
                    }

                    database.Delete<PM_ProjectProblem>("ProjectID", KeyValue, isOpenTrans);
                    List<PM_ProjectProblem> ProblemList = ProblemForm.JonsToList<PM_ProjectProblem>();
                    index = 1;
                    foreach (PM_ProjectProblem entityD in ProblemList)
                    {
                        if (!string.IsNullOrEmpty(entityD.ProblemDescripe))
                        {

                            entityD.Create();
                            entityD.PutDate = DateTime.Now;
                            entityD.ProjectID = KeyValue;
                            entityD.SortNO = index;

                            database.Insert(entityD, isOpenTrans);
                            index++;
                        }
                    }



                }
                
                //Base_FormAttributeBll.Instance.SaveBuildForm(BuildFormJson, entity.ProjectID, ModuleId, isOpenTrans);
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
            PM_Project entity = DataFactory.Database().FindEntity<PM_Project>(KeyValue);
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
            Base_SysLogBll.Instance.WriteLog<PM_Project>(array, IsOk.ToString(), Message);
        }

        public ActionResult DepartmentJson()
        {
            string sql = " select departmentid,fullname from base_department where 1=1 order by FullName ";
            DataTable dt = ProjectBll.GetDataTable(sql);
            return Content(dt.ToJson());
        }

        public ActionResult UserJson()
        {
            string sql = @"select userid,realname from Base_User where Enabled=1  ";
            DataTable dt = ProjectBll.GetDataTable(sql);
            return Content(dt.ToJson());
        }

        public ActionResult GetDetailList(string KeyValue)
        {
            try
            {
                var JsonData = new
                {
                    rows = ProjectBll.GetDetailList(KeyValue),
                };
                return Content(JsonData.ToJson());
            }
            catch (Exception ex)
            {
                Base_SysLogBll.Instance.WriteLog("", OperationType.Query, "-1", "异常错误：" + ex.Message);
                return null;
            }
        }

        public ActionResult GetActivityList(string KeyValue)
        {
            try
            {
                var JsonData = new
                {
                    rows = ProjectBll.GetActivityList(KeyValue),
                };
                return Content(JsonData.ToJson());
            }
            catch (Exception ex)
            {
                Base_SysLogBll.Instance.WriteLog("", OperationType.Query, "-1", "异常错误：" + ex.Message);
                return null;
            }
        }

        public ActionResult GetProblemList(string KeyValue)
        {
            try
            {
                var JsonData = new
                {
                    rows = ProjectBll.GetProblemList(KeyValue),
                };
                return Content(JsonData.ToJson());
            }
            catch (Exception ex)
            {
                Base_SysLogBll.Instance.WriteLog("", OperationType.Query, "-1", "异常错误：" + ex.Message);
                return null;
            }
        }

        public ActionResult UserListBatch()
        {
            return View();
        }

        public ActionResult GetUserListJson(string keywords, string CompanyId, string DepartmentId,
           JqGridParam jqgridparam, string ParameterJson, string SkillName, string SkillType)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = ProjectBll.GetUserList(keywords, ref jqgridparam, ParameterJson);
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

        public ActionResult ProfitList()
        {
            return View();
        }

        public ActionResult GridProfitListJson(string keywords, string CompanyId, string DepartmentId, JqGridParam jqgridparam, string ParameterJson)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = ProjectBll.GetProfitList(keywords, ref jqgridparam, ParameterJson);
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

        public ActionResult ProfitListForm()
        {
            return View();
        }

        public ActionResult GetProfitDetailList(string KeyValue)
        {
            try
            {
                var JsonData = new
                {
                    rows = ProjectBll.GetProfitDetailList(KeyValue),
                };
                return Content(JsonData.ToJson());
            }
            catch (Exception ex)
            {
                Base_SysLogBll.Instance.WriteLog("", OperationType.Query, "-1", "异常错误：" + ex.Message);
                return null;
            }
        }

        public ActionResult SubmitProfitForm(string KeyValue, PM_Project entity, string BuildFormJson, HttpPostedFileBase Filedata, string ProfitForm)
        {

            IDatabase database = DataFactory.Database();
            DbTransaction isOpenTrans = database.BeginTrans();


            string Message = KeyValue == "" ? "新增成功。" : "编辑成功。";

            database.Delete<PM_ProjectProfit>("ProjectID", KeyValue, isOpenTrans);
            List<PM_ProjectProfit> DetailList = ProfitForm.JonsToList<PM_ProjectProfit>();
            int index = 1;
            foreach (PM_ProjectProfit entityD in DetailList)
            {
                if (!string.IsNullOrEmpty(entityD.ProfitDate.ToString()))
                {
                    entityD.Create();
                    entityD.ProjectID = KeyValue;
                    database.Insert(entityD, isOpenTrans);
                    index++;
                }
            }
            database.Update(entity, isOpenTrans);
            database.Commit();
            return Content(new JsonMessage { Success = true, Code = "1", Message = Message }.ToString());
        }

        public ActionResult FlowForm(string ProjectID)
        {
            PM_Project entity = DataFactory.Database().FindEntity<PM_Project>(ProjectID);
            FlowDisplay flow = FlowBll.FlowDisplay(entity.FlowID);
            ViewData["flow"] = flow;
            ViewData["UserID"] = ManageProvider.Provider.Current().UserId;
            return View();
        }

        public int submit(string KeyValue,string FlowID,string type,string ProcessOpinion,string RejectNO)
        {

            int a = 0;
            StringBuilder strSql = new StringBuilder();
            //
            string sql = " select CurrentPost from Base_FlowLog where FlowID='" + FlowID + "'  ";
            DataTable dt = ProjectBll.GetDataTable(sql);
            

            //FY_ObjectTracking entity = DataFactory.Database().FindEntity<FY_ObjectTracking>(KeyValue);
            if (type == "-1" || type == "-2")
            {
                a = FlowBll.RejectFlow(FlowID,ProcessOpinion,RejectNO);
            }
            else
            {
                a = FlowBll.SubmitFlow(FlowID,ProcessOpinion);
            }
            strSql.AppendFormat(@" update PM_Project set Approvestatus='{0}' where ProjectID='{1}'  ",a,KeyValue);


            ProjectBll.ExecuteSql(strSql);
            return a;
        }

        public ActionResult PrintForm(string KeyValue)
        {
            string sql = @"  select ProjectNO,ProjectName,a.CreateDate,PlanFinishDate,
 a.ProjectNature,a.ProjectStatus,a.ProjectIndicators,a.BenchMark,a.Target,a.CalculationFormula ,c.RealName,b.FullName,a.Descripe,a.Master,a.Phone,a.ExpectedInput,a.ExpectedEarnings
 from PM_Project a
 left join Base_Department b on a.DepartMentID=b.DepartmentId
 left join Base_User c on a.DataProvider=c.UserId
 where a.ProjectID='" + KeyValue+"'";
            DataTable dt = ProjectBll.GetDataTable(sql);

            string sqlMember = @"  select UserName,PostName,Duty from PM_ProjectMember where ProjectID='"+KeyValue+"' ";
            DataTable dtMember = ProjectBll.GetDataTable(sqlMember);

            string sqlPlan = @" select ProjectCycle,CONVERT(varchar(100), PlanStartDate, 23) as PlanStartDate,CONVERT(varchar(100), PlanEndDate, 23) as PlanEndDate from PM_ProjectPlan where ProjectID='" + KeyValue+"' order by PlanStartDate ";
            DataTable dtPlan = ProjectBll.GetDataTable(sqlPlan);

            string sqlTarget = @" select TargetContent,BaseNum,TargetNum,Remark from PM_ProjectTarget where ProjectID='"+KeyValue+"' order by TargetContent ";
            DataTable dtTarget = ProjectBll.GetDataTable(sqlTarget);

            string sqlFlow = @"select case when IsFinish=1 then b.Approvestatus else '未审核' end from Base_FlowLog a left join Base_FlowLogDetail b on a.FlowID=b.FlowID 
where a.NoteID='{0}'
order by b.StepNO";
            sqlFlow = string.Format(sqlFlow, KeyValue);
            DataTable dtFlow = ProjectBll.GetDataTable(sqlFlow);

            ViewData["dt"] = dt;
            ViewData["dtMember"] = dtMember;
            ViewData["dtPlan"] = dtPlan;
            ViewData["dtTarget"] = dtTarget;
            ViewData["dtFlow"] = dtFlow;

            return View();
        }

        public ActionResult ActivityForm()
        {
            return View();
        }


        public ActionResult GetPlanList(string KeyValue)
        {
            try
            {
                var JsonData = new
                {
                    rows = ProjectBll.GetProjectPlanList(KeyValue),
                };
                return Content(JsonData.ToJson());
            }
            catch (Exception ex)
            {
                Base_SysLogBll.Instance.WriteLog("", OperationType.Query, "-1", "异常错误：" + ex.Message);
                return null;
            }
        }

        public ActionResult GetTargetList(string KeyValue)
        {
            try
            {
                var JsonData = new
                {
                    rows = ProjectBll.GetProjectTargetList(KeyValue),
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

        public ActionResult GetExistsList(string ProjectID)
        {
            string sql = @" select a.*,case when (b.CreateBy='{1}' or (exists (select * from Base_ObjectUserRelation where ObjectId='15342978-8090-4244-96ec-947472346fff' and UserId='{1}'))) and b.Approvestatus!='9'  then 1 else 0 end as canDel 
from pm_projectfile a 
left join pm_project b on a.projectid=b.projectid

where 1=1 and a.projectid='{0}' ";
            sql = string.Format(sql, ProjectID, ManageProvider.Provider.Current().UserId);
            DataTable dt = ProjectBll.GetDataTable(sql);
            return Content(dt.ToJson());
        }

        public ActionResult GetExistsListEnd(string ProjectID)
        {
            string sql = @" select a.*,case when b.isend=1 then 0 else 1 end as canDel 
from pm_projectendfile a 
left join pm_project b on a.projectid=b.projectid

where 1=1 and a.projectid='{0}' ";
            sql = string.Format(sql, ProjectID, ManageProvider.Provider.Current().UserId);
            DataTable dt = ProjectBll.GetDataTable(sql);
            return Content(dt.ToJson());
        }

        public void Download(string KeyValue)
        {
            PM_ProjectFile entity = ProjectFileBll.Repository().FindEntity(KeyValue);
            string filename = Server.UrlDecode(entity.FileName);//返回客户端文件名称
            string filepath = Server.UrlDecode(entity.FilePath);//文件虚拟路径
            if (FileDownHelper.FileExists(filepath))
            {
                FileDownHelper.DownLoadold(filepath, filename);
            }
        }

        public void DownloadEnd(string KeyValue)
        {
            PM_ProjectEndFile entity = ProjectEndFileBll.Repository().FindEntity(KeyValue);
            string filename = Server.UrlDecode(entity.FileName);//返回客户端文件名称
            string filepath = Server.UrlDecode(entity.FilePath);//文件虚拟路径
            if (FileDownHelper.FileExists(filepath))
            {
                FileDownHelper.DownLoadold(filepath, filename);
            }
        }

        public ActionResult DeleteFile(string NetworkFileId)
        {
            try
            {
                PM_ProjectFile entity = ProjectFileBll.Repository().FindEntity(NetworkFileId);
                ProjectFileBll.Repository().Delete(NetworkFileId);
                string FilePath = this.Server.MapPath(entity.FilePath);
                if (System.IO.File.Exists(FilePath))
                    System.IO.File.Delete(FilePath);
                return Content(new JsonMessage { Success = true, Code = "1", Message = "删除成功。" }.ToString());
            }
            catch (Exception ex)
            {
                return Content(new JsonMessage { Success = false, Code = "-1", Message = "操作失败：" + ex.Message }.ToString());
            }
        }

        public ActionResult DeleteFileEnd(string NetworkFileId)
        {
            try
            {
                PM_ProjectEndFile entity = ProjectEndFileBll.Repository().FindEntity(NetworkFileId);
                ProjectFileBll.Repository().Delete(NetworkFileId);
                string FilePath = this.Server.MapPath(entity.FilePath);
                if (System.IO.File.Exists(FilePath))
                    System.IO.File.Delete(FilePath);
                return Content(new JsonMessage { Success = true, Code = "1", Message = "删除成功。" }.ToString());
            }
            catch (Exception ex)
            {
                return Content(new JsonMessage { Success = false, Code = "-1", Message = "操作失败：" + ex.Message }.ToString());
            }
        }

        public string CheckFileName(string FileID)
        {
            string sql = "select * from PM_ProjectFile where FileID='" + FileID + "'";
            DataTable dt = ProjectBll.GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                if (dt.Rows[0]["FileExtensions"].ToString() == ".pdf")
                {
                    if (DirFileHelper.IsExistFile(System.Web.HttpContext.Current.Request.PhysicalApplicationPath + "\\Content\\Scripts\\pdf.js\\generic\\web\\" + dt.Rows[0]["FileID"].ToString() + ".pdf"))
                    {
                        return dt.Rows[0]["FileID"].ToString() + ".pdf";
                    }
                    else
                    {
                        DirFileHelper.CopyFile(dt.Rows[0]["FilePath"].ToString().Replace("~/", ""), "Content\\Scripts\\pdf.js\\generic\\web\\" + dt.Rows[0]["FileID"].ToString() + ".pdf");
                        return dt.Rows[0]["FileID"].ToString() + ".pdf";
                    }
                }
                else
                {
                    //将word转换为pdf再返回路径
                    if (dt.Rows[0]["FileExtensions"].ToString() == ".doc" || dt.Rows[0]["FileExtensions"].ToString() == ".docx")
                    {
                        if (DirFileHelper.IsExistFile(System.Web.HttpContext.Current.Request.PhysicalApplicationPath + "\\Content\\Scripts\\pdf.js\\generic\\web\\" + dt.Rows[0]["FileID"].ToString() + ".pdf"))
                        {
                            return dt.Rows[0]["FileID"].ToString() + ".pdf";
                        }
                        else
                        {
                            ToPDFHelper.ConvertWord2Pdf(dt.Rows[0]["FilePath"].ToString().Replace("~/", ""), "Content\\Scripts\\pdf.js\\generic\\web\\" + dt.Rows[0]["FileID"].ToString() + ".pdf");
                            return dt.Rows[0]["FileID"].ToString() + ".pdf";
                        }
                    }
                    else if (dt.Rows[0]["FileExtensions"].ToString() == ".xls" || dt.Rows[0]["FileExtensions"].ToString() == ".xlsx")
                    {
                        if (DirFileHelper.IsExistFile(System.Web.HttpContext.Current.Request.PhysicalApplicationPath + "\\Content\\Scripts\\pdf.js\\generic\\web\\" + dt.Rows[0]["FileID"].ToString() + ".pdf"))
                        {
                            return dt.Rows[0]["FileID"].ToString() + ".pdf";
                        }
                        else
                        {
                            ToPDFHelper.ConvertExcel2Pdf(dt.Rows[0]["FilePath"].ToString().Replace("~/", ""), "Content\\Scripts\\pdf.js\\generic\\web\\" + dt.Rows[0]["FileID"].ToString() + ".pdf");
                            return dt.Rows[0]["FileID"].ToString() + ".pdf";
                        }
                    }
                    else if (dt.Rows[0]["FileExtensions"].ToString() == ".ppt" || dt.Rows[0]["FileExtensions"].ToString() == ".pptx")
                    {
                        if (DirFileHelper.IsExistFile(System.Web.HttpContext.Current.Request.PhysicalApplicationPath + "\\Content\\Scripts\\pdf.js\\generic\\web\\" + dt.Rows[0]["FileID"].ToString() + ".pdf"))
                        {
                            return dt.Rows[0]["FileID"].ToString() + ".pdf";
                        }
                        else
                        {
                            ToPDFHelper.ConvertPowerPoint2Pdf(dt.Rows[0]["FilePath"].ToString().Replace("~/", ""), "Content\\Scripts\\pdf.js\\generic\\web\\" + dt.Rows[0]["FileID"].ToString() + ".pdf");
                            return dt.Rows[0]["FileID"].ToString() + ".pdf";
                        }
                    }
                    else
                    {
                        return "0";
                    }
                }
            }
            else
            {
                return "0";
            }
        }

        public string CheckFileNameEnd(string FileID)
        {
            string sql = "select * from PM_ProjectEndFile where FileID='" + FileID + "'";
            DataTable dt = ProjectBll.GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                if (dt.Rows[0]["FileExtensions"].ToString() == ".pdf")
                {
                    if (DirFileHelper.IsExistFile(System.Web.HttpContext.Current.Request.PhysicalApplicationPath + "\\Content\\Scripts\\pdf.js\\generic\\web\\" + dt.Rows[0]["FileID"].ToString() + ".pdf"))
                    {
                        return dt.Rows[0]["FileID"].ToString() + ".pdf";
                    }
                    else
                    {
                        DirFileHelper.CopyFile(dt.Rows[0]["FilePath"].ToString().Replace("~/", ""), "Content\\Scripts\\pdf.js\\generic\\web\\" + dt.Rows[0]["FileID"].ToString() + ".pdf");
                        return dt.Rows[0]["FileID"].ToString() + ".pdf";
                    }
                }
                else
                {
                    //将word转换为pdf再返回路径
                    if (dt.Rows[0]["FileExtensions"].ToString() == ".doc" || dt.Rows[0]["FileExtensions"].ToString() == ".docx")
                    {
                        if (DirFileHelper.IsExistFile(System.Web.HttpContext.Current.Request.PhysicalApplicationPath + "\\Content\\Scripts\\pdf.js\\generic\\web\\" + dt.Rows[0]["FileID"].ToString() + ".pdf"))
                        {
                            return dt.Rows[0]["FileID"].ToString() + ".pdf";
                        }
                        else
                        {
                            ToPDFHelper.ConvertWord2Pdf(dt.Rows[0]["FilePath"].ToString().Replace("~/", ""), "Content\\Scripts\\pdf.js\\generic\\web\\" + dt.Rows[0]["FileID"].ToString() + ".pdf");
                            return dt.Rows[0]["FileID"].ToString() + ".pdf";
                        }
                    }
                    else if (dt.Rows[0]["FileExtensions"].ToString() == ".xls" || dt.Rows[0]["FileExtensions"].ToString() == ".xlsx")
                    {
                        if (DirFileHelper.IsExistFile(System.Web.HttpContext.Current.Request.PhysicalApplicationPath + "\\Content\\Scripts\\pdf.js\\generic\\web\\" + dt.Rows[0]["FileID"].ToString() + ".pdf"))
                        {
                            return dt.Rows[0]["FileID"].ToString() + ".pdf";
                        }
                        else
                        {
                            ToPDFHelper.ConvertExcel2Pdf(dt.Rows[0]["FilePath"].ToString().Replace("~/", ""), "Content\\Scripts\\pdf.js\\generic\\web\\" + dt.Rows[0]["FileID"].ToString() + ".pdf");
                            return dt.Rows[0]["FileID"].ToString() + ".pdf";
                        }
                    }
                    else if (dt.Rows[0]["FileExtensions"].ToString() == ".ppt" || dt.Rows[0]["FileExtensions"].ToString() == ".pptx")
                    {
                        if (DirFileHelper.IsExistFile(System.Web.HttpContext.Current.Request.PhysicalApplicationPath + "\\Content\\Scripts\\pdf.js\\generic\\web\\" + dt.Rows[0]["FileID"].ToString() + ".pdf"))
                        {
                            return dt.Rows[0]["FileID"].ToString() + ".pdf";
                        }
                        else
                        {
                            ToPDFHelper.ConvertPowerPoint2Pdf(dt.Rows[0]["FilePath"].ToString().Replace("~/", ""), "Content\\Scripts\\pdf.js\\generic\\web\\" + dt.Rows[0]["FileID"].ToString() + ".pdf");
                            return dt.Rows[0]["FileID"].ToString() + ".pdf";
                        }
                    }
                    else
                    {
                        return "0";
                    }
                }
            }
            else
            {
                return "0";
            }
        }

        public ActionResult SubmitUploadify(string FolderId, HttpPostedFileBase Filedata)
        {
            try
            {
                Thread.Sleep(1000);////延迟500毫秒
                PM_ProjectFile entity = new PM_ProjectFile();
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

                string virtualPath = string.Format("~/Resource/Document/NetWorkDisk/ProjectFile/{0}{1}", fileGuid, FileEextension);
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
                        entity.Create();
                        entity.FilePath = virtualPath;
                        entity.ProjectID = FolderId;
                        entity.FileName = Filedata.FileName;
                        //entity.FilePath = virtualPath;
                        entity.FileSize = filesize.ToString();
                        entity.FileExtensions = FileEextension;
                        string _FileType = "";
                        string _Icon = "";
                        this.DocumentType(FileEextension, ref _FileType, ref _Icon);
                        entity.Icon = _Icon;
                        entity.FileType = _FileType;
                        IsOk = DataFactory.Database().Insert<PM_ProjectFile>(entity).ToString();
                    }
                    catch (Exception ex)
                    {
                        IsOk = ex.Message;
                        //System.IO.File.Delete(virtualPath);
                    }
                }

                StringBuilder strSql = new StringBuilder();
                //strSql.AppendFormat(@"update VP_RiskDownFollow set RealFinishDt=GETDATE() where FollowID='{0}'", FolderId);
                //RiskDownFollowBll.ExecuteSql(strSql);

                var JsonData = new
                {
                    Status = IsOk,
                    NetworkFile = entity,
                };
                return Content(JsonData.ToJson());
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        public ActionResult SubmitUploadifyEnd(string FolderId, HttpPostedFileBase Filedata)
        {
            try
            {
                Thread.Sleep(1000);////延迟500毫秒
                PM_ProjectEndFile entity = new PM_ProjectEndFile();
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

                string virtualPath = string.Format("~/Resource/Document/NetWorkDisk/ProjectFileEnd/{0}{1}", fileGuid, FileEextension);
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
                        entity.Create();
                        entity.FilePath = virtualPath;
                        entity.ProjectID = FolderId;
                        entity.FileName = Filedata.FileName;
                        //entity.FilePath = virtualPath;
                        entity.FileSize = filesize.ToString();
                        entity.FileExtensions = FileEextension;
                        string _FileType = "";
                        string _Icon = "";
                        this.DocumentType(FileEextension, ref _FileType, ref _Icon);
                        entity.Icon = _Icon;
                        entity.FileType = _FileType;
                        IsOk = DataFactory.Database().Insert<PM_ProjectEndFile>(entity).ToString();
                    }
                    catch (Exception ex)
                    {
                        IsOk = ex.Message;
                        //System.IO.File.Delete(virtualPath);
                    }
                }

                StringBuilder strSql = new StringBuilder();
                //strSql.AppendFormat(@"update VP_RiskDownFollow set RealFinishDt=GETDATE() where FollowID='{0}'", FolderId);
                //RiskDownFollowBll.ExecuteSql(strSql);

                var JsonData = new
                {
                    Status = IsOk,
                    NetworkFile = entity,
                };
                return Content(JsonData.ToJson());
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        public void DocumentType(string Eextension, ref string FileType, ref string Icon)
        {
            string _FileType = "";
            string _Icon = "";
            switch (Eextension)
            {
                case ".docx":
                    _FileType = "word文件";
                    _Icon = "doc";
                    break;
                case ".doc":
                    _FileType = "word文件";
                    _Icon = "doc";
                    break;
                case ".xlsx":
                    _FileType = "excel文件";
                    _Icon = "xls";
                    break;
                case ".xls":
                    _FileType = "excel文件";
                    _Icon = "xls";
                    break;
                case ".pptx":
                    _FileType = "ppt文件";
                    _Icon = "ppt";
                    break;
                case ".ppt":
                    _FileType = "ppt文件";
                    _Icon = "ppt";
                    break;
                case ".txt":
                    _FileType = "记事本文件";
                    _Icon = "txt";
                    break;
                case ".pdf":
                    _FileType = "pdf文件";
                    _Icon = "pdf";
                    break;
                case ".zip":
                    _FileType = "压缩文件";
                    _Icon = "zip";
                    break;
                case ".rar":
                    _FileType = "压缩文件";
                    _Icon = "rar";
                    break;
                case ".png":
                    _FileType = "png图片";
                    _Icon = "png";
                    break;
                case ".gif":
                    _FileType = "gif图片";
                    _Icon = "gif";
                    break;
                case ".jpg":
                    _FileType = "jpg图片";
                    _Icon = "jpeg";
                    break;
                case ".mp3":
                    _FileType = "mp3文件";
                    _Icon = "mp3";
                    break;
                case ".html":
                    _FileType = "html文件";
                    _Icon = "html";
                    break;
                case ".css":
                    _FileType = "css文件";
                    _Icon = "css";
                    break;
                case ".mpeg":
                    _FileType = "mpeg文件";
                    _Icon = "mpeg";
                    break;
                case ".pds":
                    _FileType = "pds文件";
                    _Icon = "pds";
                    break;
                case ".ttf":
                    _FileType = "ttf文件";
                    _Icon = "ttf";
                    break;
                case ".swf":
                    _FileType = "swf文件";
                    _Icon = "swf";
                    break;
                default:
                    _FileType = "其他文件";
                    _Icon = "new";
                    //return "else.png";
                    break;
            }
            FileType = _FileType;
            Icon = _Icon;
        }


        public ActionResult UserInfo()
        {
            return View();
        }

        public ActionResult GetUserList(string ProjectActivityID)
        {
            StringBuilder sb = new StringBuilder();
            string sql = "  select a.*,b.ProjectActivityID from Base_User a left join PM_ProjectActivityMember b on a.UserId=b.UserID and b.ProjectActivityID='" + ProjectActivityID + "' where a.Enabled=1 ";
            DataTable dt = ProjectBll.GetDataTable(sql);
            foreach (DataRow dr in dt.Rows)
            {
                string strchecked = "";
                if (!string.IsNullOrEmpty(dr["ProjectActivityID"].ToString()))//判断是否选中
                {
                    strchecked = "selected";
                }
                sb.Append("<li title=\"" + dr["RealName"] + "(" + dr["Code"] + ")" + "\" class=\"" + strchecked + "\">");
                sb.Append("<a id=\"" + dr["UserId"] + "\" name=\""+ dr["RealName"] + "\"><img src=\"../../Content/Images/Icon16/role.png \">" + dr["RealName"] + "</a><i></i>");
                sb.Append("</li>");
            }
            return Content(sb.ToString());
        }

        /// <summary>
        /// 添加评审人，模态窗口提交以后的方法
        /// </summary>
        /// <param name="ChangeID"></param>
        /// <param name="ObjectId"></param>
        /// <returns></returns>
        public ActionResult UserListSubmit(string ProjectActivityID, string ObjectId)
        {
            try
            {

                StringBuilder strSql = new StringBuilder();
                string[] array = ObjectId.Split(',');

                strSql.AppendFormat(@"delete from PM_ProjectActivityMember where ProjectActivityID='{0}' ", ProjectActivityID);

                for (int i = 0; i < array.Length - 1; i++)
                {
                    strSql.AppendFormat(@"insert into PM_ProjectActivityMember values(NEWID(),'{0}','{1}')", ProjectActivityID, array[i]);
                }
                ProjectBll.ExecuteSql(strSql);
                return Content(new JsonMessage { Success = true, Code = 1.ToString(), Message = "操作成功。" }.ToString());

            }
            catch (Exception ex)
            {
                return Content(new JsonMessage { Success = false, Code = "-1", Message = "操作失败，错误：" + ex.Message }.ToString());
            }
        }

        public string GetUserString(string ProjectActivityID)
        {
            string Result = "";
            string temp1 = "";
            string temp2 = "";
            string sql = "   select b.UserID,b.RealName from PM_ProjectActivitymember a left join Base_User b on a.userid=b.UserId where a.projectactivityid='{0}' ";
            sql = string.Format(sql, ProjectActivityID);
            DataTable dt = ProjectBll.GetDataTable(sql);
            if(dt.Rows.Count>0)
            {
               for(int i=0;i<dt.Rows.Count;i++)
                {
                    temp1+= dt.Rows[i][0].ToString() + ",";
                    temp2 += dt.Rows[i][1].ToString() + ",";
                   
                }
                temp1 = temp1.Substring(0, temp1.Length - 1);
                temp2 = temp2.Substring(0, temp2.Length - 1);
            }
            Result += temp1 +"|"+ temp2;
            return Result;
        }

        //结案管理
        public ActionResult EndProject(string KeyValue)
        {
            return View();
        }

        public int EndProjectAjax(string ProjectID)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(" update PM_Project set IsENd=1,enddate=getdate() where projectID='{0}' ",ProjectID);
            return ProjectBll.ExecuteSql(strSql);
        }

        public ActionResult ProjectNatureJson()
        {
            string sql = "select Code,FullName from Base_DataDictionaryDetail where DataDictionaryId='721fa6dd-1acf-426a-a025-db6a9608ad0b' and ParentId='0' ";
            DataTable dt = ProjectBll.GetDataTable(sql);
            return Content(dt.ToJson());
        }

        public ActionResult ProjectNatureDJson(string ParentId)
        {
            string sql = "select Code,FullName from Base_DataDictionaryDetail where DataDictionaryId='721fa6dd-1acf-426a-a025-db6a9608ad0b' and ParentID!='0'  ";
            if(ParentId!=null&&ParentId!=""&&ParentId!="Undefined")
            {
                sql = sql + " and ParentId in (select DataDictionaryDetailId from Base_DataDictionaryDetail where code='"+ParentId+"') ";
            }
            DataTable dt = ProjectBll.GetDataTable(sql);
            return Content(dt.ToJson());
        }
    }
}
