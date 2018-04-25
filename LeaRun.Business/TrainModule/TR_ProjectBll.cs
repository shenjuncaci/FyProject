using LeaRun.DataAccess;
using LeaRun.Entity;
using LeaRun.Repository;
using LeaRun.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace LeaRun.Business
{
    public class TR_ProjectBll : RepositoryFactory<TR_Project>
    {
        public DataTable GetPageList(string keyword, ref JqGridParam jqgridparam, string ParameterJson)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@" select * from tr_project where 1=1   ");
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(@" AND (projectname LIKE @keyword
                                    )");
                parameter.Add(DbFactory.CreateDbParameter("@keyword", '%' + keyword + '%'));
            }
            if (!string.IsNullOrEmpty(ParameterJson) && ParameterJson.Length > 2)
            {
                strSql.Append(ConditionBuilder.GetWhereSql(ParameterJson.JonsToList<Condition>(), out parameter));
            }
            return Repository().FindTablePageBySql(strSql.ToString(), parameter.ToArray(), ref jqgridparam);
        }

        public DataTable GetMyPageList(string keyword, ref JqGridParam jqgridparam, string ParameterJson)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.AppendFormat(@" select * from TR_Project a 
where exists (select * from TR_ProjectMember where ProjectID=a.ProjectID and UserID='{0}')   ",ManageProvider.Provider.Current().UserId);
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(@" AND (projectname LIKE @keyword
                                    )");
                parameter.Add(DbFactory.CreateDbParameter("@keyword", '%' + keyword + '%'));
            }
            if (!string.IsNullOrEmpty(ParameterJson) && ParameterJson.Length > 2)
            {
                strSql.Append(ConditionBuilder.GetWhereSql(ParameterJson.JonsToList<Condition>(), out parameter));
            }
            return Repository().FindTablePageBySql(strSql.ToString(), parameter.ToArray(), ref jqgridparam);
        }



        public DataTable GetDataTable(string sql)
        {
            return Repository().FindDataSetBySql(sql).Tables[0];
        }

        public int ExecuteSql(StringBuilder sql)
        {
            int result = Repository().ExecuteBySql(sql);
            return result;
        }

        public List<TR_ProjectDetail> GetDetailList(string KeyValue)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@"select a.*,b.SkillName from TR_ProjectDetail a left join TR_Skill b on a.SkillID=b.SkillID where a.ProjectID=@ProjectID order by score");
            //strSql.Append(" AND CustomExamID = @CustomExamID and IsEnabe=1 order by SortNO ");
            parameter.Add(DbFactory.CreateDbParameter("@ProjectID", KeyValue));
            return DataFactory.Database().FindListBySql<TR_ProjectDetail>(strSql.ToString(), parameter.ToArray());
        }

        public List<TR_CustomExamUserForDisplay> GetUserList(string KeyValue)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@"select a.UserID,b.Code,b.RealName,c.FullName from TR_ProjectMember a left join Base_User b on a.UserID=b.UserId
left join Base_Department c on b.DepartmentId=c.DepartmentId where 1=1 and a.ProjectID=@ProjectID
order by c.FullName ");

            parameter.Add(DbFactory.CreateDbParameter("@ProjectID", KeyValue));
            return DataFactory.Database().FindListBySql<TR_CustomExamUserForDisplay>(strSql.ToString(), parameter.ToArray());
        }

        public DataTable GetBaseUserList(string keyword, ref JqGridParam jqgridparam, string ParameterJson)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@" select a.UserID,a.Code,a.RealName,b.FullName from
Base_User a left join Base_Department b on a.DepartmentId=b.DepartmentId where 1=1 and a.Enabled=1  ");
            //if (!string.IsNullOrEmpty(keyword))
            //{
            //    strSql.Append(@" AND (SkillName LIKE @keyword or SkillType like @keyword
            //                        )");
            //    parameter.Add(DbFactory.CreateDbParameter("@keyword", '%' + keyword + '%'));
            //}
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.AppendFormat(keyword);
            }

            if (!string.IsNullOrEmpty(ParameterJson) && ParameterJson.Length > 2)
            {
                strSql.Append(ConditionBuilder.GetWhereSql(ParameterJson.JonsToList<Condition>(), out parameter));
            }
            return Repository().FindTablePageBySql(strSql.ToString(), parameter.ToArray(), ref jqgridparam);
        }

        public DataTable GetFileList(string keyword, ref JqGridParam jqgridparam, string ParameterJson, string SkillID)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.AppendFormat(@" select a.fileid,a.filename,a.filepath,a.fileextensions,b.SkillType from TR_SkillFile a left join TR_Skill b on a.SkillID=b.SkillID where a.skillid='{0}'  ", SkillID);
            strSql.AppendFormat(@" union  select VideoSrc,VideoSrc,VideoSrc,'','视频文件' from TR_Skill where skillid='{0}' and VideoSrc!='&nbsp;' and VideoSrc is not null and VideoSrc!='' ", SkillID);
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(keyword);
            }
            if (!string.IsNullOrEmpty(ParameterJson) && ParameterJson.Length > 2)
            {
                strSql.Append(ConditionBuilder.GetWhereSql(ParameterJson.JonsToList<Condition>(), out parameter));
            }
            return Repository().FindTablePageBySql(strSql.ToString(), parameter.ToArray(), ref jqgridparam);
        }

        public DataTable GetCourseDetailList(string keyword, ref JqGridParam jqgridparam, string ParameterJson,string KeyValue)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.AppendFormat(@" select UserID,RealName,ProjectID,
dbo.GetUserProjectPlanScore(UserID,ProjectID) as PlanScore,dbo.GetUserProjectScore(UserID,ProjectID) as Score 
from
(
select distinct a.UserID,b.RealName,a.ProjectID 
from TR_ProjectMember a left join Base_User b on a.UserID=b.UserId
where a.ProjectID='{0}'
) as a  ", KeyValue);
            return Repository().FindTablePageBySql(strSql.ToString(), parameter.ToArray(), ref jqgridparam);
        }

        //获取每个人的学习情况
        public DataTable GetStudyDetailList(string keyword, ref JqGridParam jqgridparam, string ParameterJson)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.AppendFormat(@" select UserID,RealName,ProjectID,
dbo.GetUserProjectPlanScore(UserID,ProjectID) as PlanScore,dbo.GetUserProjectScore(UserID,ProjectID) as Score 
from
(
select distinct a.UserID,b.RealName,a.ProjectID 
from TR_ProjectMember a left join Base_User b on a.UserID=b.UserId
where a.ProjectID='{0}'
) as a  ");
            return Repository().FindTablePageBySql(strSql.ToString(), parameter.ToArray(), ref jqgridparam);
        }

        public DataTable GetStudyList(string keyword, ref JqGridParam jqgridparam, string ParameterJson,string ProjectID)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@"select b.Code,b.RealName,e.FullName,c.ProjectName,d.SkillName,d.Score,StudyTime=(select top 1 StudyMin from TR_UserStudyTime where SkillID=d.SkillID and UserId=a.UserID),
getscore=case when (select max(Score) from TR_Paper where FromSource=2 and SkillID=d.SkillID and UserID=a.UserId)>=d.RequireScore then d.Score else 0 end,30 as StudyMin,
examscore=(select max(Score) from TR_Paper where FromSource=2 and SkillID=d.SkillID and UserID=a.UserId)
from TR_ProjectMember a 
left join Base_User b on a.UserID=b.UserId
left join TR_Project c on a.ProjectID=c.ProjectID
left join TR_ProjectDetail d on c.ProjectID=d.ProjectID
left join Base_Department e on b.DepartmentId=e.DepartmentId  where 1=1 and c.projectID='"+ProjectID+"'  ");
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(@" AND (b.Code LIKE @keyword or b.RealName LIKE @keyword or e.FullName LIKE @keyword or c.ProjectName LIKE @keyword 
                                    )");
                parameter.Add(DbFactory.CreateDbParameter("@keyword", '%' + keyword + '%'));
            }
            if (!string.IsNullOrEmpty(ParameterJson) && ParameterJson.Length > 2)
            {
                strSql.Append(ConditionBuilder.GetWhereSql(ParameterJson.JonsToList<Condition>(), out parameter));
            }
            return Repository().FindTablePageBySql(strSql.ToString(), parameter.ToArray(), ref jqgridparam);
        }

    }
}
