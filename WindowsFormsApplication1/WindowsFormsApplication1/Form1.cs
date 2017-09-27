using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Linq.Expressions;
using System.Reflection;
using DynamicLinqExtensionMethodsNS;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<Order> lst = new List<Order>();
	        lst.Add(new Order("a",1));
	        lst.Add(new Order("b",4));
	        lst.Add(new Order("a",5));
	        lst.Add(new Order("a",5));
	        lst.Add(new Order("c",7));
	        lst.Add(new Order("d",9));
	        lst.Add(new Order("a",14));
	        lst.Add(new Order("a",2));
	        lst.Add(new Order("a",5));
            lst.Add(new Order("ahmed tarek hasan", 0));
            lst.Add(new Order("mohamed emad eldin", -1));

            OringGroup org = new OringGroup();

            AndingGroup ang1 = new AndingGroup();
            ang1.Add(new AndingItem(){ PropertyName = "Text", Value = "a", ComparisonOperator = OperatorType.Equal});
            ang1.Add(new AndingItem(){ PropertyName = "Index", Value = 5, ComparisonOperator = OperatorType.Equal});

            AndingGroup ang2 = new AndingGroup();
            ang2.Add(new AndingItem() { PropertyName = "Text", Value = "b", ComparisonOperator = OperatorType.Equal });
            ang2.Add(new AndingItem() { PropertyName = "Index", Value = 4, ComparisonOperator = OperatorType.Equal });

            AndingGroup ang3 = new AndingGroup();
            ang3.Add(new AndingItem() { PropertyName = "Index", Value = 7, ComparisonOperator = OperatorType.GreaterThanOrEqual });

            AndingGroup ang4 = new AndingGroup();
            ang4.Add(new AndingItem() { PropertyName = "Text", Value = "tarek", ComparisonOperator = OperatorType.RegexMatch });

            AndingGroup ang5 = new AndingGroup();
            ang5.Add(new AndingItem() { PropertyName = "Text", Value = "emad", ComparisonOperator = OperatorType.Contains });

            org.Add(ang1);
            org.Add(ang2);
            org.Add(ang3);
            org.Add(ang4);
            org.Add(ang5);

            var t = lst.AsQueryable().Where(org).ToList();
			//var t = lst.AsQueryable().WhereNot(org).ToList();
            dataGridView1.DataSource = t;
        }

    }

    public class Order
    {
        string text;
        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        int index;
        public int Index
        {
            get { return index; }
            set { index = value; }
        }

        public Order()
        {

        }

        public Order(string _text, int _index)
        {
            Text = _text;
            Index = _index;
        }
    }
}
