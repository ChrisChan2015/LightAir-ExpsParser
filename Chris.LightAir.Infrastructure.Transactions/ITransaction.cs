using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using LightAir.Models;

namespace LightAir.Infrastructure.Transactions
{
    /// <summary>
    /// 定义一个表示数据库操作的接口
    /// </summary>
    /// <typeparam name="T">将要操作的实体类的类型参数</typeparam>
    public interface ITransaction<T>
        where T : ModelBase,new()
    {
        /// <summary>
        /// 实体对象
        /// </summary>
        T Entity { get; set; }

        /// <summary>
        /// 获取数据库操作类型
        /// </summary>
        TransactionType TransactionType { get; }

        /// <summary>
        /// 获取将要操作的数据字段集合
        /// </summary>
        IFieldCollection Fields { get; }
        
        /// <summary>
        /// 获取所有排序依据
        /// </summary>
        IList<ISortable> Sorts { get; }

        /// <summary>
        /// 获取数据读取操作所读取的记录数量
        /// </summary>
        int? NumOfTake { get; }

        /// <summary>
        /// 获取数据读取操作时跳过的记录数量
        /// </summary>
        int? NumOfSkip { get; }

        /// <summary>
        /// 获取Select使用的聚合函数
        /// </summary>
        AggregateFunction? AggregateFunction { get; }

        /// <summary>
        /// 获取Where表达式树集合
        /// </summary>
        IList<Expression> WhereExpressions { get; }

        /// <summary>
        /// 获取表联接信息集合
        /// </summary>
        IList<IJoinTable> JoinTables { get; }

        /// <summary>
        /// 设置当前操作为添加操作
        /// </summary>
        /// <returns></returns>
        ITransaction<T> Insert();

        /// <summary>
        /// 设置当前操作为查询操作
        /// </summary>
        /// <returns></returns>
        ITransaction<T> Select();

        /// <summary>
        /// 设置当前操作为聚合查询操作
        /// </summary>
        /// <param name="aggFunc">聚合函数</param>
        /// <returns></returns>
        ITransaction<T> Select(AggregateFunction aggFunc);

        /// <summary>
        /// 设置当前操作为更新操作
        /// </summary>
        /// <returns></returns>
        ITransaction<T> Update();

        /// <summary>
        /// 设置当前操作为删除操作
        /// </summary>
        /// <returns></returns>
        ITransaction<T> Delete();

        /// <summary>
        /// 设置当前操作条件
        /// </summary>
        /// <param name="whereExp">表示条件的表达式树</param>
        /// <returns></returns>
        ITransaction<T> Where<U>(Expression<Func<U, bool>> whereExp) where U : IModel;

        /// <summary>
        /// 设置当前操作条件
        /// </summary>
        /// <param name="whereExp">表示条件的表达式树</param>
        /// <returns></returns>
        ITransaction<T> Where(Expression<Func<T, bool>> whereExp);

        /// <summary>
        /// 设置一个内联接
        /// </summary>
        /// <typeparam name="T">主键表类型参数</typeparam>
        /// <typeparam name="F">外键表类型参数</typeparam>
        /// <param name="joinExp">表示内链接的表达式树</param>
        /// <returns></returns>
        ITransaction<T> InnerJoin<F>(Expression<Func<T, F,bool>> joinExp) where F : IModel;

        /// <summary>
        /// 设置一个左外联接
        /// </summary>
        /// <typeparam name="T">主键表类型参数</typeparam>
        /// <typeparam name="F">外键表类型参数</typeparam>
        /// <param name="joinExp">表示左外链接的表达式树</param>
        /// <returns></returns>
        ITransaction<T> LeftJoin<F>(Expression<Func<T, F,bool>> joinExp) where F : IModel;

        /// <summary>
        /// 设置一个右外联接
        /// </summary>
        /// <typeparam name="U">主键表类型参数</typeparam>
        /// <typeparam name="F">外键表主键类型参数</typeparam>
        /// <param name="joinExp">表示右外链接的表达式树</param>
        /// <returns></returns>
        ITransaction<T> RightJoin<F>(Expression<Func<T, F,bool>> joinExp) where F : IModel;

        /// <summary>
        /// 通过表达式树设置一个升序排序
        /// </summary>
        /// <typeparam name="F">字段类型参数</typeparam>
        /// <param name="orderbyExp">表示排序的表达式树</param>
        /// <returns></returns>
        ITransaction<T> OrderBy<F>(Expression<Func<T, F>> orderbyExp);

        /// <summary>
        /// 通过表达式树设置一个降序排序
        /// </summary>
        /// <typeparam name="F">字段类型参数</typeparam>
        /// <param name="orderbyExp">表示排序的表达式树</param>
        /// <returns></returns>
        ITransaction<T> OrderByDescend<F>(Expression<Func<T, F>> orderbyExp);

        /// <summary>
        /// 通过表达式树设置一个按升序的后继排序
        /// </summary>
        /// <typeparam name="F">字段类型参数</typeparam>
        /// <param name="thenbyExp">表示排序的表达式树</param>
        /// <returns></returns>
        ITransaction<T> ThenBy<F>(Expression<Func<T, F>> thenbyExp);

        /// <summary>
        /// 通过表达式树设置一个按降序的后继排序
        /// </summary>
        /// <typeparam name="F">字段类型参数</typeparam>
        /// <param name="thebyExp">表示排序的表达式树</param>
        /// <returns></returns>
        ITransaction<T> ThenByDescend<F>(Expression<Func<T, F>> thenbyExp);

        /// <summary>
        /// 设置读取时要跳过的记录数量
        /// </summary>
        /// <param name="num">跳过的记录数量</param>
        /// <returns></returns>
        ITransaction<T> Skit(int num);

        /// <summary>
        /// 设置读取时要获取的记录数量
        /// </summary>
        /// <param name="num">获取的记录数量</param>
        /// <returns></returns>
        ITransaction<T> Take(int num);

        /// <summary>
        /// 设置Select语句查询字段或Update语句修改字段
        /// </summary>
        /// <param name="fields">保存字段信息的集合</param>
        /// <returns></returns>
        ITransaction<T> SetFields(IFieldCollection fields);
    }
}
