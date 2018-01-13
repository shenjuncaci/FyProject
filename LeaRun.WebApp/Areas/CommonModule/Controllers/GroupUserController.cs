using LeaRun.Business;
using LeaRun.Entity;
using LeaRun.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LeaRun.WebApp.Areas.CommonModule.Controllers
{
    /// <summary>
    /// �û�����������
    /// </summary>
    public class GroupUserController : PublicController<Base_GroupUser>
    {
        Base_GroupUserBll base_groupuserbll = new Base_GroupUserBll();
        /// <summary>
        /// ���û�����������б�JONS
        /// </summary>
        /// <param name="CompanyId">��˾ID</param>
        /// <param name="DepartmentId">����ID</param>
        /// <param name="jqgridparam">��ҳ����</param>
        /// <returns></returns>
        public ActionResult GridPageListJson(string CompanyId, string DepartmentId, JqGridParam jqgridparam)
        {
            try
            {
                Stopwatch watch = CommonHelper.TimerStart();
                DataTable ListData = base_groupuserbll.GetPageListNew(CompanyId, DepartmentId, ref jqgridparam);
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
                Base_SysLogBll.Instance.WriteLog("", OperationType.Query, "-1", "�쳣����" + ex.Message);
                return null;
            }
        }

        public ActionResult FormNew()
        {
            return View();
        }

        public virtual ActionResult SubmitFormNew(Base_GroupUser entity, string KeyValue)
        {
            try
            {
                int IsOk = 0;
                string Message = KeyValue == "" ? "�����ɹ���" : "�༭�ɹ���";
                if (!string.IsNullOrEmpty(KeyValue))
                {
                    
                    Base_GroupUser Oldentity = repositoryfactory.Repository().FindEntity(KeyValue);//��ȡû����֮ǰʵ�����
                    entity.Duty = CompareTime(entity.StartTime, entity.EndTime);
                    entity.Modify(KeyValue);
                    IsOk = repositoryfactory.Repository().Update(entity);
                    this.WriteLog(IsOk, entity, Oldentity, KeyValue, Message);
                }
                else
                {
                    entity.Duty = CompareTime(entity.StartTime, entity.EndTime);
                    entity.Create();
                    IsOk = repositoryfactory.Repository().Insert(entity);
                    this.WriteLog(IsOk, entity, null, KeyValue, Message);
                }
                return Content(new JsonMessage { Success = true, Code = IsOk.ToString(), Message = Message }.ToString());
            }
            catch (Exception ex)
            {
                this.WriteLog(-1, entity, null, KeyValue, "����ʧ�ܣ�" + ex.Message);
                return Content(new JsonMessage { Success = false, Code = "-1", Message = "����ʧ�ܣ�" + ex.Message }.ToString());
            }
        }

        public string CompareTime(string startTime,string endTime)
        {
            string duty = "";

            int StartHour = 0;
            int EndHour = 0;
            StartHour = Convert.ToInt32(startTime.Split(':')[0]);
            EndHour = Convert.ToInt32(endTime.Split(':')[0]);
            if(StartHour> EndHour)
            {
                duty = "ҹ��";
            }
            else
            {
                duty = "�װ�";
            }
            return duty;
        }
    }
}