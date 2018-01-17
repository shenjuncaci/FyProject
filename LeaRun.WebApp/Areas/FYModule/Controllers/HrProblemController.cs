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
    public class HrProblemController : Controller
    {
        RepositoryFactory<FY_HrProblem> repositoryfactory = new RepositoryFactory<FY_HrProblem>();
        FY_HrProblemBll PostBll = new FY_HrProblemBll();
        Base_FlowBll FlowBll = new Base_FlowBll();
        //
        // GET: /FYModule/Process/

        //员工咨询
        public ActionResult Index()
        {
            return View();
        }
        //心声反馈
        public ActionResult Index2()
        {
            return View();
        }
        //员工投诉
        public ActionResult Index3()
        {
            return View();
        }
        //外部咨询
        public ActionResult Index4()
        {
            return View();
        }

        public ActionResult GridPageListJson(string keywords, string CompanyId, string DepartmentId, 
            JqGridParam jqgridparam, string ParameterJson,string type,string IsMy)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = PostBll.GetPageList(keywords, ref jqgridparam, ParameterJson,type,IsMy);
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



        public ActionResult Form(string KeyValue)
        {
            FY_HrProblem entity = DataFactory.Database().FindEntity<FY_HrProblem>(KeyValue);
            FlowDisplay flow = FlowBll.FlowDisplay(entity.FlowID);
            ViewData["flow"] = flow;
            ViewData["UserID"] = ManageProvider.Provider.Current().UserId;
            return View();
        }

        public ActionResult FormNew(string type)
        {
            string ProblemType = "";
            if(type=="1")
            {
                ProblemType = "员工咨询";
            }
            if(type=="2")
            {
                ProblemType = "	心声反馈";
            }
            if(type=="3")
            {
                ProblemType = "员工投诉";
            }
            if(type=="4")
            {
                ProblemType = "外部咨询";
            }
            ViewData["ProblemType"] = ProblemType;
            
            ViewData["ReplyDt"]=DateTime.Now.AddDays(1).ToString();

            return View();
        }

        [HttpPost]
        public ActionResult SubmitForm(string KeyValue, FY_HrProblem entity, string BuildFormJson, HttpPostedFileBase Filedata)
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
                    string GetReciverSql = " select Email from base_user where userid='" + entity.ResponseBy + "' ";
                    DataTable dt = PostBll.GetDataTable(GetReciverSql);
                    if (dt.Rows.Count > 0)
                    {
                       
                        //问题提交的时候。先给人事部发一个邮件，提示
                        MailHelper.SendEmail("hongfang.zhou@fuyaogroup.com", "您好，有一条新的员工关系，请登录系统确认是否需要调整责任人。网址：172.19.0.5:8086");
                    }
                    entity.Create();

                    //entity.FlowID=RegistFlow(entity.ProblemID);
                   

                    

                    //新建单据的时候，注册流程】
                    entity.FlowID = FlowBll.RegistFlow("Sj_HrProblem", entity.ProblemID,entity.ResponseBy);

                    database.Insert(entity, isOpenTrans);

                    Base_DataScopePermissionBll.Instance.AddScopeDefault(ModuleId, ManageProvider.Provider.Current().UserId, entity.ProblemID, isOpenTrans);
                }
                Base_FormAttributeBll.Instance.SaveBuildForm(BuildFormJson, entity.ProblemID, ModuleId, isOpenTrans);
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
            FY_HrProblem entity = DataFactory.Database().FindEntity<FY_HrProblem>(KeyValue);
            if (entity == null)
            {
                return Content("");
            }

            string strJson = entity.ToJson();

            strJson = strJson.Insert(1, Base_FormAttributeBll.Instance.GetBuildForm(KeyValue));
            return Content(strJson);
        }


        public ActionResult SubmitUploadify(string FolderId, HttpPostedFileBase Filedata,string type)
        {
            try
            {

                Thread.Sleep(1000);////延迟500毫秒
                Base_NetworkFile entity = new Base_NetworkFile();
                FY_HrProblem PAentity = DataFactory.Database().FindEntity<FY_HrProblem>(FolderId);

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

                string virtualPath = string.Format("~/Resource/Document/NetworkDisk/{0}/{1}/{2}{3}", "HrProblem", uploadDate, fileGuid, FileEextension);
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
                        if (type == "1")
                        {
                            PAentity.ProblemAttach = virtualPath;
                        }
                        else
                        {
                            PAentity.AttachPath = virtualPath;
                        }


                        DataFactory.Database().Update<FY_HrProblem>(PAentity);
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


        public ActionResult Approve(string ActionID, string Tag, string ActionContent)
        {

            StringBuilder strSql = new StringBuilder();
            FY_HrProblem entity = DataFactory.Database().FindEntity<FY_HrProblem>(ActionID);
            if (ManageProvider.Provider.Current().UserId == entity.CreateBy)
            {
                if (Tag == "Yes")
                {
                    strSql.AppendFormat(@"update FY_HrProblem set ProblemState='已完成',RealDt=getdate(),CompleteRate=(cast(cast (DATEDIFF(day,createdt,plandt)/
(case when DATEDIFF(day,createdt,realdt)=0 then 1 else DATEDIFF(day,createdt,realdt) end)
/1.0  as decimal(18,2)) as nvarchar(50))+'%') where
                   ProblemID='{0}'", ActionID);

                   // strSql.AppendFormat(@"update FY_HrProblem set ProblemState='已完成',RealDt=getdate() where
                   //ProblemID='{0}'", ActionID);
                }
                else
                {
                    strSql.AppendFormat(@"update FY_HrProblem set ProblemState='退回' where
                   ProblemID='{0}'", ActionID);
                }
            }
            if (ManageProvider.Provider.Current().UserId == entity.ResponseBy)
            {
                strSql.AppendFormat(@"update FY_HrProblem set ProblemState='待审',ProblemAction='{1}' where
                   ProblemID='{0}'", ActionID, ActionContent);
            }
            PostBll.ExecuteSql(strSql);


            return Content(new JsonMessage { Success = true, Code = 1.ToString(), Message = "操作成功。" }.ToString());
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
            Base_SysLogBll.Instance.WriteLog<FY_HrProblem>(array, IsOk.ToString(), Message);
        }

        public int RegistFlow(string KeyValue)
        {
            FY_HrProblem entity = DataFactory.Database().FindEntity<FY_HrProblem>(KeyValue);
            entity.FlowID =  FlowBll.RegistFlow("Sj_HrProblem", KeyValue,"");
            DataFactory.Database().Update<FY_HrProblem>(entity);
            return 1;

        }

        public int submit(string KeyValue, string type,string FlowID, FY_HrProblem entity, string BuildFormJson, HttpPostedFileBase Filedata)
        {

            int a = 0;
            StringBuilder strSql = new StringBuilder();
            //
            string sql = " select CurrentPost from Base_FlowLog where FlowID='"+FlowID+"'  ";
            DataTable dt = PostBll.GetDataTable(sql);
            if(dt.Rows[0][0].ToString()== "1538456f-0a50-4f1f-a6e2-5b3a5862f53a")
            {
                if(type!="-1"&&type!="-2")
                {
                    entity.RealReplyDt = DateTime.Now;
                    int realNum2 = Math.Abs(((TimeSpan)(entity.RealReplyDt - entity.CreateDt)).Days);
                    int planNum2 = Math.Abs(((TimeSpan)(entity.ReplyDt - entity.CreateDt)).Days);
                    if (planNum2 == 0)
                    {
                        planNum2 = 1;
                    }

                    entity.ReplyCompleteRate = Math.Round(((2.0 - (realNum2 / (planNum2 * 1.0))) * 100), 2).ToString() + "%";
                }
            }
            if(dt.Rows[0][0].ToString()== "14a798a0-2f9d-4b58-88a2-9b5860b80eb2")
            {
                if (type != "-1" && type != "-2")
                {
                    string GetReciverSql = " select Email from base_user where userid='" + entity.ResponseBy + "' ";
                    DataTable dt1 = PostBll.GetDataTable(GetReciverSql);
                    //把发送邮件功能写到unitity的静态类中，以后直接调用,此部分移动到第一个节点提交的时候触发
                    MailHelper.SendEmail(dt1.Rows[0][0].ToString(), "您好，您有一个问题需要处理，请注意登录系统点击人事模块查看问题。网址：172.19.0.5:8086");
                }
            }

            //FY_ObjectTracking entity = DataFactory.Database().FindEntity<FY_ObjectTracking>(KeyValue);
            if (type == "-1" || type == "-2")
            {
                a = FlowBll.RejectFlow(FlowID);
            }
            else
            {
                a = FlowBll.SubmitFlow(FlowID);
            }
//            if (a == 9)
//            {
//                strSql.AppendFormat(@"update FY_HrProblem set RealDt=GETDATE(),ProblemState='已完成',
//CompleteRate=cast(cast(100.0*datediff(day,CreateDt,PlanDt)/( case when datediff(day,CreateDt,RealDt)=0 then 1 else datediff(day,CreateDt,RealDt) end) as decimal(18,2)) as nvarchar(50))+'%' where FlowID='{0}' ",
//                    FlowID);
//            }

            //保存单据
            string ModuleId = DESEncrypt.Decrypt(CookieHelper.GetCookie("ModuleId"));
            IDatabase database = DataFactory.Database();
            DbTransaction isOpenTrans = database.BeginTrans();
            entity.Approvestatus = a;
            if(a==9)
            {
                entity.RealDt = DateTime.Now;

                int realNum = Math.Abs(((TimeSpan)(entity.RealDt - entity.CreateDt)).Days);
                int planNum = Math.Abs(((TimeSpan)(entity.PlanDt - entity.CreateDt)).Days);
                if(planNum==0)
                {
                    planNum = 1;
                }
                entity.CompleteRate = Math.Round(((2.0 - (realNum / (planNum * 1.0))) * 100), 2).ToString() + "%";
                entity.ProblemState = "已完成";

                
                

            }

            entity.Modify(KeyValue);


            database.Update(entity, isOpenTrans);
            Base_FormAttributeBll.Instance.SaveBuildForm(BuildFormJson, entity.ProblemID, ModuleId, isOpenTrans);
            database.Commit();


            PostBll.ExecuteSql(strSql);
            return a;
        }


        public ActionResult ResponseAllJson(string keywords)
        {
            
            string sql = " select distinct a.UserId,a.RealName from Base_User a  where 1=1 and Enabled=1 ";
            if (keywords != null || keywords != "undefined" || keywords != "")
            {
                sql = sql + " and a.realname like '%"+keywords+"%' ";
            }
            DataTable ListData = PostBll.GetDataTable(sql);
            return Content(ListData.ToJson());
        }


        //按需求导出excel
        public void ExcelExport(string type)
        {
            ExcelHelper ex = new ExcelHelper();
            string sql = @" select dbo.GetState(a.ProblemID) as 状态,a.ReplyCompleteRate as 回复及时率,
a.CompleteRate as 完成率,a.ProblemType as 问题大类,a.ProblemTypeD as 问题小类,a.ProblemDescripe as 问题描述,a.ProblemAction as 对策措施,
b.RealName as 责任人,(select top 1 FullName from Base_Department where DepartmentId=b.DepartmentId) as 责任部门,
c.RealName as 提出人,(select top 1 FullName from Base_Department where DepartmentId=c.DepartmentId) as 提出部门,
a.PlanDt as 计划完成日期,a.RealDt as 实际完成日期
from fy_hrproblem a 
left join Base_User b on a.ResponseBy=b.UserId 
left join Base_User c on a.CreateBy=c.UserId 
where 1=1 ";

            if (type == "1")
            {
                //sql.Append(" and ProblemType='员工咨询' ");
                sql = sql + " and ProblemType='员工咨询' ";
            }
            if (type == "2")
            {
                
                sql = sql + " and ProblemType='心声反馈' ";
            }
            if (type == "3")
            {
                
                sql = sql + " and ProblemType='员工投诉' ";
            }
            if (type == "4")
            {
                
                sql = sql + " and ProblemType='外部咨询' ";
            }

            DataTable ListData = PostBll.GetDataTable(sql);
            ex.EcportExcel(ListData, "员工关系导出");
        }

        public ActionResult UserList()
        {
            return View();
        }

        public ActionResult GetUserList()
        {
            StringBuilder sb = new StringBuilder();
            string sql = " select UserId,RealName+'('+Code+')' as UserName from Base_User where Enabled=1 ";
            DataTable dt = PostBll.GetDataTable(sql);
            foreach (DataRow dr in dt.Rows)
            {
                string strchecked = "";
                //都修改为不要选中的状态
                //if (!string.IsNullOrEmpty(dr["relationid"].ToString()))//判断是否选中
                //{
                //    strchecked = "selected";
                //}
                sb.Append("<li title=\"" + dr["UserName"] + "\" class=\"" + strchecked + "\">");
                sb.Append("<a id=\"" + dr["UserId"] + "\"><img src=\"../../Content/Images/Icon16/role.png \">" + dr["UserName"] + "</a><i></i>");
                sb.Append("</li>");
            }
            return Content(sb.ToString());
        }

        public ActionResult UserListSubmit(string DepartmentID, string ObjectId)
        {
            try
            {
                //IDatabase database = DataFactory.Database();
                //                StringBuilder strSql = new StringBuilder();
                //                string[] array = ObjectId.Split(',');


                //                for (int i = 0; i < array.Length - 1; i++)
                //                {
                //                    //strSql.AppendFormat(@"delete from TR_PostDepartmentRelation where departmentid='{0}' ", ManageProvider.Provider.Current().UserId);
                //                    strSql.AppendFormat(@" insert into TR_PostDepartmentRelation (relationid,postid,departmentid) 
                //values(NEWID(),'{0}','{1}') ", array[i], ManageProvider.Provider.Current().DepartmentId);
                //                }
                //                PostBll.ExecuteSql(strSql);
                string[] array = ObjectId.Split(',');
                if(array.Length>2)
                {
                    return Content(new JsonMessage { Success = true, Code = 1.ToString(), Message = "一次只能选择一个责任人。" }.ToString());
                }
                else
                {
                    string sql = " select * from base_user where userid='"+array[0]+ "' where Enabled=1 ";
                    DataTable dt = PostBll.GetDataTable(sql);
                    

                    StringBuilder strSql = new StringBuilder();
                    //修改表单里的责任人
                    strSql.AppendFormat(@" update FY_HrProblem set ResponseBy='{1}',ResponseByName='{2}' 
where ProblemID='{0}' ",DepartmentID,array[0],dt.Rows[0]["RealName"].ToString());

                    //修改流程中的责任人
                    strSql.AppendFormat(@"update Base_FlowLog set CurrentPerson='{0}' 
where CurrentPost in ('3efe8c99-fcaa-4efd-9523-ef0fa652557c','1538456f-0a50-4f1f-a6e2-5b3a5862f53a') and NoteID='{1}' ",
array[0],DepartmentID);
                    strSql.AppendFormat(@" update Base_FlowLogDetail set ApproveBy='{0}' 
where FlowID in (select FlowID from FY_HrProblem where ProblemID='{1}') 
and ApprovePost in ('3efe8c99-fcaa-4efd-9523-ef0fa652557c','1538456f-0a50-4f1f-a6e2-5b3a5862f53a')  ",array[0],DepartmentID);

                    PostBll.ExecuteSql(strSql);

                    return Content(new JsonMessage { Success = true, Code = 1.ToString(), Message = "操作成功。" }.ToString());
                }

                


            }
            catch (Exception ex)
            {
                return Content(new JsonMessage { Success = false, Code = "-1", Message = "操作失败，错误：" + ex.Message }.ToString());
            }


        }



    }
}
