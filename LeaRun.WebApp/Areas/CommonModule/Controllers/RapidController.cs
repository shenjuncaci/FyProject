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
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace LeaRun.WebApp.Areas.CommonModule.Controllers
{
    public class RapidController : Controller
    {
        RepositoryFactory<FY_Rapid> repositoryfactory = new RepositoryFactory<FY_Rapid>();
        FY_RapidBll rapidbll = new FY_RapidBll();
        //
        // GET: /ExampleModule/Test/

        public ActionResult Index()
        {
            return View();
        }


        public ActionResult GridPageListJson(string keywords, string CompanyId, string DepartmentId, JqGridParam jqgridparam, string ParameterJson, string MyTask)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = rapidbll.GetPageList(keywords, ref jqgridparam, ParameterJson, MyTask);
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
            DataTable dt = rapidbll.GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                ViewData["dt"] = dt.Rows[0][0].ToString();
            }
            string UserName = ManageProvider.Provider.Current().Code;
            ViewData["UserName"] = UserName;
            return View();
        }


        [HttpPost]
        public ActionResult SubmitTestTableForm(string KeyValue, FY_Rapid rapid, string BuildFormJson, HttpPostedFileBase Filedata)
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

                    //base_user.Modify(KeyValue);
                    rapid.Modify(KeyValue);
                    if (rapid.IsEmail != 1)
                    {
                        //测试下微信公众号消息通知
                        WeChatHelper.SendWxMessage(rapid.res_cpeo, "您好，您有一条新的快速反应需要处理，具体如下：" + rapid.res_ms + "\n 请登录系统处理：172.19.0.5:8086  ");

                        int IsEmail = SendEmail(rapid.res_cpeo, "您好，您有一条新的快速反应需要处理，具体如下：" + rapid.res_ms + "\n 请登录系统处理：172.19.0.5:8086  ");
                        rapid.IsEmail = IsEmail;
                    }
                    if (rapid.RealTime != null && rapid.res_cdate != null && rapid.PlanTime != null)
                    {
                        //int planNum = new TimeSpan(rapid.PlanTime.Ticks - rapid.res_cdate.Ticks).Days;
                        //int realNum = (rapid.RealTime - rapid.res_cdate).Days;
                        //TimeSpan d3 = rapid.RealTime.Subtract(rapid.res_cdate);
                        int realNum = Math.Abs(((TimeSpan)(rapid.RealTime - rapid.res_cdate)).Days);
                        int planNum = Math.Abs(((TimeSpan)(rapid.PlanTime - rapid.res_cdate)).Days);
                        rapid.Rate = Math.Round(((2.0 - (realNum / (planNum * 1.0))) * 100), 2).ToString() + "%";
                        rapid.RapidState = "已完成";
                    }
                    database.Update(rapid, isOpenTrans);

                }
                else
                {
                    


                    rapid.Create();
                    //测试下微信公众号消息通知
                    WeChatHelper.SendWxMessage(rapid.res_cpeo, "您好，您有一条新的快速反应需要处理，具体如下：" + rapid.res_ms + "\n 请登录系统处理：172.19.0.5:8086  ");
                    int IsEmail = SendEmail(rapid.res_cpeo, "您好，您有一条新的快速反应需要处理，具体如下：" + rapid.res_ms + "\n 请登录系统处理：172.19.0.5:8086  ");
                    rapid.IsEmail = IsEmail;
                    rapid.PlanTime = rapid.res_cdate.AddDays(40);
                    database.Insert(rapid, isOpenTrans);

                    //创建的同时新增一条记录到VP_RiskDownFlow
                    VP_RiskDownFollow followentity = new VP_RiskDownFollow();
                    followentity.Create();
                    followentity.HighRiskItem = rapid.res_ms;
                    followentity.FromSource = "快速反应改善项";
                    database.Insert(followentity, isOpenTrans);
                    

                    //database.Insert(base_employee, isOpenTrans);
                    Base_DataScopePermissionBll.Instance.AddScopeDefault(ModuleId, ManageProvider.Provider.Current().UserId, rapid.res_id, isOpenTrans);
                }
                Base_FormAttributeBll.Instance.SaveBuildForm(BuildFormJson, rapid.res_id, ModuleId, isOpenTrans);
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

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="code">用户编码</param>
        /// <returns>1成功，0失败</returns>
        public int SendEmail(string code, string Content)
        {
            string sql = " select Email from base_user where code='" + code + "'";
            DataTable dt = rapidbll.GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                try
                {
                    var emailAcount = "shfy_it@fuyaogroup.com";
                    var emailPassword = "Sj1234";
                    var reciver = dt.Rows[0][0].ToString();
                    var content = Content;
                    MailMessage message = new MailMessage();
                    //设置发件人,发件人需要与设置的邮件发送服务器的邮箱一致
                    MailAddress fromAddr = new MailAddress("shfy_it@fuyaogroup.com");
                    message.From = fromAddr;
                    //设置收件人,可添加多个,添加方法与下面的一样
                    message.To.Add(reciver);
                    //设置抄送人
                    message.CC.Add("jun.shen@fuyaogroup.com");
                    //设置邮件标题
                    message.Subject = "QSB快速反应";
                    //设置邮件内容
                    message.Body = content;
                    //设置邮件发送服务器,服务器根据你使用的邮箱而不同,可以到相应的 邮箱管理后台查看,下面是QQ的
                    SmtpClient client = new SmtpClient("mail.fuyaogroup.com", 25);
                    //设置发送人的邮箱账号和密码
                    client.Credentials = new NetworkCredential(emailAcount, emailPassword);
                    //启用ssl,也就是安全发送
                    client.EnableSsl = true;
                    //发送邮件
                    //加这段之前用公司邮箱发送报错：根据验证过程，远程证书无效
                    //加上后解决问题
                    ServicePointManager.ServerCertificateValidationCallback =
    delegate (Object obj, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) { return true; };
                    client.Send(message);
                    return 1;
                }
                catch
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }


        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SetTestForm(string KeyValue)
        {
            FY_Rapid rapid = DataFactory.Database().FindEntity<FY_Rapid>(KeyValue);
            if (rapid == null)
            {
                return Content("");
            }
            //Base_Employee base_employee = DataFactory.Database().FindEntity<Base_Employee>(KeyValue);
            //Base_Company base_company = DataFactory.Database().FindEntity<Base_Company>(base_user.CompanyId);
            string strJson = rapid.ToJson();
            //公司
            //strJson = strJson.Insert(1, "\"CompanyName\":\"" + base_company.FullName + "\",");
            //员工信息
            //strJson = strJson.Insert(1, base_employee.ToJson().Replace("{", "").Replace("}", "") + ",");
            //自定义
            strJson = strJson.Insert(1, Base_FormAttributeBll.Instance.GetBuildForm(KeyValue));
            return Content(strJson);
        }






        public ActionResult SubmitUploadify(string FolderId, HttpPostedFileBase Filedata, string type)
        {
            try
            {

                Thread.Sleep(1000);////延迟500毫秒
                Base_NetworkFile entity = new Base_NetworkFile();
                FY_Rapid rapidentity = DataFactory.Database().FindEntity<FY_Rapid>(FolderId);

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

                        if (type == "res_msfj")
                        {
                            rapidentity.res_msfj = virtualPath;
                        }
                        if (type == "res_yzb")
                        {
                            rapidentity.res_yzbfj = virtualPath;
                        }
                        if (type == "res_fx")
                        {
                            rapidentity.res_fxfj = virtualPath;
                        }
                        if (type == "res_cs")
                        {
                            rapidentity.res_csfj = virtualPath;
                        }
                        if (type == "res_fcf")
                        {
                            rapidentity.res_fcffj = virtualPath;
                        }
                        if (type == "res_fcsh")
                        {
                            rapidentity.res_fcshfj = virtualPath;
                        }
                        if (type == "res_csgz")
                        {
                            rapidentity.res_csgzfj = virtualPath;
                        }
                        if (type == "res_fmea")
                        {
                            rapidentity.res_fmeafj = virtualPath;
                        }
                        if (type == "res_jyjx")
                        {
                            rapidentity.res_jyjxfj = virtualPath;
                        }
                        if (type == "uploadifyNotCompleteReason")
                        {
                            rapidentity.NotCompleteReasonfj = virtualPath;
                        }
                        if (type == "Action")
                        {
                            rapidentity.Actionfj = virtualPath;
                        }
                        if (type == "res_bzgx")
                        {
                            rapidentity.res_bzgxfj = virtualPath;
                        }
                        if (type == "res_8d")
                        {
                            rapidentity.res_8dfj = virtualPath;

                        }
                        DataFactory.Database().Update<FY_Rapid>(rapidentity);
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



        /// <summary>
        /// 审批
        /// </summary>
        /// <param name="KeyValue">rapid主键</param>
        /// <param name="field">字段名称</param>
        /// <param name="state">当前状态</param>
        /// <param name="isok">选择操作</param>
        /// <returns></returns>
        public int Approve(string KeyValue, string field, string state, string isok, string dt, string node)
        {
            try
            {

                int result = rapidbll.Approve(KeyValue, field, state, isok, dt, node);
                return result;
            }
            catch
            {
                return 0;
            }
        }

        //人员下拉列表

        public ActionResult ListJson(string CompanyId)
        {
            DataTable ListData = rapidbll.GetList();
            return Content(ListData.ToJson());
        }

        //人员下拉列表按部门

        public ActionResult DepartUserListJson(string CompanyId)
        {
            DataTable ListData = rapidbll.GetDepartUserList();
            return Content(ListData.ToJson());
        }

        //客户下拉列表
        public ActionResult CustomerJson()
        {
            DataTable ListData = rapidbll.GetCustomerList();
            return Content(ListData.ToJson());
        }



        //月度报表
        public ActionResult RapidMonthlyReport()
        {
            return View();
        }

        public ActionResult GetReportJson(string keywords, string CompanyId, string DepartmentId, JqGridParam jqgridparam)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = rapidbll.GetReportJson(keywords, ref jqgridparam);
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

        public ActionResult PictureReport()
        {
            return View();
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
            Base_SysLogBll.Instance.WriteLog<FY_Rapid>(array, IsOk.ToString(), Message);
        }



        public ActionResult PictureIndex()
        {
            return View();
        }


        public string GetPictueData(string year)
        {
            string result = "";
            string temp1 = "";
            string temp2 = "";
            string temp3 = "";
            //string sql = " select count(*) as rapidcount,MONTH(res_cdate) as month from FY_Rapid where YEAR(res_cdate)='"+year+"' group by MONTH(res_cdate),YEAR(res_cdate)  ";
            string sql = "select isnull(rapidcount,0),ISNULL(rapidOKcount,0),basicmonth from base_month a left join (select count(*) as rapidcount,MONTH(res_cdate) as month from FY_Rapid where YEAR(res_cdate)='" + year + "' group by MONTH(res_cdate),YEAR(res_cdate)) as b on b.month=a.basicmonth left join (select count(*) as rapidOKcount,MONTH(res_cdate) as month from FY_Rapid where YEAR(res_cdate)='"+year+"' and  RapidState='已完成' group by MONTH(res_cdate),YEAR(res_cdate)) c on c.month=a.BasicMonth ";
            DataTable dt = rapidbll.GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    temp1 = temp1 + dt.Rows[i][0] + ",";
                    temp2 = temp2 + dt.Rows[i][1] + ",";
                    temp3 = temp3 + dt.Rows[i][2] + ",";
                }
                temp1 = temp1.Substring(0, temp1.Length - 1);
                temp2 = temp2.Substring(0, temp2.Length - 1);
                temp3 = temp3.Substring(0, temp3.Length - 1);
            }
            result = temp1 + "|" + temp2 + "|" + temp3;


            return result;
        }

        public string GetPieData(string year)
        {
            string result = "";
            string temp1 = "";
            string temp2 = "";
            string temp3 = "";
            //string sql = "select count(*) as cishu,fullname from RapidList_New where YEAR(res_cdate)='" + year + 
            //    "' group by fullname ";
            string sql = @"select count(distinct a.res_id),count(distinct b.res_id),FullName from [RapidList_New] a 
left join [OverdueList] b on a.res_id=b.res_id where YEAR(res_cdate)='{0}'
group by FullName ";
            sql = string.Format(sql, year);
            DataTable dt = rapidbll.GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    temp1 = temp1 + dt.Rows[i][0] + ",";
                    temp2 = temp2 + dt.Rows[i][1] + ",";
                    temp3 = temp3 + dt.Rows[i][2] + ",";
                    
                }
                temp1 = temp1.Substring(0, temp1.Length - 1);
                temp2 = temp2.Substring(0, temp2.Length - 1);
                temp3 = temp3.Substring(0, temp3.Length - 1);
            }
            result = temp1 + "|" + temp2+"|"+temp3;
            return result;
        }

        #region highcharts需要的json数据格式
        public string DataTableToJson(DataTable dt)
        {
            StringBuilder jsonBuilder = new StringBuilder();
            jsonBuilder.Append("{\"");
            jsonBuilder.Append("list");
            jsonBuilder.Append("\":[");

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                jsonBuilder.Append("{");
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    jsonBuilder.Append("\"");
                    jsonBuilder.Append(dt.Columns[j].ColumnName);
                    jsonBuilder.Append("\":");
                    //jsonBuilder.Append("\":\"");
                    //判断下是否纯数字，highcharts插件不是纯数字的值要加双引号
                    if (IsNumber(dt.Rows[i][j].ToString()))
                    {
                        jsonBuilder.Append(dt.Rows[i][j].ToString());
                    }
                    else
                    {
                        jsonBuilder.Append("\"");
                        jsonBuilder.Append(dt.Rows[i][j].ToString());
                        jsonBuilder.Append("\"");
                    }
                    jsonBuilder.Append(",");
                    //jsonBuilder.Append("\",");
                }
                jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
                jsonBuilder.Append("},");
            }
            jsonBuilder.Remove(jsonBuilder.Length - 1, 1);
            jsonBuilder.Append("]");
            jsonBuilder.Append("}");
            return jsonBuilder.ToString();
        }

        //json转换是判断是否纯数字
        public bool IsNumber(string str)
        {
            if (str == null || str.Length == 0)    //验证这个参数是否为空  
                return false;                           //是，就返回False  
            ASCIIEncoding ascii = new ASCIIEncoding();//new ASCIIEncoding 的实例  
            byte[] bytestr = ascii.GetBytes(str);         //把string类型的参数保存到数组里  

            foreach (byte c in bytestr)                   //遍历这个数组里的内容  
            {
                if (c < 48 || c > 57)                          //判断是否为数字  
                {
                    return false;                              //不是，就返回False  
                }
            }
            return true;                                        //是，就返回True  
        }
        #endregion


        public ActionResult FormNew()
        {
            return View();
        }

        //按需求导出excel
        public void ExcelExport(string condition)
        {
            ExcelHelper ex = new ExcelHelper();
            string sql = @" select RapidState as 状态,a.res_jb as 投诉级别,a.res_area as 产品区域,a.res_dd as 事发地,a.res_type as 问题类型,a.IsCheck as 是否考核,a.res_again as 是否重复发生,
a.res_ok as 问题类别,b.RealName as 责任人,c.FullName as 责任部门,
res_kf as 客户,res_ms as 问题描述,CONVERT(varchar(100), res_cdate, 23) as 发生日期,res_fxnode as 根本原因分析,
res_csnode as 纠正措施,NotCompleteReason as 未按进度完成原因,Action as 对应措施,Requirements as 规范要求,ActualResults as 规范操作

from FY_Rapid a 
left join Base_User b on a.res_cpeo=b.Code left join Base_Department c on b.DepartmentId=c.DepartmentId where 1=1 ";
            sql = sql + condition;
            DataTable ListData = rapidbll.GetDataTable(sql);
            ex.EcportExcel(ListData, "快速反应导出");
        }

        public ActionResult DownForm()
        {
            return View();
        }

        //匹配人员，每次ajax都会刷新模态窗口，这个功能暂时不能用
        public String MatchUser(string condition)
        {
            string Result = "";
            string sql = " select Code,RealName from Base_User where RealName like '%" + condition + "%' or Code like '%" + condition + "%' ";
            DataTable dt = rapidbll.GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                Result = dt.Rows[0]["Code"] + "," + dt.Rows[0]["RealName"];
            }
            return Result;
            //if(dt.Rows.Count>0)
            //{
            //    return Content(JsonConvert.SerializeObject(dt));
            //}
            //else
            //{
            //    return Content("");
            //}
        }

        public ActionResult ChooseUser()
        {
            return View();
        }
        public ActionResult GetChooseUserList()
        {
            StringBuilder sb = new StringBuilder();
            string sql = " select Code,RealName from Base_User where 1=1 and Enabled=1 ";
            DataTable dt = rapidbll.GetDataTable(sql);
            foreach (DataRow dr in dt.Rows)
            {
                string strchecked = "";
                //if (!string.IsNullOrEmpty(dr["objectid"].ToString()))//判断是否选中
                //{
                //    strchecked = "selected";
                //}
                sb.Append("<li title=\"" + dr["Code"] + ";" + dr["RealName"] + "\" class=\"\">");
                sb.Append("<a id=\"" + dr["Code"] + "\"><img src=\"../../Content/Images/Icon16/role.png \">" + dr["RealName"] + "</a><i></i>");
                sb.Append("</li>");
            }
            return Content(sb.ToString());

        }
        //考勤列表
        public ActionResult AttendanceList()
        {
            return View();
        }

        public ActionResult GetAttendanceListJson(string keywords, string CompanyId, string DepartmentId, JqGridParam jqgridparam, string ParameterJson, string MyTask)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = rapidbll.GetAttendanceList(keywords, ref jqgridparam, ParameterJson, MyTask);
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
        /// <param name="Type">1表示准时出勤，2表示迟到，3表示缺席</param>
        /// <param name="UserID">用户的ID</param>
        /// <returns>受影响的行数，一般为1，其余情况异常</returns>
        public int Attendance(string Type, string UserID)
        {
            StringBuilder strSql = new StringBuilder();
            //此段sql用于判断是否已产生记录，产生了update，未产生insert，已当天，userid为依据判断
            string sql = " select * from  FY_Attendance where UserID='" + UserID + "' and cast(AttendanceDate as date)=cast('" + DateTime.Now.ToString() + "' as date)  ";
            DataTable dt = rapidbll.GetDataTable(sql);

            if (dt.Rows.Count > 0)
            {
                strSql.Append("update FY_Attendance set AttendanceState='" + Type + "' where AttendanceID='" + dt.Rows[0][0].ToString() + "' ");
            }
            else
            {
                strSql.Append("insert into FY_Attendance values(newid(),'" + DateTime.Now.ToString() + "','" + Type + "','" + UserID + "','') ");
            }


            return rapidbll.ExecuteSql(strSql);
        }

        public ActionResult tAttendanceReport()
        {
            return View();
        }
        public ActionResult GetAttendanceReportJson(string keywords, string CompanyId, string DepartmentId, JqGridParam jqgridparam, string startdate, string enddate)
        {
            try
            {
                if (startdate == null || startdate == "undefined" || startdate == "")
                {
                    startdate = DateTime.Now.ToString();
                }
                if (enddate == null || enddate == "undefined" || enddate == "")
                {
                    enddate = DateTime.Now.ToString();
                }
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = rapidbll.GetAttendanceReportJson(keywords, ref jqgridparam, startdate, enddate);
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

        public void AttendanceExcelExport(string startDate, string endDate, JqGridParam jqgridparam)
        {
            ExcelHelper ex = new ExcelHelper();
            DataTable ListData = rapidbll.GetAttendanceReportJson("", ref jqgridparam, startDate, endDate);

            ListData.Columns["RealName"].ColumnName = "用户名";
            ListData.Columns["cqCount"].ColumnName = "出勤次数";
            ListData.Columns["delayCount"].ColumnName = "迟到次数";
            ListData.Columns["qxCount"].ColumnName = "缺席次数";
            ListData.Columns["ztCount"].ColumnName = "早退次数";
            ListData.Columns["cql"].ColumnName = "出席率";
            ex.EcportExcel(ListData, "快速反应出勤率导出");
        }

        //每个人的扣分情况的图形报表，依照分数大小来排序
        public ActionResult ScoreReport()
        {
            return View();
        }

        public string GetScoreData(string startDate, string endDate, string dataType, string person)
        {
            string result = "";
            string temp1 = "";
            string temp2 = "";
            DateTime now = DateTime.Now;
            DateTime d1 = new DateTime(now.Year, now.Month, 1);
            DateTime d2 = d1.AddMonths(1).AddDays(-1);
            if (startDate == "undefined" || startDate == "" || startDate == null)
            {
                startDate = d1.ToString();
            }
            if (endDate == "undefined" || endDate == "" || endDate == null)
            {
                endDate = d2.ToString();
            }
            string dataCondition = "";
            if (dataType == "出勤")
            {
                dataCondition = " and a.Remark in ('缺席','迟到','早退') ";
            }
            if (dataType == "回复质量")
            {
                dataCondition = " and a.Remark not in ('缺席','迟到','早退') ";
            }
            string personCondition = "";
            if (person == "责任人")
            {
                personCondition = " and not exists (select * from temp_usercode where UserCode=b.code) ";
            }
            if (person == "常委")
            {
                personCondition = " and exists (select * from temp_usercode where UserCode=b.code) ";
            }

            string sql = " select b.RealName,sum(a.ScoreNum) as num from FY_RapidScore a left join Base_User b on a.UserID=b.UserId" +
" where cast(ScoreDate as date)>=cast('" + startDate + "' as date) and cast(ScoreDate as date)<=cast('" + endDate + "' as date) " +
dataCondition + personCondition +
" and b.realname!='待定' group by a.UserID,b.RealName  order by num ";
            DataTable dt = rapidbll.GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    temp1 += dt.Rows[i][0].ToString() + ",";
                    temp2 += dt.Rows[i][1].ToString() + ",";
                }
                temp1 = temp1.Substring(0, temp1.Length - 1);
                temp2 = temp2.Substring(0, temp2.Length - 1);

            }
            result = temp1 + "|" + temp2;

            return result;
        }

        public ActionResult DetialForm(string startDate, string endDate, string dataType, string Name)
        {
            DateTime now = DateTime.Now;
            DateTime d1 = new DateTime(now.Year, now.Month, 1);
            DateTime d2 = d1.AddMonths(1).AddDays(-1);
            if (startDate == "undefined" || startDate == "" || startDate == null)
            {
                startDate = d1.ToString();
            }
            if (endDate == "undefined" || endDate == "" || endDate == null)
            {
                endDate = d2.ToString();
            }
            string dataCondition = "";
            if (dataType == "出勤")
            {
                dataCondition = " and a.Remark in ('缺席','迟到','早退') ";
            }
            if (dataType == "回复质量")
            {
                dataCondition = " and a.Remark not in ('缺席','迟到','早退') ";
            }
            string sql = " select b.RealName,a.ScoreNum,cast(a.ScoreDate as date),a.remark from FY_RapidScore a left join Base_User b on a.UserID=b.UserId" +
" where cast(ScoreDate as date)>=cast('" + startDate + "' as date) and cast(ScoreDate as date)<=cast('" + endDate + "' as date) " +
dataCondition + " and b.RealName='" + Name + "' " +
"  order by a.ScoreDate ";
            DataTable dt = rapidbll.GetDataTable(sql);
            ViewData["dt"] = dt;
            return View();
        }

        public ActionResult DeleteFcsh(string KeyValue,string ProcessID)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(@"delete from FY_Process where ProcessID='{0}' ", ProcessID);
            strSql.AppendFormat(@"update FY_Rapid  set ProcessID='' where res_id='{0}' ", KeyValue);
            rapidbll.ExecuteSql(strSql);
            return Content(new JsonMessage { Success = true, Code = 1.ToString(), Message = "操作成功。" }.ToString());
        }

        public ActionResult FormForRapid(string tag,string ProcessID,string rapidID,string state)
        {
            string sql = "";
            ViewData["tag"] = tag;
            ViewData["rapidID"] = rapidID;
            ViewData["ProcessID"] = ProcessID;
            ViewData["state"] = state;
            ViewData["CanEdit"] = 0;
            if (tag == "edit")
            {
                if (state == "只读")
                {
                    ViewData["CanEdit"] = 0;
                }
                else
                {
                    sql = " select res_fcsh from fy_rapid where res_id='" + rapidID + "' ";
                    DataTable dt = rapidbll.GetDataTable(sql);
                    if (dt.Rows[0][0].ToString() == "未提交"|| dt.Rows[0][0].ToString() == "回退")
                    {
                        ViewData["CanEdit"] = 1;
                    }
                }

            }
            return View();
        }

        public ActionResult EventJson(string DepartmentID)
        {
            string sql = " select postname from fy_post where DepartMentID='"+DepartmentID+"' ";
            DataTable ListData = rapidbll.GetDataTable(sql);
            return Content(ListData.ToJson());
        }

        public ActionResult DepartmentJson()
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append("select distinct DepartmentID,FullName,sortcode from Base_Department  order by sortcode ");
            DataTable dt = rapidbll.GetDataTable(strSql.ToString());
            return Content(dt.ToJson());
        }

        public ActionResult EventAllJson()
        {
            string sql = " select postname from fy_post where 1=1 ";
            DataTable ListData = rapidbll.GetDataTable(sql);
            return Content(ListData.ToJson());
        }

        [HttpPost]
        public ActionResult ChangeToGeneral(string KeyValue)
        {
            try
            {
                //将不是很重要的问题，移动到一般问题处理中
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

        public string TransToNormal(string ID)
        {
            StringBuilder strSql = new StringBuilder();

            try
            {
                //将记录插入到一般问题表中
                strSql.AppendFormat(@" insert into FY_GeneralProblem
select NEWID(),res_area,res_ok,res_again,res_type,res_cpeo,res_kf,res_ms,res_cdate,
res_fxnode,res_csnode,res_msfj,'',RapidState,null,DATEADD(DAY,7,res_cdate),'','','',IsCheck,res_jb,res_dd,res_mc,res_cd,Requirements,ActualResults
from FY_Rapid where res_id='{0}' ", ID);
                rapidbll.ExecuteSql(strSql);
            }
            catch
            {
                //避免插入失败还把原来的记录删掉了
                return "-1";
            }

            //删除原来的记录
            StringBuilder DeleteSql = new StringBuilder();
            DeleteSql.AppendFormat(" delete from  FY_Rapid where res_id='{0}' ", ID);
            rapidbll.ExecuteSql(DeleteSql);
            return "0";

        }

        public string SendEmailToLeader(string ID)
        {
            string sql = " select a.*,b.Email from RapidList_New a left join Base_User b on a.res_cpeo=b.Code where res_id='" + ID+"' ";
            DataTable dt = rapidbll.GetDataTable(sql);

            string Content = " 您好，您有一条新的快速反应信息，请知悉。 \n ";
            Content += "问题描述:" + dt.Rows[0]["res_ms"].ToString()+" \n ";
            Content += "问题类别:" + dt.Rows[0]["res_ok"].ToString()+"    "+"部门:"+dt.Rows[0]["FullName"].ToString()+"     "+"负责人:"+dt.Rows[0]["realname"];

            

            var emailAcount = "shfy_it@fuyaogroup.com";
            var emailPassword = "Sj1234";
            var reciver = dt.Rows[0][0].ToString();
            var content = Content;
            MailMessage message = new MailMessage();
            //设置发件人,发件人需要与设置的邮件发送服务器的邮箱一致
            MailAddress fromAddr = new MailAddress("shfy_it@fuyaogroup.com");
            message.From = fromAddr;
            //设置收件人,可添加多个,添加方法与下面的一样
            message.To.Add("yao.sun@fuyaogroup.com");
            
            message.To.Add("zhonghua.yan@fuyaogroup.com");

            message.To.Add("li.wang@fuyaogroup.com");

            message.To.Add(dt.Rows[0]["Email"].ToString());

            //设置抄送人
             message.CC.Add("qingfa.chen@fuyaogroup.com");
            //设置邮件标题
            message.Subject = "QSB快速反应系统邮件";
            //设置邮件内容
            message.Body = content;
            //设置邮件发送服务器,服务器根据你使用的邮箱而不同,可以到相应的 邮箱管理后台查看,下面是QQ的
            SmtpClient client = new SmtpClient("mail.fuyaogroup.com", 25);
            //设置发送人的邮箱账号和密码
            client.Credentials = new NetworkCredential(emailAcount, emailPassword);
            //启用ssl,也就是安全发送
            client.EnableSsl = true;
            //发送邮件
            //加这段之前用公司邮箱发送报错：根据验证过程，远程证书无效
            //加上后解决问题
            ServicePointManager.ServerCertificateValidationCallback =
delegate (Object obj, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) { return true; };
            client.Send(message);
            return "0";
        }

        public ActionResult QualityQPicture()
        {
            return View();
        }

        public ActionResult YearListJson()
        {
            string sql = " select distinct(year(res_cdate)) year from FY_Rapid where 1=1 ";
            DataTable dt = rapidbll.GetDataTable(sql);
            return Content(dt.ToJson());
        }

        public ActionResult MonthListJson(string Year)
        {
            string sql = " select distinct MONTH(res_cdate) as month from FY_Rapid where Year(res_cdate)='" + Year+"' ";
            DataTable dt = rapidbll.GetDataTable(sql);
            return Content(dt.ToJson());
        }

        public ActionResult QPictureListJson(string Year,string Month)
        {
            
            string sql = @" select *,
(select count(*) from FY_Rapid where YEAR(res_cdate)='{0}' and MONTH(res_cdate)='{1}' and DAY(res_cdate)=a.basicday and res_ok='外部' ) as waibuNum,
(select count(*) from FY_Rapid where YEAR(res_cdate)='{0}' and MONTH(res_cdate)='{1}' and DAY(res_cdate)=a.basicday and res_ok='内部' ) as neiNum,
IsOverDue=case when '{1}'=MONTH(GETDATE()) and a.BasicDay>DAY(GETDATE()) then 1 else 0 end 
from base_day a where 1=1 order by a.basicday ";
            sql = string.Format(sql, Year, Month);
            DataTable dt = rapidbll.GetDataTable(sql);
            return Content(dt.ToJson());
        }



    }
}
