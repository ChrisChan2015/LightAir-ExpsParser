#define TEST
#undef TEST

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Security.Cryptography;
using LightAir.Infrastructure.Transactions;
using LightAir.Models;
using LightAir.Models.Attributes;

namespace LightAir.Data.Parsers.InConsoleTest
{
    class Program
    {
        static ParserBase _parser = new SqlParser();

        static void Main(string[] args)
        {
#if TEST


            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            StringBuilder statement = new StringBuilder();
            HashSet<DbParameter> dbParams = new HashSet<DbParameter>();

            Expression<Func<TestClass, bool>> exp = tc => null != tc.Name && 18 > tc.Age;

            _parser.ParseWhere(exp, statement, dbParams);
            Console.WriteLine(statement);

            //Test(s => s.Contains(""));

            //string s="";
            //Expression<Func<string>> exp = () => s;
            //Console.WriteLine(exp.Body.NodeType);

            statement.Clear();
            Expression<Func<TestClass, bool>> exp1 = tc => (tc.Name.EndsWith(DateTime.Now.ToString("yyyy-MM-dd")) || tc.Name.Contains("test"));
            _parser.ParseWhere(exp1, statement, dbParams);
            Console.WriteLine(statement);



            statement.Clear();
            string test = "123";
            Expression<Func<TestClass, bool>> exp2 = tc => tc.Name.Contains(test + "456") && tc.Age > (15 + 3);
            _parser.ParseWhere(exp2, statement, dbParams);
            Console.WriteLine(statement);

            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
#endif

            //List<ISortable> stList = new List<ISortable>();
            //Expression<Func<TestClass, string>> odExp1 = tc => tc.Name;
            //stList.Add(new Sortable { OrderBy = OrderBy.Descend, OrderByExpression = odExp1 });
            //Expression<Func<TestClass, int>> odExp2 = tc => tc.Age;
            //stList.Add(new Sortable { OrderBy = OrderBy.Ascend, OrderByExpression = odExp2 });
            //Expression<Func<TestClass, DateTime>> odExp3 = tc => tc.Birthday;
            //stList.Add(new Sortable { OrderBy = OrderBy.Descend, OrderByExpression = odExp3 });
            //Console.WriteLine(_parser.ParseOrderBy(stList));

            //Expression<Func<TestClass, TestClass1, bool>> dpExp = (t1, t2) => t1.Name == "123" && t2.Name != "456";
            //statement.Clear();
            //_parser.ParseWhere(dpExp, statement, dbParams);
            //Console.WriteLine(statement);

            //Console.Write("");
            //foreach (var p in dbParams)
            //{
            //    Console.WriteLine(p.Value);
            //}

            //ISet<DbParameter> dbParams = new HashSet<DbParameter>();
            //TestModel tm = new TestModel();
            //ITransaction<TestModel> transaction = new Transaction<TestModel>();
            //transaction.Entity = tm;
            //transaction.Delete()
            //    .Where<TestModel>(t => t.Id > 0 && t.Age > 18 && t.Name == "test");
            //string statement = _parser.BuildSqlStatement(transaction, dbParams);

            //Console.WriteLine(statement);
            //foreach (DbParameter dbP in dbParams)
            //{
            //    Console.WriteLine(dbP.Value);
            //}

            //Console.WriteLine("".PadLeft(80, '-'));

            //dbParams.Clear();
            //tm.Name = "test";
            //tm.Age = 18;
            //ITransaction<TestModel> insertTransaction = new Transaction<TestModel>();
            //insertTransaction.Entity = tm;
            //insertTransaction.Insert();
            //string insertStatement = _parser.BuildSqlStatement(insertTransaction, dbParams);
            //Console.WriteLine(insertStatement);
            //Console.WriteLine("".PadLeft(80, '-'));

            //dbParams.Clear();
            //ITransaction<TestModel> updateTransaction = new Transaction<TestModel>();
            //updateTransaction.Entity = tm;
            //updateTransaction.Update()
            //    .Where<TestModel>(t => t.Id > 0 && t.Age > 18);
            //string updateStatement = _parser.BuildSqlStatement(updateTransaction, dbParams);
            //Console.WriteLine(updateStatement);
            //Console.ReadKey();

            //Stopwatch sw = new Stopwatch();
            //sw.Start();
            //Type at = typeof(TestAttribute);
            //for (int i = 0; i < 10; i++)
            //{
            //    Type type = typeof(TestClass2);
            //    PropertyInfo[] pis = type.GetProperties(BindingFlags.Instance | BindingFlags.Public
            //        );
            //    foreach (PropertyInfo p in pis)
            //    {
            //        p.GetCustomAttributes(at, true);
            //    }
            //}
            //sw.Stop();
            //Console.WriteLine(sw.ElapsedMilliseconds);

            //Console.WriteLine("--------------");
            //TTT ttt=new TTT(1);

            //List<DbParameter> dbPms = new List<DbParameter>();
            //TestModel tm1 = new TestModel();
            //ITransaction<TestModel> trans = new Transaction<TestModel> { Entity = tm1 };
            //trans.Select()
            //    .Where<TestModel>(tm => tm.Age >= 18 && tm.Age < 36)
            //    .Where<TestModel>(tm => tm.Name.Contains("C"))
            //    .InnerJoin<TestModel2>((tm, tmt) => tm.Id == tmt.RefId)
            //    .OrderByDescend(tm => tm.Age)
            //    .ThenByDescend(tm => tm.Id);

            //string sqlStatement = _parser.BuildSqlStatement(trans, dbPms);
            //Console.WriteLine(sqlStatement);
            //Console.WriteLine("-------------------------------------------------");
            //trans.WhereExpressions.Clear();
            //trans.Update()
            //    .Where<TestModel>(tm => tm.Name.StartsWith("T"))
            //    .Where<TestModel>(tm => tm.Id > 10);
            //sqlStatement = _parser.BuildSqlStatement(trans, dbPms);
            //Console.WriteLine(sqlStatement);

            //using (SqlConnection conn = new SqlConnection("Data Source=.;Initial Catalog=Test;User Id=sa;Pwd=sa"))
            //{
            //    IList<DbParameter> prms=new List<DbParameter>();
            //    Article article = new Article();
            //    ITransaction<Article> tran = new Transaction<Article> { Entity = article };
            //    tran.Insert();

            //    SqlCommand com = conn.CreateCommand();
            //    conn.Open();
            //    //for (int i = 1; i <= 100; i++)
            //    //{
            //    //    string num = i.ToString();
            //    //    article.Title = string.Format("Test{0} Title", num);
            //    //    article.Content = string.Format("Test{0} Content", num);
            //    //    com.CommandText = _parser.BuildSqlStatement(tran,prms);
            //    //    com.Parameters.AddRange(prms.ToArray());
            //    //    int count = com.ExecuteNonQuery();
            //    //    Console.WriteLine(count);
            //    //    prms.Clear();
            //    //}

            //   // tran.Delete()
            //   //     .Where(art => art.Id > 0 && art.Id < 10);
            //   // string deleteStatement = _parser.BuildSqlStatement(tran, prms);
            //   // com.CommandText = deleteStatement;
            //   // com.Parameters.AddRange(prms.ToArray());
            //   //int count = com.ExecuteNonQuery();

            //    //tran.Select(AggregateFunction.Count);
            //    //string selectStatement = _parser.BuildSqlStatement(tran, prms);
            //    //com.CommandText = selectStatement;
            //    //object result = com.ExecuteScalar();

            //    //tran.Select()
            //    //    .RightJoin<Category>((art, ctg) => art.CategoryId==ctg.Id )
            //    //    .Where(art => art.Id > 0)
            //    //    .Where<Category>(ctg=>ctg.Name=="C2")
            //    //    .OrderByDescend(art => art.Id)
            //    //    .ThenBy(art => art.Title);
                    


            //    //string selectStatement = _parser.BuildSqlStatement(tran, prms);


            //    //com.CommandText = selectStatement;
            //    //com.Parameters.AddRange(prms.ToArray());
            //    //var reader = com.ExecuteReader();
            //    //Console.WriteLine(selectStatement);
            //    //while (reader.Read())
            //    //{
            //    //    //Console.WriteLine(string.Format("{0},{1},{2}", reader[0], reader[1], reader[2]));
            //    //}
            //    //reader.Close();

            //    //article.Title = "updated Title";
            //    //article.Content = "updated Content";
            //    //tran.Update()
            //    //    .Where(art => art.Id >= 40 && art.Id < 50);
            //    //string updateStatement = _parser.BuildSqlStatement(tran, prms);
            //    //com.CommandText = updateStatement;
            //    //com.Parameters.AddRange(prms.ToArray());
            //    //int count = com.ExecuteNonQuery();
            //    //conn.Close();

            //    //Console.WriteLine(updateStatement);
            //    //foreach (var p in prms)
            //    //{
            //    //    Console.WriteLine("{0}={1}", p.ParameterName, p.Value);
            //    //}
            //    //Console.WriteLine(count);
            //}

            List<DbParameter> dbParams = new List<DbParameter>();
            Transaction<Article> tran=new Transaction<Article>();
            tran.Entity=new Article();
            tran.Select()
                .InnerJoin<Category>((art, cat) => art.CategoryId == cat.Id)
                .Where(art => art.Title.Contains("Test"))
                .Where<Category>(cat => cat.Name.StartsWith("T"))
                .OrderByDescend(art => art.Id)
                .ThenByDescend(art => art.Title);

            string statement = _parser.BuildSqlStatement(tran, dbParams);
            Console.WriteLine(statement);
            Console.ReadKey();
        }



        static void Test(Expression<Func<string, bool>> exp)
        {
            MethodCallExpression mcExp = exp.Body as MethodCallExpression;
            Console.WriteLine(mcExp.Method.Name);
            Console.WriteLine(mcExp.Object.NodeType);
        }
    }

    public class Article : ModelBase
    {
        [SelectField]
        public int Id { get; set; }

        [SelectField]
        [InsertField]
        [UpdateField]
        public string Title { get; set; }

        [SelectField]
        [InsertField]
        [UpdateField]
        public string Content { get; set; }

        [SelectField]
        [InsertField]
        [UpdateField]
        public int CategoryId { get; set; }
    }

    public class Category : ModelBase
    {
        [SelectField]
        public int Id { get; set; }

        [InsertField]
        [UpdateField]
        [SelectField]
        public string Name { get; set; }
    }

    public class TestModel : ModelBase
    {
        private RuntimeTypeHandle _typeHandle;
        private IFieldCollection _fields;

        public TestModel()
        {
            _typeHandle = this.GetType().TypeHandle;
            _fields = new FieldCollection();
            _fields.Fields.Add(new Field("TestModel", "Id"));
            _fields.Fields.Add(new Field("TestModel", "Name"));
            _fields.Fields.Add(new Field("TestModel", "Age"));
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public int Age { get; set; }

        public IFieldCollection Fields
        {
            get { return _fields; }
        }

        public RuntimeTypeHandle TypeHandle
        {
            get { return _typeHandle; }
        }
    }

    public class TestModel2 : IModel
    {
        private IFieldCollection _fields;

        public TestModel2()
        {
            _fields = new FieldCollection();
            _fields.Fields.Add(new Field("TestModel2", "Id"));
            _fields.Fields.Add(new Field("TestModel2", "RefId"));
        }

        public IFieldCollection Fields
        {
            get { return _fields; }
        }

        public int Id { get; set; }

        public int RefId { get; set; }

    }

    public class TestAttribute : Attribute
    {

    }

    public class TestClass
    {
        public string Name { get; set; }

        public DateTime Birthday { get; set; }

        public int Age { get; set; }
    }

    public class TestClass1
    {
        public string Name { get; set; }

        public DateTime Birthday { get; set; }

        public int Age { get; set; }
    }

    public class TestClass2
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime BirthDay { get; set; }

        public decimal Height { get; set; }

        public int Age { get; set; }

        public string Address { get; set; }

        public string Province { get; set; }

        public string City { get; set; }

        public string Area { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string OpenId { get; set; }

        private string TEst { get; set; }
    }

    public class TTT
    {
        public TTT()
        {
            Console.WriteLine("11");
        }
        public TTT(int a):this()
        {
            Console.WriteLine("22");
        }
    }

    public class NewModel : ModelBase
    {
        [SelectField]
        public int Id { get; set; }

        [SelectField]
        public int Name { get; set; }
    }
}
