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
    public class VP_VerifyPostBll : RepositoryFactory<VP_VerifyPost>
    {
        public DataTable GetPageList(string keyword, ref JqGridParam jqgridparam, string ParameterJson)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.Append(@" select *,
(select sum(DefectNum1+DefectNum2+DefectNum3+DefectNum4+DefectNum5+DefectNum6) from VP_VerifyPostDetail where VerifyPostID=a.VerifyPostID) as DefectNum,
(select max(rn)
from
(
select VerifyPostID, VerifyDate, ROW_NUMBER() OVER(partition by VerifyPostID order by VerifyDate asc) as rn
from VP_VerifyPostDetail a
where DefectNum1=0 and DefectNum2=0 and DefectNum3=0 
and DefectNum4=0 and DefectNum5=0 and DefectNum6=0 and VerifyDate<=cast(GETDATE() as date)
and VerifyPostID=a.VerifyPostID
) as aa) as ZeroNum,
'查看' as trend
from VP_VerifyPost a where 1=1   ");
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(@" AND (SetReason LIKE @keyword
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

        public DataTable GetDetailPageList(string keyword, ref JqGridParam jqgridparam, string ParameterJson)
        {
            StringBuilder strSql = new StringBuilder();
            List<DbParameter> parameter = new List<DbParameter>();
            strSql.AppendFormat(@" select b.VerifyPostDID,c.FullName,a.SetProcess,a.VerifyProduct,a.VerifyDefect,b.VerifyDate,
b.DefectNum1+b.DefectNum2+b.DefectNum3+b.DefectNum4+b.DefectNum5+b.DefectNum6 as DefectNumAll,
b.CheckNum1+b.CheckNum2+b.CheckNum3+b.CheckNum4+b.CheckNum5+b.CheckNum6 as CheckNumAll,
cast(100.0*(b.DefectNum1+b.DefectNum2+b.DefectNum3+b.DefectNum4+b.DefectNum5+b.DefectNum6)/(case when (b.CheckNum1+b.CheckNum2+b.CheckNum3+b.CheckNum4+b.CheckNum5+b.CheckNum6)=0 then 1 else (b.CheckNum1+b.CheckNum2+b.CheckNum3+b.CheckNum4+b.CheckNum5+b.CheckNum6) end) as decimal(18,2)) as DefectRate,
DefectNum1,DefectNum2,DefectNum3,DefectNum4,DefectNum5,DefectNum6,
CheckNum1,CheckNum2,CheckNum3,CheckNum4,CheckNum5,CheckNum6,
b.QualityApprove,b.FactoryManager,b.WorkShopManager,b.GroupManager,b.Status1,b.Status2
,b.Status3,b.Status4,b.Status5,b.Status6,(select COUNT(*) from VP_VerifyPostDetailReview where Result!='' and VerifyPostDID=b.VerifyPostDID and reviewby='QA') as QAcount,
(select COUNT(*) from VP_VerifyPostDetailReview where Result!='' and VerifyPostDID=b.VerifyPostDID and reviewby='FM') as FMcount,
(select COUNT(*) from VP_VerifyPostDetailReview where Result!='' and VerifyPostDID=b.VerifyPostDID and reviewby='WM') as WMcount,
(select COUNT(*) from VP_VerifyPostDetailReview where Result!='' and VerifyPostDID=b.VerifyPostDID and reviewby='GM') as GMcount
from VP_VerifyPost a
left join VP_VerifyPostDetail b on a.VerifyPostID=b.VerifyPostID
left join Base_Department c on a.SetDepart=c.DepartmentId where 1=1 and 
a.SetDepart='{0}' and a.Status!='已完成' ", ManageProvider.Provider.Current().DepartmentId);
            if (!string.IsNullOrEmpty(keyword))
            {
                strSql.Append(keyword);
                
            }
            else
            {
                strSql.Append(" and 1=2 ");
            }
            if (!string.IsNullOrEmpty(ParameterJson) && ParameterJson.Length > 2)
            {
                strSql.Append(ConditionBuilder.GetWhereSql(ParameterJson.JonsToList<Condition>(), out parameter));
            }
            return Repository().FindTablePageBySql(strSql.ToString(), parameter.ToArray(), ref jqgridparam);
        }
    }
}
