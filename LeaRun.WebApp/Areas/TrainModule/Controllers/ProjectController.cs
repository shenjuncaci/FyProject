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
    public class ProjectController : Controller
    {
        RepositoryFactory<TR_Project> repositoryfactory = new RepositoryFactory<TR_Project>();
        TR_ProjectBll ProjectBll = new TR_ProjectBll();
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
            //Base_NoteNOBll notenobll = new Base_NoteNOBll();
            //string KeyValue = Request["KeyValue"];
            //if (string.IsNullOrEmpty(KeyValue))
            //{
            //    ViewBag.BillNo = notenobll.Code("ProjectNO");
            //    ViewBag.CreateUserName = ManageProvider.Provider.Current().UserName;
            //}
            return View();
        }

        [HttpPost]
        public ActionResult SubmitForm(string KeyValue, TR_Project entity, string BuildFormJson, HttpPostedFileBase Filedata, string DetailForm)
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

                    database.Delete<TR_ProjectDetail>("ProjectID", KeyValue, isOpenTrans);
                    List<TR_ProjectDetail> DetailList = DetailForm.JonsToList<TR_ProjectDetail>();
                    int index = 1;
                    foreach (TR_ProjectDetail entityD in DetailList)
                    {
                        if (!string.IsNullOrEmpty(entityD.SkillID))
                        {
                            entityD.Create();
                            entityD.ProjectID = KeyValue;
                            database.Insert(entityD, isOpenTrans);
                            index++;
                        }
                    }
                    //database.Commit();


                    database.Update(entity, isOpenTrans);

                }
                else
                {

                    entity.Create();

                    List<TR_ProjectDetail> DetailList = DetailForm.JonsToList<TR_ProjectDetail>();
                    int index = 1;
                    foreach (TR_ProjectDetail entityD in DetailList)
                    {
                        if (!string.IsNullOrEmpty(entityD.SkillID))
                        {
                            entityD.Create();
                            entityD.ProjectID = entity.ProjectID;
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

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SetForm(string KeyValue)
        {
            TR_Project entity = DataFactory.Database().FindEntity<TR_Project>(KeyValue);
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
            Base_SysLogBll.Instance.WriteLog<TR_Project>(array, IsOk.ToString(), Message);
        }

        public ActionResult DepartmentJson()
        {
            string sql = " select departmentid,fullname from base_department where 1=1 order by FullName ";
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

        public ActionResult SkillListBatch()
        {
            return View();
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
                    rows = ProjectBll.GetUserList(KeyValue),
                };
                return Content(JsonData.ToJson());
            }
            catch (Exception ex)
            {
                Base_SysLogBll.Instance.WriteLog("", OperationType.Query, "-1", "异常错误：" + ex.Message);
                return null;
            }
        }

        public ActionResult DeleteUser(string KeyValue, string UserID)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(@" delete from TR_ProjectMember where ProjectID='{0}' and 
UserID='{1}' ", KeyValue, UserID);
            ProjectBll.ExecuteSql(strSql);
            return Content(new JsonMessage { Success = true, Code = "1", Message = "删除成功" }.ToString());

        }

        public ActionResult InsertUser(string KeyValue, string UserID)
        {
            string[] UserIDArr = UserID.Split(',');
            StringBuilder strSql = new StringBuilder();
            for (int i = 0; i < UserIDArr.Length; i++)
            {
                strSql.AppendFormat(@" insert into TR_ProjectMember values(NEWID(),'{0}','{1}',0) ", KeyValue, UserIDArr[i]);
            }
            strSql.AppendFormat(@" delete from TR_ProjectMember where ProjectID='{0}'
and UserID in (select UserID from TR_ProjectMember where ProjectID='{0}' 
group by UserID having count(UserID) > 1)
and   ProjectMemberID not in (select min(ProjectMemberID)  from TR_ProjectMember 
where ProjectID='{0}' group by UserID     having count(UserID)>1)  ", KeyValue);
            ProjectBll.ExecuteSql(strSql);
            return Content(new JsonMessage { Success = true, Code = "1", Message = "添加成功" }.ToString());
        }


        public ActionResult InsertUserOnDept(string KeyValue, string DepartID)
        {
            string[] DepartIDArr = DepartID.Split(',');
            StringBuilder strSql = new StringBuilder();
            for (int i = 0; i < DepartIDArr.Length; i++)
            {
                strSql.AppendFormat(@" insert into TR_ProjectMember select newid(),'{0}',userid,0 from 
base_user where departmentid='{1}' and Enabled=1 ", KeyValue, DepartIDArr[i]);
            }
            strSql.AppendFormat(@" delete from TR_ProjectMember where ProjectID='{0}'
and UserID in (select UserID from TR_ProjectMember where ProjectID='{0}' 
group by UserID having count(UserID) > 1)
and   ProjectMemberID not in (select min(ProjectMemberID)  from TR_ProjectMember 
where ProjectID='{0}' group by UserID     having count(UserID)>1)  ", KeyValue);
            ProjectBll.ExecuteSql(strSql);
            return Content(new JsonMessage { Success = true, Code = "1", Message = "添加成功" }.ToString());
        }

        public ActionResult InsertUserOnCode(string KeyValue, string StartCode, string EndCode)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(@" insert into TR_ProjectMember select newid(),'{0}',userid,0 from 
base_user where  Code>='{1}' and Code <='{2}' and Enabled=1 ", KeyValue, StartCode, EndCode);

            strSql.AppendFormat(@" delete from TR_ProjectMember where ProjectID='{0}'
and UserID in (select UserID from TR_ProjectMember where ProjectID='{0}' 
group by UserID having count(UserID) > 1)
and   ProjectMemberID not in (select min(ProjectMemberID)  from TR_ProjectMember 
where ProjectID='{0}' group by UserID     having count(UserID)>1)  ", KeyValue);
            ProjectBll.ExecuteSql(strSql);
            return Content(new JsonMessage { Success = true, Code = "1", Message = "添加成功" }.ToString());
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
                DataTable ListData = ProjectBll.GetBaseUserList(keywords, ref jqgridparam, ParameterJson);
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

        public ActionResult DeptListBatch()
        {
            return View();
        }

        public ActionResult CodeForm(string StartCode, string EndCode)
        {
            return View();
        }

        public ActionResult MyIndex()
        {
            return View();
        }

        public ActionResult SkillDetail()
        {
            return View();
        }

        public ActionResult GridMyPageListJson(string keywords, string CompanyId, string DepartmentId, JqGridParam jqgridparam, string ParameterJson)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = ProjectBll.GetMyPageList(keywords, ref jqgridparam, ParameterJson);
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

        public string getskilldata(string ProjectID)
        {
            string table = "";
            string OperateTable = "";
            string sql = " select a.skillid,b.skillname,dbo.[GetStudyState]('"+ManageProvider.Provider.Current().UserId+"',b.SkillID) as StudyState from TR_ProjectDetail a left join tr_skill b on a.skillid=b.skillid where a.ProjectID='{0}' ";
            sql = string.Format(sql, ProjectID);
            DataTable dt = ProjectBll.GetDataTable(sql);
            int RowNumber = 0;
            if (dt.Rows.Count > 0)
            {
                table += "<table>";
                RowNumber = dt.Rows.Count / 2;
                if(RowNumber<=1)
                {
                    table += "<tr>";
                    OperateTable += "<tr>";
                    for (int i=0;i<dt.Rows.Count;i++)
                    {
                        table += "<td style=\"border-right:#cccccc solid 1px;border-left:#cccccc solid 1px;border-top:#cccccc solid 1px;border-bottom:#cccccc solid 1px;background-image:url(../../../../Content/Images/background/1.png)\"><div style=\"width:240px;height:150px\"><font size=\"5\" color='yellow'>" + dt.Rows[i]["SkillName"].ToString() + "</font></div></td>";
                        table += "<td><div style=\"width:150px;height:150px\"></div></td>";
                        OperateTable += "<td style=\"border-right:#cccccc solid 1px;border-left:#cccccc solid 1px;border-top:#cccccc solid 1px;border-bottom:#cccccc solid 1px;\"><label class='lb'>状态：" + dt.Rows[i]["StudyState"].ToString() + "</label>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<button  class='btn_pr' onclick=\"study('" + dt.Rows[i]["skillid"].ToString()+ "')\">开始学习</button>&nbsp;&nbsp;&nbsp;&nbsp;<button  class='btn_pr' onclick=\"exam('" + dt.Rows[i]["skillid"].ToString()+"')\">开始考试</button></td>";
                        OperateTable += "<td></td>";
                    }
                    table += "</tr>";
                    OperateTable += "</tr>";
                }
                else
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (i == 0)
                        {
                            table += "<tr>";
                            OperateTable += "<tr>";
                            table += "<td style=\"border-right:#cccccc solid 1px;border-left:#cccccc solid 1px;border-top:#cccccc solid 1px;border-bottom:#cccccc solid 1px;background-image:url(../../../../Content/Images/background/1.png)\"><div style=\"width:240px;height:150px\"><font size='5' color='yellow'>" + dt.Rows[i]["SkillName"].ToString() + "</font></div></td>";
                            table += "<td><div style=\"width:50px;height:150px\"></div></td>";
                            OperateTable += "<td style=\"border-right:#cccccc solid 1px;border-left:#cccccc solid 1px;border-top:#cccccc solid 1px;border-bottom:#cccccc solid 1px;\"><label class='lb'>状态：" + dt.Rows[i]["StudyState"].ToString() + "</label>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<button  class='btn_pr' onclick=\"study('" + dt.Rows[i]["skillid"].ToString() + "')\">开始学习</button>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<button  class='btn_pr' onclick=\"exam('" + dt.Rows[i]["skillid"].ToString() + "')\">开始考试</button></td>";
                            OperateTable += "<td></td>";
                        }
                        else
                        {
                            if(i%4==0)
                            {
                                table += "</tr>"+OperateTable+ "</tr><tr style=\"height:20px\"><td colspan=\"6\"></td></tr><tr>";
                                OperateTable = "<tr>";
                                table += "<td style=\"border-right:#cccccc solid 1px;border-left:#cccccc solid 1px;border-top:#cccccc solid 1px;border-bottom:#cccccc solid 1px;background-image:url(../../../../Content/Images/background/1.png)\"><div style=\"width:240px;height:150px\"><font size='5' color='yellow'>" + dt.Rows[i]["SkillName"].ToString() + "</font></div></td>";
                                table += "<td><div style=\"width:50px;height:150px\"></div></td>";
                                OperateTable += "<td style=\"border-right:#cccccc solid 1px;border-left:#cccccc solid 1px;border-top:#cccccc solid 1px;border-bottom:#cccccc solid 1px;\"><label class='lb'>状态：" + dt.Rows[i]["StudyState"].ToString() + "</label>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<button  class='btn_pr' onclick=\"study('" + dt.Rows[i]["skillid"].ToString() + "')\">开始学习</button>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<button class='btn_pr' onclick=\"exam('" + dt.Rows[i]["skillid"].ToString() + "')\">开始考试</button></td>";
                                OperateTable += "<td></td>";
                                //table += OperateTable;
                                //OperateTable = "";
                            }
                            else
                            {
                                table += "<td style=\"border-right:#cccccc solid 1px;border-left:#cccccc solid 1px;border-top:#cccccc solid 1px;border-bottom:#cccccc solid 1px;background-image:url(../../../../Content/Images/background/1.png)\"><div style=\"width:240px;height:150px\"><font size='5' color='yellow'>" + dt.Rows[i]["SkillName"].ToString() + "</font></div></td>";
                                table += "<td><div style=\"width:50px;height:150px\"></div></td>";
                                OperateTable += "<td style=\"border-right:#cccccc solid 1px;border-left:#cccccc solid 1px;border-top:#cccccc solid 1px;border-bottom:#cccccc solid 1px;\"><label class='lb'>状态：" + dt.Rows[i]["StudyState"].ToString() + "</label>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<button class='btn_pr' onclick=\"study('" + dt.Rows[i]["skillid"].ToString() + "')\">开始学习</button>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<button class='btn_pr' onclick=\"exam('" + dt.Rows[i]["skillid"].ToString() + "')\">开始考试</button></td>";
                                OperateTable += "<td></td>";
                            }
                        }
                        
                    }
                }
                table +=OperateTable+"</table>";
            }
            
            return table;
        }

        public ActionResult FileList()
        {
            return View();
        }

        public ActionResult GetFileList(string keywords, string CompanyId, string DepartmentId,
            JqGridParam jqgridparam, string ParameterJson, string SkillID)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = ProjectBll.GetFileList(keywords, ref jqgridparam, ParameterJson, SkillID);
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

        /// <summary>
        /// 首先判断是否有未审批的申请，如果有的话，不允许重复提交
        /// </summary>
        /// <param name="SkillID"></param>
        /// <returns></returns>
        public string ExamApply(string SkillID)
        {
            StringBuilder strSql = new StringBuilder();

            string sql = " select * from tr_examapply where examid='" + SkillID + "' and userid='" + ManageProvider.Provider.Current().UserId + "' ";
            sql += " and IsOK=0 and source=2 ";

            DataTable dt = ProjectBll.GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                //表示已经有申请记录了，不能重复申请
                return "-1";
            }

            strSql.AppendFormat(@" insert into tr_examapply(applyid,userid,examid,source,applydate,isok) values(newid(),'{0}','{1}',2,getdate(),0) "
, ManageProvider.Provider.Current().UserId, SkillID);
            ProjectBll.ExecuteSql(strSql);
            return "0";
        }

        //判断是否学习足够时间
        public string IsApply(string SkillID)
        {
            string sql = " select * from TR_UserStudyTime  where SkillID='" + SkillID + "' and userid='" + ManageProvider.Provider.Current().UserId + "' ";
            

            DataTable dt = ProjectBll.GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                if (Convert.ToInt32(dt.Rows[0]["StudyMin"].ToString()) > 30)
                {
                    //表示学习时间ok，可以进行考试
                    return "1";
                }
                
                else
                {
                    //表示学习时间不足够
                    return "0";
                }
            }
            else
            {
                //表示尚未开始学习
                return "-1";
            }

            //return "0";
        }

        public ActionResult StudyDone(string SkillID,string StudyMin)
        {
            string sql = " select * from TR_UserStudyTime where userid='{0}' and SkillID='{1}' ";
            sql = string.Format(sql, ManageProvider.Provider.Current().UserId, SkillID);
            DataTable dt = ProjectBll.GetDataTable(sql);
            StringBuilder strSql = new StringBuilder();
            if (dt.Rows.Count>0)
            {
                strSql.AppendFormat(@" update TR_UserStudyTime set StudyMin=StudyMin+" + StudyMin+" where UserID='"+ManageProvider.Provider.Current().UserId+"' and skillid='"+SkillID+"' ");
            }
            else
            {
                strSql.AppendFormat(" insert into TR_UserStudyTime values(newid(),'{0}','{1}','{2}','{3}') ", ManageProvider.Provider.Current().UserId, "学习完成", SkillID, StudyMin);
            }

            
            
            ProjectBll.ExecuteSql(strSql);
            return Content(new JsonMessage { Success = true, Code = "1", Message = "学习完成" }.ToString());
        }


        public ActionResult ViewVideo(string VideoSrc)
        {
            VideoSrc = VideoSrc.Replace("~", "../../../../..");
            ViewData["VideoSrc"] = "../../../../../Resource/Document/NetworkDisk/TrainVideo/" + VideoSrc;
            //ViewData["VideoSrc"] = VideoSrc;
            return View();
        }

        public ActionResult CourseRank()
        {
            return View();
        }

        public ActionResult CourseRankDetail(string KeyValue)
        {
            return View();
        }

        public ActionResult GridCourseDetailListJson(string keywords, string CompanyId, string DepartmentId, JqGridParam jqgridparam, string ParameterJson,string KeyValue)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = ProjectBll.GetCourseDetailList(keywords, ref jqgridparam, ParameterJson, KeyValue);
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

        public ActionResult PersonalLearning()
        {
            return View();
        }

        public ActionResult GridSubListJson(string projectid, JqGridParam jqgridparam)
        {
            //            Stopwatch watch = CommonHelper.TimerStart();
            string sql = @" select a.SkillName,a.Score as PlanScore,(select max(Score) from TR_Paper where SkillID=a.SkillID and UserID='{0}') as PaperScore,
case when (select max(Score) from TR_Paper where SkillID=a.SkillID and UserID='{0}')>=a.RequireScore then a.Score 
else 0 end  as RealScore
from TR_ProjectDetail a 
where ProjectID='{1}'
union
select '总计',sum(aa.PlanScore),AVG(aa.PaperScore),sum(aa.RealScore) from
(
select a.SkillName,a.Score as PlanScore,(select max(Score) from TR_Paper where SkillID=a.SkillID and UserID='{0}') as PaperScore,
case when (select max(Score) from TR_Paper where SkillID=a.SkillID and UserID='{0}')>=a.RequireScore then a.Score 
else 0 end  as RealScore
from TR_ProjectDetail a 
where ProjectID='{1}') as aa ";
            sql = string.Format(sql, ManageProvider.Provider.Current().UserId, projectid);
            DataTable dt = ProjectBll.GetDataTable(sql);
            //            var JsonData = new
            //            {
            //                rows = dt,
            //            };

            var aa = "{" +
          "    \"page\":\"1\"," +
          "    \"total\":1," +
          "    \"records\":\""+dt.Rows.Count+"\"," +
          "    \"rows\":[";
            if(dt.Rows.Count>0)
            {
                for(int i=0;i<dt.Rows.Count;i++)
                {
                    aa += "      {" +
          "        \"id\":\""+i+"\"," +
          "        \"cell\":[\""+i+"\",\""+dt.Rows[i]["SkillName"].ToString()+"\",\""+dt.Rows[i]["PlanScore"].ToString()+"\",\""+dt.Rows[i]["PaperScore"].ToString() +"\",\""+dt.Rows[i]["RealScore"].ToString() +"\"]" +
          "      },";
                }
                aa = aa.Substring(0, aa.Length - 1);
            }
            aa+=
          "    ]" +
          "  }";
            return Content(aa);
        }

        public string GetGeneralData(string ProjectID)
        {
            string sql = @" select sum(aa.Score),sum(aa.getscore) from
(
select b.Score,
getscore=case when (select max(Score) from TR_Paper where FromSource=2 and SkillID=b.SkillID and UserID='{0}')>=b.RequireScore then b.Score else 0 end
from TR_Project a
left join TR_ProjectDetail b on a.ProjectID=b.ProjectID
left join TR_Skill c on b.SkillID=c.SkillID
where exists (select * from TR_ProjectMember where UserID='{0}' and ProjectID=a.ProjectID) and a.ProjectID='{1}')
as aa ";
            sql = string.Format(sql, ManageProvider.Provider.Current().UserId, ProjectID);
            DataTable dt = ProjectBll.GetDataTable(sql);
            if(dt.Rows.Count>0)
            {
                return dt.Rows[0][0].ToString() + "/" + dt.Rows[0][1].ToString();
            }
            else
            {
                return "error";
            }
        }

        public string GetGeneralData1(string ProjectID)
        {
            string sql1 = @"select count(*) from
(
select b.Score,
getscore=case when (select max(Score) from TR_Paper where FromSource=2 and SkillID=b.SkillID and UserID='{0}')>=b.RequireScore then b.Score else 0 end
from TR_Project a
left join TR_ProjectDetail b on a.ProjectID=b.ProjectID
left join TR_Skill c on b.SkillID=c.SkillID
where exists (select * from TR_ProjectMember where UserID='{0}' and ProjectID=a.ProjectID) and a.ProjectID='{1}')
as aa";
            string sql2= @"select count(*) from
(
select b.Score,
getscore=case when (select max(Score) from TR_Paper where FromSource=2 and SkillID=b.SkillID and UserID='{0}')>=b.RequireScore then b.Score else 0 end
from TR_Project a
left join TR_ProjectDetail b on a.ProjectID=b.ProjectID
left join TR_Skill c on b.SkillID=c.SkillID
where exists (select * from TR_ProjectMember where UserID='{0}' and ProjectID=a.ProjectID) and a.ProjectID='{1}')
as aa where aa.getscore!=0 ";
            sql1 = string.Format(sql1,ManageProvider.Provider.Current().UserId,ProjectID);
            sql2 = string.Format(sql2, ManageProvider.Provider.Current().UserId,ProjectID);
            DataTable dt1 = ProjectBll.GetDataTable(sql1);
            DataTable dt2 = ProjectBll.GetDataTable(sql2);
            return dt1.Rows[0][0].ToString() + "/" + dt2.Rows[0][0].ToString();
        }

        public string GetGeneralData2(string ProjectID)
        {
            string sql1 = @"select count(*)*30 from
(
select b.Score,
getscore=case when (select max(Score) from TR_Paper where FromSource=2 and SkillID=b.SkillID and UserID='{0}')>=b.RequireScore then b.Score else 0 end
from TR_Project a
left join TR_ProjectDetail b on a.ProjectID=b.ProjectID
left join TR_Skill c on b.SkillID=c.SkillID
where exists (select * from TR_ProjectMember where UserID='{0}' and ProjectID=a.ProjectID) and a.ProjectID='{1}')
as aa";
            string sql2 = @"select sum(StudyMin) from TR_UserStudyTime where SkillID in 
(select b.SkillID
from TR_Project a
left join TR_ProjectDetail b on a.ProjectID=b.ProjectID
left join TR_Skill c on b.SkillID=c.SkillID
where exists (select * from TR_ProjectMember where UserID='{0}' and ProjectID=a.ProjectID) and a.ProjectID='{1}')
and UserID='{0}'";
            sql1 = string.Format(sql1, ManageProvider.Provider.Current().UserId, ProjectID);
            sql2 = string.Format(sql2, ManageProvider.Provider.Current().UserId, ProjectID);
            DataTable dt1 = ProjectBll.GetDataTable(sql1);
            DataTable dt2 = ProjectBll.GetDataTable(sql2);
            return dt1.Rows[0][0].ToString() + "/" + dt2.Rows[0][0].ToString();
        }


        public string GetPictureData(string ProjectID)
        {
            string sql = @" select * from ( select bb.RealName,isnull(( select sum(aa.getscore) from
(
select b.Score,
getscore=case when (select max(Score) from TR_Paper where FromSource=2 and SkillID=b.SkillID and UserID=bb.UserId)>=b.RequireScore then b.Score else 0 end
from TR_Project a
left join TR_ProjectDetail b on a.ProjectID=b.ProjectID
left join TR_Skill c on b.SkillID=c.SkillID
where exists (select * from TR_ProjectMember where UserID=bb.UserId and ProjectID=a.ProjectID) and a.ProjectID='{0}')
as aa ),0) score from TR_ProjectMember aa
left join Base_User bb on aa.UserID=bb.UserId 
where ProjectID='{0}') as bb order by score asc
 ";
            sql = string.Format(sql, ProjectID);
            DataTable dt = ProjectBll.GetDataTable(sql);
            string X = "";
            string Y = "";
            string Result = "";
            if(dt.Rows.Count>0)
            {
                for(int i=0;i<dt.Rows.Count;i++)
                {
                    X += dt.Rows[i][1].ToString()+",";
                    Y += dt.Rows[i][0].ToString() + ",";
                }
                X = X.Substring(0, X.Length - 1);
                Y = Y.Substring(0, Y.Length - 1);
                Result = X + "|" + Y;

            }

            return Result;

        }

        public ActionResult StudyDetail()
        {
            return View();
        }


        public ActionResult GridStudyListJson(string keywords, string CompanyId, string DepartmentId, JqGridParam jqgridparam, string ParameterJson,string ProjectID)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = ProjectBll.GetStudyList(keywords, ref jqgridparam, ParameterJson,ProjectID);
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

        public void ExcelExport(string condition,string ProjectID)
        {
            ExcelHelper ex = new ExcelHelper();
            string sql = @"select b.Code as 工号,b.RealName as 姓名,e.FullName as 部门,c.ProjectName as 项目,d.SkillName as 课程,30 as 要求学时,
完成学时=(select top 1 StudyMin from TR_UserStudyTime where SkillID=d.SkillID and UserId=a.UserID),d.Score as 学分, 
获得学分=case when (select max(Score) from TR_Paper where FromSource=2 and SkillID=d.SkillID and UserID=a.UserId)>=d.RequireScore then d.Score else 0 end,
考试成绩=(select max(Score) from TR_Paper where FromSource=2 and SkillID=d.SkillID and UserID=a.UserId)
from TR_ProjectMember a 
left join Base_User b on a.UserID=b.UserId
left join TR_Project c on a.ProjectID=c.ProjectID
left join TR_ProjectDetail d on c.ProjectID=d.ProjectID
left join Base_Department e on b.DepartmentId=e.DepartmentId  where 1=1   ";
            sql += " and c.projectid='"+ProjectID+"' ";
            if (!string.IsNullOrEmpty(condition))
            {
                sql += @"AND (b.Code LIKE '%{0}%' or b.RealName LIKE '%{0}%' or e.FullName LIKE '%{0}%' or c.ProjectName LIKE '%{0}%' 
                                    )";
                sql = string.Format(sql, condition);
                
            }
            DataTable ListData = ProjectBll.GetDataTable(sql);
            ex.EcportExcel(ListData, "学习情况导出"+DateTime.Now.ToString());
        }

        public void ExcelExportByProject(string ProjectID)
        {
            ExcelHelper ex = new ExcelHelper();
            string sql = @"select b.Code as 工号,b.RealName as 姓名,e.FullName as 部门,c.ProjectName as 项目,d.SkillName as 课程,30 as 要求学时,
完成学时=(select top 1 StudyMin from TR_UserStudyTime where SkillID=d.SkillID and UserId=a.UserID),d.Score as 学分, 
获得学分=case when (select max(Score) from TR_Paper where FromSource=2 and SkillID=d.SkillID and UserID=a.UserId)>=d.RequireScore then d.Score else 0 end,
考试成绩=(select max(Score) from TR_Paper where FromSource=2 and SkillID=d.SkillID and UserID=a.UserId)
from TR_ProjectMember a 
left join Base_User b on a.UserID=b.UserId
left join TR_Project c on a.ProjectID=c.ProjectID
left join TR_ProjectDetail d on c.ProjectID=d.ProjectID
left join Base_Department e on b.DepartmentId=e.DepartmentId  where 1=1  ";
            sql = sql + " and c.projectid='"+ProjectID+"' ";
            DataTable ListData = ProjectBll.GetDataTable(sql);
            ex.EcportExcel(ListData, "学习情况导出" + DateTime.Now.ToString());
        }

    }
}
