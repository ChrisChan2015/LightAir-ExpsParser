using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using LightAir.Models;

namespace LightAir.Infrastructure.Transactions
{
    public class Transaction<T> : ITransaction<T>
        where T : ModelBase,new()
    {
        private TransactionType _transactionType;
        private IFieldCollection _fields;
        private int? _numOfTake;
        private int? _numOfSkip;
        private IList<ISortable> _sorts;
        private IList<Expression> _whereExps;
        private IList<IJoinTable> _joinTables;
        private AggregateFunction? _aggFunc;

        /// <summary>
        /// 实体对象
        /// </summary>
        public T Entity
        {
            get;
            set;
        }

        /// <summary>
        /// 获取数据库操作类型
        /// </summary>
        public TransactionType TransactionType
        {
            get { return _transactionType; }
        }

        /// <summary>
        /// 获取将要操作的数据字段集合
        /// </summary>
        public IFieldCollection Fields
        {
            get { return _fields; }
        }

        /// <summary>
        /// 获取所有排序依据
        /// </summary>
        public IList<ISortable> Sorts
        {
            get { return _sorts; }
        }

        /// <summary>
        /// 获取数据读取操作所读取的记录数量
        /// </summary>
        public int? NumOfTake
        {
            get { return _numOfTake; }
        }

        /// <summary>
        /// 获取数据读取操作时跳过的记录数量
        /// </summary>
        public int? NumOfSkip
        {
            get { return _numOfSkip; }
        }

        /// <summary>
        /// 获取Select使用的聚合函数
        /// </summary>
        public AggregateFunction? AggregateFunction
        {
            get { return this._aggFunc; }
        }

        /// <summary>
        /// 获取Where表达式树集合
        /// </summary>
        public IList<Expression> WhereExpressions
        {
            get { return _whereExps; }
        }

        public Transaction()
        {
            _fields = new FieldCollection();
            _sorts = new List<ISortable>();
            _whereExps = new List<Expression>();
            _joinTables = new List<IJoinTable>();
        }

        /// <summary>
        /// 设置当前操作为添加操作
        /// </summary>
        /// <returns></returns>
        public ITransaction<T> Insert()
        {
            _transactionType = Transactions.TransactionType.Insert;
            return this;
        }

        /// <summary>
        /// 设置当前操作为查询操作
        /// </summary>
        /// <returns></returns>
        public ITransaction<T> Select()
        {
            _transactionType = Transactions.TransactionType.Select;
            return this;
        }

        /// <summary>
        /// 设置当前操作为聚合查询操作
        /// </summary>
        /// <param name="aggFunc">聚合函数</param>
        /// <returns></returns>
        public ITransaction<T> Select(AggregateFunction aggFunc)
        {
            this.Select();
            this._aggFunc = aggFunc;
            return this;
        }

        /// <summary>
        /// 设置当前操作为更新操作
        /// </summary>
        /// <returns></returns>
        public ITransaction<T> Update()
        {
            _transactionType = Transactions.TransactionType.Update;
            return this;
        }

        /// <summary>
        /// 设置当前操作为删除操作
        /// </summary>
        /// <returns></returns>
        public ITransaction<T> Delete()
        {
            _transactionType = Transactions.TransactionType.Delete;
            return this;
        }

        /// <summary>
        /// 设置当前操作条件
        /// </summary>
        /// <param name="whereExp">表示条件的表达式树</param>
        /// <returns></returns>
        public ITransaction<T> Where<U>(Expression<Func<U, bool>> whereExp)
            where U : IModel
        {
            if (whereExp != null)
                _whereExps.Add(whereExp);
            return this;
        }

        /// <summary>
        /// 设置当前操作条件
        /// </summary>
        /// <param name="whereExp">表示条件的表达式树</param>
        /// <returns></returns>
        public ITransaction<T> Where(Expression<Func<T, bool>> whereExp)
        {
            if (whereExp != null)
                _whereExps.Add(whereExp);
            return this;
        }

        /// <summary>
        /// 通过表达式树设置一个升序排序
        /// </summary>
        /// <typeparam name="F">字段类型参数</typeparam>
        /// <param name="orderbyExp">表示排序的表达式树</param>
        /// <returns></returns>
        public ITransaction<T> OrderBy<F>(Expression<Func<T, F>> orderbyExp)
        {
            if (_sorts.Count > 0)
            {
                _sorts.Clear();
            }
            Sortable st = new Sortable(Transactions.OrderBy.Asc, orderbyExp);
            _sorts.Add(st);
            return this;
        }

        /// <summary>
        /// 通过表达式树设置一个降序排序
        /// </summary>
        /// <typeparam name="F">字段类型参数</typeparam>
        /// <param name="orderbyExp">表示排序的表达式树</param>
        /// <returns></returns>
        public ITransaction<T> OrderByDescend<F>(Expression<Func<T, F>> orderbyExp)
        {
            if (_sorts.Count > 0)
            {
                _sorts.Clear();
            }
            Sortable st = new Sortable(Transactions.OrderBy.Desc, orderbyExp);
            _sorts.Add(st);
            return this;
        }

        /// <summary>
        /// 通过表达式树设置一个按升序的后继排序
        /// </summary>
        /// <typeparam name="F">字段类型参数</typeparam>
        /// <param name="thenbyExp">表示排序的表达式树</param>
        /// <returns></returns>
        public ITransaction<T> ThenBy<F>(Expression<Func<T, F>> thenbyExp)
        {
            Sortable st = new Sortable(Transactions.OrderBy.Asc, thenbyExp);
            _sorts.Add(st);
            return this;
        }

        /// <summary>
        /// 通过表达式树设置一个按降序的后继排序
        /// </summary>
        /// <typeparam name="F">字段类型参数</typeparam>
        /// <param name="thebyExp">表示排序的表达式树</param>
        /// <returns></returns>
        public ITransaction<T> ThenByDescend<F>(Expression<Func<T, F>> thenbyExp)
        {
            Sortable st = new Sortable(Transactions.OrderBy.Desc, thenbyExp);
            return this;
        }

        /// <summary>
        /// 设置读取时要跳过的记录数量
        /// </summary>
        /// <param name="num">跳过的记录数量</param>
        /// <returns></returns>
        public ITransaction<T> Skit(int num)
        {
            _numOfSkip = num;
            return this;
        }

        /// <summary>
        /// 设置读取时要获取的记录数量
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public ITransaction<T> Take(int num)
        {
            _numOfTake = num;
            return this;
        }

        /// <summary>
        /// 获取表联接信息集合
        /// </summary>
        public IList<IJoinTable> JoinTables
        {
            get { return _joinTables; }
        }

        /// <summary>
        /// 设置一个内联接
        /// </summary>
        /// <typeparam name="T">主键表类型参数</typeparam>
        /// <typeparam name="F">外键表类型参数</typeparam>
        /// <param name="joinExp">表示内链接的表达式树</param>
        /// <returns></returns>
        public ITransaction<T> InnerJoin<F>(Expression<Func<T, F,bool>> joinExp) where F : IModel
        {
            if (joinExp != null)
            {
                string fkTableName = typeof(F).Name;
                JoinTable jt = new JoinTable(JoinType.InnerJoin, fkTableName, joinExp);
                _joinTables.Add(jt);
            }
            return this;
        }

        /// <summary>
        /// 设置一个左外联接
        /// </summary>
        /// <typeparam name="T">主键表类型参数</typeparam>
        /// <typeparam name="F">外键表类型参数</typeparam>
        /// <param name="joinExp">表示左外链接的表达式树</param>
        /// <returns></returns>
        public ITransaction<T> LeftJoin<F>(Expression<Func<T, F,bool>> joinExp) where F : IModel
        {
            if (joinExp != null)
            {
                string fkTableName = typeof(F).Name;
                JoinTable jt = new JoinTable(JoinType.LeftJoin, fkTableName, joinExp);
                _joinTables.Add(jt);
            }
            return this;
        }

        /// <summary>
        /// 设置一个右外联接
        /// </summary>
        /// <typeparam name="U">主键表类型参数</typeparam>
        /// <typeparam name="F">外键表主键类型参数</typeparam>
        /// <param name="joinExp">表示右外链接的表达式树</param>
        /// <returns></returns>
        public ITransaction<T> RightJoin<F>(Expression<Func<T, F,bool>> joinExp) where F : IModel
        {
            if (joinExp != null)
            {
                string fkTableName = typeof(F).Name;
                JoinTable jt = new JoinTable(JoinType.RightJoin, fkTableName, joinExp);
                _joinTables.Add(jt);
            }
            return this;
        }

        /// <summary>
        /// 设置Select语句查询字段或Update语句修改字段
        /// </summary>
        /// <param name="fields">保存字段信息的集合</param>
        /// <returns></returns>
        public ITransaction<T> SetFields(IFieldCollection fields)
        {
            this._fields = fields;
            return this;
        }
    }
}
