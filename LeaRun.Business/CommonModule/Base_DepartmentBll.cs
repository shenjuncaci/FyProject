//=====================================================================================
// All Rights Reserved , Copyright @ Learun 2014
// Software Developers @ Learun 2014
//=====================================================================================

using LeaRun.DataAccess;
using LeaRun.Entity;
using LeaRun.Repository;
using LeaRun.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace LeaRun.Business
{
    /// <summary>
    /// ���Ź���
    /// <author>
    ///		<name>she</name>
    ///		<date>2014.08.07 12:34</date>
    /// </author>
    /// </summary>
    public class Base_DepartmentBll : RepositoryFactory<Base_Department>
    {
        /// <summary>
        /// ��ȡ ��˾������ �б�
        /// </summary>
        /// <returns></returns>
        public DataTable GetTree()
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append(@"SELECT  *
                            FROM    ( SELECT    CompanyId,				--��˾ID
												CompanyId AS DepartmentId ,--����ID
                                                Code ,					--����
                                                FullName ,				--����
                                                ParentId ,				--�ڵ�ID
                                                SortCode,				--�������
                                                'Company' AS Sort		--����
                                      FROM      Base_Company			--��˾��
                                      UNION
                                      SELECT    CompanyId,				--��˾ID
												DepartmentId,			--����ID
                                                Code ,					--����
                                                FullName ,				--����
                                                CompanyId AS ParentId ,	--�ڵ�ID
                                                SortCode,				--�������
                                                'Department' AS Sort	--����
                                      FROM      Base_Department	where 	ParentId='0'	--���ű�ParentId=0
                                    ) T WHERE 1=1 ");
            //if (!ManageProvider.Provider.Current().IsSystem)
            //{
            //    strSql.Append(" AND ( DepartmentId IN ( SELECT ResourceId FROM Base_DataScopePermission WHERE");
            //    strSql.Append(" ObjectId IN ('" + ManageProvider.Provider.Current().ObjectId.Replace(",", "','") + "') ");
            //    strSql.Append(" ) )");
            //}
            strSql.Append(" ORDER BY SortCode ASC");
            return Repository().FindTableBySql(strSql.ToString());
        }

        public DataTable GetTreeNew()
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append(@"SELECT  *
                            FROM    ( SELECT    CompanyId,				--��˾ID
												CompanyId AS DepartmentId ,--����ID
                                                Code ,					--����
                                                FullName ,				--����
                                                ParentId ,				--�ڵ�ID
                                                SortCode,				--�������
                                                'Company' AS Sort		--����
                                      FROM      Base_Company			--��˾��
                                      UNION
                                      SELECT    CompanyId,				--��˾ID
												DepartmentId,			--����ID
                                                Code ,					--����
                                                FullName ,				--����
                                                CompanyId AS ParentId ,	--�ڵ�ID
                                                SortCode,				--�������
                                                'Department' AS Sort	--����
                                      FROM      Base_Department			--���ű�ParentId=0
                                    ) T WHERE 1=1 ");
            if (!ManageProvider.Provider.Current().IsSystem)
            {
                strSql.Append(" AND ( DepartmentId IN ('"+ManageProvider.Provider.Current().DepartmentId+ "','31b05701-60ef-405c-87ba-af47049e3f48','1')");
                strSql.Append(" ");
                strSql.Append("  )");
            }
            strSql.Append(" ORDER BY SortCode ASC");
            return Repository().FindTableBySql(strSql.ToString());
        }
        /// <summary>
        /// ���ݹ�˾id��ȡ���� �б�
        /// </summary>
        /// <param name="CompanyId">��˾ID</param>
        /// <returns></returns>
        public DataTable GetList(string CompanyId)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append(@"SELECT  *
                            FROM    ( SELECT    d.DepartmentId ,			--����
                                                c.FullName AS CompanyName ,	--������˾
                                                d.CompanyId ,				--������˾Id
                                                d.ParentID,                 --�ϼ����ŵ�ID
                                                d.Code ,					--����
                                                d.FullName ,				--��������
                                                d.ShortName ,				--���ż��
                                                d.Nature ,					--��������
                                                d.Manager ,					--������
                                                d.Phone ,					--�绰
                                                d.Fax ,						--����
                                                d.Enabled ,					--��Ч
                                                d.SortCode,                 --������
                                                d.Remark,					--˵��
(select top 1 name 
from
(
select 1 as sortno,RealName+'('+code+')' as name from Base_User a left join Base_ObjectUserRelation b on a.UserId=b.UserId where b.ObjectId='91c17ca4-0cbf-43fa-829e-3021b055b6c4' 
and a.departmentid=d.departmentid
union 
select 2, RealName+'('+code+')' from Base_PartDept a left join Base_User b on a.UserID=b.UserId
where DeptID=d.departmentid
) as a order by sortno) as WorkShopDirector
                                      FROM      Base_Department d
                                                LEFT JOIN Base_Company c ON c.CompanyId = d.CompanyId
                                    ) T WHERE 1=1 ");
            List<DbParameter> parameter = new List<DbParameter>();
            if (!string.IsNullOrEmpty(CompanyId))
            {
                if (CompanyId != "31b05701-60ef-405c-87ba-af47049e3f48")
                {
                    strSql.Append(" AND ParentID = @CompanyId");
                    parameter.Add(DbFactory.CreateDbParameter("@CompanyId", CompanyId));
                }
                else
                {
                    strSql.Append(" and ParentID='0' ");
                }
            }
            //if (!ManageProvider.Provider.Current().IsSystem)
            //{
            //    //strSql.Append(" AND ( DepartmentId IN ( SELECT ResourceId FROM Base_DataScopePermission WHERE");
            //    //strSql.Append(" ObjectId IN ('" + ManageProvider.Provider.Current().ObjectId.Replace(",", "','") + "') ");
            //    //strSql.Append(" ) )");
            //}
            strSql.Append(" ORDER BY ParentID ASC,SortCode ASC");
            return Repository().FindTableBySql(strSql.ToString(), parameter.ToArray());
        }


        public DataTable GetListAll(string CompanyId)
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append(@"SELECT  *
                            FROM    ( SELECT    d.DepartmentId ,			--����
                                                c.FullName AS CompanyName ,	--������˾
                                                d.CompanyId ,				--������˾Id
                                                d.ParentID,                 --�ϼ����ŵ�ID
                                                d.Code ,					--����
                                                d.FullName ,				--��������
                                                d.ShortName ,				--���ż��
                                                d.Nature ,					--��������
                                                d.Manager ,					--������
                                                d.Phone ,					--�绰
                                                d.Fax ,						--����
                                                d.Enabled ,					--��Ч
                                                d.SortCode,                 --������
                                                d.Remark,					--˵��
(select top 1 RealName+'('+code+')' from Base_User a left join Base_ObjectUserRelation b on a.UserId=b.UserId where b.ObjectId='91c17ca4-0cbf-43fa-829e-3021b055b6c4' 
and a.departmentid=d.DepartmentId) as WorkShopDirector
                                      FROM      Base_Department d
                                                LEFT JOIN Base_Company c ON c.CompanyId = d.CompanyId
                                    ) T WHERE 1=1 ");
            List<DbParameter> parameter = new List<DbParameter>();
            //if (!string.IsNullOrEmpty(CompanyId))
            //{
            //    if (CompanyId != "31b05701-60ef-405c-87ba-af47049e3f48")
            //    {
            //        strSql.Append(" AND ParentID = @CompanyId");
            //        parameter.Add(DbFactory.CreateDbParameter("@CompanyId", CompanyId));
            //    }
            //    else
            //    {
            //        strSql.Append(" and ParentID='0' ");
            //    }
            //}
            //if (!ManageProvider.Provider.Current().IsSystem)
            //{
            //    //strSql.Append(" AND ( DepartmentId IN ( SELECT ResourceId FROM Base_DataScopePermission WHERE");
            //    //strSql.Append(" ObjectId IN ('" + ManageProvider.Provider.Current().ObjectId.Replace(",", "','") + "') ");
            //    //strSql.Append(" ) )");
            //}
            strSql.Append(" ORDER BY ParentID ASC,SortCode ASC");
            return Repository().FindTableBySql(strSql.ToString(), parameter.ToArray());
        }

        public DataTable GetUserList()
        {
            StringBuilder strSql = new StringBuilder();
            strSql.Append(@"select * from Base_User where Enabled=1");
            return Repository().FindTableBySql(strSql.ToString());
        }

        public DataTable GetPartDeptList()
        {
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat(@"select deptid,deptname from base_partdept where UserID='{0}' ",ManageProvider.Provider.Current().UserId);
            return Repository().FindTableBySql(strSql.ToString());
        }
    }
}