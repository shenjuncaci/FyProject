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
    public class Base_NoteNOBll : RepositoryFactory<Base_NoteNOBll>
    {
        

        public DataTable GetDataTable(string sql)
        {
            return Repository().FindDataSetBySql(sql).Tables[0];
        }

        public int ExecuteSql(StringBuilder sql)
        {
            int result = Repository().ExecuteBySql(sql);
            return result;
        }

        public string Code(string NoteName)
        {
            string result = "";
            string sql = "select * from base_NoteNO where NoteName='"+NoteName+"'";
            StringBuilder strSql = new StringBuilder();

            DataTable dt = GetDataTable(sql);
            if(dt.Rows.Count>0)
            {
                //string temp = Convert.ToDateTime(dt.Rows[0]["ModifyDt"].ToString()).ToString("yyyy-MM-dd");
                //string temp2 = DateTime.Now.ToString("yyyy-MM-dd");
                //如果日期相同，则累加。日期不同的话重置为1，继续累加
                if (Convert.ToDateTime(dt.Rows[0]["ModifyDt"].ToString()).ToString("yyyy-MM-dd") == DateTime.Now.ToString("yyyy-MM-dd"))
                {
                    string last = "";
                    if(dt.Rows[0]["CurrentNo"].ToString().Length<3)
                    {
                        int Num = 3 - dt.Rows[0]["CurrentNo"].ToString().Length;
                        for (int i=0;i< Num;i++)
                        {
                            last += "0";
                        }
                    }
                    result += dt.Rows[0]["BeginName"].ToString() + DateTime.Now.ToString("yyyy-MM-dd").Replace("-", "") + last + dt.Rows[0]["CurrentNo"].ToString();
                    strSql.AppendFormat(@"update base_NoteNO set currentno=currentno+1 where NoteName='{0}'", NoteName);
                    ExecuteSql(strSql);
                }
                else
                {
                    result += dt.Rows[0]["BeginName"].ToString() + DateTime.Now.ToString("yyyy-MM-dd").Replace("-", "") + "001";
                    strSql.AppendFormat(@"update base_NoteNO set currentno=2,ModifyDt='{1}' where NoteName='{0}'", NoteName,DateTime.Now);
                    ExecuteSql(strSql);
                }
            }
            return result;
        }

        //按年重置的4位流水号
        public string CodeByYear(string NoteName)
        {
            string result = "";
            string sql = "select BeginName,YEAR(ModifyDt) as Year,CurrentNo from Base_NoteNO where NoteName='" + NoteName + "'";
            StringBuilder strSql = new StringBuilder();

            DataTable dt = GetDataTable(sql);
            if (dt.Rows.Count > 0)
            {
               
                //按年累计，则判断年份和当前时间的年份是否一样
                if (dt.Rows[0]["Year"].ToString() == DateTime.Now.Year.ToString())
                {
                    string last = "";
                    if (dt.Rows[0]["CurrentNo"].ToString().Length < 4)
                    {
                        int Num = 4 - dt.Rows[0]["CurrentNo"].ToString().Length;
                        for (int i = 0; i < Num; i++)
                        {
                            last += "0";
                        }
                    }
                    result += dt.Rows[0]["BeginName"].ToString() + DateTime.Now.Year.ToString() + last + dt.Rows[0]["CurrentNo"].ToString();
                    strSql.AppendFormat(@"update base_NoteNO set currentno=currentno+1 where NoteName='{0}'", NoteName);
                    ExecuteSql(strSql);
                }
                else
                {
                    result += dt.Rows[0]["BeginName"].ToString() + DateTime.Now.Year.ToString() + "0001";
                    strSql.AppendFormat(@"update base_NoteNO set currentno=2,ModifyDt='{1}' where NoteName='{0}'", NoteName, DateTime.Now);
                    ExecuteSql(strSql);
                }
            }
            return result;
        }
    }
}
