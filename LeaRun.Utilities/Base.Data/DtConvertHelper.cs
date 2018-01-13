using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Reflection;
using System.Web.Mvc;

namespace LeaRun.Utilities
{
    /// <summary>
    ///功能描述    :    数据转换类
    ///开发者      :    shenjun
    ///建立时间    :    2017-07-1 17:26:30
    ///修订描述    :    
    ///进度描述    :    
    ///版本号      :    1.0
    ///最后修改时间:    2017-07-1 17:26:30
    /// </summary>
    public static class DtConvertHelper
    {
        /// <summary>
        /// 根据传入集合、对象、分隔符的定义，返回一个name字符串
        /// </summary>
        /// <typeparam name="T">实体类</typeparam>
        /// <param name="lis">实体类集合</param>
        /// <param name="split">分隔符</param>
        /// <param name="propertyFuncs">属性方法</param>
        /// <returns>按规则、以特定分隔符组成的字符串</returns>
        public static string ConvertToString<T>(ICollection<T> lis, string split, params Func<T, string>[] propertyFuncs)
        {
            StringBuilder sb = new StringBuilder();
            int i = 0;
            foreach (T obj in lis)
            {
                foreach (Func<T, string> propertyFunc in propertyFuncs)
                {
                    sb.Append(propertyFunc(obj));
                    if (i < lis.Count - 1) { sb.Append(split); }
                    i++;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// 按长度返回字符串，和所有字符串，用于显示和提示
        /// </summary>
        /// <typeparam name="T">实体类</typeparam>
        /// <param name="lis">实体类集合</param>
        /// <param name="len">返回的个数</param>
        /// <param name="split">分隔符</param>
        /// <param name="propertyFuncs">属性方法</param>
        /// <returns>按规则、以特定分隔符组成的字符串，一维是半截的，二维是全部的</returns>
        public static string[] ConvertToString<T>(ICollection<T> lis, int len, string split, params Func<T, string>[] propertyFuncs)
        {
            string[] arr = { "", "" };
            try
            {
                StringBuilder sb = new StringBuilder();
                int i = 0;
                foreach (T obj in lis)
                {
                    foreach (Func<T, string> propertyFunc in propertyFuncs)
                    {
                        sb.Append(propertyFunc(obj));
                        if (i <= len) { arr[0] = sb.ToString(); }
                        if (i < lis.Count - 1) { sb.Append(split); }
                        i++;
                    }
                }
                if (i > len) { arr[0] += " ... ..."; }
                arr[1] = sb.ToString();
            }
            catch { }
            return arr;
        }

        /// <summary>
        /// DataTable转为IList(T)，简单转换，碰到实体类属性是Parent(T)，数据库却是PID(int)这种就tm不行了。
        /// TODO:用到这个转换的基本都是通过存储过程的方法，以后报表类可能会经常使用，所以暂时把映射关系都弄成PID这种方式，避免出错。
        /// </summary>
        /// <param name="dt">DataTable对象</param>
        /// <returns></returns>
        public static IList<T> ConvertToModelList<T>(DataTable dt) where T : new()
        {
            // 定义集合
            IList<T> ts = new List<T>();
            if (dt == null || dt.Rows.Count <= 0) { return ts; }
            // 获得此模型的类型
            Type type = typeof(T);
            string tempName = "";
            foreach (DataRow dr in dt.Rows)
            {
                T t = new T();
                // 获得此模型的公共属性
                PropertyInfo[] propertys = t.GetType().GetProperties();
                foreach (PropertyInfo pi in propertys)
                {
                    tempName = pi.Name;
                    // 检查DataTable是否包含此列
                    if (dt.Columns.Contains(tempName))
                    {
                        // 判断此属性是否有Setter
                        if (!pi.CanWrite) continue;
                        object value = dr[tempName];
                        try
                        {
                            if (value != DBNull.Value)
                            {
                                if (pi.PropertyType == typeof(string)) { pi.SetValue(t, value.ToString(), null); }
                                else if (pi.PropertyType == typeof(int)) { pi.SetValue(t, Convert.ToInt32(value), null); }
                                else if (pi.PropertyType == typeof(decimal)) { pi.SetValue(t, Convert.ToDecimal(value), null); }
                                else if (pi.PropertyType == typeof(DateTime)) { pi.SetValue(t, Convert.ToDateTime(value), null); }
                                else { pi.SetValue(t, value, null); }
                            }
                        }
                        catch
                        {
                            //pi.SetValue(t, value.ToString(), null);
                        }

                    }

                }
                ts.Add(t);
            }
            return ts;
        }

        //public static List<T> ConvertToModelList<T>(DataTable dt, string sortField, ReverserInfo.Direction dir) where T : new()
        //{
        //    // 定义集合
        //    List<T> ts = new List<T>();
        //    if (dt == null || dt.Rows.Count <= 0) { return ts; }
        //    // 获得此模型的类型
        //    Type type = typeof(T);
        //    string tempName = "";
        //    foreach (DataRow dr in dt.Rows)
        //    {
        //        T t = new T();
        //        // 获得此模型的公共属性
        //        PropertyInfo[] propertys = t.GetType().GetProperties();
        //        foreach (PropertyInfo pi in propertys)
        //        {
        //            tempName = pi.Name;
        //            // 检查DataTable是否包含此列
        //            if (dt.Columns.Contains(tempName))
        //            {
        //                // 判断此属性是否有Setter
        //                if (!pi.CanWrite) continue;
        //                object value = dr[tempName];
        //                try
        //                {
        //                    if (value != DBNull.Value)
        //                    {
        //                        if (pi.PropertyType == typeof(string)) { pi.SetValue(t, value.ToString(), null); }
        //                        else if (pi.PropertyType == typeof(int)) { pi.SetValue(t, Convert.ToInt32(value), null); }
        //                        else if (pi.PropertyType == typeof(decimal)) { pi.SetValue(t, Convert.ToDecimal(value), null); }
        //                        else if (pi.PropertyType == typeof(DateTime)) { pi.SetValue(t, Convert.ToDateTime(value), null); }
        //                        else { pi.SetValue(t, value, null); }
        //                    }
        //                }
        //                catch
        //                {
        //                    //pi.SetValue(t, value.ToString(), null);
        //                }

        //            }

        //        }
        //        ts.Add(t);
        //    }

        //    if (CheckUtil.CheckIsNull(sortField))
        //    {
        //        return ts;
        //    }
        //    else
        //    {
        //        List<T> lis = new List<T>(ts);
        //        //根据sortField字段重新排序
        //        Reverser<T> reverser = new Reverser<T>(typeof(T), sortField, dir);
        //        lis.Sort(reverser);

        //        return lis;
        //    }
        //}

        /// <summary>
        /// 根据DataTable的第一行，填充一个对象，并返回。只用于目前的DTO类型，没有其它多余的映射，完成简单类得转换。类似Parent映射PID这种，就算了吧。。。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static T ConvertToModel<T>(DataTable dt, int n) where T : new()
        {

            T t = new T();
            if (dt == null || dt.Rows.Count <= 0 || dt.Rows.Count <= n) { return t; }

            Type type = typeof(T);
            string tempName = "";
            // 获得此模型的公共属性
            PropertyInfo[] propertys = t.GetType().GetProperties();
            foreach (PropertyInfo pi in propertys)
            {
                tempName = pi.Name;
                // 检查DataTable是否包含此列
                if (dt.Columns.Contains(tempName))
                {
                    // 判断此属性是否有Setter
                    if (!pi.CanWrite) continue;
                    object value = dt.Rows[0][tempName];
                    try
                    {
                        if (value != DBNull.Value)
                        {
                            if (pi.PropertyType == typeof(string)) { pi.SetValue(t, value.ToString(), null); }
                            else if (pi.PropertyType == typeof(int)) { pi.SetValue(t, Convert.ToInt32(value), null); }
                            else if (pi.PropertyType == typeof(decimal)) { pi.SetValue(t, Convert.ToDecimal(value), null); }
                            else if (pi.PropertyType == typeof(DateTime)) { pi.SetValue(t, Convert.ToDateTime(value), null); }
                            else { pi.SetValue(t, value, null); }
                        }
                    }
                    catch
                    {
                        //pi.SetValue(t, value.ToString(), null); 
                    }
                }
            }
            return t;
        }

        /// <summary>
        /// DataTable转为IList(string)
        /// </summary>
        /// <param name="dt">DataTable</param>
        /// <returns></returns>
        public static IList<string> ConvertToStringList(DataTable dt)
        {
            IList<string> str = new List<string>();
            foreach (DataRow row in dt.Rows)
            {
                foreach (DataColumn Col in dt.Columns)
                {
                    str.Add(row[Col].ToString());
                }
            }
            return str;
        }

        /// <summary>
        /// 转换一个枚举到SelectList，指定一个int值来选择默认选择项
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="enumObj"></param>
        /// <param name="selected"></param>
        /// <returns></returns>
        public static SelectList ToSelectList<TEnum>(this TEnum enumObj, int selected)
        {
            try
            {
                var values = from TEnum e in Enum.GetValues(typeof(TEnum))
                             select new SelectListItem { Value = (Convert.ToInt32(e).ToString()), Text = e.ToString() };
                return new SelectList(values, "Value", "Text", selected);
            }
            catch
            {
                return new SelectList(new List<string> { "转换枚举类型到列表时出现错误" }, selected);
            }
        }
        /// <summary>
        /// 转换一个枚举到SelectList，指定一个string值来选择默认选择项
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="enumObj"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public static SelectList ToSelectList<TEnum>(this TEnum enumObj, string val)
        {
            try
            {
                var values = from TEnum e in Enum.GetValues(typeof(TEnum))
                             select new SelectListItem { Value = e.ToString(), Text = e.ToString() };
                //int i=0;
                //foreach (SelectListItem obj in values) { if (obj.Text == val) { break; } i++; }
                return new SelectList(values, "Value", "Text", val);
            }
            catch
            {
                return new SelectList(new List<string> { "转换枚举类型到列表时出现错误" }, 0);
            }
        }

        /// <summary> 
        /// 转半角的函数(DBC case)
        /// 全角空格为12288，半角空格为32
        /// 其他字符半角(33-126)与全角(65281-65374)的对应关系是：均相差65248
        /// </summary>
        /// <param name="input">任意字符串</param>
        /// <returns>半角字符串</returns>
        public static string ConvertToDBC(string input)
        {
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 12288) { c[i] = (char)32; continue; }
                if (c[i] > 65280 && c[i] < 65375) { c[i] = (char)(c[i] - 65248); }
            }
            return new String(c);
        }

        /// <summary>
        /// 转全角的函数(SBC case)
        /// 全角空格为12288，半角空格为32
        /// 其他字符半角(33-126)与全角(65281-65374)的对应关系是：均相差65248
        /// </summary>
        /// <param name="input">任意字符串</param>
        /// <returns>全角字符串</returns>
        public static String ConvertToSBC(String input)
        {
            // 半角转全角：
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 32) { c[i] = (char)12288; continue; }
                if (c[i] < 127) { c[i] = (char)(c[i] + 65248); }
            }
            return new String(c);
        }

        #region 财务、金额相关
        public static string ConvertMoneyToCn(decimal num)
        {
            string str1 = "零壹贰叁肆伍陆柒捌玖";            //0-9所对应的汉字 
            string str2 = "万仟佰拾亿仟佰拾万仟佰拾元角分"; //数字位所对应的汉字 
            string str3 = "";    //从原num值中取出的值 
            string str4 = "";    //数字的字符串形式 
            string str5 = "";  //人民币大写金额形式 
            int i;    //循环变量 
            int j;    //num的值乘以100的字符串长度 
            string ch1 = "";    //数字的汉语读法 
            string ch2 = "";    //数字位的汉字读法 
            int nzero = 0;  //用来计算连续的零值是几个 
            int temp;            //从原num值中取出的值 

            num = Math.Round(Math.Abs(num), 2);    //将num取绝对值并四舍五入取2位小数 
            str4 = ((long)(num * 100)).ToString();        //将num乘100并转换成字符串形式 
            j = str4.Length;      //找出最高位 
            if (j > 15) { return "溢出"; }
            str2 = str2.Substring(15 - j);   //取出对应位数的str2的值。如：200.55,j为5所以str2=佰拾元角分 

            //循环取出每一位需要转换的值 
            for (i = 0; i < j; i++)
            {
                str3 = str4.Substring(i, 1);          //取出需转换的某一位的值 
                temp = Convert.ToInt32(str3);      //转换为数字 
                if (i != (j - 3) && i != (j - 7) && i != (j - 11) && i != (j - 15))
                {
                    //当所取位数不为元、万、亿、万亿上的数字时 
                    if (str3 == "0")
                    {
                        ch1 = "";
                        ch2 = "";
                        nzero = nzero + 1;
                    }
                    else
                    {
                        if (str3 != "0" && nzero != 0)
                        {
                            ch1 = "零" + str1.Substring(temp * 1, 1);
                            ch2 = str2.Substring(i, 1);
                            nzero = 0;
                        }
                        else
                        {
                            ch1 = str1.Substring(temp * 1, 1);
                            ch2 = str2.Substring(i, 1);
                            nzero = 0;
                        }
                    }
                }
                else
                {
                    //该位是万亿，亿，万，元位等关键位 
                    if (str3 != "0" && nzero != 0)
                    {
                        ch1 = "零" + str1.Substring(temp * 1, 1);
                        ch2 = str2.Substring(i, 1);
                        nzero = 0;
                    }
                    else
                    {
                        if (str3 != "0" && nzero == 0)
                        {
                            ch1 = str1.Substring(temp * 1, 1);
                            ch2 = str2.Substring(i, 1);
                            nzero = 0;
                        }
                        else
                        {
                            if (str3 == "0" && nzero >= 3)
                            {
                                ch1 = "";
                                ch2 = "";
                                nzero = nzero + 1;
                            }
                            else
                            {
                                if (j >= 11)
                                {
                                    ch1 = "";
                                    nzero = nzero + 1;
                                }
                                else
                                {
                                    ch1 = "";
                                    ch2 = str2.Substring(i, 1);
                                    nzero = nzero + 1;
                                }
                            }
                        }
                    }
                }
                if (i == (j - 11) || i == (j - 3))
                {
                    //如果该位是亿位或元位，则必须写上 
                    ch2 = str2.Substring(i, 1);
                }
                str5 = str5 + ch1 + ch2;

                if (i == j - 1 && str3 == "0")
                {
                    //最后一位（分）为0时，加上“整” 
                    str5 = str5;
                }
            }
            if (num == 0)
            {
                str5 = "零元";
            }
            return str5;
        }
        #endregion
    }
}
