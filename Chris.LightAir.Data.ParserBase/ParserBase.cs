#define TEST
#undef TEST
#define PN_FOR_GUID
#undef PN_FOR_GUID

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using System.Data.Common;
using LightAir.Infrastructure.Transactions;
using LightAir.Models;

namespace LightAir.Data.Parsers
{
    /// <summary>
    /// 表达式树解析器的基类；负责将表达式树解析成SQL语句元素。
    /// </summary>
    public abstract class ParserBase : IParser
    {
        #region Where子句解析

        /// <summary>
        /// 将表达式解析为Where子句元素。
        /// 作为解析器的入口。
        /// </summary>
        /// <param name="exp">表达式对象</param>
        /// <param name="statement">用于存储解析后语句的StringBuilder对象</param>
        /// <param name="dbParams">用于存放从表达式树中解析出来的DbParameter元素集合</param>
        /// <param name="append">表示是否拼接到statement、dbParams参数中，true为拼接，false不拼接；默认为true</param>
        /// <returns>返回从表达式树中解析出的值，如果表达式树不存在值则返回null</returns>
        public object ParseWhere(Expression exp, StringBuilder statement, IList<DbParameter> dbParams = null, bool append = true)
        {
            if (exp == null)
                throw new ArgumentNullException("exp", "exp can't null.");
            if (statement == null)
                throw new ArgumentNullException("statement", "statement can't null.");
            object result = null;

            //TODO:当前这种使用表达式树来解析组成SQL语句的方式，虽然灵活，但也是因为太灵活，所以在编译器无法检测出表达式是否能被解析，解析出来的东西是否是合法。

            //根据表达式的节点类型，转换为特定的表达式实现类，并分发到解析方法对应的重载中。
            switch (exp.NodeType)
            {
                case ExpressionType.Lambda:
                    //Lambda表达式
                    LambdaExpression lamExp = exp as LambdaExpression;
                    //获取Lambda表达式的主体部分，并传递到本方法中继续执行分发。
                    Expression expBody = lamExp.Body;
                    result = ParseWhere(expBody, statement, dbParams, append);
                    break;

                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                    //二元表达式
                    BinaryExpression binExp = exp as BinaryExpression;
                    result = ParseWhere(binExp, statement, dbParams, append);
                    break;

                case ExpressionType.Constant:
                    //常量表达式
                    ConstantExpression constExp = exp as ConstantExpression;
                    result = ParseWhere(constExp, statement, dbParams, append);
                    break;

                case ExpressionType.MemberAccess:
                    //成员访问表达式
                    MemberExpression memExp = exp as MemberExpression;
                    result = ParseWhere(memExp, statement, dbParams, append);
                    break;

                case ExpressionType.Parameter:
                    //参数表达式
                    ParameterExpression paramExp = exp as ParameterExpression;
                    result = ParseWhere(paramExp, statement, dbParams, append);
                    break;

                case ExpressionType.Call:
                    //方法调用表达式
                    MethodCallExpression methodExp = exp as MethodCallExpression;
                    result = ParseWhere(methodExp, statement, dbParams, append);
                    break;
                default:
                    //不支持的表达式类型
                    throw new ArgumentException("Not support the expression type.", "exp");
            }
            return result;
        }

        /// <summary>
        /// 将常量表达式解析为Where子句元素。
        /// </summary>
        /// <param name="exp">常量表达式对象</param>
        /// <param name="statement">用于存储解析后语句的StringBuilder对象</param>
        /// <param name="dbParams">用于存放从表达式树中解析出来的DbParameter元素集合</param>
        /// <param name="append">表示是否拼接到statement、dbParams参数中，true为拼接，false不拼接</param>
        /// <returns>返回从表达式树中解析出的值</returns>
        public virtual object ParseWhere(ConstantExpression exp, StringBuilder statement, IList<DbParameter> dbParams, bool append)
        {
            if (exp == null)
                throw new ArgumentNullException("exp", "exp can't null.");
            if (statement == null)
                throw new ArgumentNullException("statement", "statement can't null.");

            //获取常量表达式的值
            object val = exp.Value;

            //拼接上从表达式树解析出来的值
            AppendExpressionValue(val, statement, dbParams, append);

            return val;
        }


        /// <summary>
        /// 将成员（字段/属性）访问表达式解析为Where子句元素。
        /// </summary>
        /// <param name="exp">成员访问表达式对象</param>
        /// <param name="statement">用于存储解析后语句的StringBuilder对象</param>
        /// <param name="dbParams">用于存放从表达式树中解析出来的DbParameter元素集合</param>
        /// <param name="append">表示是否拼接到statement、dbParams参数中，true为拼接，false不拼接</param>
        /// <returns>返回从表达式树中解析出的值</returns>
        public virtual object ParseWhere(MemberExpression exp, StringBuilder statement, IList<DbParameter> dbParams, bool append)
        {
            if (exp == null)
                throw new ArgumentNullException("exp", "exp can't null.");
            if (statement == null)
                throw new ArgumentNullException("statement", "statement can't null.");

            object result = null;
            //表达式中访问的对象成员
            MemberInfo memInfo = exp.Member;
            //表达式被访问对象成员的所有者
            Expression ownerExp = exp.Expression;

            if (ownerExp == null || ownerExp.NodeType == ExpressionType.Constant)
            {
                //如果所有者为【常量】，那么被访问的成员将被解析为SQL语句的参数

                //被访问成员所属对象的常量表达式
                ConstantExpression constExpForOwner = ownerExp as ConstantExpression;
                //被访问成员所属的对象，有可能为null，因为成员有可能是静态成员
                object owner = constExpForOwner != null ? constExpForOwner.Value : null;
                object val = null;
                //获取被访问成员的值
                //TODO:这里似乎忽略了一个问题，如果该成员并非基础数据类型的话，拿到的只是类型名称。
                val = GetMemberValue(owner, memInfo);
                result = val;

                //拼接上从表达式树解析出来的值
                AppendExpressionValue(val, statement, dbParams, append);
            }
            else if (ownerExp.NodeType == ExpressionType.Parameter && append)
            {
                //如果所有者为【参数】，那么它是一个实体类，也相当于数据表；最终解析为：tableName.[fieldName]
                //TODO:当前只支持一层成员调用关系，比如：obj.Name；当前的常规SQL语句也是：表名.字段名，暂时应无大碍，但待改进。
                string ownerName = ownerExp.Type.Name;
                string memberName = memInfo.Name;
                statement.AppendFormat("{0}.[{1}]", ownerName, memberName);
            }
            else if (ownerExp.NodeType == ExpressionType.MemberAccess)
            {
                //获取成员调用链中最底部的值
                string callChain = null;
                result = GetLastMemberValueForMemberExp(exp, dbParams, out callChain);
                if (append)
                    statement.Append(callChain);
            }
            else
            {
                //其它表达式类型，再次分发解析
                result = ParseWhere(ownerExp, statement, dbParams, append);
            }
            return result;
        }

        /// <summary>
        /// 获取调用链中最后一个成员值或整个参数表达式的调用链语句
        /// </summary>
        /// <param name="memExp">最底部的成员表达式</param>
        /// <param name="callChain">存储调用链的字符串输出参数；加上out是为了更明确它是输出参数</param>
        /// <param name="dbParams">用于存放从表达式树中解析出来的DbParameter元素集合</param>
        /// <returns>返回调用链中最底部的值</returns>
        public virtual object GetLastMemberValueForMemberExp(MemberExpression memExp, IList<DbParameter> dbParams, out string callChain)
        {
            StringBuilder statement = new StringBuilder();
            //用于存储成员调用链中的所有表达式
            Stack<Expression> exps = new Stack<Expression>();
            //获取调用链中的所有表达式
            GetMembersWithOwner(memExp, exps);

            //成员的所有者对象
            object owner = null;
            //第一个为所有者表达式
            Expression ownerExp = exps.Pop();
            //表示当前表达式是否为MemberExpression，true为是，false为ParameterExpression
            bool isMemberExp = false;

            if (ownerExp == null)
            {
                //所有者可能为null，因为可能是调用静态成员。
                //该表达式为MemberExpression
                isMemberExp = true;
            }
            else if (ownerExp.NodeType == ExpressionType.Constant)
            {
                //如果所有者为常量表达式，成员所有者对象为该表达式的值
                ConstantExpression constExp = ownerExp as ConstantExpression;
                owner = constExp.Value;
                isMemberExp = true;
            }
            else if (ownerExp.NodeType == ExpressionType.Parameter)
            {
                //如果所有者为ParameterExpression，拼接上该参数的类型名称（表名）
                ParameterExpression paramExp = ownerExp as ParameterExpression;
                statement.AppendFormat("{0}.", paramExp.Type.Name);
            }

            //集合中表达式元素的个数，因为Pop会移除元素，导致循环中元素个数产生变化
            int expCount = exps.Count;
            for (int i = 0; i < expCount; i++)
            {
                MemberExpression mExp = exps.Pop() as MemberExpression;
                MemberInfo memInfo = mExp.Member;
                if (isMemberExp)
                {
                    //获取成员的值
                    owner = GetMemberValue(owner, memInfo);
                }
                else
                {
                    //拼接上成员名称
                    statement.AppendFormat("[{0}].", memInfo.Name);
                }
            }

            if (isMemberExp)
            {
                //MemberExpression只需拼接上调用链中最后一个成员的值
                AppendExpressionValue(owner, statement, dbParams, true);
            }
            else if (statement.Length > 0)
            {
                //ParamesterExpression需要移除最后多余的“.”
                statement.Remove(statement.Length - 1, 1);
            }
            //调用链
            callChain = statement.ToString();

            return owner;
        }

        /// <summary>
        /// 递归获取整个成员调用链中的表达式，并存储在堆栈集合中
        /// </summary>
        /// <param name="exp">当前表达式，可能为调用链中的一个成员表达式，也可能是最顶部的所有者</param>
        /// <param name="exps">存储调用链中所有表达式的堆栈集合</param>
        public virtual void GetMembersWithOwner(Expression exp, Stack<Expression> exps)
        {
            if (exp == null || exp.NodeType != ExpressionType.MemberAccess)
            {
                //如果表达式为null或者表达式类型不为MemberAccess，说明已经到达成员调用链的顶部；将它放入集合中并结束递归。
                exps.Push(exp);
                return;
            }
            else if (exp.NodeType == ExpressionType.MemberAccess)
            {
                //如果表达式类型为MemberAccess，说明还未到达成员调用链的顶部，将当前表达式保存至集合，并使用所有者表达式继续递归。
                MemberExpression memExp = exp as MemberExpression;
                Expression ownerExp = memExp.Expression;
                exps.Push(exp);
                if (exp == null)
                    return;
                GetMembersWithOwner(ownerExp, exps);
            }
        }

        /// <summary>
        /// 递归获取整个方法调用链中的表达式，并存储在堆栈集合中
        /// </summary>
        /// <param name="exp">当前表达式，可能为调用链中的一个表达式，也可能是最顶部的所有者</param>
        /// <param name="exps">存储调用链中所有表达式的堆栈集合</param>
        public virtual void GetMethodCallChain(Expression exp, Stack<Expression> exps)
        {
            Expression obj = null;
            if (exp == null || exp.NodeType != ExpressionType.Call && exp.NodeType != ExpressionType.MemberAccess)
            {
                exps.Push(exp);
                return;
            }
            else if (exp.NodeType == ExpressionType.Call)
            {
                MethodCallExpression mcExp = exp as MethodCallExpression;
                obj = mcExp.Object;
            }
            else if (exp.NodeType == ExpressionType.MemberAccess)
            {
                MemberExpression memExp = exp as MemberExpression;
                obj = memExp.Expression;
            }

            exps.Push(obj);
            if (obj == null)
                return;
            GetMethodCallChain(obj, exps);
        }

        /// <summary>
        /// 将一元表达式解析成Where子句元素。
        /// </summary>
        /// <param name="exp">一元表达式对象</param>
        /// <param name="statement">用于存储解析后语句的StringBuilder对象</param>
        /// <param name="dbParams">用于存放从表达式树中解析出来的DbParameter元素集合</param>
        /// <param name="append">表示是否拼接到statement、dbParams参数中，true为拼接，false不拼接</param>
        public virtual void ParseWhere(UnaryExpression exp, StringBuilder statement, IList<DbParameter> dbParams, bool append)
        {
            if (exp == null)
                throw new ArgumentNullException("exp", "exp can't null.");
            if (statement == null)
                throw new ArgumentNullException("statement", "statement can't null.");

            //一元表达式中包含的操作数
            Expression operand = exp.Operand;
            //将表达式交从IParser中继承而来的入口方法分发
            ParseWhere(operand, statement, dbParams, append);
        }

        /// <summary>
        /// 将二元表达式解析成Where子句元素。
        /// </summary>
        /// <param name="exp">二元表达式对象</param>
        /// <param name="statement">用于存储解析后语句的StringBuilder对象</param>
        /// <param name="dbParams">用于存放从表达式树中解析出来的DbParameter元素集合</param>
        /// <param name="append">表示是否拼接到statement、dbParams参数中，true为拼接，false不拼接</param>
        /// <returns>返回从表达式树中解析出的值</returns>
        public virtual object ParseWhere(BinaryExpression exp, StringBuilder statement, IList<DbParameter> dbParams, bool append)
        {
            if (exp == null)
                throw new ArgumentNullException("exp", "exp can't null.");
            if (statement == null)
                throw new ArgumentNullException("statement", "statement can't null.");

            Expression leftExp = exp.Left;
            Expression rightExp = exp.Right;
            object result = null;

            string expStr = exp.ToString();
            //对二元表达式进行解析
            Stack<Expression> exps = new Stack<Expression>();
            Expression topEle = null;
            byte allParamExps = 0;

            //TODO:这里对左右两部分都是采取由下至上遍历表达式树，感觉不是很好，有待改进。
            //判断左部是否为参数表达式
            if (leftExp.NodeType == ExpressionType.Parameter)
            {
                //如果左部表达式为参数表达式，那么将返回值赋值为该参数表达式，用作最后判断是否拼接如SQL语句中
                result = leftExp;
                allParamExps++;
            }
            else if (leftExp.NodeType == ExpressionType.MemberAccess)
            {
                GetMembersWithOwner(leftExp, exps);
                topEle = exps.Pop();
                if (topEle != null && topEle.NodeType == ExpressionType.Parameter)
                {
                    //如果左部成员调用链顶部为参数表达式，那么将返回值赋值为该参数表达式，用作最后判断是否拼接如SQL语句中
                    result = topEle;
                    allParamExps++;
                }
                exps.Clear();
            }
            else if (leftExp.NodeType == ExpressionType.Call)
            {
                GetMethodCallChain(leftExp, exps);
                topEle = exps.Pop();
                if (topEle != null && topEle.NodeType == ExpressionType.Parameter)
                {
                    //如果左部方法调用链顶部为参数表达式，那么将返回值赋值为该参数表达式，用作最后判断是否拼接如SQL语句中
                    result = topEle;
                    allParamExps++;
                }
                exps.Clear();
            }

            //判断右部是否为参数表达式
            if (rightExp.NodeType == ExpressionType.Parameter)
            {
                //如果右部表达式为参数表达式，那么将返回值赋值为该参数表达式，用作最后判断是否拼接如SQL语句中
                result = rightExp;
                allParamExps++;
            }
            else if (rightExp.NodeType == ExpressionType.MemberAccess)
            {
                GetMembersWithOwner(rightExp, exps);
                topEle = exps.Pop();
                if (topEle != null && topEle.NodeType == ExpressionType.Parameter)
                {
                    //如果右部成员调用链顶部为参数表达式，那么将返回值赋值为该参数表达式，用作最后判断是否拼接如SQL语句中
                    result = topEle;
                    allParamExps++;
                }
            }
            else if (rightExp.NodeType == ExpressionType.Call)
            {
                GetMethodCallChain(rightExp, exps);
                topEle = exps.Pop();
                if (topEle != null && topEle.NodeType == ExpressionType.Parameter)
                {
                    //如果右部方法调用链顶部为参数表达式，那么将返回值赋值为该参数表达式，用作最后判断是否拼接如SQL语句中
                    result = topEle;
                    allParamExps++;
                }
            }

            //1.解析左部表达式
            object leftVal = ParseWhere(leftExp, statement, dbParams, append);
            //2.解析拼接符号
            if (append)
            {
                string expOperator = GetOperatorByNodeType(exp.NodeType);
                statement.AppendFormat("{0}", expOperator);
            }
            //3.解析右部表达式
            object rightVal = ParseWhere(rightExp, statement, dbParams, append);

            if (allParamExps == 0 && !(leftVal is ParameterExpression) && !(rightVal is ParameterExpression))
            {
                result = Operating(leftVal, rightVal, exp.NodeType);
                AppendExpressionValue(result, statement, dbParams, append);
            }

            return result;
        }

        /// <summary>
        /// 将参数表达式解析成Where子句元素。
        /// </summary>
        /// <param name="exp">参数表达式对象</param>
        /// <param name="statement">用于存储解析后语句的StringBuilder对象</param>
        /// <param name="dbParams">用于存放从表达式树中解析出来的DbParameter元素集合</param>
        /// <param name="append">表示是否拼接到statement、dbParams参数中，true为拼接，false不拼接</param>
        /// <returns>,ParameterExpression固定返回null</returns>
        public virtual object ParseWhere(ParameterExpression exp, StringBuilder statement, IList<DbParameter> dbParams, bool append)
        {
            if (exp == null)
                throw new ArgumentNullException("exp", "exp can't null.");
            if (statement == null)
                throw new ArgumentNullException("statement", "statement can't null.");

            //解析出参数的类型名称
            string paramTypeName = exp.Type.Name;
            if (append)
                statement.Append(paramTypeName);

            return null;
        }

        /// <summary>
        /// 解析方法参数
        /// </summary>
        /// <param name="arguments">包含方法参数的集合</param>
        /// <param name="statement">用于存储解析后语句的StringBuilder对象</param>
        /// <param name="dbParams">用于存放从表达式树中解析出来的DbParameter元素集合</param>
        /// <returns>返回包含参数表达式的值的集合</returns>
        public virtual IList<object> ParseWhere(ReadOnlyCollection<Expression> arguments, StringBuilder statement, IList<DbParameter> dbParams)
        {
            List<object> results = new List<object>();

            foreach (Expression exp in arguments)
            {
                object r = ParseWhere(exp, statement, dbParams, false);
                results.Add(r);
            }

            return results;
        }

        /// <summary>
        /// 将方法调用表达式解析成Where子句元素。
        /// </summary>
        /// <param name="exp">方法调用表达式对象</param>
        /// <param name="statement">用于存储解析后语句的StringBuilder对象</param>
        /// <param name="dbParams">用于存放从表达式树中解析出来的DbParameter元素集合</param>
        /// <param name="append">表示是否拼接到statement、dbParams参数中，true为拼接，false不拼接</param>
        /// <returns>返回从表达式树中解析出的值，如果表达式树不存在值则返回null</returns>
        public virtual object ParseWhere(MethodCallExpression exp, StringBuilder statement, IList<DbParameter> dbParams, bool append)
        {
            //解析方法参数集合
            object[] args = ParseWhere(exp.Arguments, statement, dbParams).ToArray();
            Expression caller = exp.Object;
            MethodInfo method = exp.Method;
            object callerObj = null;
            object result = null;

            if (caller != null)
            {
                if (caller.NodeType == ExpressionType.Parameter)
                {
                    //TODO:对于调用者为参数表达式的情况下，暂不支持多层调用
                    ParameterExpression paramExp = caller as ParameterExpression;
                    TypeCode tCode = Type.GetTypeCode(paramExp.Type);

                    //TODO:对于参数调用者，当前只支持String类型及其Contains、StartWith、EndWith方法
                    if (tCode != TypeCode.String)
                    {
                        throw new Exception("参数表达式暂不支持该方法");
                    }

                    string fieldName = paramExp.Name;
                    object firstArgs = args.FirstOrDefault();

#if !TEST
                    string paramName = ParameterNaming();
                    string paramVal = null;
                    switch (method.Name)
                    {
                        case "Contains":
                            statement.AppendFormat("{0} like {1}", fieldName, paramName);
                            paramVal = string.Format("%{0}%", firstArgs);
                            break;
                        case "StartsWith":
                            statement.AppendFormat("{0} like {1}", fieldName, paramName);
                            paramVal = string.Format("{0}%", firstArgs);
                            break;
                        case "EndWith":
                            statement.AppendFormat("{0} like {1}", fieldName, paramName);
                            paramVal = string.Format("%{0}", firstArgs);
                            break;
                        default:
                            throw new NotSupportedException("Not supprted the method.");
                    }
                    if (dbParams != null)
                    {
                        DbParameter dbParamItem = DbParamFactory(paramName, paramVal);
                        dbParams.Add(dbParamItem);
                    }

#else
                    switch (method.Name)
                    {
                        case "Contains":
                            statement.AppendFormat("{0} like '%{1}%'", fieldName, firstArgs);
                            break;
                        case "StartsWith":
                            statement.AppendFormat("{0} like '{1}%'", fieldName, firstArgs);
                            break;
                        case "EndWith":
                            statement.AppendFormat("{0} like '%{1}'", fieldName, firstArgs);
                            break;
                        default:
                            throw new NotSupportedException("Not supprted the method.");
                    }
#endif
                }
                else if (caller.NodeType == ExpressionType.MemberAccess)
                {
                    //方法调用者为属性或字段
                    Stack<Expression> exps = new Stack<Expression>();
                    //获取整个调用链
                    GetMembersWithOwner(caller, exps);
                    //判断调用链最顶端是否为参数表达式
                    Expression topEle = exps.Peek();
                    bool isParam = topEle != null && topEle.NodeType == ExpressionType.Parameter;
                    if (isParam)
                    {
                        int stkCount = exps.Count;
                        foreach (Expression ele in exps)
                        {
                            switch (ele.NodeType)
                            {
                                case ExpressionType.Parameter:
                                    ParameterExpression pExp = ele as ParameterExpression;
                                    statement.AppendFormat("{0}.", pExp.Type.Name);
                                    break;
                                case ExpressionType.MemberAccess:
                                    MemberExpression mExp = ele as MemberExpression;
                                    statement.AppendFormat("[{0}].", mExp.Member.Name);
                                    break;
                            }
                        }
                        //移除最后一个"."
                        statement.Remove(statement.Length - 1, 1);

                        object firstArgs = args.FirstOrDefault();

#if !TEST
                        string paramName = ParameterNaming();
                        string paramVal = null;
                        switch (method.Name)
                        {
                            case "Contains":
                                statement.AppendFormat(" like {0}", paramName);
                                paramVal = string.Format("%{0}%", firstArgs);
                                break;
                            case "StartsWith":
                                statement.AppendFormat(" like {0}", paramName);
                                paramVal = string.Format("{0}%", firstArgs);
                                break;
                            case "EndsWith":
                                statement.AppendFormat(" like {0}", paramName);
                                paramVal = string.Format("%{0}", firstArgs);
                                break;
                            default:
                                throw new NotSupportedException("Not supprted the method.");
                        }
                        if (dbParams != null)
                        {
                            DbParameter dbParamItem = DbParamFactory(paramName, paramVal);
                            dbParams.Add(dbParamItem);
                        }
#else
                        switch (method.Name)
                        {
                            case "Contains":
                                statement.AppendFormat(" like '%{0}%'", firstArgs);
                                break;
                            case "StartsWith":
                                statement.AppendFormat(" like '{0}%'", firstArgs);
                                break;
                            case "EndsWith":
                                statement.AppendFormat(" like '%{0}'", firstArgs);
                                break;
                            default:
                                throw new NotSupportedException("Not supprted the method.");
                        }
#endif
                    }
                    else
                    {
                        //最顶端非参数表达式，继续分发解析
                        result = ParseWhere(caller, statement, dbParams, isParam);
                        result = method.Invoke(result, args);
                        AppendExpressionValue(result, statement, dbParams, append);
                    }
                }
                else if (caller.NodeType == ExpressionType.Constant)
                {
                    //调用者为常量
                    ConstantExpression constExp = caller as ConstantExpression;
                    callerObj = constExp.Value;
                    result = method.Invoke(callerObj, args);
                    AppendExpressionValue(result, statement, dbParams, append);
                }
            }
            else if (method.IsStatic)
            {
                //静态方法调用
                callerObj = null;
                if (caller != null)
                {
                    callerObj = ParseWhere(caller, statement, dbParams, false);
                }
                //解析调用者对象
                result = method.Invoke(callerObj, args);
                AppendExpressionValue(result, statement, dbParams, append);
            }

            return result;
        }

        /// <summary>
        /// 根据表达式节点的类型获取表达式的操作符
        /// </summary>
        /// <param name="nodeType">表达式节点类型</param>
        /// <returns>返回字符串类型的操作符</returns>
        public virtual string GetOperatorByNodeType(ExpressionType nodeType)
        {
            string expOperator = string.Empty;
            switch (nodeType)
            {
                case ExpressionType.AndAlso:
                    expOperator = " and ";
                    break;
                case ExpressionType.OrElse:
                    expOperator = " or ";
                    break;
                case ExpressionType.Equal:
                    expOperator = "=";
                    break;
                case ExpressionType.NotEqual:
                    expOperator = "<>";
                    break;
                case ExpressionType.Not:
                    expOperator = "!";
                    break;
                case ExpressionType.GreaterThan:
                    expOperator = ">";
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    expOperator = ">=";
                    break;
                case ExpressionType.LessThan:
                    expOperator = "<";
                    break;
                case ExpressionType.LessThanOrEqual:
                    expOperator = "<=";
                    break;
                default:
                    throw new ArgumentException("Not support the node type.", "nodeType");
            }

            return expOperator;
        }

        /// <summary>
        /// 对二元表达式的两个操作对象进行运算
        /// </summary>
        /// <param name="o1">左部操作对象</param>
        /// <param name="o2">右部操作对象</param>
        /// <param name="nodeType">操作类型</param>
        /// <returns>返回运算结果</returns>
        public virtual object Operating(object o1, object o2, ExpressionType nodeType)
        {
            dynamic d1 = o1;
            dynamic d2 = o2;
            object result = null;
            switch (nodeType)
            {
                case ExpressionType.AndAlso:
                    result = d1 && d2;
                    break;
                case ExpressionType.OrElse:
                    result = d1 || d2;
                    break;
                case ExpressionType.Equal:
                    result = d1 == d2;
                    break;
                case ExpressionType.NotEqual:
                    result = d1 != d2;
                    break;
                case ExpressionType.GreaterThan:
                    result = d1 > d2;
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    result = d1 >= d2;
                    break;
                case ExpressionType.LessThan:
                    result = d1 < d2;
                    break;
                case ExpressionType.LessThanOrEqual:
                    result = d1 <= d2;
                    break;
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    result = d1 + d2;
                    break;
                case ExpressionType.Subtract:
                case ExpressionType.SubtractAssignChecked:
                    result = d1 - d2;
                    break;
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    result = d1 * d2;
                    break;
                case ExpressionType.Divide:
                    result = d1 / d2;
                    break;
                case ExpressionType.Modulo:
                    result = d1 % d2;
                    break;
                default:
                    throw new ArgumentException("Not support the node type.", "nodeType");
            }
            return result;
        }

        /// <summary>
        /// 用于替换null之前的：=/<>操作符为：is/is not
        /// </summary>
        /// <param name="statement">Where子句的StringBuilder对象</param>
        /// <returns>返回修改后的StringBuilder对象</returns>
        public virtual StringBuilder ReplaceBeforeNullOperator(StringBuilder statement)
        {
            //TODO:null在表达式左边的话将无法替换为特定的表达式

            string str = statement.ToString();
            //等于号正确的索引位置
            int equalsCorrectIdx = str.Length - 1;
            //不等于号正确的索引位置
            int notEqualsCorrectIdx = str.Length - 2;
            int equalsIdx = str.LastIndexOf("=");
            int notEqualsIdx = str.LastIndexOf("<>");

            if (equalsIdx != -1 && equalsIdx == equalsCorrectIdx)
            {
                //如果最后一个字符为等于号，那么将其替换为：is 
                string beforeOperator = str.Substring(0, equalsCorrectIdx);
                statement.Clear();
                statement.AppendFormat("{0} is ", beforeOperator);
            }
            else if (notEqualsIdx != -1 && notEqualsIdx == notEqualsCorrectIdx)
            {
                //如果最后一个字符为不等于号，那么将其替换为：is not
                string beforeOperator = str.Substring(0, notEqualsCorrectIdx);
                statement.Clear();
                statement.AppendFormat("{0} is not ", beforeOperator);
            }

            return statement;
        }

        /// <summary>
        /// 向语句拼接上从表达式中解析出来的值
        /// </summary>
        /// <param name="val">解析出来的表达式值</param>
        /// <param name="statement">Where子句的StringBuilder对象</param>
        /// /// <param name="append">表示是否拼接到statement参数中，true为拼接，false不拼接</param>
        public virtual void AppendExpressionValue(object val, StringBuilder statement, IList<DbParameter> dbParams, bool append)
        {
            if (!append)
                return;

            if (val == null)
            {
#if TEST
                //值为null，拼接上null
                statement = ReplaceBeforeNullOperator(statement);
                statement.Append("null");
#else
                if (dbParams != null)
                {
                    string paramName = ParameterNaming();
                    statement.Append(paramName);
                    DbParameter param = DbParamFactory(paramName, DBNull.Value);
                    dbParams.Add(param);
                }

#endif
            }
#if TEST
            else
            {
                //如果值类型为字符串或日期时间，需要加上单引号
                string valTypeName = val.GetType().Name;
                if (valTypeName.Equals("String") || valTypeName.Equals("DateTime"))
                {
                    statement.AppendFormat("'{0}'", val);
                }
                else
                {
                    statement.Append(val);
                }
            }
#else
            else if (dbParams != null)
            {
                string paramName = ParameterNaming();
                statement.Append(paramName);
                DbParameter param = DbParamFactory(paramName, val);
                dbParams.Add(param);
            }
#endif
        }

        /// <summary>
        /// 获取成员的值
        /// </summary>
        /// <param name="owner">成员所有者对象，静态成员的所有者为null</param>
        /// <param name="memInfo">成员信息对象</param>
        /// <returns>返回object类型的成员值</returns>
        public virtual object GetMemberValue(object owner, MemberInfo memInfo)
        {
            object val = null;
            switch (memInfo.MemberType)
            {
                case MemberTypes.Field:
                    FieldInfo fInfo = memInfo as FieldInfo;
                    val = fInfo.GetValue(owner);
                    break;
                case MemberTypes.Property:
                    PropertyInfo pInfo = memInfo as PropertyInfo;
                    val = pInfo.GetValue(owner, null);
                    break;
            }
            return val;
        }

        /// <summary>
        /// 随机一个DbParameter名称
        /// </summary>
        /// <returns>返回DbParameter名称</returns>
        public abstract string ParameterNaming();

        /// <summary>
        /// 创建SQL参数
        /// </summary>
        /// <param name="name">参数名称</param>
        /// <param name="value">参数值</param>
        /// <returns>返回SQL命令参数</returns>
        public abstract DbParameter DbParamFactory(string name, object value);

        #endregion

        #region SQL语句的构建

        /// <summary>
        /// 从一系列表达式树中解析查询排序子句的方法
        /// </summary>
        /// <param name="sortables">包含排序信息对象的集合</param>
        /// <returns>返回排序子句</returns>
        public string ParseOrderBy(IEnumerable<ISortable> sortables)
        {
            if (sortables == null || !sortables.Any())
                return null;

            StringBuilder orderByStatement = new StringBuilder("order by ");
            StringBuilder fieldStr = new StringBuilder();
            foreach (ISortable sortable in sortables)
            {
                //解析表达式中的字段信息
                ParseWhere(sortable.OrderByExpression, fieldStr);
                orderByStatement.AppendFormat("{0} {1},", fieldStr, sortable.OrderBy == OrderBy.Asc ? "asc" : "desc");
                fieldStr.Clear();
            }
            //移除最后一个逗号
            orderByStatement.Remove(orderByStatement.Length - 1, 1);
            return orderByStatement.ToString();
        }

        /// <summary>
        /// 从表达式集合中解析出Where子句
        /// </summary>
        /// <param name="whereExps">将要解析成Where</param>
        /// <param name="dbParams">用于存放从表达式树中解析出来的DbParameter元素集合</param>
        /// <returns>返回解析出的Where子句</returns>
        protected virtual string BuildWhereStatement(IEnumerable<Expression> whereExps, IList<DbParameter> dbParams)
        {
            if (whereExps == null || !whereExps.Any())
            {
                return null;
            }

            StringBuilder whereStatement = new StringBuilder();
            string connection = " and ";
            int connLen = connection.Length;
            foreach (Expression exp in whereExps)
            {
                ParseWhere(exp, whereStatement, dbParams);
                whereStatement.Append(connection);
            }
            //移除最后的" and "
            whereStatement.Remove(whereStatement.Length - connLen, connLen)
                .Append(" ");
            return whereStatement.ToString();
        }

        /// <summary>
        /// 通过ITransctiond派生类对象解析出SQL语句
        /// </summary>
        /// <typeparam name="T">实体对象的类型参数</typeparam>
        /// <param name="transaction">包含一系列SQL操作信息的对象</param>
        /// <param name="dbParams">从SQL中解析出来的参数集合</param>
        /// <returns>返回解析出的SQL语句</returns>
        public string BuildSqlStatement<T>(ITransaction<T> transaction, IList<DbParameter> dbParams)
            where T : ModelBase,new()
        {
            if (transaction == null)
                throw new ArgumentNullException("transaction", "transction 不能为空。");
            if (dbParams == null)
                throw new ArgumentNullException("dbParams", "dbParams 不能为空。");

            //**默认解析MS SQL语句
            string statement = null;
            switch (transaction.TransactionType)
            {
                case TransactionType.Insert:
                    statement = BuildInsertStatement(transaction, dbParams);
                    break;
                case TransactionType.Select:
                    statement = BuildSelectStatement(transaction, dbParams);
                    break;
                case TransactionType.Update:
                    statement = BuildUpdateStatement(transaction, dbParams);
                    break;
                case TransactionType.Delete:
                    statement = BuildDeleteStatement(transaction, dbParams);
                    break;
            }

            return statement;
        }

        /// <summary>
        /// 构建Select语句
        /// </summary>
        /// <typeparam name="T">实体对象的类型参数</typeparam>
        /// <param name="transaction">包含一系列SQL操作信息的对象</param>
        /// <param name="dbParams">从SQL中解析出来的参数集合</param>
        /// <returns>返回Select语句</returns>
        protected string BuildSelectStatement<T>(ITransaction<T> transaction, IList<DbParameter> dbParams)
            where T : ModelBase,new()
        {
            IModel entity = transaction.Entity;
            if (entity == null)
                throw new ArgumentNullException("entity 不能为空。");
            Type entityType = entity.GetType();
            //获取属性集合
            IDictionary<string, PropertyInfo> propDic = ModelBase.GetProperties(entityType);
            if (propDic.Count == 0)
                throw new ArgumentNullException("找不到任何属性。");
            //获取参与Select操作的字段名集合
            IList<string> fieldNames = ModelBase.GetSelectFields(entityType);
            if (fieldNames.Count == 0)
                throw new ArgumentNullException("找不到任何将参与Select的字段名。");

            //是否分页
            bool isPaging = transaction.NumOfSkip != null && transaction.NumOfTake != null;

            StringBuilder statement = new StringBuilder("select ");
            //是否使用Top关键字
            if (transaction.NumOfTake != null && !isPaging)
            {
                statement.AppendFormat("top {0} ", transaction.NumOfTake.Value.ToString());
            }
            //首字段
            string firstField = fieldNames[0];
            //表名
            string tableName = entityType.Name;

            //Order By子句
            string orderByStatement = ParseOrderBy(transaction.Sorts);

            if (transaction.AggregateFunction != null && !isPaging)
            {
                //聚合查询
                statement.AppendFormat("{0}({1}.[{2}])", transaction.AggregateFunction.Value.ToString(), tableName, firstField);
            }
            else
            {
                //非聚合查询
                foreach (string f in fieldNames)
                {
                    statement.AppendFormat("{0}.[{1}],", tableName, f);
                }
                statement.Remove(statement.Length - 1, 1);

                //分页情况下加上ROW_NUMBER子句
                if (isPaging)
                {
                    //如果没有排序条件，则默认以ID降序排序
                    string orderByStat = orderByStatement != null ? orderByStatement : string.Format("order by {0}.[id] desc", tableName);
                    statement.AppendFormat(",row_number() over({0}) as rowNum", orderByStat);
                }
            }

            //from子句
            statement.AppendFormat(" from {0}", tableName);
            //表链接
            if (transaction.JoinTables.Count > 0)
            {
                statement.Append(" ");
                foreach (IJoinTable jt in transaction.JoinTables)
                {
                    switch (jt.JoinType)
                    {
                        case JoinType.InnerJoin:
                            statement.Append("inner join ");
                            break;
                        case JoinType.LeftJoin:
                            statement.Append("left join ");
                            break;
                        case JoinType.RightJoin:
                            statement.Append("right join ");
                            break;
                    }
                    statement.AppendFormat("{0} on ", jt.FKTableName);
                    //on子句
                    ParseWhere(jt.JoinExpression, statement);
                    statement.Append(" ");
                }
            }

            //Where子句
            string whereStatement = BuildWhereStatement(transaction.WhereExpressions, dbParams);
            if (!string.IsNullOrWhiteSpace(whereStatement))
            {
                statement.AppendFormat(" where {0}", whereStatement);
            }

            if (isPaging)
            {
                //分页子查询
                statement.Insert(0, "select * from (");
                statement.AppendFormat(") resultSet where rowNum between {0} and {1}",
                    (transaction.NumOfSkip.Value + 1).ToString(),
                    (transaction.NumOfTake.Value + transaction.NumOfSkip).ToString());
            }
            else if (orderByStatement != null)
            {
                //非分页情况下的Order By子句
                statement.Append(orderByStatement.ToString());
            }

            return statement.ToString();
        }

        /// <summary>
        /// 构建Delete语句
        /// </summary>
        /// <typeparam name="T">模型类的泛型参数</typeparam>
        /// <param name="transaction">包含数据库操作信息的对象</param>
        /// <param name="dbParams">包含命令参数的集合</param>
        /// <returns>返回Delete语句</returns>
        protected string BuildDeleteStatement<T>(ITransaction<T> transaction, IList<DbParameter> dbParams)
            where T : ModelBase,new()
        {
            StringBuilder statement = new StringBuilder();
            ModelBase entity = transaction.Entity;
            Type entityType = entity.GetType();
            IList<Expression> exps = transaction.WhereExpressions;

            statement.AppendFormat("delete {0}", entityType.Name);
            //Where子句
            string whereStatement = BuildWhereStatement(transaction.WhereExpressions, dbParams);
            if (!string.IsNullOrWhiteSpace(whereStatement))
            {
                statement.AppendFormat(" where {0}", whereStatement);
            }

            return statement.ToString();
        }

        /// <summary>
        /// 构建Insert语句
        /// </summary>
        /// <typeparam name="T">模型类的泛型参数</typeparam>
        /// <param name="transaction">包含数据库操作信息的对象</param>
        /// <param name="dbParams">包含命令参数的集合</param>
        /// <returns>返回Insert语句</returns>
        protected string BuildInsertStatement<T>(ITransaction<T> transaction, IList<DbParameter> dbParams)
            where T : ModelBase,new()
        {
            ModelBase entity = transaction.Entity;
            if (entity == null)
                throw new ArgumentNullException("entity 不能为空。");
            Type entityType = entity.GetType();
            //获取属性集合
            IDictionary<string, PropertyInfo> propDic = ModelBase.GetProperties(entityType);
            if (propDic.Count == 0)
                throw new ArgumentNullException("找不到任何属性。");
            //获取参与Insert操作的字段名集合
            IList<string> fieldNames = ModelBase.GetInsertFields(entityType);
            if (fieldNames.Count == 0)
                throw new ArgumentNullException("找不到任何将参与Insert的字段名。");

            StringBuilder statement = new StringBuilder("insert into ");
            //values子句
            StringBuilder valuesStatement = new StringBuilder("values(");
            statement.AppendFormat("{0}(", entityType.Name);

            foreach (string field in fieldNames)
            {
                //拼接字段名
                statement.AppendFormat("[{0}],", field);
                //获取属性值
                PropertyInfo propInfo = propDic[field];
                object val = propInfo.GetValue(entity, null);
                //参数化
                string dbParamName = ParameterNaming();
                DbParameter dbParam = DbParamFactory(dbParamName, val);
                //将参数名拼接到values部分
                valuesStatement.AppendFormat("{0},", dbParamName);
                dbParams.Add(dbParam);
            }
            valuesStatement.Remove(valuesStatement.Length - 1, 1)
                .Append(")");
            statement.Remove(statement.Length - 1, 1)
                .Append(") ")
                .Append(valuesStatement);

            return statement.ToString();
        }

        /// <summary>
        /// 构建Update语句
        /// </summary>
        /// <typeparam name="T">模型类的泛型参数</typeparam>
        /// <param name="transaction">包含数据库操作信息的对象</param>
        /// <param name="dbParams">包含命令参数的集合</param>
        /// <returns>返回Update语句</returns>
        protected string BuildUpdateStatement<T>(ITransaction<T> transaction, IList<DbParameter> dbParams)
            where T : ModelBase,new()
        {
            if (transaction.Entity == null)
                throw new ArgumentNullException("transaction.Entity 不能为空。");
            ModelBase entity = transaction.Entity;
            Type entityType = entity.GetType();
            string entityName = entityType.Name;
            //获取属性集合
            IDictionary<string, PropertyInfo> propDic = ModelBase.GetProperties(entityType);
            if (propDic.Count == 0)
                throw new ArgumentNullException("找不到任何属性。");
            //获取参与Update操作的字段名集合
            IList<string> fieldNames = ModelBase.GetUpdateFields(entityType);
            if (fieldNames.Count == 0)
                throw new ArgumentNullException("找不到任何将参与Update的字段名。");

            StringBuilder statement = new StringBuilder();
            //拼接set子句
            statement.AppendFormat("update {0} set ", entityType.Name);
            foreach (string field in fieldNames)
            {
                string paramName = ParameterNaming();
                //获取属性值
                PropertyInfo propInfo = propDic[field];
                object fValue = propInfo.GetValue(entity, null);
                DbParameter dbPam = DbParamFactory(paramName, fValue);
                statement.AppendFormat("{0}.[{1}]={2},", entityName, field, paramName);
                dbParams.Add(dbPam);
            }
            statement.Remove(statement.Length - 1, 1);

            //拼接Where子句
            string whereStatement = BuildWhereStatement(transaction.WhereExpressions, dbParams);
            if (!string.IsNullOrWhiteSpace(whereStatement))
            {
                statement.AppendFormat(" where {0}", whereStatement);
            }

            return statement.ToString();
        }

        /// <summary>
        /// 获取指定属性的属性值
        /// </summary>
        /// <param name="propName">属性名</param>
        /// <param name="type">属性所属对象的Type</param>
        /// <param name="obj">属性所属对象</param>
        /// <returns></returns>
        protected object GetValueFromProperty(string propName, Type type, object obj)
        {
            if (propName == null)
                throw new ArgumentNullException("propName", "propName 不能为空。");
            if (type == null)
                throw new ArgumentNullException("type", "type 不能为空。");
            if (obj == null)
                throw new ArgumentNullException("obj", "obj 不能为空。");

            PropertyInfo prop = type.GetProperty(propName);
            if (prop == null)
                throw new NullReferenceException(string.Format("{0} not found.", propName));

            object val = prop.GetValue(obj, null);
            return val;
        }

        #endregion
    }
}
