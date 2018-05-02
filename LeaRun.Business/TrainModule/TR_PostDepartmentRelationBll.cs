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
    public class TR_PostDepartmentRelationBll : RepositoryFactory<TR_PostDepartmentRelation>
    {
        public DataTable GetPageList(string keyword, ref JqGridParam jqgridparam, string ParameterJson)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@" select a.relationid,b.PostName from TR_PostDepartmentRelation a left join TR_Post b on a.postid=b.PostID where 1=1 and a.DepartmentID='"+ManageProvider.Provider.Current().DepartmentId+"'   ");
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(@" AND (PostName LIKE @keyword
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

        public List<TR_PostDepartmentRelationDetail> GetDetailList(string RelationID)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@"SELECT * FROM TR_PostDepartmentRelationDetail WHERE 1=1");
            strSql.Append(" AND RelationID = @RelationID order by SkillWeight desc ");
            parameter.Add(DbFactory.CreateDbParameter("@RelationID", RelationID));
            return DataFactory.Database().FindListBySql<TR_PostDepartmentRelationDetail>(strSql.ToString(), parameter.ToArray());
        }

        public List<TR_EvaluateDetail> GetEvaluateDetailList(string RelationID)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@"SELECT * FROM TR_EvaluateDetail WHERE 1=1 and Enabled=1 ");
            strSql.Append(" AND UserPostRelationID = @UserPostRelationID ");
            strSql.Append(" order by SkillWeight desc ");
            parameter.Add(DbFactory.CreateDbParameter("@UserPostRelationID", RelationID));
            return DataFactory.Database().FindListBySql<TR_EvaluateDetail>(strSql.ToString(), parameter.ToArray());
        }


        public DataTable GetSkillList(string keyword, ref JqGridParam jqgridparam, string ParameterJson,
            string SkillName,string SkillType)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@" select * from TR_Skill where 1=1 and Enable=1 and IsAudit=1  ");
            //if (!string.IsNullOrEmpty(keyword))
            //{
            //    strSql.Append(@" AND (SkillName LIKE @keyword or SkillType like @keyword
            //                        )");
            //    parameter.Add(DbFactory.CreateDbParameter("@keyword", '%' + keyword + '%'));
            //}
            if (!string.IsNullOrEmpty(SkillName))
            {
                strSql.AppendFormat(@" and skillname like '%{0}%' ",SkillName);
            }
            if (!string.IsNullOrEmpty(SkillType))
            {
                strSql.AppendFormat(@" and SkillType like '%{0}%' ", SkillType);
            }
            if (!string.IsNullOrEmpty(ParameterJson) && ParameterJson.Length > 2)
            {
                strSql.Append(ConditionBuilder.GetWhereSql(ParameterJson.JonsToList<Condition>(), out parameter));
            }
            return Repository().FindTablePageBySql(strSql.ToString(), parameter.ToArray(), ref jqgridparam);
        }


        public DataTable GetUserList(string keyword, ref JqGridParam jqgridparam, string ParameterJson)
        {
            //判断下是否是根部门的负责人
            string IsDept = "  select ParentId from Base_Department where DepartmentId='{0}' ";
            IsDept = string.Format(IsDept, ManageProvider.Provider.Current().DepartmentId);
            DataTable dtIsdept = GetDataTable(IsDept);
            string IsManage = dtIsdept.Rows[0][0].ToString();

            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@" select * , (select '['+b.PostName+']' from TR_PostDepartmentRelation a left join TR_Post b on a.PostID=b.PostID left join TR_UserPost c on
a.RelationID=c.DepartmentPostID and c.userid=aa.userid 
where c.UserID=aa.userid and IsMain=0 for xml path('')) as PostList,
(select top 1 b.PostName from TR_PostDepartmentRelation a left join TR_Post b on a.PostID=b.PostID left join TR_UserPost c on
a.RelationID=c.DepartmentPostID and c.userid=aa.userid 
where c.UserID=aa.userid and IsMain=1 ) as MainPost
from Base_User aa where 
( DepartmentId='" + ManageProvider.Provider.Current().DepartmentId+ "' or DepartmentID in (select DepartmentId from Base_Department where ParentId='"+ManageProvider.Provider.Current().DepartmentId+"') ) and UserID!='"+ManageProvider.Provider.Current().UserId+ "' and aa.Enabled=1 ");
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(@" AND (realname LIKE @keyword
                                    )");
                parameter.Add(DbFactory.CreateDbParameter("@keyword", '%' + keyword + '%'));
            }
            if(IsManage=="0")
            {
                strSql.AppendFormat(@" and (exists( select * from Base_ObjectUserRelation where 
ObjectId='91c17ca4-0cbf-43fa-829e-3021b055b6c4' and UserId=aa.userid) or DepartmentID='{0}' ) ",
ManageProvider.Provider.Current().DepartmentId);
            }
            if(ManageProvider.Provider.Current().DepartmentId== "aabd7992-dbcc-40eb-ad9f-c01920343b0e"||ManageProvider.Provider.Current().DepartmentId== "159df668-e428-4dcb-9a71-c7cdeabdeb03")
            {
                strSql.AppendFormat(@" union select * , (select '['+b.PostName+']' from TR_PostDepartmentRelation a left join TR_Post b on a.PostID=b.PostID left join TR_UserPost c on
a.RelationID=c.DepartmentPostID and c.userid=aa.userid 
where c.UserID=aa.userid and IsMain=0 for xml path('')) as PostList,
(select top 1 b.PostName from TR_PostDepartmentRelation a left join TR_Post b on a.PostID=b.PostID left join TR_UserPost c on
a.RelationID=c.DepartmentPostID and c.userid=aa.userid 
where c.UserID=aa.userid and IsMain=1 ) as MainPost
from Base_User aa where aa.Enabled=1 and DepartmentId in (select DepartmentId from Base_Department where ParentId='0')
and exists (select * from Base_ObjectUserRelation where UserId=aa.UserId and ObjectId='91c17ca4-0cbf-43fa-829e-3021b055b6c4') and aa.Enabled=1 ");
            }
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(@" AND (realname LIKE @keyword1
                                    )");
                parameter.Add(DbFactory.CreateDbParameter("@keyword1", '%' + keyword + '%'));
            }
            if (!string.IsNullOrEmpty(ParameterJson) && ParameterJson.Length > 2)
            {
                strSql.Append(ConditionBuilder.GetWhereSql(ParameterJson.JonsToList<Condition>(), out parameter));
            }
            return Repository().FindTablePageBySql(strSql.ToString(), parameter.ToArray(), ref jqgridparam);
        }

        public DataTable GetEvaluateList(string keyword, ref JqGridParam jqgridparam, string ParameterJson)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.AppendFormat(@"select a.UserPostRelationID,d.Code,d.RealName,c.PostName,a.Evaluate from TR_UserPost a
left join TR_PostDepartmentRelation b on a.DepartmentPostID=b.RelationID
left join TR_Post c on b.PostID=c.PostID
left join Base_User d on a.UserID=d.UserId
where d.Enabled=1 and b.RelationID is not null and (d.DepartmentId='{0}' or d.DepartmentID in (select DepartmentID from Base_Department where ParentID='{0}' ))
and a.UserID!='{1}' and exists (select * from tr_skill where skillid in (select skillid from TR_PostDepartmentRelationDetail where RelationID=b.RelationID) )
 ", ManageProvider.Provider.Current().DepartmentId,ManageProvider.Provider.Current().UserId);
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(@" AND (realname LIKE @keyword
                                    )");
                parameter.Add(DbFactory.CreateDbParameter("@keyword", '%' + keyword + '%'));
            }
            if (!string.IsNullOrEmpty(ParameterJson) && ParameterJson.Length > 2)
            {
                strSql.Append(ConditionBuilder.GetWhereSql(ParameterJson.JonsToList<Condition>(), out parameter));
            }
            return Repository().FindTablePageBySql(strSql.ToString(), parameter.ToArray(), ref jqgridparam);
        }

        public DataTable GetAuditList(string keyword, ref JqGridParam jqgridparam, string ParameterJson)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.AppendFormat(@"select 
a.AuditID,a.audittype,d.RealName as createby,b.RealName as diaodongren,a.createdt,c.FullName as departmentname,a.TargetDepartmentID
from tr_hrauditlist a
left join Base_User b on a.AuditID=b.UserId
left join Base_Department c on a.targetdepartmentid=c.DepartmentId
left join base_user d on a.createby=d.UserId
where auditby=''
union
select a.SkillID,'技能新增审批' as type,a.CreateBy,a.skillname,CreateDt,b.fullname,''  
from TR_Skill a
left join Base_Department b on a.DepartmentID=b.DepartmentId
where IsAudit=0 and Enable=1
union
select a.SkillID,'技能删除审批' as type,a.CreateBy,a.skillname,CreateDt,b.fullname,''  
from TR_Skill a
left join Base_Department b on a.DepartmentID=b.DepartmentId
where IsAudit=0 and Enable=0 
 ");
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(@" AND (realname LIKE @keyword
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
