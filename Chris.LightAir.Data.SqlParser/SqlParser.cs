using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;

namespace LightAir.Data.Parsers
{
    /// <summary>
    /// 表达式树解析器，将表达式解析成SQL Server语句元素。
    /// </summary>
    public class SqlParser : ParserBase
    {
        /// <summary>
        /// 随机数生成器；如果在每次调用ParameterNaming方法时创建实例，可能会生成重复随机数。
        /// </summary>
        private Random _rand;

        public SqlParser()
        {
            _rand=new Random();
        }

        /// <summary>
        /// 创建SQL参数
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="value">参数值</param>
        /// <returns>返回SQL命令参数</returns>
        public override DbParameter DbParamFactory(string name, object value)
        {
            //TODO:不指定VARCHAR的长度会造成SQL SERVER创建多个缓存计划。
            SqlParameter param = new SqlParameter(name, value);
            return param;
        }

        /// <summary>
        /// 随机一个DbParameter名称
        /// </summary>
        /// <returns>返回DbParameter名称</returns>
        public override string ParameterNaming()
        {
            //TODO:随机参数名称会造成SQL SERVER创建多个参数缓存计划。
            string paramName = null;
#if PN_FOR_GUID
            //使用GUID作为参数名
            paramName = string.Format("@{0}", Guid.NewGuid().ToString("N"));
#else
            //使用8位随机数作为命令参数名称
            int num = _rand.Next(10000000, 100000000);
            paramName = string.Format("@{0}", num.ToString());
#endif
            return paramName;
        }
    }
}
