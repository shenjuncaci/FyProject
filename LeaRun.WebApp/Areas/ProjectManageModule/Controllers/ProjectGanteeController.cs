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
        G_PLM_ProjectBll ProjectNewBll = new G_PLM_ProjectBll();
        G_PLM_ProjectFileBll ProjectFileBll = new G_PLM_ProjectFileBll();

        G_PLM_ProjectStructure_1Bll Structure1Bll = new G_PLM_ProjectStructure_1Bll();
        G_PLM_ProjectStructure_2Bll Structure2Bll = new G_PLM_ProjectStructure_2Bll();

        Base_FlowBll FlowBll = new Base_FlowBll();
        //
        // GET: /ProjectManageModule/ProjectGantee/
        //public int InsertUser()
        //{
        //    Base_ObjectUserRelationBll base_objectuserrelationbll = new Base_ObjectUserRelationBll();
        //    //ManageProvider.Provider.Current().DepartmentId = "71172861-0af0-4cb7-9ba2-924424cd27d5";
        //    IManageUser imanageuser = new IManageUser();
        //    imanageuser.UserId = "ffeee604-9c4e-4743-a77c-ea26352b5de4";
        //    imanageuser.Account = "096827";
        //    imanageuser.UserName = "沈骏";
        //    imanageuser.Gender = "男";
        //    imanageuser.Password = "1ee4dcc39cac066fa91be5c9b021c147";
        //    imanageuser.Code = "096827";
        //    imanageuser.Secretkey = "246b745babb2a9a6";
        //    imanageuser.LogTime = DateTime.Now;
        //    imanageuser.CompanyId = "31b05701-60ef-405c-87ba-af47049e3f48";
        //    imanageuser.DepartmentId = "71172861-0af0-4cb7-9ba2-924424cd27d5";
        //    imanageuser.ObjectId = "7183d9c5-d48b-436a-9f62-7f30f5a02c5c";
        //    //imanageuser.GroupID = ManageProvider.Provider.Current().GroupID;
        //    //imanageuser.IPAddress = ManageProvider.Provider.Current().IPAddress;
        //    //imanageuser.IPAddressName = ManageProvider.Provider.Current().IPAddressName;
        //    imanageuser.IsSystem = false;
        //    //imanageuser.DepartmentName = base_objectuserrelationbll.GetDepartmentName(DepartmentID);
        //    ManageProvider.Provider.AddCurrent(imanageuser);
        //    return 0;
        //}
        public ActionResult Index()
        {
            Base_NoteNOBll notenobll = new Base_NoteNOBll();
            ViewBag.BillNo = notenobll.Code("ProjectGanteNO");
            //InsertUser();
            return View();
        }

        public ActionResult ProjectIndex()
        {
            return View();
        }

        public ActionResult SubmitGanteeData(string tasks, string removeds, string ProjectID)
        {
            IDatabase database = DataFactory.Database();
            DbTransaction isOpenTrans = database.BeginTrans();
            StringBuilder sqldel = new StringBuilder();
            sqldel.AppendFormat(@" delete from G_PLM_ProjectPreLink where PredecessorUID in ( select UID from G_PLM_ProjectGantee where ProjectID='" + ProjectID + "')  ");
            sqldel.AppendFormat(@" delete from G_PLM_ProjectGantee where ProjectID='" + ProjectID + "' ");

            ProjectBll.ExecuteSql(sqldel);

            //database.Delete<PM_ProjectMember>("PM_ProjectGantee", ProjectID, isOpenTrans);
            //树形结构反序列化,需要保存的数据
            List<ProjectGanteeDisplay> ProjectGanteeList = tasks.JonsToList<ProjectGanteeDisplay>();
            foreach (ProjectGanteeDisplay pgd in ProjectGanteeList)
            {
                G_PLM_ProjectGantee entity = new G_PLM_ProjectGantee();
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
                entity.ProjectID = ProjectID;
                entity.StructureForm = pgd.StructureForm;


                database.Insert(entity, isOpenTrans);
                //前置节点处理
                if (pgd.PredecessorLink.Count != 0)
                {
                    foreach (G_PLM_ProjectPreLink prl in pgd.PredecessorLink)
                    {
                        database.Insert(prl, isOpenTrans);
                    }
                }
                //子节点处理
                if (pgd.children != null)
                {
                    foreach (ProjectGanteeDisplay pgdc in pgd.children)
                    {
                        G_PLM_ProjectGantee entityD = new G_PLM_ProjectGantee();
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
                        entityD.ProjectID = ProjectID;
                        entityD.StructureForm = pgdc.StructureForm;
                        database.Insert(entityD, isOpenTrans);

                        if (pgdc.PredecessorLink.Count != 0)
                        {
                            foreach (G_PLM_ProjectPreLink prl in pgdc.PredecessorLink)
                            {
                                G_PLM_ProjectPreLink entityl = new G_PLM_ProjectPreLink();
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

        public ActionResult GetData(string ProjectID)
        {
            string sql = " select * from G_PLM_ProjectGantee where ParentID='0' and projectiD='" + ProjectID + "' order by start  ";
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
                sql = " select * from G_PLM_ProjectPreLink where TaskUID='" + pm.UID + "' ";
                dt = ProjectBll.GetDataTable(sql);
                if (dt.Rows.Count > 0)
                {
                    List<G_PLM_ProjectPreLink> listpre = DtConvertHelper.ConvertToModelListNew<G_PLM_ProjectPreLink>(dt);
                    foreach (G_PLM_ProjectPreLink pmpre in listpre)
                    {
                        pmpre.Limit = true;
                    }
                    pm.PredecessorLink = listpre;
                }

                sql = " select * from G_PLM_ProjectGantee where ParentID='" + pm.UID + "' order by start ";
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

        public ActionResult ProjectJson()
        {
            string sql = " select ProjectID,ProjectName from G_PLM_Project where 1=1 ";
            DataTable dt = ProjectBll.GetDataTable(sql);
            return Content(dt.ToJson());
        }

        //获取列表数据
        public ActionResult GridPageListJson(string keywords, string CompanyId, string DepartmentId, JqGridParam jqgridparam, string ParameterJson)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = ProjectNewBll.GetPageList(keywords, ref jqgridparam, ParameterJson);
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
            return View();
        }

        [HttpPost]
        public ActionResult SubmitForm(string KeyValue, G_PLM_Project entity, string BuildFormJson, HttpPostedFileBase Filedata)
        {
            string ModuleId = DESEncrypt.Decrypt(CookieHelper.GetCookie("ModuleId"));
            IDatabase database = DataFactory.Database();
            DbTransaction isOpenTrans = database.BeginTrans();
            try
            {
                string Message = KeyValue == "" ? "新增成功。" : "编辑成功。";
                if (!string.IsNullOrEmpty(KeyValue))
                {



                    entity.Modify(KeyValue);
                    entity.ModifyBy = ManageProvider.Provider.Current().UserName;
                    entity.ModifyDate = DateTime.Now;

                    database.Update(entity, isOpenTrans);

                }
                else
                {
                    
                    entity.Create();
                    entity.CreateBy = ManageProvider.Provider.Current().UserName;
                    entity.CreateDate = DateTime.Now;
                    entity.FlowID = FlowBll.RegistFlow("Sj-ProjectGantee", entity.ProjectID, "");

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
            G_PLM_Project entity = DataFactory.Database().FindEntity<G_PLM_Project>(KeyValue);
            if (entity == null)
            {
                return Content("");
            }

            string strJson = entity.ToJson();

            //strJson = strJson.Insert(1, Base_FormAttributeBll.Instance.GetBuildForm(KeyValue));
            return Content(strJson);
        }


        [HttpPost]
        public ActionResult Delete(string KeyValue)
        {
            RepositoryFactory<G_PLM_Project> repositoryfactory = new RepositoryFactory<G_PLM_Project>();
            try
            {
                var Message = "删除失败。";
                int IsOk = 0;

                IsOk = repositoryfactory.Repository().Delete(KeyValue);
                if (IsOk > 0)
                {
                    Message = "删除成功。";
                }


                return Content(new JsonMessage { Success = true, Code = IsOk.ToString(), Message = Message }.ToString());
            }
            catch (Exception ex)
            {

                return Content(new JsonMessage { Success = false, Code = "-1", Message = "操作失败：" + ex.Message }.ToString());
            }
        }

        public ActionResult Uploadify()
        {
            return View();
        }
        public ActionResult GetExistsList(string UID)
        {
            string sql = @" select a.*,1 canDel 
from G_PLM_ProjectFile a 

where 1=1 and a.UID='{0}' ";
            sql = string.Format(sql, UID);
            DataTable dt = ProjectBll.GetDataTable(sql);
            return Content(dt.ToJson());
        }
        public void Download(string KeyValue)
        {
            G_PLM_ProjectFile entity = ProjectFileBll.Repository().FindEntity(KeyValue);
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
                G_PLM_ProjectFile entity = ProjectFileBll.Repository().FindEntity(NetworkFileId);
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
            string sql = "select * from G_PLM_ProjectFile where FileID='" + FileID + "'";
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
                G_PLM_ProjectFile entity = new G_PLM_ProjectFile();
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
                        entity.UID = FolderId;
                        entity.FileName = Filedata.FileName;
                        //entity.FilePath = virtualPath;
                        entity.FileSize = filesize.ToString();
                        entity.FileExtensions = FileEextension;
                        string _FileType = "";
                        string _Icon = "";
                        this.DocumentType(FileEextension, ref _FileType, ref _Icon);
                        entity.Icon = _Icon;
                        entity.FileType = _FileType;
                        IsOk = DataFactory.Database().Insert<G_PLM_ProjectFile>(entity).ToString();
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

        public string CheckStructureID(string KeyValue, string StructureForm)
        {
            //根据StructureForm来选择对应的数据表，可自定义添加
            string table = "";
            if (StructureForm == "1")
            {
                table = "G_PLM_ProjectStructure_1";
            }
            if (StructureForm == "2")
            {
                table = "G_PLM_ProjectStructure_2";
            }
            string sql = " select * from " + table + " where UID='" + KeyValue + "' ";
            DataTable dt = ProjectBll.GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
                return dt.Rows[0][0].ToString();
            }
            else
            {
                return "";
            }
        }

        public ActionResult StructForm1()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SubmitForm_Struct1(string KeyValue, G_PLM_ProjectStructure_1 entity, string BuildFormJson, HttpPostedFileBase Filedata, string UID)
        {
            string ModuleId = DESEncrypt.Decrypt(CookieHelper.GetCookie("ModuleId"));
            IDatabase database = DataFactory.Database();
            DbTransaction isOpenTrans = database.BeginTrans();
            try
            {
                string Message = KeyValue == "" ? "新增成功。" : "编辑成功。";
                if (!string.IsNullOrEmpty(KeyValue))
                {



                    entity.Modify(KeyValue);
                    entity.UID = UID;


                    database.Update(entity, isOpenTrans);

                }
                else
                {

                    entity.Create();
                    entity.UID = UID;


                    database.Insert(entity, isOpenTrans);

                    Base_DataScopePermissionBll.Instance.AddScopeDefault(ModuleId, ManageProvider.Provider.Current().UserId, entity.StructureID, isOpenTrans);
                }
                Base_FormAttributeBll.Instance.SaveBuildForm(BuildFormJson, entity.StructureID, ModuleId, isOpenTrans);
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
        public ActionResult SetForm_Struct1(string KeyValue)
        {
            G_PLM_ProjectStructure_1 entity = DataFactory.Database().FindEntity<G_PLM_ProjectStructure_1>(KeyValue);
            if (entity == null)
            {
                return Content("");
            }

            string strJson = entity.ToJson();

            //strJson = strJson.Insert(1, Base_FormAttributeBll.Instance.GetBuildForm(KeyValue));
            return Content(strJson);
        }

        public ActionResult StructForm2()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SubmitForm_Struct2(string KeyValue, G_PLM_ProjectStructure_2 entity, string BuildFormJson, HttpPostedFileBase Filedata)
        {
            string ModuleId = DESEncrypt.Decrypt(CookieHelper.GetCookie("ModuleId"));
            IDatabase database = DataFactory.Database();
            DbTransaction isOpenTrans = database.BeginTrans();
            try
            {
                string Message = KeyValue == "" ? "新增成功。" : "编辑成功。";
                if (!string.IsNullOrEmpty(KeyValue))
                {



                    entity.Modify(KeyValue);


                    database.Update(entity, isOpenTrans);

                }
                else
                {

                    entity.Create();


                    database.Insert(entity, isOpenTrans);

                    Base_DataScopePermissionBll.Instance.AddScopeDefault(ModuleId, ManageProvider.Provider.Current().UserId, entity.StructureID, isOpenTrans);
                }
                Base_FormAttributeBll.Instance.SaveBuildForm(BuildFormJson, entity.StructureID, ModuleId, isOpenTrans);
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
        public ActionResult SetForm_Struct2(string KeyValue)
        {
            G_PLM_ProjectStructure_2 entity = DataFactory.Database().FindEntity<G_PLM_ProjectStructure_2>(KeyValue);
            if (entity == null)
            {
                return Content("");
            }

            string strJson = entity.ToJson();

            //strJson = strJson.Insert(1, Base_FormAttributeBll.Instance.GetBuildForm(KeyValue));
            return Content(strJson);
        }

        public ActionResult GanteTemplate()
        {
            return View();
        }

        public ActionResult ChooseTemplate()
        {
            return View();
        }



        public ActionResult ImportGanteeData(string tasks, string removeds, string ProjectID)
        {
            IDatabase database = DataFactory.Database();
            DbTransaction isOpenTrans = database.BeginTrans();
            StringBuilder sqldel = new StringBuilder();
            sqldel.AppendFormat(@" delete from G_PLM_ProjectPreLink where PredecessorUID in ( select UID from G_PLM_ProjectGantee where ProjectID='" + ProjectID + "')  ");
            sqldel.AppendFormat(@" delete from G_PLM_ProjectGantee where ProjectID='" + ProjectID + "' ");

            ProjectBll.ExecuteSql(sqldel);

            //database.Delete<PM_ProjectMember>("PM_ProjectGantee", ProjectID, isOpenTrans);
            //树形结构反序列化,需要保存的数据
            List<ProjectGanteeDisplay> ProjectGanteeList = tasks.JonsToList<ProjectGanteeDisplay>();
            foreach (ProjectGanteeDisplay pgd in ProjectGanteeList)
            {
                G_PLM_ProjectGantee entity = new G_PLM_ProjectGantee();
                entity.UID = CommonHelper.GetGuid;
                pgd.UID = entity.UID;
                entity.Name = pgd.Name;
                entity.Start = pgd.Start;
                entity.Finish = pgd.Finish;
                entity.Duration = pgd.Duration;
                entity.PercentComplete = pgd.PercentComplete;
                entity.Summary = pgd.Summary;
                entity.Critical = pgd.Critical;
                entity.Milestone = pgd.Milestone;
                entity.ParentID = "0";
                entity.ProjectID = ProjectID;
                entity.StructureForm = pgd.StructureForm;


                database.Insert(entity, isOpenTrans);
                //前置节点处理
                if (pgd.PredecessorLink.Count != 0)
                {
                    foreach (G_PLM_ProjectPreLink prl in pgd.PredecessorLink)
                    {
                        prl.PredecessorUID= CommonHelper.GetGuid;
                        prl.TaskUID = entity.UID;
                        database.Insert(prl, isOpenTrans);
                    }
                }
                //子节点处理
                if (pgd.children != null)
                {
                    foreach (ProjectGanteeDisplay pgdc in pgd.children)
                    {
                        G_PLM_ProjectGantee entityD = new G_PLM_ProjectGantee();
                        entityD.UID =CommonHelper.GetGuid;
                        entityD.Name = pgdc.Name;
                        entityD.Start = pgdc.Start;
                        entityD.Finish = pgdc.Finish;
                        entityD.Duration = pgdc.Duration;
                        entityD.PercentComplete = pgdc.PercentComplete;
                        entityD.Summary = pgdc.Summary;
                        entityD.Critical = pgdc.Critical;
                        entityD.Milestone = pgdc.Milestone;
                        entityD.ParentID = pgd.UID;
                        entityD.ProjectID = ProjectID;
                        entityD.StructureForm = pgdc.StructureForm;
                        database.Insert(entityD, isOpenTrans);

                        if (pgdc.PredecessorLink.Count != 0)
                        {
                            foreach (G_PLM_ProjectPreLink prl in pgdc.PredecessorLink)
                            {
                                G_PLM_ProjectPreLink entityl = new G_PLM_ProjectPreLink();
                                //entity.
                                prl.PredecessorUID= CommonHelper.GetGuid;
                                prl.TaskUID = entityD.UID;
                                database.Insert(prl, isOpenTrans);
                            }
                        }
                    }
                }

            }
            database.Commit();

            return Content("成功");
        }

        public ActionResult FlowForm(string ProjectID)
        {
            G_PLM_Project entity = DataFactory.Database().FindEntity<G_PLM_Project>(ProjectID);
            FlowDisplay flow = FlowBll.FlowDisplay(entity.FlowID);
            ViewData["flow"] = flow;
            ViewData["UserID"] = ManageProvider.Provider.Current().UserId;
            return View();
        }

        public int submit(string KeyValue, string FlowID, string type, string ProcessOpinion, string RejectNO)
        {

            int a = 0;
            StringBuilder strSql = new StringBuilder();
            //
            string sql = " select CurrentPost from Base_FlowLog where FlowID='" + FlowID + "'  ";
            DataTable dt = ProjectBll.GetDataTable(sql);


            //FY_ObjectTracking entity = DataFactory.Database().FindEntity<FY_ObjectTracking>(KeyValue);
            if (type == "-1" || type == "-2")
            {
                a = FlowBll.RejectFlow(FlowID, ProcessOpinion, RejectNO);
            }
            else if (type == "-9" || type == "-92")
            {
                //终止流程
                a = FlowBll.StopFlow(FlowID, ProcessOpinion);
            }
            else
            {
                a = FlowBll.SubmitFlow(FlowID, ProcessOpinion);
            }
            if (a == 9)
            {
                Base_NoteNOBll notenobll = new Base_NoteNOBll();
                //strSql.AppendFormat(@" update  PM_Project set CreateDate=GETDATE(),ProjectNO='{0}',ProjectStatus='已登录' where ProjectID='{1}' ", notenobll.CodeByYear("ProjectNO"), KeyValue);

            }
            strSql.AppendFormat(@" update G_PLM_Project set Approvestatus='{0}' where ProjectID='{1}'  ", a, KeyValue);


            ProjectBll.ExecuteSql(strSql);
            return a;
        }

        public ActionResult UserInfo()
        {
            return View();
        }

        public ActionResult UserListBatch()
        {
            return View();
        }

        public ActionResult GridUserListJson(string keywords, string CompanyId, string DepartmentId, JqGridParam jqgridparam, string ParameterJson)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = ProjectNewBll.GetUserList(keywords, ref jqgridparam, ParameterJson);
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

        public ActionResult GetDetailList(string ProjectID,string UID)
        {
            try
            {
                var JsonData = new
                {
                    rows = ProjectNewBll.GetDetailList(ProjectID,UID),
                };
                return Content(JsonData.ToJson());
            }
            catch (Exception ex)
            {
                Base_SysLogBll.Instance.WriteLog("", OperationType.Query, "-1", "异常错误：" + ex.Message);
                return null;
            }
        }

        public ActionResult SubmitDetailForm(string ProjectID, string BuildFormJson, HttpPostedFileBase Filedata, string DetailForm,string UID)
        {
            IDatabase database = DataFactory.Database();
            DbTransaction isOpenTrans = database.BeginTrans();
            database.Delete<G_PLM_ProjectGanteeUser>("ProjectID", ProjectID, isOpenTrans);
            List<G_PLM_ProjectGanteeUser> DetailList = DetailForm.JonsToList<G_PLM_ProjectGanteeUser>();
            int index = 1;
            foreach (G_PLM_ProjectGanteeUser entityD in DetailList)
            {
                if (!string.IsNullOrEmpty(entityD.UserID))
                {
                    entityD.Create();
                    entityD.UID = UID;
                    entityD.ProjectID = ProjectID;
                    database.Insert(entityD, isOpenTrans);
                    index++;
                }
            }
            database.Commit();
            return Content(new JsonMessage { Success = true, Code = "1", Message = "编辑成功" }.ToString());
        }

        public ActionResult MyTask()
        {
            return View();
        }

        public ActionResult GridMyTaskListJson(string keywords, string CompanyId, string DepartmentId, JqGridParam jqgridparam, string ParameterJson)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = ProjectNewBll.GetMyTaskList(keywords, ref jqgridparam, ParameterJson);
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

        public ActionResult TaskForm()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SubmitForm_Task(string KeyValue, G_PLM_ProjectGantee entity, string BuildFormJson, HttpPostedFileBase Filedata)
        {
            string ModuleId = DESEncrypt.Decrypt(CookieHelper.GetCookie("ModuleId"));
            IDatabase database = DataFactory.Database();
            DbTransaction isOpenTrans = database.BeginTrans();
            try
            {
                string Message = KeyValue == "" ? "新增成功。" : "编辑成功。";
                if (!string.IsNullOrEmpty(KeyValue))
                {



                    entity.Modify(KeyValue);


                    database.Update(entity, isOpenTrans);

                }
                else
                {

                    entity.Create();


                    database.Insert(entity, isOpenTrans);

                    Base_DataScopePermissionBll.Instance.AddScopeDefault(ModuleId, ManageProvider.Provider.Current().UserId, entity.UID, isOpenTrans);
                }
                Base_FormAttributeBll.Instance.SaveBuildForm(BuildFormJson, entity.UID, ModuleId, isOpenTrans);
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
        public ActionResult SetForm_Task(string KeyValue)
        {
            G_PLM_ProjectGantee entity = DataFactory.Database().FindEntity<G_PLM_ProjectGantee>(KeyValue);
            if (entity == null)
            {
                return Content("");
            }

            string strJson = entity.ToJson();

            //strJson = strJson.Insert(1, Base_FormAttributeBll.Instance.GetBuildForm(KeyValue));
            return Content(strJson);
        }
    }


}
