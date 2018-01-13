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
    public class CustomExamController : Controller
    {
        RepositoryFactory<TR_CustomExam> repositoryfactory = new RepositoryFactory<TR_CustomExam>();
        RepositoryFactory<TR_CustomExamChoice> detailrepository = new RepositoryFactory<TR_CustomExamChoice>();
        TR_CustomExamBll CustomExamBll = new TR_CustomExamBll();
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
                DataTable ListData = CustomExamBll.GetPageList(keywords, ref jqgridparam, ParameterJson);
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
            ViewData["UserName"] = ManageProvider.Provider.Current().UserName;
            return View();
        }

        [HttpPost]
        public ActionResult SubmitForm(string KeyValue, TR_CustomExam entity, string BuildFormJson,
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
                    List<TR_CustomExamChoice> DetailList = DetailForm.JonsToList<TR_CustomExamChoice>();
                    foreach (TR_CustomExamChoice entityD in DetailList)
                    {
                        if (!string.IsNullOrEmpty(entityD.QuestionDescripe))
                        {
                            if (!string.IsNullOrEmpty(entityD.CustomExamChoiceID))
                            {
                                entityD.Modify(entityD.CustomExamChoiceID);
                                database.Update(entityD, isOpenTrans);
                                index++;
                            }
                            else
                            {
                                entityD.Create();
                                entityD.CustomExamID = entity.CustomExamID;
                               
                                database.Insert(entityD, isOpenTrans);
                                index++;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(entityD.CustomExamChoiceID))
                            {
                                detailrepository.Repository().Delete(entityD.CustomExamChoiceID);
                                
                            }
                        }
                    }


                    database.Update(entity, isOpenTrans);

                }
                else
                {

                    entity.Create();
                    int index = 1;
                    //List<TR_CustomExamChoice> DetailList = DetailForm.JonsToList<TR_CustomExamChoice>();
                    //foreach (TR_CustomExamChoice entityD in DetailList)
                    //{
                    //    if (!string.IsNullOrEmpty(entityD.QuestionDescripe))
                    //    {
                    //        if (!string.IsNullOrEmpty(entityD.CustomExamChoiceID))
                    //        {
                    //            entityD.Modify(entityD.CustomExamChoiceID);
                    //            database.Update(entity, isOpenTrans);
                    //            index++;
                    //        }
                    //        else
                    //        {
                    //            entityD.Create();
                    //            entityD.CustomExamID = entity.CustomExamID;

                    //            database.Insert(entityD, isOpenTrans);
                    //            index++;
                    //        }
                    //    }
                    //}

                    database.Insert(entity, isOpenTrans);

                    Base_DataScopePermissionBll.Instance.AddScopeDefault(ModuleId, ManageProvider.Provider.Current().UserId, entity.CustomExamID, isOpenTrans);
                }
                Base_FormAttributeBll.Instance.SaveBuildForm(BuildFormJson, entity.CustomExamID, ModuleId, isOpenTrans);
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
            TR_CustomExam entity = DataFactory.Database().FindEntity<TR_CustomExam>(KeyValue);
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

                StringBuilder strSql = new StringBuilder();
                strSql.AppendFormat(" update tr_customexam set IsEnable=0 where CustomExamID = '{0}' ", KeyValue);
                IsOk = CustomExamBll.ExecuteSql(strSql);

               // IsOk = repositoryfactory.Repository().Delete(KeyValue);
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
            Base_SysLogBll.Instance.WriteLog<TR_CustomExam>(array, IsOk.ToString(), Message);
        }

        public ActionResult AddExamUser(string CustomExamID)
        {
            return View();
        }


        public ActionResult GetUserList(string CustomExamID)
        {
            StringBuilder sb = new StringBuilder();
            string sql = "  select * from Base_User a left join TR_CustomExamUser b on a.UserId=b.UserID and b.CustomExamID='" + CustomExamID + "' where a.Enabled=1 ";
            DataTable dt = CustomExamBll.GetDataTable(sql);
            foreach (DataRow dr in dt.Rows)
            {
                string strchecked = "";
                if (!string.IsNullOrEmpty(dr["CustomExamID"].ToString()))//判断是否选中
                {
                    strchecked = "selected";
                }
                sb.Append("<li title=\"" + dr["RealName"] + "(" + dr["Code"] + ")" + "\" class=\"" + strchecked + "\">");
                sb.Append("<a id=\"" + dr["UserId"] + "\"><img src=\"../../Content/Images/Icon16/role.png \">" + dr["RealName"] + "</a><i></i>");
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
        public ActionResult UserListSubmit(string CustomExamID, string ObjectId)
        {
            try
            {

                StringBuilder strSql = new StringBuilder();
                string[] array = ObjectId.Split(',');

                strSql.AppendFormat(@"delete from TR_CustomExamUser where CustomExamID='{0}' ", CustomExamID);

                for (int i = 0; i < array.Length - 1; i++)
                {
                    strSql.AppendFormat(@"insert into TR_CustomExamUser values(NEWID(),'{0}','{1}')", CustomExamID, array[i]);
                }
                CustomExamBll.ExecuteSql(strSql);
                return Content(new JsonMessage { Success = true, Code = 1.ToString(), Message = "操作成功。" }.ToString());

            }
            catch (Exception ex)
            {
                return Content(new JsonMessage { Success = false, Code = "-1", Message = "操作失败，错误：" + ex.Message }.ToString());
            }
        }

        public ActionResult SetExamUser(string KeyValue)
        {
            string result = "";
            string sql = "select b.RealName from TR_CustomExamUser a left join Base_User b on a.UserID=b.UserId where a.CustomExamID='" + KeyValue + "'";
            DataTable dt = CustomExamBll.GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    result += dt.Rows[i][0].ToString() + ",";
                }
                result = result.Substring(0, result.Length - 1);
            }
            //return result;
            return Content(new JsonMessage { Success = true, Code = 1.ToString(), Content = result, Message = "操作成功。" }.ToString());
        }

        public ActionResult AddExamSkill(string CustomExamID)
        {
            return View();
        }


        public ActionResult GetSkillList(string CustomExamID)
        {
            StringBuilder sb = new StringBuilder();
            string sql = "  select a.*,b.CustomExamID from TR_Skill a left join TR_CustomExamSkill b on a.SkillID=b.skillid and b.CustomExamID='" + CustomExamID+"' ";
            DataTable dt = CustomExamBll.GetDataTable(sql);
            foreach (DataRow dr in dt.Rows)
            {
                string strchecked = "";
                if (!string.IsNullOrEmpty(dr["CustomExamID"].ToString()))//判断是否选中
                {
                    strchecked = "selected";
                }
                sb.Append("<li title=\"" + dr["SkillName"]  + "\" class=\"" + strchecked + "\">");
                sb.Append("<a id=\"" + dr["SkillID"] + "\"><img src=\"../../Content/Images/Icon16/role.png \">" + dr["SkillName"] + "</a><i></i>");
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
        public ActionResult SkillListSubmit(string CustomExamID, string ObjectId)
        {
            try
            {

                StringBuilder strSql = new StringBuilder();
                string[] array = ObjectId.Split(',');

                strSql.AppendFormat(@"delete from TR_CustomExamSkill where CustomExamID='{0}' ", CustomExamID);

                for (int i = 0; i < array.Length - 1; i++)
                {
                    strSql.AppendFormat(@"insert into TR_CustomExamSkill values(NEWID(),'{0}','{1}')", CustomExamID, array[i]);
                }
                CustomExamBll.ExecuteSql(strSql);
                return Content(new JsonMessage { Success = true, Code = 1.ToString(), Message = "操作成功。" }.ToString());

            }
            catch (Exception ex)
            {
                return Content(new JsonMessage { Success = false, Code = "-1", Message = "操作失败，错误：" + ex.Message }.ToString());
            }
        }

        public ActionResult SetExamSkill(string KeyValue)
        {
            string result = "";
            string sql = "select b.Skillname from TR_CustomExamSkill a left join TR_Skill b on a.SkillID=b.SkillID where a.CustomExamID='" + KeyValue + "'";
            DataTable dt = CustomExamBll.GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    result += dt.Rows[i][0].ToString() + ",";
                }
                result = result.Substring(0, result.Length - 1);
            }
            //return result;
            return Content(new JsonMessage { Success = true, Code = 1.ToString(), Content = result, Message = "操作成功。" }.ToString());
        }

        public ActionResult FormNew()
        {
            return View();
        }



        public ActionResult MyExam()
        {
            return View();
        }

        public ActionResult GridExamListJson(string keywords, string CompanyId, string DepartmentId,
            JqGridParam jqgridparam, string ParameterJson)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = CustomExamBll.GetExamList(keywords, ref jqgridparam, ParameterJson);
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

        public ActionResult ExamForm(string KeyValue)
        {
            //获取上次的考试时间
            string IsExam = "-1";
            string sqlDateSpan = " select * from TR_Paper where KnowledgeBaseID='" + KeyValue + "' and UserID='" + ManageProvider.Provider.Current().UserId + "' order by PaperDate desc ";
            DataTable dt5 = CustomExamBll.GetDataTable(sqlDateSpan);
            if (dt5.Rows.Count > 0)
            {
                IsExam = "1";
            }
            ViewData["IsExam"] = IsExam;

            string sqlInfo = " select *,ExamMinutes as ExamTime from TR_CustomExam  where CustomExamID='" + KeyValue + "' ";
            DataTable dt3 = CustomExamBll.GetDataTable(sqlInfo);

            string ChoiceCount = dt3.Rows[0]["ChoiceQuestion"].ToString();
            string JudgmentCount = dt3.Rows[0]["JudgmentQuestion"].ToString();

            //string sql = " select top " + ChoiceCount + " * from TR_ChoiceQuestion where SkillID in(select SkillID from TR_CustomExamSkill where CustomExamID='" + KeyValue + "')  order by newid()  ";
            //string sql2 = " select top " + JudgmentCount + " * from TR_JudgmentQuestion where SkillID in(select SkillID from TR_CustomExamSkill where CustomExamID='" + KeyValue + "')  order by newid() ";
            //DataTable dt = CustomExamBll.GetDataTable(sql);
            //DataTable dt2 = CustomExamBll.GetDataTable(sql2);

            //IList<TR_ChoiceQuestion> Choicelist = DtConvertHelper.ConvertToModelList<TR_ChoiceQuestion>(dt);
            //IList<TR_JudgmentQuestion> JudgmentList = DtConvertHelper.ConvertToModelList<TR_JudgmentQuestion>(dt2);

            //ViewData["ChoiceList"] = Choicelist;
            //ViewData["JudgmentList"] = JudgmentList;

            //此部分移动到最上面，根据设定的题目数，随机从题库中抽选题目
            //string sqlInfo = " select b.SkillName from TR_KnowledgeBase a left join TR_Skill b on a.SkillID=b.SkillID where KnowledgeBaseID='"+KeyValue+"' ";
            //DataTable dt3 = KnowledgeBaseBll.GetDataTable(sqlInfo);

            TR_PersonalInfo pp = new TR_PersonalInfo();
            pp.ExamName = dt3.Rows[0]["CustomeExamName"].ToString();
            pp.UserName = ManageProvider.Provider.Current().UserName;
            pp.UserCode = ManageProvider.Provider.Current().Code;
            if (Convert.ToInt32(dt3.Rows[0]["ExamTime"].ToString()) > 60)
            {
                pp.ExamTime = "60";
            }
            else
            {
                pp.ExamTime = dt3.Rows[0]["ExamTime"].ToString();
            }
            pp.StartTime = DateTime.Now;
            pp.EndTime = DateTime.Now.AddMinutes(Convert.ToDouble(pp.ExamTime));

            ViewData["Info"] = pp;
            return View();
        }

        public ActionResult SubmitExam(string KeyValue, string DetailForm)
        {
            IDatabase database = DataFactory.Database();
            DbTransaction isOpenTrans = database.BeginTrans();

            //先建立一个主表
            TR_Paper paper = new TR_Paper();
            paper.KnowledgeBaseID = KeyValue;
            paper.SkillID = KeyValue;
            paper.FromSource = 1; //表示是自定义的学习考试
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
where PaperID='{0}'", paper.PaperID);

            strSql.AppendFormat(@"update TR_PaperDetail set Istrue=1 where answer=trueanswer and paperid='{0}' ", paper.PaperID);
            strSql.AppendFormat(@"update TR_Paper set score=(select dbo.CountScore('{0}')) where PaperID='{0}' ", paper.PaperID);

            CustomExamBll.ExecuteSql(strSql);

            string sql = " select score from tr_paper where paperid='" + paper.PaperID + "' ";
            string Score = CustomExamBll.GetDataTable(sql).Rows[0][0].ToString();
            Score = "您好！您的本次考试成绩为" + Score;
            return Content(new JsonMessage { Success = true, Code = "1", Message = "保存成功", Content = Score }.ToString());
        }


        public ActionResult GetDetailList(string KeyValue)
        {
            try
            {
                var JsonData = new
                {
                    rows = CustomExamBll.GetDetailList(KeyValue),
                };
                return Content(JsonData.ToJson());
            }
            catch (Exception ex)
            {
                Base_SysLogBll.Instance.WriteLog("", OperationType.Query, "-1", "异常错误：" + ex.Message);
                return null;
            }
        }

        public ActionResult GetPaper(string KeyValue)
        {
           //string sqlInfo = " select b.SkillName,a.ChoiceQuestion,a.JudgmentQuestion,a.ExamMinutes from TR_KnowledgeBase a left join TR_Skill b on a.SkillID=b.SkillID where KnowledgeBaseID='" + KeyValue + "' ";

            string sqlInfo = " select *,DATEDIFF(MINUTE,StartTime,EndTime) as ExamTime from TR_CustomExam  where CustomExamID='" + KeyValue + "' ";
            DataTable dt3 = CustomExamBll.GetDataTable(sqlInfo);

            string ChoiceCount = dt3.Rows[0]["ChoiceQuestion"].ToString();
            string JudgmentCount = dt3.Rows[0]["JudgmentQuestion"].ToString();

            string sql = " select top " + ChoiceCount + " * from TR_CustomExamChoice where CustomExamID ='" + KeyValue + "' and IsEnable=1 order by newid()  ";
            string sql2 = " select top " + JudgmentCount + " * from TR_JudgmentQuestion where SkillID in(select SkillID from TR_CustomExamSkill where CustomExamID='" + KeyValue + "')  order by newid() ";
            DataTable dt = CustomExamBll.GetDataTable(sql);
            //DataTable dt2 = CustomExamBll.GetDataTable(sql2);

            return Content(dt.ToJson());
        }

        public ActionResult UserForm()
        {
            return View();
        }

        public ActionResult GeUserList(string KeyValue)
        {
            try
            {
                var JsonData = new
                {
                    rows = CustomExamBll.GetUserList(KeyValue),
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
           JqGridParam jqgridparam, string ParameterJson)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = CustomExamBll.GetBaseUserList(keywords, ref jqgridparam, ParameterJson);
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

        public ActionResult InsertUser(string KeyValue,string UserID)
        {
            string[] UserIDArr = UserID.Split(',');
            StringBuilder strSql = new StringBuilder();
            for(int i=0;i<UserIDArr.Length;i++)
            {
                strSql.AppendFormat(@" insert into TR_CustomExamUser values(NEWID(),'{0}','{1}') ",KeyValue,UserIDArr[i]);
            }
            strSql.AppendFormat(@" delete from TR_CustomExamUser where CustomExamID='{0}'
and UserID in (select UserID from TR_CustomExamUser where CustomExamID='{0}' 
group by UserID having count(UserID) > 1)
and   CustomExamUserID not in (select min(CustomExamUserID)  from TR_CustomExamUser 
where CustomExamID='{0}' group by UserID     having count(UserID)>1)  ", KeyValue);
            CustomExamBll.ExecuteSql(strSql);
            return Content(new JsonMessage { Success = true, Code = "1", Message = "添加成功" }.ToString());
        }

        public ActionResult DeleteUser(string KeyValue, string UserID)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(@" delete from TR_CustomExamUser where CustomExamID='{0}' and 
UserID='{1}' ", KeyValue,UserID);
            CustomExamBll.ExecuteSql(strSql);
            return Content(new JsonMessage { Success = true, Code = "1", Message = "删除成功" }.ToString());

        }

        public ActionResult DeptListBatch()
        {
            return View();
        }

        public ActionResult GetDeptListJson(string keywords, string CompanyId, string DepartmentId,
          JqGridParam jqgridparam, string ParameterJson)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = CustomExamBll.GetDeptList(keywords, ref jqgridparam, ParameterJson);
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

        public ActionResult InsertUserOnDept(string KeyValue, string DepartID)
        {
            string[] DepartIDArr = DepartID.Split(',');
            StringBuilder strSql = new StringBuilder();
            for (int i = 0; i < DepartIDArr.Length; i++)
            {
                strSql.AppendFormat(@" insert into TR_CustomExamUser select newid(),'{0}',userid from 
base_user where departmentid='{1}' ", KeyValue, DepartIDArr[i]);
            }
            strSql.AppendFormat(@" delete from TR_CustomExamUser where CustomExamID='{0}'
and UserID in (select UserID from TR_CustomExamUser where CustomExamID='{0}' 
group by UserID having count(UserID) > 1)
and   CustomExamUserID not in (select min(CustomExamUserID)  from TR_CustomExamUser 
where CustomExamID='{0}' group by UserID     having count(UserID)>1)  ", KeyValue);
            CustomExamBll.ExecuteSql(strSql);
            return Content(new JsonMessage { Success = true, Code = "1", Message = "添加成功" }.ToString());
        }

        public ActionResult CodeForm(string StartCode,string EndCode)
        {
            return View();
        }

        public ActionResult InsertUserOnCode(string KeyValue,string StartCode,string EndCode)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(@" insert into TR_CustomExamUser select newid(),'{0}',userid from 
base_user where  Code>='{1}' and Code <='{2}' ", KeyValue,StartCode,EndCode);

            strSql.AppendFormat(@" delete from TR_CustomExamUser where CustomExamID='{0}'
and UserID in (select UserID from TR_CustomExamUser where CustomExamID='{0}' 
group by UserID having count(UserID) > 1)
and   CustomExamUserID not in (select min(CustomExamUserID)  from TR_CustomExamUser 
where CustomExamID='{0}' group by UserID     having count(UserID)>1)  ",KeyValue);
            CustomExamBll.ExecuteSql(strSql);
            return Content(new JsonMessage { Success = true, Code = "1", Message = "添加成功" }.ToString());
        }


        public string ExamApply(string CustomExamID)
        {
            StringBuilder strSql = new StringBuilder();

            string sql = " select * from tr_examapply where examid='" + CustomExamID + "' and userid='" + ManageProvider.Provider.Current().UserId + "' ";
            sql += " and IsOK in (0,1) and source=1 ";

            DataTable dt = CustomExamBll.GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                //表示已经有待申请记录了，不能重复申请
                return "-1";
            }

            strSql.AppendFormat(@" insert into tr_examapply(applyid,userid,examid,source,applydate,isok) values(newid(),'{0}','{1}',1,getdate(),0) "
, ManageProvider.Provider.Current().UserId, CustomExamID);
            CustomExamBll.ExecuteSql(strSql);
            return "0";
        }

        public string IsApply(string CustomExamID)
        {
            string sql = " select * from tr_examapply  where examid='" + CustomExamID + "' and userid='" + ManageProvider.Provider.Current().UserId + "' ";
            sql += " and IsOK in (0,1) and source=1 ";

            DataTable dt = CustomExamBll.GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                //将状态修改成2，防止刷题
                StringBuilder strSql = new StringBuilder();
                strSql.AppendFormat(@" update tr_examapply  set IsOK = 2 where examid='{0}' and userid='{1}'  and IsOK=1 and source=1 ", CustomExamID, ManageProvider.Provider.Current().UserId);
                CustomExamBll.ExecuteSql(strSql);
                //可以正常进入考试
                return "0";
            }
            else
            {
                //请先提交申请，并且等待申请通过
                return "-1";
            }

            //return "0";
        }



    }
}
