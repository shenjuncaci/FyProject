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
    public class ProjectGanteeController : Controller
    {
        PM_ProjectGanteeBll ProjectBll = new PM_ProjectGanteeBll();
        //
        // GET: /ProjectManageModule/ProjectGantee/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult SubmitGanteeData(string tasks,string removeds,string ProjectID)
        {
            IDatabase database = DataFactory.Database();
            DbTransaction isOpenTrans = database.BeginTrans();
            StringBuilder sqldel = new StringBuilder();
            sqldel.AppendFormat( @" delete from PM_ProjectPreLink where PredecessorUID in ( select UID from PM_ProjectGantee where ProjectID is null)  ");
            sqldel.AppendFormat(@" delete from PM_ProjectGantee where ProjectID is null ");

            ProjectBll.ExecuteSql(sqldel);

            //database.Delete<PM_ProjectMember>("PM_ProjectGantee", ProjectID, isOpenTrans);
            //树形结构反序列化,需要保存的数据
            List<ProjectGanteeDisplay> ProjectGanteeList = tasks.JonsToList<ProjectGanteeDisplay>();
            foreach(ProjectGanteeDisplay pgd in ProjectGanteeList)
            {
                PM_ProjectGantee entity = new PM_ProjectGantee();
                entity.UID = pgd.UID;
                entity.Name = pgd.Name;
                entity.Start = pgd.Start;
                entity.Finish = pgd.Finish;
                entity.Duration = pgd.Duration;
                entity.PercentComplete = pgd.PercentComplete;
                entity.Summary = pgd.Summary;
                entity.Critical = pgd.Critical;
                entity.Milestone = pgd.Milestone;
                entity.ParentID = "0";
                
                database.Insert(entity, isOpenTrans);
                //前置节点处理
                if (pgd.PredecessorLink.Count!=0)
                {
                    foreach(PM_ProjectPreLink prl in pgd.PredecessorLink)
                    {
                        database.Insert(prl, isOpenTrans);
                    }
                }
                //子节点处理
                if(pgd.children!=null)
                {
                    foreach(ProjectGanteeDisplay pgdc in pgd.children)
                    {
                        PM_ProjectGantee entityD = new PM_ProjectGantee();
                        entityD.UID = pgdc.UID;
                        entityD.Name = pgdc.Name;
                        entityD.Start = pgdc.Start;
                        entityD.Finish = pgdc.Finish;
                        entityD.Duration = pgdc.Duration;
                        entityD.PercentComplete = pgdc.PercentComplete;
                        entityD.Summary = pgdc.Summary;
                        entityD.Critical = pgdc.Critical;
                        entityD.Milestone = pgdc.Milestone;
                        entityD.ParentID = pgd.UID;
                        database.Insert(entityD, isOpenTrans);

                        if (pgdc.PredecessorLink.Count != 0)
                        {
                            foreach (PM_ProjectPreLink prl in pgdc.PredecessorLink)
                            {
                                PM_ProjectPreLink entityl = new PM_ProjectPreLink();
                                //entity.

                                database.Insert(prl, isOpenTrans);
                            }
                        }
                    }
                }

            }
            database.Commit();

            return Content("成功");
        }

        public ActionResult GetData()
        {
            string sql = " select * from PM_ProjectGantee where ParentID='0'  ";
            DataTable dt = ProjectBll.GetDataTable(sql);
            List<ProjectGanteeDisplay> list1 = DtConvertHelper.ConvertToModelListNew<ProjectGanteeDisplay>(dt);
            //while (dt.Rows.Count>0)
            //{

            //    foreach(ProjectGanteeDisplay pm in list1)
            //    {
            //        sql = " select * from PM_ProjectGantee where ParentID='"+pm.UID+"' ";
            //        dt = ProjectBll.GetDataTable(sql);

            //        if(dt.Rows.Count>0)
            //        {
            //            List<ProjectGanteeDisplay> listD = DtConvertHelper.ConvertToModelListNew<ProjectGanteeDisplay>(dt);
            //            pm.children = listD;
            //        }

            //    }
            //    return Content(list1.ToJson());
            //}
            return Content(IsChildren(list1).ToJson());

            //return Content("没有数据啊");

            
        }

        public List<ProjectGanteeDisplay> IsChildren(List<ProjectGanteeDisplay> list)
        {
            string sql = "";
            DataTable dt;
            foreach (ProjectGanteeDisplay pm in list)
            {
                sql = " select * from PM_ProjectPreLink where TaskUID='" + pm.UID + "' ";
                dt = ProjectBll.GetDataTable(sql);
                if (dt.Rows.Count>0)
                {
                    List<PM_ProjectPreLink> listpre= DtConvertHelper.ConvertToModelListNew<PM_ProjectPreLink>(dt);
                    foreach(PM_ProjectPreLink pmpre in listpre)
                    {
                        pmpre.Limit = true;
                    }
                    pm.PredecessorLink = listpre;
                }

                    sql = " select * from PM_ProjectGantee where ParentID='" + pm.UID + "' ";
                dt = ProjectBll.GetDataTable(sql);
                if (dt.Rows.Count > 0)
                {
                    List<ProjectGanteeDisplay> listD = DtConvertHelper.ConvertToModelListNew<ProjectGanteeDisplay>(dt);
                    pm.children = listD;
                    //listD=ChildrenList(listD);
                    IsChildren(listD);
                   
                }

                
            }
            return list;
        }

        //public List<ProjectGanteeDisplay> ChildrenList(List<ProjectGanteeDisplay> list)
        //{ }

    }
}
