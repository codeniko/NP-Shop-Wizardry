using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NP_Shop_Wizardry
{
    public class ListItem
    {
        public string name;
        public int price;

        public ListItem(string name, string price)
        {
            this.name = name;
            this.price = Convert.ToInt32(price);
        }

        public ListItem(string name, int price)
        {
            this.name = name;
            this.price = price;
        }
    }
}
