using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using LightAir.Models.Attributes;

namespace LightAir.Models
{
    /// <summary>
    /// 所有模型类的直接基类
    /// </summary>
    public abstract class ModelBase : IModel
    {
        #region Private Fields

        /// <summary>
        /// 储存insert语句所用的字段；Key=模型类名/表名，Value=字段集合
        /// </summary>
        private static readonly IDictionary<string, IList<string>> _insertFields;
        /// <summary>
        /// 储存update语句所用的字段；Key=模型类名/表名，Value=字段集合
        /// </summary>
        private static readonly IDictionary<string, IList<string>> _updateFields;
        /// <summary>
        /// 储存select语句所用的字段；Key=模型类名/表名，Value=字段集合
        /// </summary>
        private static readonly IDictionary<string, IList<string>> _selectFields;
        /// <summary>
        ///  储存模型类的所有公共实例属性成员；Key=模型类名/表名，Value=属性成员字典
        /// </summary>
        private static readonly IDictionary<string, IDictionary<string,PropertyInfo>> _properties;

        #endregion

        #region Construct Methods

        static ModelBase()
        {
            _insertFields = new Dictionary<string, IList<string>>();
            _updateFields = new Dictionary<string, IList<string>>();
            _selectFields = new Dictionary<string, IList<string>>();
            _properties = new Dictionary<string, IDictionary<string,PropertyInfo>>();
        }

        #endregion

        #region Statis Methods

        /// <summary>
        /// 根据Type对象获取类的所有公共实例属性的数组，并缓存起来。
        /// </summary>
        /// <param name="type">指定类的Type对象</param>
        /// <returns>返回包含所有公共实力属性的数组</returns>
        public static IDictionary<string,PropertyInfo> GetProperties(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type", "type不能为空");
            }
            //检查是否已缓存，有则直接返回缓存
            string className = type.Name;
            if (_properties.ContainsKey(className))
            {
                return _properties[className];
            }
            //获取所有公共实例属性，并缓存起来
            PropertyInfo[] props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            IDictionary<string, PropertyInfo> propDic = new Dictionary<string, PropertyInfo>();
            if (props.Length > 0)
            {
                foreach (PropertyInfo prp in props)
                {
                    propDic.Add(prp.Name, prp);
                }
            }
            //缓存起来
            _properties.Add(className, propDic);

            return propDic;
        }

        /// <summary>
        ///  获取模型类上参与Insert语句的字段名称集合
        /// </summary>
        /// <param name="type">模型类的Type对象</param>
        /// <returns>返回字段名称集合</returns>
        public static IList<string> GetInsertFields(Type type)
        {
            //检索缓存中是否有数据，有则直接使用缓存
            string className = type.Name;
            if (_insertFields.ContainsKey(className))
            {
                return _insertFields[className];
            }

            //获取模型类的属性集合
            IDictionary<string,PropertyInfo> propDic = GetProperties(type);
            if (propDic.Count == 0)
            {
                return null;
            }
            IList<string> fields = new List<string>();
            Object[] attrs = null;
            List<InsertFieldAttribute> instAttrs = new List<InsertFieldAttribute>();
            foreach (PropertyInfo pi in propDic.Values)
            {
                //查找每个元素上的InsertFieldAttribute特性
                attrs = pi.GetCustomAttributes(typeof(InsertFieldAttribute), false);
                if (attrs.Length == 0)
                {
                    continue;
                }
                InsertFieldAttribute instAtt = (InsertFieldAttribute)attrs[0];
                instAtt.Name = pi.Name;
                instAttrs.Add(instAtt);
            }
            if (instAttrs.Count > 0)
            {
                //根据特性上的Index属性值进行升序排序
                instAttrs = instAttrs.OrderBy(attr => attr.Index).ToList();
                foreach (InsertFieldAttribute attr in instAttrs)
                {
                    //获取特性上的定义字段名
                    fields.Add(attr.Name);
                }
            }
            //将字段名集合缓存起来
            _insertFields.Add(className, fields);
            return fields;
        }

        /// <summary>
        ///  获取模型类上参与Update语句的字段名称集合
        /// </summary>
        /// <param name="type">模型类的Type对象</param>
        /// <returns>返回字段名称集合</returns>
        public static IList<string> GetUpdateFields(Type type)
        {
            //检索缓存中是否有数据，有则直接使用缓存
            string className = type.Name;
            if (_updateFields.ContainsKey(className))
            {
                return _updateFields[className];
            }

            //获取模型类的属性集合
            IDictionary<string,PropertyInfo> propDic= GetProperties(type);
            if (propDic.Count == 0)
            {
                return null;
            }
            IList<string> fields = new List<string>();
            Object[] attrs = null;
            foreach (PropertyInfo pi in propDic.Values)
            {
                //查找每个元素上的UpdateFieldAttribute特性
                attrs = pi.GetCustomAttributes(typeof(UpdateFieldAttribute), false);                
                if (attrs.Length == 0)
                {
                    continue;
                }
                fields.Add(pi.Name);
            }
            //将字段名集合缓存起来
            _updateFields.Add(className, fields);
            return fields;
        }

        /// <summary>
        ///  获取模型类上参与Select语句的字段名称集合
        /// </summary>
        /// <param name="type">模型类的Type对象</param>
        /// <returns>返回字段名称集合</returns>
        public static IList<string> GetSelectFields(Type type)
        {
            //检索缓存中是否有数据，有则直接使用缓存
            string className = type.Name;
            if (_selectFields.ContainsKey(className))
            {
                return _selectFields[className];
            }

            //获取模型类的属性集合
            IDictionary<string,PropertyInfo> propDic = GetProperties(type);
            if (propDic.Count == 0)
            {
                return null;
            }
            IList<string> fields = new List<string>();
            Object[] attrs = null;
            List<SelectFieldAttribute> seltAttrs = new List<SelectFieldAttribute>();
            foreach (PropertyInfo pi in propDic.Values)
            {
                //查找每个元素上的SelectFieldAttribute特性
                attrs = pi.GetCustomAttributes(typeof(SelectFieldAttribute), false);
                if (attrs.Length == 0)
                {
                    continue;
                }
                SelectFieldAttribute seltAtt = (SelectFieldAttribute)attrs[0];
                seltAtt.Name = pi.Name;
                seltAttrs.Add(seltAtt);
            }
            if (seltAttrs.Count > 0)
            {
                //根据特性上的Index属性值进行升序排序
                seltAttrs = seltAttrs.OrderBy(attr => attr.Index).ToList();
                foreach (SelectFieldAttribute attr in seltAttrs)
                {
                    //获取特性上的定义字段名
                    fields.Add(attr.Name);
                }
            }
            //将字段名集合缓存起来
            _selectFields.Add(className, fields);
            return fields;
        }

        #endregion

    }
}
