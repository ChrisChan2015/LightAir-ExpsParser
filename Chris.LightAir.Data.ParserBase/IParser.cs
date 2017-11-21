using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Data.Common;
using LightAir.Infrastructure.Transactions;
using LightAir.Models;

namespace LightAir.Data.Parsers
{
    /// <summary>
    /// 表达式树解析器接口，所有的表达树解析器都间接继承自本接口，是表达式解析器继承结构中的最顶端。
    /// </summary>
    public interface IParser
    {
        /// <summary>
        /// 将表达式树解析成SQL Where子句的方法
        /// </summary>
        /// <param name="exp">将要解析的表达式树对象</param>
        /// <param name="statement">用于存储解析后语句的StringBuilder对象</param>
        /// <param name="dbParams">用于存放从表达式树中解析出来的DbParameter元素集合</param>
        /// <param name="append">表示是否要将解析出来的值拼接至staement对象中并添加元素至dbParams中</param>
        /// <returns>返回从表达式树中解析出的值，如果表达式树不存在值则返回null</returns>
        object ParseWhere(Expression exp, StringBuilder statement,IList<DbParameter> dbParams=null, bool append=true);

        /// <summary>
        /// 随机一个DbParameter名称
        /// </summary>
        /// <returns>返回DbParameter名称</returns>
        string ParameterNaming();

        /// <summary>
        /// 创建SQL参数
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="value">参数值</param>
        /// <returns>返回SQL命令参数</returns>
        DbParameter DbParamFactory(string name, object value);

        /// <summary>
        /// 从一系列表达式树中解析查询排序子句的方法
        /// </summary>
        /// <param name="sortables">包含排序信息对象的集合</param>
        /// <returns>返回排序子句</returns>
        string ParseOrderBy(IEnumerable<ISortable> sortables);

        /// <summary>
        /// 通过ITransctiond派生类对象解析出SQL语句
        /// </summary>
        /// <typeparam name="T">实体对象的类型参数</typeparam>
        /// <param name="transaction">包含一系列SQL操作信息的对象</param>
        /// <param name="dbParams">从SQL中解析出来的参数集合</param>
        /// <returns>返回解析出的</returns>
        string BuildSqlStatement<T>(ITransaction<T> transaction, IList<DbParameter> dbParams)
            where T : ModelBase, new();
    }
}
