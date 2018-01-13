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

namespace LeaRun.WebApp.Areas.TrainModule.Controllers
{
    public class KnowledgeBaseController : Controller
    {
        RepositoryFactory<TR_KnowledgeBase> repositoryfactory = new RepositoryFactory<TR_KnowledgeBase>();
        TR_KnowledgeBaseBll KnowledgeBaseBll = new TR_KnowledgeBaseBll();
        //
        // GET: /FYModule/Process/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GridPageListJson(string keywords, string CompanyId, string DepartmentId,
            JqGridParam jqgridparam, string ParameterJson)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = KnowledgeBaseBll.GetPageList(keywords, ref jqgridparam, ParameterJson);
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
        public ActionResult SubmitForm(string KeyValue, TR_KnowledgeBase entity, string BuildFormJson, HttpPostedFileBase Filedata)
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
                    string sql = " select * from TR_KnowledgeBase where SkillID='"+entity.SkillID+"' ";
                    DataTable dt = KnowledgeBaseBll.GetDataTable(sql);
                    if(dt.Rows.Count>0)
                    {
                        return Content(new JsonMessage { Success = false, Code = "-1", Message = "操作失败：该技能已存在对应的知识库" }.ToString());
                    }

                    entity.Create();


                    database.Insert(entity, isOpenTrans);

                    Base_DataScopePermissionBll.Instance.AddScopeDefault(ModuleId, ManageProvider.Provider.Current().UserId, entity.KnowledgeBaseID, isOpenTrans);
                }
                Base_FormAttributeBll.Instance.SaveBuildForm(BuildFormJson, entity.KnowledgeBaseID, ModuleId, isOpenTrans);
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
            TR_KnowledgeBase entity = DataFactory.Database().FindEntity<TR_KnowledgeBase>(KeyValue);
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
            Base_SysLogBll.Instance.WriteLog<TR_KnowledgeBase>(array, IsOk.ToString(), Message);
        }


        public ActionResult SkillJson()
        {
            string sql = " select SkillID,SkillName from TR_Skill where 1=1  ";
            DataTable dt = KnowledgeBaseBll.GetDataTable(sql);
            return Content(dt.ToJson());
        }

        public ActionResult SubmitUploadify(string FolderId, HttpPostedFileBase Filedata, string type)
        {
            try
            {

                Thread.Sleep(1000);////延迟500毫秒
                Base_NetworkFile entity = new Base_NetworkFile();
                TR_KnowledgeBase PAentity = DataFactory.Database().FindEntity<TR_KnowledgeBase>(FolderId);

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

                string virtualPath = string.Format("~/Content/Scripts/pdf.js/generic/web/{0}{1}", fileGuid, FileEextension);
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
                        
                        PAentity.Attach = virtualPath;
                        PAentity.AttachName = fileGuid + FileEextension;

                        DataFactory.Database().Update<TR_KnowledgeBase>(PAentity);
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


        public ActionResult SubmitVideo(string FolderId, HttpPostedFileBase Filedata, string type)
        {
            try
            {

                Thread.Sleep(1000);////延迟500毫秒
                Base_NetworkFile entity = new Base_NetworkFile();
                TR_KnowledgeBase PAentity = DataFactory.Database().FindEntity<TR_KnowledgeBase>(FolderId);

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

                string virtualPath = string.Format("~/Resource/Document/NetworkDisk/TrainVideo/{0}{1}", fileGuid, FileEextension);
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

                        PAentity.VideoSrc = fileGuid + FileEextension;
                        

                        DataFactory.Database().Update<TR_KnowledgeBase>(PAentity);
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
        public ActionResult MyIndex()
        {
            return View();
        }

        //GridMyListJson
        public ActionResult GridMyListJson(string keywords, string CompanyId, string DepartmentId,
           JqGridParam jqgridparam, string ParameterJson)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = KnowledgeBaseBll.GetMyList(keywords, ref jqgridparam, ParameterJson);
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

        public ActionResult StudyForm()
        {
            return View();
        }

        public ActionResult ExamForm(string KeyValue)
        {
            //获取上次的考试时间
            string DateSpan = "-1";
            string sqlDateSpan = " select top 1 DATEDIFF(DAY,PaperDate,GETDATE()) from TR_Paper where KnowledgeBaseID='"+KeyValue+"' and UserID='"+ManageProvider.Provider.Current().UserId+"' order by PaperDate desc ";
            DataTable dt5 = KnowledgeBaseBll.GetDataTable(sqlDateSpan);
            if(dt5.Rows.Count>0)
            {
                DateSpan = dt5.Rows[0][0].ToString();
            }
            ViewData["DateSpan"] = DateSpan;

            string sqlInfo = " select b.SkillName,a.ChoiceQuestion,a.JudgmentQuestion,a.ExamMinutes from TR_KnowledgeBase a left join TR_Skill b on a.SkillID=b.SkillID where KnowledgeBaseID='" + KeyValue + "' ";
            DataTable dt3 = KnowledgeBaseBll.GetDataTable(sqlInfo);

            string ChoiceCount=dt3.Rows[0][1].ToString();
            string JudgmentCount = dt3.Rows[0][2].ToString();

           // string sql = " select top "+ ChoiceCount +" * from TR_ChoiceQuestion where SkillID in(select SkillID from TR_KnowledgeBase where KnowledgeBaseID='"+KeyValue+ "')  order by newid()  ";
           //// string sql2 = " select top "+JudgmentCount +" * from TR_JudgmentQuestion where SkillID in(select SkillID from TR_KnowledgeBase where KnowledgeBaseID='" + KeyValue+ "')  order by newid() ";
           // DataTable dt = KnowledgeBaseBll.GetDataTable(sql);
            //DataTable dt2 = KnowledgeBaseBll.GetDataTable(sql2);
            
            
            //IList<TR_ChoiceQuestion> Choicelist = DtConvertHelper.ConvertToModelList<TR_ChoiceQuestion>(dt);
            //IList<TR_JudgmentQuestion> JudgmentList = DtConvertHelper.ConvertToModelList<TR_JudgmentQuestion>(dt2);

            //ViewData["ChoiceList"] = Choicelist;
            //ViewData["JudgmentList"] = JudgmentList;

            //此部分移动到最上面，根据设定的题目数，随机从题库中抽选题目
            //string sqlInfo = " select b.SkillName from TR_KnowledgeBase a left join TR_Skill b on a.SkillID=b.SkillID where KnowledgeBaseID='"+KeyValue+"' ";
            //DataTable dt3 = KnowledgeBaseBll.GetDataTable(sqlInfo);

            TR_PersonalInfo pp = new TR_PersonalInfo();
            pp.ExamName = dt3.Rows[0][0].ToString();
            pp.UserName = ManageProvider.Provider.Current().UserName;
            pp.UserCode = ManageProvider.Provider.Current().Code;
            pp.ExamTime = dt3.Rows[0][3].ToString();
            pp.StartTime = DateTime.Now;
            pp.EndTime = DateTime.Now.AddMinutes(Convert.ToDouble(pp.ExamTime));

            ViewData["Info"] = pp;

            return View();
        }


        public ActionResult SubmitExam(string KeyValue,string DetailForm)
        {
            IDatabase database = DataFactory.Database();
            DbTransaction isOpenTrans = database.BeginTrans();

            //先建立一个主表
            TR_Paper paper = new TR_Paper();
            paper.KnowledgeBaseID = KeyValue;
            paper.FromSource = 0; //表示是常规学习考试
            paper.Create();


            database.Insert(paper, isOpenTrans);


            List<TR_PaperDetail> POOrderEntryList = DetailForm.JonsToList<TR_PaperDetail>();
            int index = 1;
            foreach (TR_PaperDetail entry in POOrderEntryList)
            {


                    entry.Create();
                    entry.PaperID = paper.PaperID;
                    database.Insert(entry, isOpenTrans);
                    index++;
                
            }
            database.Commit();

            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(@"update TR_PaperDetail set TrueAnswer=(select Answer from Questions where QuestionID=TR_PaperDetail.QuestionID) 
where PaperID='{0}'",paper.PaperID);

            strSql.AppendFormat(@"update TR_PaperDetail set Istrue=1 where answer=trueanswer and paperid='{0}' ", paper.PaperID);
            strSql.AppendFormat(@"update TR_Paper set score=(select dbo.CountScore('{0}')) where PaperID='{0}' ",paper.PaperID);

            KnowledgeBaseBll.ExecuteSql(strSql);

            string sql = " select score from tr_paper where paperid='"+paper.PaperID+"' ";
            string Score = KnowledgeBaseBll.GetDataTable(sql).Rows[0][0].ToString();
            Score = "您好！您的本次考试成绩为"+Score;
            return Content(new JsonMessage { Success = true, Code = "1", Message = "保存成功",Content=Score }.ToString());
        }


        public ActionResult HistoryIndex()
        {
            return View();
        }


        public ActionResult GridHistoryListJson(string keywords, string CompanyId, string DepartmentId,
           JqGridParam jqgridparam, string ParameterJson)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = KnowledgeBaseBll.GetHistoryList(keywords, ref jqgridparam, ParameterJson);
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

        public ActionResult HistoryExamForm(string KeyValue,string Source)
        {

            string sql = @" select b.QuestionDescripe,b.Option1,b.Option2,b.Option3,b.Option4,b.Option5,b.Option6,a.Answer,a.TrueAnswer,a.IsTrue 
from TR_PaperDetail a inner join TR_ChoiceQuestion b on a.QuestionID=b.QuestionID
where a.PaperID='" + KeyValue+"' ";

            string sql2 = @" select b.QuestionDescripe,a.Answer,a.TrueAnswer,a.IsTrue 
from TR_PaperDetail a inner join TR_JudgmentQuestion b on a.QuestionID=b.QuestionID
where a.PaperID='"+KeyValue+"' ";
            if(Source=="1")
            {
                //表示来源是自定义的考试
                sql = @" select b.QuestionDescripe,b.Option1,b.Option2,b.Option3,b.Option4,b.Option5,b.Option6,a.Answer,a.TrueAnswer,a.IsTrue 
from TR_PaperDetail a inner join TR_CustomExamChoice b on a.QuestionID=b.CustomExamChoiceID
where a.PaperID='" + KeyValue + "' ";
            }
            DataTable dt = KnowledgeBaseBll.GetDataTable(sql);
            DataTable dt2 = KnowledgeBaseBll.GetDataTable(sql2);

            IList<TR_ChoiceQuestionDisplay> Choicelist = DtConvertHelper.ConvertToModelList<TR_ChoiceQuestionDisplay>(dt);
            IList<TR_JudgmentQuestionDisplay> JudgmentList = DtConvertHelper.ConvertToModelList<TR_JudgmentQuestionDisplay>(dt2);

            ViewData["ChoiceList"] = Choicelist;
            ViewData["JudgmentList"] = JudgmentList;
            string sql3 = " select score from tr_paper where PaperID='"+KeyValue+"' ";
            DataTable dt3 = KnowledgeBaseBll.GetDataTable(sql3);

            TR_PersonalInfo pp = new TR_PersonalInfo();
            pp.UserCode = ManageProvider.Provider.Current().Code;
            pp.UserName = ManageProvider.Provider.Current().UserName;
            pp.Score = dt3.Rows[0][0].ToString();
            ViewData["Info"] = pp;

            return View();
        }

        public ActionResult viewer(string file)
        {
            return View();
        }

        public ActionResult GetPaper(string KeyValue)
        {
            string sqlInfo = " select b.SkillName,a.ChoiceQuestion,a.JudgmentQuestion,a.ExamMinutes from TR_KnowledgeBase a left join TR_Skill b on a.SkillID=b.SkillID where KnowledgeBaseID='" + KeyValue + "' ";
            DataTable dt3 = KnowledgeBaseBll.GetDataTable(sqlInfo);

            string ChoiceCount = dt3.Rows[0][1].ToString();
            string JudgmentCount = dt3.Rows[0][2].ToString();

            string sql = " select top " + ChoiceCount + " * from TR_ChoiceQuestion where SkillID in(select SkillID from TR_KnowledgeBase where KnowledgeBaseID='" + KeyValue + "')  order by newid()  ";
            // string sql2 = " select top "+JudgmentCount +" * from TR_JudgmentQuestion where SkillID in(select SkillID from TR_KnowledgeBase where KnowledgeBaseID='" + KeyValue+ "')  order by newid() ";
            DataTable dt = KnowledgeBaseBll.GetDataTable(sql);
            return Content(dt.ToJson());
        }

        public ActionResult HistoryExamFormNew(string KeyValue)
        {
            string sql3 = " select score from tr_paper where PaperID='" + KeyValue + "' ";
            DataTable dt3 = KnowledgeBaseBll.GetDataTable(sql3);

            TR_PersonalInfo pp = new TR_PersonalInfo();
            pp.UserCode = ManageProvider.Provider.Current().Code;
            pp.UserName = ManageProvider.Provider.Current().UserName;
            pp.Score = dt3.Rows[0][0].ToString();
            ViewData["Info"] = pp;
            return View();
        }

        public ActionResult GetHistoryExam(string KeyValue, string Source)
        {
            string sql = @" select b.QuestionType,b.QuestionDescripe,b.Option1,b.Option2,b.Option3,b.Option4,b.Option5,b.Option6,a.Answer,a.TrueAnswer,a.IsTrue 
from TR_PaperDetail a inner join TR_ChoiceQuestion b on a.QuestionID=b.QuestionID
where a.PaperID='" + KeyValue + "' ";

           
            if (Source == "1")
            {
                //表示来源是自定义的考试
                sql = @" select b.QuestionType,b.QuestionDescripe,b.Option1,b.Option2,b.Option3,b.Option4,'' as Option5,'' as Option6,a.Answer,a.TrueAnswer,a.IsTrue 
from TR_PaperDetail a inner join TR_CustomExamChoice b on a.QuestionID=b.CustomExamChoiceID
where a.PaperID='" + KeyValue + "' ";
            }
            DataTable dt = KnowledgeBaseBll.GetDataTable(sql);
            return Content(dt.ToJson());
        }

        public ActionResult ViewVideo(string VideoSrc)
        {
            VideoSrc= VideoSrc.Replace("~", "../../../../..");
            ViewData["VideoSrc"] = "../../../../../Resource/Document/NetworkDisk/TrainVideo/"+VideoSrc;
            //ViewData["VideoSrc"] = VideoSrc;
            return View();
        }


    }
}
