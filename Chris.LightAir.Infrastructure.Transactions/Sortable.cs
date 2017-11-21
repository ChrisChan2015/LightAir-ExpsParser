using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LightAir.Infrastructure.Transactions
{
    /// <summary>
    /// 包含SQL查询结果的排序信息
    /// </summary>
    public class Sortable : ISortable
    {
        OrderBy _orderBy;
        Expression _orderByExpression;

        /// <summary>
        /// 排序方式
        /// </summary>
        public OrderBy OrderBy
        {
            get
            {
                return _orderBy;
            }
            set
            {
                _orderBy = value;
            }
        }

        /// <summary>
        /// 包含排序信息的表达式树
        /// </summary>
        public Expression OrderByExpression
        {
            get
            {
                return _orderByExpression;
            }
            set
            {
                _orderByExpression = value;
            }
        }

        public Sortable() { }

        public Sortable(OrderBy orderBy, Expression orderByExpression)
        {
            this._orderBy = orderBy;
            this._orderByExpression = orderByExpression;
        }
    }
}
