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
    public class SkillController : Controller
    {
        RepositoryFactory<TR_ChoiceQuestion> detailrepository = new RepositoryFactory<TR_ChoiceQuestion>();
        RepositoryFactory<TR_Skill> repositoryfactory = new RepositoryFactory<TR_Skill>();
        TR_SkillBll SkillBll = new TR_SkillBll();
        //
        // GET: /FYModule/Process/

        public ActionResult Index()
        {
            string sql = @" select FullName,case when ParentId='0' then '' else (select FullName Base_Department where DepartmentId=a.ParentId) end 
from Base_Department a where DepartmentId='{0}' ";
            sql = string.Format(sql, ManageProvider.Provider.Current().DepartmentId);
            DataTable dt = SkillBll.GetDataTable(sql);
            ViewData["Department"] = dt.Rows[0][0].ToString();
            if (dt.Rows[0][1].ToString() == "")
            {
                ViewData["Department"] = dt.Rows[0][1].ToString() + "-" + dt.Rows[0][0].ToString();
            }

            return View();
        }

        public ActionResult GridPageListJson(string keywords, string CompanyId, string DepartmentId, 
            JqGridParam jqgridparam, string ParameterJson)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = SkillBll.GetPageList(keywords, ref jqgridparam, ParameterJson);
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

        public ActionResult GetFileList(string keywords, string CompanyId, string DepartmentId,
            JqGridParam jqgridparam, string ParameterJson,string SkillID)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = SkillBll.GetFileList(keywords, ref jqgridparam, ParameterJson,SkillID);
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
        public ActionResult SubmitForm(string KeyValue, TR_Skill entity, string BuildFormJson, HttpPostedFileBase Filedata)
        {
            string ModuleId = DESEncrypt.Decrypt(CookieHelper.GetCookie("ModuleId"));
            IDatabase database = DataFactory.Database();
            DbTransaction isOpenTrans = database.BeginTrans();
            try
            {
                string Message = KeyValue == "" ? "新增成功,等待人事部审批。" : "编辑成功。";
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
                    string sql = " select * from tr_skill where skillname='{0}' and (enable=1 or IsAudit=0) ";
                    sql = string.Format(sql, entity.SkillName);
                    DataTable dt = SkillBll.GetDataTable(sql);
                    if(dt.Rows.Count>0)
                    {
                        return Content(new JsonMessage { Success = false, Code = "-1", Message = "已有相同名称的技能，不允许重复添加" }.ToString());

                    }

                    entity.Create();


                    database.Insert(entity, isOpenTrans);

                    Base_DataScopePermissionBll.Instance.AddScopeDefault(ModuleId, ManageProvider.Provider.Current().UserId, entity.SkillID, isOpenTrans);
                }
                Base_FormAttributeBll.Instance.SaveBuildForm(BuildFormJson, entity.SkillID, ModuleId, isOpenTrans);
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
            TR_Skill entity = DataFactory.Database().FindEntity<TR_Skill>(KeyValue);
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

                strSql.AppendFormat(@" update TR_Skill set enable=0,IsAudit=0 where SkillID='{0}'  ",KeyValue);
                ////将对应关系也删掉
                //strSql.AppendFormat(@" delete from TR_PostDepartmentRelationDetail where SkillID='{0}' ",KeyValue);
                IsOk = repositoryfactory.Repository().ExecuteBySql(strSql);
                if (IsOk > 0)
                {
                    Message = "删除成功,请等待人事部审批。";
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
            Base_SysLogBll.Instance.WriteLog<TR_Skill>(array, IsOk.ToString(), Message);
        }


        //技能矩阵报表,思路非常复杂，如要修改，只能gg
        public ActionResult SkillMatrix(string DepartmentID)
        {
            
//            try
//            {

//                if (DepartmentID == null) { DepartmentID = ManageProvider.Provider.Current().DepartmentId; }
//                double srd = 0;
//                string[] strColumns = null;
//                DataTable dt = SkillBll.GetSkillMatrixData(DepartmentID);
//                string SkillName = "";
//                string SkillID = "";
//                string SkillWeight = "";
//                string SkillRequire = "";
//                string[] SkillArrary = new string[4];

//                //拼接最终的字符串
//                string table = "";
//                table += " <table id=\"mytable\" class=\"gridtable\" width=\"90%\"> ";

//                //表头信息存储在dt的标题中
//                //第一步，获取dt的标题到一个数组中
//                if (dt.Columns.Count > 0)
//                {
//                    int columnNum = 0;
//                    columnNum = dt.Columns.Count;
//                    strColumns = new string[columnNum];
//                    table += "<tr>";
                    
//                    for (int i = 0; i < dt.Columns.Count - 1; i++)
//                    {
//                        strColumns[i] = dt.Columns[i].ColumnName;

//                        if (i >= 5)
//                        {
//                            table += "<th><h4>" + strColumns[i].Substring(0, strColumns[i].IndexOf('*')) + "</h4></th>";
//                        }
//                        else
//                        {
//                            if (i == 0)
//                            { }
//                            else
//                            {
//                                if (i == 1)
//                                {
//                                    table += "<th>工号</th>";
//                                }
//                                else if (i == 2)
//                                {
//                                    table += "<th><h4>姓名</h4></th>";
//                                }
//                                else if(i==3)
//                                {
//                                    table += "<th><h4>岗位</h4></th>";
//                                }
//                                else if(i==4)
//                                {
//                                    table += "<th><h4>胜任度</h4></th>";
//                                    table += "<th><h4>主管评价</h4></th>";
//                                    table += "<th><h4>考试结果</h4></th>";
//                                }
//                                else
//                                {
//                                    table += "<th><h4>胜任度</h4></th>";
//                                }


//                            }
//                        }

//                    }
                    
//                    table += "</tr>";
//                }

//                ViewData["strColumns"] = strColumns;

//                //至此，表头信息搞定

//                //第二部，更新内部数据
//                string SqlIsIN = "";
//                int SrdFz = 0;
//                int SrdFm = 0;

//                int Evaluate = 0;
//                if (dt.Rows.Count > 0)
//                {
//                    for (int j = 0; j < dt.Rows.Count; j++)
//                    {
//                        SrdFz = 0;
//                        SrdFm = 0;
//                        Evaluate = 0;
//                        table += "<tr>";
//                        for (int k = 0; k < dt.Columns.Count - 1; k++)
//                        {

//                            if (k < 5)
//                            {
//                                if (k == 0)
//                                { }
//                                else
//                                {
//                                    if (k == 5)
//                                    {
//                                        table += "胜任度内容";
//                                    }
//                                    else if(k==4)
//                                    {
//                                        if(dt.Rows[j][k].ToString()=="")
//                                        {
//                                            dt.Rows[j][k] = "0";
//                                        }
//                                        Evaluate = Convert.ToInt32(dt.Rows[j][k].ToString());
//                                        if (Convert.ToInt32(dt.Rows[j][k].ToString())>=90)
//                                        {
//                                            table += "综合胜任度";
//                                            table += "<td bgcolor=\"green\">" + dt.Rows[j][k].ToString() + "%</td>";
//                                            table += "胜任度内容";
//                                        }
//                                        else if(Convert.ToInt32(dt.Rows[j][k].ToString()) >= 80)
//                                        {
//                                            table += "综合胜任度";
//                                            table += "<td bgcolor=\"yellow\">" + dt.Rows[j][k].ToString() + "%</td>";
//                                            table += "胜任度内容";
//                                        }
//                                        else
//                                        {
//                                            table += "综合胜任度";
//                                            table += "<td bgcolor=\"red\">" + dt.Rows[j][k].ToString() + "%</td>";
//                                            table += "胜任度内容";
//                                        }
//                                    }
//                                    else
//                                    {
//                                        table += "<td>" + dt.Rows[j][k].ToString() + "</td>";
//                                    }
//                                }
//                            }

//                            else
//                            {
//                                SkillArrary = dt.Columns[k].ToString().Split('*');
//                                //判断如果单元格是空的话，就去判断下该用户是否有权限
//                                SqlIsIN = @" select e.SkillRequire,e.SkillWeight
//from TR_UserPost a 
//left join TR_PostDepartmentRelation b on a.DepartmentPostID=b.RelationID
//left join TR_Post c on b.PostID=c.PostID 
//left join Base_User d on a.UserID=d.UserId
//left join TR_PostDepartmentRelationDetail e on e.RelationID=b.RelationID
//left join tr_skill f on e.SkillID=f.SkillID
//where f.SkillID is not null and f.IsExam=0 and f.SkillID='" + SkillArrary[1] + "' and d.UserId='" + dt.Rows[j][0].ToString() + "' and c.PostName='" + dt.Rows[j]["PostName"].ToString() + "' ";
//                                DataTable dtIsIN = SkillBll.GetDataTable(SqlIsIN);

//                                if (dt.Rows[j][k].ToString() == "")
//                                {
                                    
//                                    if (dtIsIN.Rows.Count > 0)
//                                    {
//                                        table += "<td width=\"5%\" bgcolor=\"red\" title=\"要求分数:" + dtIsIN.Rows[0]["SkillRequire"].ToString() + "\">";
//                                        dt.Rows[j][k] = "1";
//                                        table += "0";
//                                        SrdFz += 0;
//                                        SrdFm += Convert.ToInt32(dtIsIN.Rows[0]["SkillWeight"].ToString());
//                                    }
//                                    else
//                                    {
//                                        table += "<td width=\"5%\">NA";
//                                        table += "";
//                                    }

//                                }
//                                else
//                                {
//                                    //int test3 = Convert.ToInt32(100.00);
//                                    //string aa = dt.Rows[j][k].ToString();
//                                    //int test = Convert.ToInt32(dt.Rows[j][k]);
//                                    //int test2 = Convert.ToInt32(SkillArrary[3]);
//                                    SrdFz += Convert.ToInt32(dtIsIN.Rows[0]["SkillWeight"].ToString()) * Convert.ToInt32(dt.Rows[j][k]);
//                                    SrdFm += Convert.ToInt32(dtIsIN.Rows[0]["SkillWeight"].ToString());

//                                    if (Convert.ToInt32(dt.Rows[j][k]) > Convert.ToInt32(dtIsIN.Rows[0]["SkillRequire"].ToString()))
//                                    {
//                                        table += "<td width=\"5%\" bgcolor=\"green\" title=\"要求分数:" + dtIsIN.Rows[0]["SkillRequire"].ToString() + "\">" + dt.Rows[j][k].ToString();

//                                    }
//                                    else
//                                    {
//                                        if (Convert.ToInt32(dt.Rows[j][k]) > (Convert.ToInt32(dtIsIN.Rows[0]["SkillRequire"].ToString()) - 10))
//                                        {
//                                            table += "<td width=\"5%\" bgcolor=\"yellow\" title=\"要求分数:" + dtIsIN.Rows[0]["SkillRequire"].ToString() + "\" > " + dt.Rows[j][k].ToString();
//                                        }
//                                        else
//                                        {
//                                            table += "<td width=\"5%\" bgcolor=\"red\" title=\"要求分数:" + dtIsIN.Rows[0]["SkillRequire"].ToString() + "\">" + dt.Rows[j][k].ToString();
//                                        }
//                                    }
//                                }

//                                table += "</td>";
//                            }
//                        }
//                        double srdL = Math.Round(1.0 * SrdFz / SrdFm, 0);
//                        double srdZh = Math.Round(1.0 * (srdL + Evaluate) / 2, 0);
//                        if (srdL > 90)
//                        {
//                            table = table.Replace("胜任度内容", "<td width=\"5%\" bgcolor=\"green\" title=\"" + "(技能1权重*考试分+...+技能n权重*考试分）/（技能1权重+...+技能n权重)/100" + "\">" + srdL + "%</td>");
//                            //table += "<td width=\"5%\" bgcolor=\"green\" title=\"" + "(技能1权重*考试分+...+技能n权重*考试分）/（技能1权重+...+技能n权重)/100"+"\">" + srdL + "%</td>";
//                        }
//                        else
//                        {
//                            if (srdL > 80)
//                            {
//                                table = table.Replace("胜任度内容", "<td width=\"5%\" bgcolor=\"yellow\" title=\"" + "(技能1权重*考试分+...+技能n权重*考试分）/（技能1权重+...+技能n权重)/100" + "\">" + srdL + "%</td>");
//                                //table += "<td width=\"5%\" bgcolor=\"yellow\" title=\"" + "(技能1权重*考试分+...+技能n权重*考试分）/（技能1权重+...+技能n权重)/100" + "\">" + srdL + "%</td>";
//                            }
//                            else
//                            {
//                                table = table.Replace("胜任度内容", "<td width=\"5%\" bgcolor=\"red\" title=\"" + "(技能1权重*考试分+...+技能n权重*考试分）/（技能1权重+...+技能n权重)/100" + "\">" + srdL + "%</td>");
//                                //table += "<td width=\"5%\" bgcolor=\"red\" title=\"" + "(技能1权重*考试分+...+技能n权重*考试分）/（技能1权重+...+技能n权重)/100" + "\">" + srdL + "%</td>";
//                            }
//                        }

//                        if (srdZh > 90)
//                        {
//                            table = table.Replace("综合胜任度", "<td width=\"5%\" bgcolor=\"green\" >" + srdZh + "%</td>");
//                            //table += "<td width=\"5%\" bgcolor=\"green\" title=\"" + "(技能1权重*考试分+...+技能n权重*考试分）/（技能1权重+...+技能n权重)/100"+"\">" + srdL + "%</td>";
//                        }
//                        else
//                        {
//                            if (srdZh > 80)
//                            {
//                                table = table.Replace("综合胜任度", "<td width=\"5%\" bgcolor=\"yellow\" >" + srdZh + "%</td>");
//                                //table += "<td width=\"5%\" bgcolor=\"yellow\" title=\"" + "(技能1权重*考试分+...+技能n权重*考试分）/（技能1权重+...+技能n权重)/100" + "\">" + srdL + "%</td>";
//                            }
//                            else
//                            {
//                                table = table.Replace("综合胜任度", "<td width=\"5%\" bgcolor=\"red\">" + srdZh + "%</td>");
//                                //table += "<td width=\"5%\" bgcolor=\"red\" title=\"" + "(技能1权重*考试分+...+技能n权重*考试分）/（技能1权重+...+技能n权重)/100" + "\">" + srdL + "%</td>";
//                            }
//                        }

//                        table += "</tr>";
//                    }
//                }
//                ViewData["table"] = table;
//                ViewData["dt"] = dt;
//            }
//            catch(Exception ex)
//            {
//                ViewData["table"] = "无数据";
//            }
            return View();
        }

        public string SkillMatrixData(string DepartmentID,string Year)
        {
            try
            {
                if (DepartmentID == null || DepartmentID == "") { DepartmentID = ManageProvider.Provider.Current().DepartmentId; }
                if (Year == null || Year == "") { Year = DateTime.Now.Year.ToString(); }
                double srd = 0;
                string[] strColumns = null;
                DataTable dt = SkillBll.GetSkillMatrixData(DepartmentID);
                string SkillName = "";
                string SkillID = "";
                string SkillWeight = "";
                string SkillRequire = "";
                string[] SkillArrary = new string[4];

                string table = "";

                //为了配合列数来调整每一个th的宽度~~~
                int tempwidth = dt.Columns.Count * 4;
                if(tempwidth>100)
                {
                    tempwidth = 100;
                }
                table += " <table id=\"mytable\" class=\"gridtable\" width=\""+tempwidth+"%\"> ";

                //分解dt中的数据，再进行组合查询
                if (dt.Columns.Count > 0)
                {
                    int columnNum = dt.Columns.Count;
                    strColumns = new string[columnNum];
                    table += "<tr>";

                    for (int i = 0; i < dt.Columns.Count - 1; i++)
                    {
                        strColumns[i] = dt.Columns[i].ColumnName;
                        if (i >= 4)
                        {
                            table += "<th><h4>" + strColumns[i].Substring(0, strColumns[i].IndexOf('*')) + "</h4></th>";
                        }
                        else
                        {
                            if (i == 0)
                            {
                                table += "<th><h4>工号</h4></th>";
                            }


                            else if (i == 1)
                            {
                                table += "<th><h4>姓名</h4></th>";
                            }
                            else if (i == 2)
                            {
                                table += "<th><h4>岗位</h4></th>";
                            }
                            else if (i == 3)
                            {
                                table += "<th><h4>胜任度</h4></th>";

                            }
                        }
                    }

                    table += "</tr>";


                }


                string SqlIsIN = "";
                int SrdFz = 0;
                int SrdFm = 0;
                int Evaluate = 0;
                int TempScore = 0;
                int QualifiedCount = 0;
                DataTable DtIsIn;
                if (dt.Rows.Count > 0)
                {
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        SrdFz = 0;
                        SrdFm = 0;
                        Evaluate = 0;
                        
                        table += "<tr>";
                        for (int k = 0; k < dt.Columns.Count - 1; k++)
                        {
                            if (k < 4)
                            {
                                if (k == 0)
                                { }
                                else if (k == 3)
                                {
                                    table += "<td bgcolor=\"dedede\">" + dt.Rows[j][k].ToString() + "</td>";
                                    table += "胜任度内容";
                                }
                                else
                                {
                                    table += "<td  bgcolor=\"dedede\">" + dt.Rows[j][k].ToString() + "</td>";
                                }
                            }
                            else
                            {
                                QualifiedCount = 0;
                                SkillArrary = dt.Columns[k].ToString().Split('*');
                                if (dt.Rows[j][k].ToString() == "")
                                {
                                    table += "<td width=\"5%\">NA</td>";
                                }
                                else if (dt.Rows[j][k].ToString() == "0")
                                {
                                    SqlIsIN = @"select isnull(g.EvaluateScore,0) as EvaluateScore,e.SkillRequire,
e.SkillWeight,isnull(ExamPer,0) as ExamPer,isnull(EvaluationPer,0) as EvaluationPer,
isnull((select top 1 Score from TR_Paper where UserID=d.UserID and Skillid=f.SkillID and year(PaperDate)='{0}'  order by PaperDate desc),0) as ExamScore
from TR_UserPost a
left join TR_PostDepartmentRelation b on a.DepartmentPostID = b.RelationID
left join TR_Post c on b.PostID = c.PostID
left join Base_User d on a.UserID = d.UserId
left join TR_PostDepartmentRelationDetail e on e.RelationID = b.RelationID
left join tr_skill f on e.SkillID = f.SkillID
left join TR_EvaluateDetail g on g.UserPostRelationID=a.UserPostRelationID and g.SkillID=f.SkillID
where f.SkillID is not null 
and f.SkillID = '" + SkillArrary[1] + "' and d.UserId = '" + dt.Rows[j][0].ToString() + "' and c.PostName = '" + dt.Rows[j]["PostName"].ToString() + "'";
                                    SqlIsIN = string.Format(SqlIsIN, Year);
                                    DtIsIn = SkillBll.GetDataTable(SqlIsIN);
                                    TempScore = Convert.ToInt32(Convert.ToInt32(DtIsIn.Rows[0]["ExamScore"].ToString())*Convert.ToInt32(DtIsIn.Rows[0]["ExamPer"].ToString())*0.01+Convert.ToInt32(DtIsIn.Rows[0]["EvaluateScore"].ToString())*Convert.ToInt32(DtIsIn.Rows[0]["EvaluationPer"].ToString())*0.01);
                                    
                                    if (TempScore >= Convert.ToInt32(DtIsIn.Rows[0]["SkillRequire"].ToString()))
                                    {
                                        table += "<td width=\"5%\" bgcolor=\"green\" title=\"要求分数:" + DtIsIn.Rows[0]["SkillRequire"].ToString() + ",考试分数:" + DtIsIn.Rows[0]["ExamScore"].ToString() + ",考试权重:" + DtIsIn.Rows[0]["ExamPer"].ToString() + "%,主管评价:" + DtIsIn.Rows[0]["EvaluateScore"].ToString() + ",主管评价权重:"+ DtIsIn.Rows[0]["EvaluationPer"].ToString() + "%\">" + TempScore + "</td>";
                                        QualifiedCount=1;
                                    }
                                    else
                                    {
                                        if (TempScore >= (Convert.ToInt32(DtIsIn.Rows[0]["SkillRequire"].ToString()) - 10))
                                        {
                                            table += "<td width=\"5%\" bgcolor=\"yellow\" title=\"要求分数:" + DtIsIn.Rows[0]["SkillRequire"].ToString() + ",考试分数:" + DtIsIn.Rows[0]["ExamScore"].ToString() + ",考试权重:"+ DtIsIn.Rows[0]["ExamPer"].ToString() + "%,主管评价:" + DtIsIn.Rows[0]["EvaluateScore"].ToString() + ",主管评价权重:" + DtIsIn.Rows[0]["EvaluationPer"].ToString() + "%\">" + TempScore + "</td>";
                                        }
                                        else
                                        {
                                            table += "<td width=\"5%\" bgcolor=\"red\" title=\"要求分数:" + DtIsIn.Rows[0]["SkillRequire"].ToString() + ",考试分数:" + DtIsIn.Rows[0]["ExamScore"].ToString() + ",考试权重:" + DtIsIn.Rows[0]["ExamPer"].ToString() + "%,主管评价:" + DtIsIn.Rows[0]["EvaluateScore"].ToString() + ",主管评价权重:" + DtIsIn.Rows[0]["EvaluationPer"].ToString() + "%\">" + TempScore + "</td>";
                                        }
                                    }

                                    //table += "<td width=\"5%\" title=\"要求分数:" + DtIsIn.Rows[0]["SkillRequire"].ToString() + ",考试分数:" + DtIsIn.Rows[0]["ExamScore"].ToString() + ",主管评价:" + DtIsIn.Rows[0]["EvaluateScore"].ToString() + "\">" + TempScore + "</td>";
                                    SrdFz += Convert.ToInt32(DtIsIn.Rows[0]["SkillWeight"].ToString()) * QualifiedCount;
                                    SrdFm += Convert.ToInt32(DtIsIn.Rows[0]["SkillWeight"].ToString());
                                }
                                else
                                {
                                    table += "<td width=\"5%\">err</td>";
                                }
                            }
                        }

                        double srdL = Math.Round(100.0 * SrdFz / SrdFm, 0);
                        string bgcolor = "";
                        if(srdL>=70)
                        {
                            if (srdL >= 80)
                            {
                                bgcolor = "green";
                            }
                            else
                            {
                                bgcolor = "yellow";
                            }
                        }
                        else
                        {
                            bgcolor = "red";
                        }
                        table = table.Replace("胜任度内容", "<td  bgcolor=\""+bgcolor+"\" width=\"5%\" title=\"" + "及格的科目数量比率，考虑了权重。" + "\">" + srdL + "%</td>");
                    }
                    
                }
                return table;
            }
            catch(Exception ex)
            {
                return "没有数据";
            }
        }


        public ActionResult DepartmentJson()
        {
            string sql = " select distinct DepartmentId,FullName from base_department where 1=1 ";
            DataTable dt = SkillBll.GetDataTable(sql);
            return Content(dt.ToJson());
        }

        public ActionResult DelFile(string FileID)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(" delete from tr_skillfile where fileid='{0}' ",FileID);
            SkillBll.ExecuteSql(strSql);
            return Content(new JsonMessage { Success = true, Code = 1.ToString(), Message = "操作成功。" }.ToString());
        }

        public ActionResult Uploadify()
        {
            return View();
        }

        //多文件上传，每次insert
        public ActionResult SubmitUploadify(string FolderId, HttpPostedFileBase Filedata)
        {
            try
            {
                Thread.Sleep(1000);////延迟500毫秒
                TR_SkillFile entity = new TR_SkillFile();
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
                        entity.FilePath = fileGuid + FileEextension; ;
                        entity.SkillID = FolderId;
                        entity.FileName = Filedata.FileName;
                       //entity.FilePath = virtualPath;
                        entity.FileSize = filesize.ToString();
                        entity.FileExtensions = FileEextension;
                        string _FileType = "";
                        string _Icon = "";
                        this.DocumentType(FileEextension, ref _FileType, ref _Icon);
                        entity.Icon = _Icon;
                        entity.FileType = _FileType;
                        IsOk = DataFactory.Database().Insert<TR_SkillFile>(entity).ToString();
                    }
                    catch (Exception ex)
                    {
                        IsOk = ex.Message;
                        //System.IO.File.Delete(virtualPath);
                    }
                }
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


        //移动我的学习模块到技能清单下面
        public ActionResult MyIndex()
        {
            return View();
        }


        public ActionResult GridMyListJson(string keywords, string CompanyId, string DepartmentId,
          JqGridParam jqgridparam, string ParameterJson)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = SkillBll.GetMyStudyList(keywords, ref jqgridparam, ParameterJson);
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

        public ActionResult ViewVideo(string VideoSrc)
        {
            VideoSrc = VideoSrc.Replace("~", "../../../../..");
            ViewData["VideoSrc"] = "../../../../../Resource/Document/NetworkDisk/TrainVideo/" + VideoSrc;
            //ViewData["VideoSrc"] = VideoSrc;
            return View();
        }

        public ActionResult ExamForm(string KeyValue)
        {
            
            //获取上次的考试时间
            string DateSpan = "-1";
            string sqlDateSpan = " select top 1 DATEDIFF(DAY,PaperDate,GETDATE()) from TR_Paper where KnowledgeBaseID='" + KeyValue + "' and UserID='" + ManageProvider.Provider.Current().UserId + "' order by PaperDate desc ";
            DataTable dt5 = SkillBll.GetDataTable(sqlDateSpan);
            if (dt5.Rows.Count > 0)
            {
                DateSpan = dt5.Rows[0][0].ToString();
            }
            ViewData["DateSpan"] = DateSpan;

            string sqlInfo = " select a.SkillName,a.QuestionNum,a.ExamMinutes from TR_Skill a  where SkillID='" + KeyValue + "' ";
            DataTable dt3 = SkillBll.GetDataTable(sqlInfo);

            string QuestionCount = dt3.Rows[0][1].ToString();
            

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
            pp.ExamTime = dt3.Rows[0][2].ToString();
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
            paper.SkillID = KeyValue;
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
where PaperID='{0}'", paper.PaperID);

            strSql.AppendFormat(@"update TR_PaperDetail set Istrue=1 where answer=trueanswer and paperid='{0}' ", paper.PaperID);
            strSql.AppendFormat(@"update TR_Paper set score=(select dbo.CountScore('{0}')) where PaperID='{0}' ", paper.PaperID);

            SkillBll.ExecuteSql(strSql);

            string sql = " select score from tr_paper where paperid='" + paper.PaperID + "' ";
            string Score = SkillBll.GetDataTable(sql).Rows[0][0].ToString();
            Score = "您好！您的本次考试成绩为" + Score;
            return Content(new JsonMessage { Success = true, Code = "1", Message = "保存成功", Content = Score }.ToString());
        }

        public ActionResult GetPaper(string KeyValue)
        {
            string sqlInfo = " select a.SkillName,a.QuestionNum,a.ExamMinutes from TR_Skill a where SkillID='" + KeyValue + "' ";
            DataTable dt3 = SkillBll.GetDataTable(sqlInfo);

            string QuestionNum = dt3.Rows[0][1].ToString();
            

            string sql = " select top " + QuestionNum + " * from TR_ChoiceQuestion where SkillID='"+KeyValue+"' and IsEnable=1  order by newid()  ";
            // string sql2 = " select top "+JudgmentCount +" * from TR_JudgmentQuestion where SkillID in(select SkillID from TR_KnowledgeBase where KnowledgeBaseID='" + KeyValue+ "')  order by newid() ";
            DataTable dt = SkillBll.GetDataTable(sql);
            return Content(dt.ToJson());
        }

        public ActionResult FileList()
        {
            return View();
        }
        /// <summary>
        /// 首先判断是否有未审批的申请，如果有的话，不允许重复提交
        /// </summary>
        /// <param name="SkillID"></param>
        /// <returns></returns>
        public string ExamApply(string SkillID)
        {
            StringBuilder strSql = new StringBuilder();

            string sql = " select * from tr_examapply where examid='"+SkillID+"' and userid='"+ManageProvider.Provider.Current().UserId+"' ";
            sql += " and IsOK=0 and source=0 ";

            DataTable dt = SkillBll.GetDataTable(sql);
            if(dt.Rows.Count>0)
            {
                //表示已经有申请记录了，不能重复申请
                return "-1";
            }

            strSql.AppendFormat(@" insert into tr_examapply(applyid,userid,examid,source,applydate,isok) values(newid(),'{0}','{1}',0,getdate(),0) "
,ManageProvider.Provider.Current().UserId,SkillID);
            SkillBll.ExecuteSql(strSql);
            return "0";
        }
        /// <summary>
        /// 判断是否成功提交了申请
        /// </summary>
        /// <param name="SkillID"></param>
        /// <returns></returns>
        public string IsApply(string SkillID)
        {
            string sql = " select * from tr_examapply  where examid='" + SkillID + "' and userid='" + ManageProvider.Provider.Current().UserId + "' ";
            sql += " and IsOK in (0,1) and source=0 ";

            DataTable dt = SkillBll.GetDataTable(sql);
            if(dt.Rows.Count>0)
            {
                //将状态修改成2，防止刷题
                StringBuilder strSql = new StringBuilder();
                strSql.AppendFormat(@" update tr_examapply  set IsOK = 2 where examid='{0}' and userid='{1}'  and IsOK=1 and source=0 ",SkillID,ManageProvider.Provider.Current().UserId);
                SkillBll.ExecuteSql(strSql);
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

        public ActionResult ApplyList()
        {
            //第一次加载的时候就删除一次
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(@"delete from TR_ExamApply where  DATEADD(MINUTE,(select ExamMinutes from View_Exam where skillid=TR_ExamApply.ExamID and FromSource=TR_ExamApply.Source),ApproveDate)<GETDATE()
");
            SkillBll.ExecuteSql(strSql);
            return View();
        }

        public ActionResult GridApplyListJson(string keywords, string CompanyId, string DepartmentId,
         JqGridParam jqgridparam, string ParameterJson)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = SkillBll.GetApplyList(keywords, ref jqgridparam, ParameterJson);
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
        /// 
        /// </summary>
        /// <param name="Type">0同意审批，1删除</param>
        /// <param name="ApplyID">可以有多个ID，逗号分隔</param>
        /// <returns></returns>
        public string ApplyProcess(string Type,string ApplyID)
        {
            StringBuilder strSql = new StringBuilder();
            ApplyID = ApplyID.Replace(",", "','");
            if (Type=="0")
            {
                
                strSql.AppendFormat(@" update TR_ExamApply set IsOk=1,ApproveDate=getdate() where ApplyID in ('{0}') and IsOK=0 ",ApplyID);
            }
            else if(Type=="1")
            {
                strSql.AppendFormat(@" delete from TR_ExamApply where ApplyID in ('{0}') ", ApplyID);
            }
            SkillBll.ExecuteSql(strSql);
            return "0";
        }

        public ActionResult ExamScoreList()
        {
            return View();
        }

        //GridExamScoreListJson
        public ActionResult GridExamScoreListJson(string keywords, string CompanyId, string DepartmentId,
           JqGridParam jqgridparam, string ParameterJson)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = SkillBll.GetExamScoreList(keywords, ref jqgridparam, ParameterJson);
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

        public ActionResult PostJson(string UserCode)
        {
            string sql = @"select distinct dd.PostID,dd.PostName from TR_UserPost aa 
left join TR_PostDepartmentRelation bb on aa.DepartmentPostID=bb.RelationID
left join TR_PostDepartmentRelationDetail cc on bb.RelationID=cc.RelationID 
left join TR_Post dd on bb.PostID=dd.PostID
left join base_user ee on aa.userid=ee.userid
where ee.Code='" + UserCode + "'";
            DataTable dt = SkillBll.GetDataTable(sql);
            return Content(dt.ToJson());
        }

        public void ExcelExport(string condition)
        {
            StringBuilder strSql = new StringBuilder();
           
            strSql.Append(@" select 考试类型=case when fromsource=0 then '技能考试' when fromsource=1 then '专项考试' else '错误' end,
code as 工号,realname as 姓名,Skillname as 技能专项名称,postname as 岗位名称,score as 考试成绩
from V_ExamScore where 1=1 ");
            if (!string.IsNullOrEmpty(condition))
            {
                strSql.Append(condition);
            }
            ExcelHelper ex = new ExcelHelper();
//            string sql = @" select a.res_area as 产品区域,a.res_type as 问题类型,a.res_again as 是否重复发生,
//a.res_ok as 问题类别,b.RealName as 责任人,c.FullName as 责任部门,
//res_kf as 客户,res_ms as 问题描述,CONVERT(varchar(100), res_cdate, 23) as 发生日期,res_fxnode as 根本原因分析,
//res_csnode as 纠正措施,NotCompleteReason as 未按进度完成原因,Action as 对应措施

//from FY_Rapid a 
//left join Base_User b on a.res_cpeo=b.Code left join Base_Department c on b.DepartmentId=c.DepartmentId where 1=1 ";
            //sql = sql + condition;
            DataTable ListData = SkillBll.GetDataTable(strSql.ToString());
            ex.EcportExcel(ListData, "考试成绩导出");
        }

        public ActionResult UpdateVideo(string ID)
        {
            return View();
        }

        public string UpdateVideoSql(string SkillID,string VideoSrc)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(@" update tr_skill set videosrc='{0}' where skillid='{1}'  ",VideoSrc,SkillID);
            SkillBll.ExecuteSql(strSql);
            return "0";
        }

        public ActionResult QuestionForm()
        {
            return View();
        }

        public ActionResult GetQuestionList(string KeyValue)
        {
            try
            {
                var JsonData = new
                {
                    rows = SkillBll.GetQuestionList(KeyValue),
                };
                return Content(JsonData.ToJson());
            }
            catch (Exception ex)
            {
                Base_SysLogBll.Instance.WriteLog("", OperationType.Query, "-1", "异常错误：" + ex.Message);
                return null;
            }
        }

        public ActionResult SubmitQuestionForm(string KeyValue, string BuildFormJson,
           HttpPostedFileBase Filedata, string DetailForm)
        {
            string ModuleId = DESEncrypt.Decrypt(CookieHelper.GetCookie("ModuleId"));
            IDatabase database = DataFactory.Database();
            DbTransaction isOpenTrans = database.BeginTrans();
            try
            {
                string Message = KeyValue == "" ? "新增成功。" : "编辑成功。";
                if (!string.IsNullOrEmpty(KeyValue))
                {
                    int index = 1;
                    List<TR_ChoiceQuestion> DetailList = DetailForm.JonsToList<TR_ChoiceQuestion>();
                    foreach (TR_ChoiceQuestion entityD in DetailList)
                    {
                        if (!string.IsNullOrEmpty(entityD.QuestionDescripe))
                        {
                            if (!string.IsNullOrEmpty(entityD.QuestionID))
                            {
                                entityD.Modify(entityD.QuestionID);
                                database.Update(entityD, isOpenTrans);
                                index++;
                            }
                            else
                            {
                                entityD.Create();
                                entityD.SkillID = KeyValue;

                                database.Insert(entityD, isOpenTrans);
                                index++;
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(entityD.QuestionID))
                            {
                                detailrepository.Repository().Delete(entityD.QuestionID);

                            }
                        }
                    }


                    

                }
                
                //Base_FormAttributeBll.Instance.SaveBuildForm(BuildFormJson, entity.CustomExamID, ModuleId, isOpenTrans);
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


        public string DeleteApply()
        {
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(@"delete from TR_ExamApply where  DATEADD(MINUTE,(select ExamMinutes from View_Exam where skillid=TR_ExamApply.ExamID and FromSource=TR_ExamApply.Source),ApproveDate)<GETDATE()
");
            SkillBll.ExecuteSql(strSql);
            return "0";
        }

        public ActionResult YearJson()
        {
            string sql = " select distinct year(PaperDate) as year from TR_Paper ";
            DataTable dt = SkillBll.GetDataTable(sql);
            return Content(dt.ToJson());
        }





    }
}
