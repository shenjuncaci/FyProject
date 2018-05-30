﻿using LeaRun.Business;
using LeaRun.DataAccess;
using LeaRun.Entity;
using LeaRun.Repository;
using LeaRun.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace LeaRun.WebApp.Controllers
{
    //测试同步，测试另一台电脑
    public class HomeController : Controller
    {
        public Base_ModuleBll base_modulebll = new Base_ModuleBll();
        Base_ModulePermissionBll base_modulepermissionbll = new Base_ModulePermissionBll();
        /// <summary>
        /// 初始化页面
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Login");
        }
        /// <summary>
        /// 访问模块，写入系统菜单Id
        /// </summary>
        /// <param name="ModuleId">模块id</param>
        /// <param name="ModuleName">模块名称</param>
        /// <returns></returns>
        public ActionResult SetModuleId(string ModuleId, string ModuleName)
        {
            string _ModuleId = DESEncrypt.Encrypt(ModuleId);
            CookieHelper.WriteCookie("ModuleId", _ModuleId);
            if (!string.IsNullOrEmpty(ModuleName))
            {
                Base_SysLogBll.Instance.WriteLog(ModuleId, OperationType.Visit, "1", ModuleName);
            }
            return Content(_ModuleId);
        }
        /// <summary>
        /// 离开tab事件
        /// </summary>
        /// <param name="ModuleId">模块id</param>
        /// <param name="ModuleName">模块名称</param>
        /// <returns></returns>
        public ActionResult SetLeave(string ModuleId, string ModuleName)
        {
            Base_SysLogBll.Instance.WriteLog(ModuleId, OperationType.Leave, "1", ModuleName);
            return Content(ModuleId);
        }

        #region 后台首页-开始菜单
        /// <summary>
        /// 开始菜单UI
        /// </summary>
        /// <returns></returns>
        [LoginAuthorize]
        public ActionResult StartIndex()
        {
            ViewBag.Account = ManageProvider.Provider.Current().Account + "（" + ManageProvider.Provider.Current().UserName + "）";
            return View();
        }
        /// <summary>
        /// 开始-欢迎首页
        /// </summary>
        /// <returns></returns>
        [LoginAuthorize]
        public ActionResult StartPanel()
        {
            return View();
        }
        /// <summary>
        /// 加载开始菜单
        /// </summary>
        /// <returns></returns>
        public ActionResult LoadStartMenu()
        {
            string ObjectId = ManageProvider.Provider.Current().ObjectId;
            List<Base_Module> list = base_modulepermissionbll.GetModuleList(ObjectId).FindAll(t => t.Enabled == 1);
            return Content(list.ToJson().Replace("&nbsp;", ""));
        }
        #endregion

        #region 后台首页-手风琴菜单
        /// <summary>
        /// 手风琴UI
        /// </summary>
        /// <returns></returns>
        [LoginAuthorize]
        public ActionResult AccordionIndex()
        {
            ViewBag.Account = ManageProvider.Provider.Current().Account + "（" + ManageProvider.Provider.Current().UserName + "）";
            return View();
        }
        /// <summary>
        /// 手风琴-欢迎首页
        /// </summary>
        /// <returns></returns>
        [LoginAuthorize]
        public ActionResult AccordionPage()
        {
            return View();
        }
        /// <summary>
        /// 加载手风琴菜单
        /// </summary>
        /// <returns></returns>
        public ActionResult LoadAccordionMenu()
        {
            string ObjectId = ManageProvider.Provider.Current().ObjectId;
            List<Base_Module> list = base_modulepermissionbll.GetModuleList(ObjectId).FindAll(t => t.Enabled == 1);
            return Content(list.ToJson().Replace("&nbsp;", "").Replace("分层审核","分层审核"+"("+ManageProvider.Provider.Current().DepartmentName+")"));
        }
        #endregion

        #region 后台首页-无限树菜单
        /// <summary>
        /// 无限树UI
        /// </summary>
        /// <returns></returns>
        [LoginAuthorize]
        public ActionResult TreeIndex()
        {
            ViewBag.Account = ManageProvider.Provider.Current().Account + "（" + ManageProvider.Provider.Current().UserName + "）";
            return View();
        }
        /// <summary>
        /// 无限树-欢迎首页
        /// </summary>
        /// <returns></returns>
        [LoginAuthorize]
        public ActionResult TreePage()
        {
            return View();
        }
        /// <summary>
        /// 加载无限树菜单
        /// </summary>
        /// <returns></returns>
        public ActionResult LoadTreeMenu(string ModuleId)
        {
            string ObjectId = ManageProvider.Provider.Current().ObjectId;
            List<Base_Module> list = base_modulepermissionbll.GetModuleList(ObjectId).FindAll(t => t.Enabled == 1);
            List<TreeJsonEntity> TreeList = new List<TreeJsonEntity>();
            foreach (Base_Module item in list)
            {
                TreeJsonEntity tree = new TreeJsonEntity();
                bool hasChildren = false;
                List<Base_Module> childnode = list.FindAll(t => t.ParentId == item.ModuleId);
                if (childnode.Count > 0)
                {
                    hasChildren = true;
                }
                if (item.Category == "页面")
                {
                    tree.Attribute = "Location";
                    tree.AttributeValue = item.Location;
                }
                tree.id = item.ModuleId;
                tree.text = item.FullName;
                tree.value = item.ModuleId;
                tree.isexpand = false;
                tree.complete = true;
                tree.hasChildren = hasChildren;
                tree.parentId = item.ParentId;
                tree.img = item.Icon != null ? "/Content/Images/Icon16/" + item.Icon : item.Icon;
                TreeList.Add(tree);
            }
            return Content(TreeList.TreeToJson(ModuleId).Replace("分层审核", "分层审核" + "(" + ManageProvider.Provider.Current().DepartmentName + ")"));
        }
        #endregion

        #region 快捷方式设置
        /// <summary>
        /// 快捷方式设置
        /// </summary>
        /// <returns></returns>
        [LoginAuthorize]
        public ActionResult Shortcuts()
        {
            return View();
        }
        /// <summary>
        /// 快捷方式 返回菜单模块树JSON
        /// </summary>
        /// <returns></returns>
        public ActionResult ShortcutsModuleTreeJson()
        {
            Base_ShortcutsBll base_shortcutsbll = new Base_ShortcutsBll();
            string UserId = ManageProvider.Provider.Current().UserId;
            List<Base_Module> ShortcutList = base_shortcutsbll.GetShortcutList(UserId);
            string ObjectId = ManageProvider.Provider.Current().ObjectId;
            List<Base_Module> list = base_modulepermissionbll.GetModuleList(ObjectId).FindAll(t => t.Enabled == 1);
            List<TreeJsonEntity> TreeList = new List<TreeJsonEntity>();
            foreach (Base_Module item in list)
            {
                TreeJsonEntity tree = new TreeJsonEntity();
                tree.id = item.ModuleId;
                tree.text = item.FullName;
                tree.value = item.ModuleId;
                if (item.Category == "页面")
                {
                    tree.checkstate = ShortcutList.FindAll(t => t.ModuleId == item.ModuleId).Count == 0 ? 0 : 1;
                    //tree.checkstate = item["objectid"].ToString() != "" ? 1 : 0;
                    tree.showcheck = true;
                }
                tree.isexpand = true;
                tree.complete = true;
                tree.hasChildren = list.FindAll(t => t.ParentId == item.ModuleId).Count > 0 ? true : false;
                tree.parentId = item.ParentId;
                tree.img = item.Icon != null ? "/Content/Images/Icon16/" + item.Icon : item.Icon;
                TreeList.Add(tree);
            }
            return Content(TreeList.TreeToJson());
        }
        /// <summary>
        /// 快捷方式列表返回JSON
        /// </summary>
        /// <returns></returns>
        public ActionResult ShortcutsListJson()
        {
            Base_ShortcutsBll base_shortcutsbll = new Base_ShortcutsBll();
            string UserId = ManageProvider.Provider.Current().UserId;
            List<Base_Module> ShortcutList = base_shortcutsbll.GetShortcutList(UserId);
            return Content(ShortcutList.ToJson());
        }
        /// <summary>
        /// 快捷方式设置 提交保存
        /// </summary>
        /// <param name="ModuleId"></param>
        /// <returns></returns>
        public ActionResult SubmitShortcuts(string ModuleId)
        {
            try
            {
                Base_ShortcutsBll base_shortcutsbll = new Base_ShortcutsBll();
                string UserId = ManageProvider.Provider.Current().UserId;
                int IsOk = base_shortcutsbll.SubmitForm(ModuleId, UserId);
                return Content(new JsonMessage { Success = true, Code = IsOk.ToString(), Message = "设置成功。" }.ToString());
            }
            catch (Exception ex)
            {
                return Content(new JsonMessage { Success = false, Code = "-1", Message = "操作失败：" + ex.Message }.ToString());
            }
        }
        #endregion

        #region 技术支持
        /// <summary>
        /// 技术支持
        /// </summary>
        /// <returns></returns>
        [LoginAuthorize]
        public ActionResult SupportPage()
        {
            return View();
        }
        #endregion

        #region 关于我们
        /// <summary>
        /// 关于我们
        /// </summary>
        /// <returns></returns>
        [LoginAuthorize]
        public ActionResult AboutPage()
        {
            return View();
        }
        #endregion

        #region 个性化皮肤设置
        /// <summary>
        /// 个性化皮肤设置
        /// </summary>
        /// <returns></returns>
        [LoginAuthorize]
        public ActionResult SkinIndex()
        {
            return View();
        }
        /// <summary>
        /// 切换主题
        /// </summary>
        /// <param name="UItheme"></param>
        /// <returns></returns>
        public ActionResult SwitchTheme(string UItheme)
        {
            CookieHelper.WriteCookie("UItheme", UItheme, 43200);
            return Content("1");
        }
        #endregion

        public string WaitToDoList()
        {
            IDatabase database = DataFactory.Database();
            string sql = @"select a.ProblemID as KeyValue,'/FYModule/HrProblem/Form' as Url,'员工关系 '+a.ProblemDescripe as ProblemDescripe from FY_HrProblem a left join Base_FlowLog b on a.FlowID=b.FlowID where CurrentPerson='" + ManageProvider.Provider.Current().UserId+"'";
            sql += " union select res_id as KeyValue,'/CommonModule/Rapid/Form' as Url,'快速反应 '++REPLACE(res_ms,' ','') as ProblemDescripe from FY_Rapid where RapidState!='已完成' and res_cpeo='" + ManageProvider.Provider.Current().Code+"'  ";
            if(ManageProvider.Provider.Current().UserId == "03e8fe3d-f8fc-4eb7-aab5-f6bf228bc0d8")
            {
                sql += " union select ChangeID as KeyValue,'/FYModule/Change/Form' as Url,'变更管理 '+ changeno as ProblemDescripe from FY_Change where ChangeState='等待总经理批准' ";
            }
            sql += " union select ProblemID as KeyValue,'/FYModule/ProblemTrack/Form' as Url,'问题跟踪 '++REPLACE(ProblemDescripe,' ','') as ProblemDescripe from FY_ProblemTrack where Status!='已完成' and (ResponseBy='" + ManageProvider.Provider.Current().UserId + "' or AgentBy='"+ManageProvider.Provider.Current().UserId+"') ";
            DataTable dt = database.FindDataSetBySql(sql).Tables[0];
            string temp;
            if (dt.Rows.Count > 0)
            {
                temp = dt.ToJson();
            }
            else
            {
                temp = "-1";
            }
            return temp;
        }

        public ActionResult GetGridList(string keywords, string CompanyId, string DepartmentId, JqGridParam jqgridparam, string ParameterJson)
        {
            try
            {
                IDatabase database = DataFactory.Database();
                Stopwatch watch = CommonHelper.TimerStart();
                string sql = @"select a.ProblemID as KeyValue,'员工关系 ' as mokuai,'/FYModule/HrProblem/Form' as Url,a.ProblemDescripe as ProblemDescripe from FY_HrProblem a left join Base_FlowLog b on a.FlowID=b.FlowID where CurrentPerson='" + ManageProvider.Provider.Current().UserId + "'";
                sql += " union select res_id as KeyValue,'快速反应 ','/CommonModule/Rapid/Form' as Url,REPLACE(res_ms,' ','') as ProblemDescripe from FY_Rapid where RapidState!='已完成' and res_cpeo='" + ManageProvider.Provider.Current().Code + "'  ";
                if (ManageProvider.Provider.Current().UserId == "03e8fe3d-f8fc-4eb7-aab5-f6bf228bc0d8")
                {
                    sql += " union select ChangeID as KeyValue,'变更管理 ','/FYModule/Change/Form' as Url,changeno as ProblemDescripe from FY_Change where ChangeState='等待总经理批准' ";
                }
                sql += " union select ProblemID as KeyValue,'问题跟踪 ','/FYModule/ProblemTrack/Form' as Url,REPLACE(ProblemDescripe,' ','') as ProblemDescripe from FY_ProblemTrack where Status!='已完成' and (ResponseBy='" + ManageProvider.Provider.Current().UserId + "' or AgentBy='" + ManageProvider.Provider.Current().UserId + "') ";
                sql += " union select a.ProjectID as KeyValue,'项目管理 ' as mokuai,'/ProjectManageModule/Project/Form' as Url,a.ProjectName from PM_Project a left join Base_FlowLog b on a.FlowID=b.FlowID where CurrentPerson = '" + ManageProvider.Provider.Current().UserId + "'";
                DataTable ListData = database.FindDataSetBySql(sql).Tables[0];
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

        public int IsPartDept()
        {
            IDatabase database = DataFactory.Database();
            string sql = " select * from base_partdept where UserID='"+ManageProvider.Provider.Current().UserId+"'  ";
            DataTable dt = database.FindDataSetBySql(sql).Tables[0];
            if(dt.Rows.Count>0)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        public ActionResult PartDeptSelect()
        {
            return View();
        }

        public int ChangeDept(string DepartmentID)
        {
            Base_ObjectUserRelationBll base_objectuserrelationbll = new Base_ObjectUserRelationBll();
            ManageProvider.Provider.Current().DepartmentId = DepartmentID;
            IManageUser imanageuser = new IManageUser();
            imanageuser.UserId = ManageProvider.Provider.Current().UserId;
            imanageuser.Account = ManageProvider.Provider.Current().Account;
            imanageuser.UserName = ManageProvider.Provider.Current().UserName;
            imanageuser.Gender = ManageProvider.Provider.Current().Gender;
            imanageuser.Password = ManageProvider.Provider.Current().Password;
            imanageuser.Code = ManageProvider.Provider.Current().Code;
            imanageuser.Secretkey = ManageProvider.Provider.Current().Secretkey;
            imanageuser.LogTime = DateTime.Now;
            imanageuser.CompanyId = ManageProvider.Provider.Current().CompanyId;
            imanageuser.DepartmentId = DepartmentID;
            imanageuser.ObjectId = ManageProvider.Provider.Current().ObjectId;
            imanageuser.GroupID = ManageProvider.Provider.Current().GroupID;
            imanageuser.IPAddress = ManageProvider.Provider.Current().IPAddress;
            imanageuser.IPAddressName = ManageProvider.Provider.Current().IPAddressName;
            imanageuser.IsSystem = ManageProvider.Provider.Current().IsSystem;
            imanageuser.DepartmentName = base_objectuserrelationbll.GetDepartmentName(DepartmentID);
            ManageProvider.Provider.AddCurrent(imanageuser);
            return 0;
        }
    }
}
