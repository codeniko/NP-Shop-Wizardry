using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NP_Shop_Wizardry
{
    public class Item
    {
        //lookup properties
        public string name;
        public string seller;
        public int price;
        public string objID;

        //Additional MyShop properties
        public int oldPrice;

        //Properties for restocker comparison
        public int profit;
        public int stock;
        public string hash;

        //Myshop constructor
        public Item(string name, string objID, string oldCost, string stock)
        {
            this.name = name;
            this.objID = objID;
            this.oldPrice = Convert.ToInt32(oldCost);
            this.price = this.oldPrice;
            this.stock = Convert.ToInt32(stock);
        }

        public Item(string name, string seller, string objID, int price)
        {
            this.name = name;
            this.seller = seller;
            this.objID = objID;
            this.price = price;
        }

        public Item(string objID, string name, int price, int profit) //only buy one
        {
            this.objID = objID;
            this.name = name;
            this.price = price;
            this.profit = profit;
        }

        public Item(string objID, string name, int price, int profit, string hash) //stock
        {
            this.objID = objID;
            this.name = name;
            this.price = price;
            this.profit = profit;
            this.hash = hash;
        }

        public Item(string name, int price)
        {
            this.name = name;
            this.price = price;
        }
    }
}
