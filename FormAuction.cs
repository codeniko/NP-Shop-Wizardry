using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections;
using System.Threading;
using System.Net;


namespace NP_Shop_Wizardry
{
    public partial class FormAuction : Form
    {
        private ArrayList items = new ArrayList();

        public FormAuction()
        {
            InitializeComponent();
            readAuctionFile();
        }

        public delegate void updateTextDelegate(string t);
        private void log(string data)
        {
            if (dataBox.InvokeRequired)
            {
                this.Invoke(new updateTextDelegate(log), data);
                return;
            }
            dataBox.Text = data;
        }

        private void debug(string data)
        {
            TextWriter tw = new StreamWriter("debug.html");
            tw.Write(data);
            tw.Close();
        }

        private void readAuctionFile()
        {
            try
            {
                string[] lines = System.IO.File.ReadAllLines(@"auctionlist.txt");

                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Equals(""))
                        continue;
                    string[] s = Regex.Split(lines[i],"@=@");
                    items.Add(new AuctionItem(s[0],s[1],s[2]));
                }
            }
            catch (FileNotFoundException e)
            { return; }

            foreach (AuctionItem i in items)
                listBoxAuctions.Items.Add(i.name + " " + i.startingPrice + " " + i.minIncrement);
        }

        private void buttonRemoveSelectedAuctions_Click(object sender, EventArgs e)
        {
            if (listBoxAuctions.SelectedIndex == -1)
                return;

            items.RemoveAt(listBoxAuctions.SelectedIndex);
            listBoxAuctions.Items.RemoveAt(listBoxAuctions.SelectedIndex);
        }

        private void buttonAddAuction_Click(object sender, EventArgs e)
        {
            items.Add(new AuctionItem(textBox1.Text, textBox2.Text, textBox3.Text));
            listBoxAuctions.Items.Add(textBox1.Text + " " + textBox2.Text + " " + textBox3.Text);
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            TextWriter tw = new StreamWriter("auctionlist.txt");
            foreach (AuctionItem i in items)
                tw.WriteLine(i.name + "@=@" + i.startingPrice + "@=@" + i.minIncrement);
            tw.Close();
            log("Auction list saved!");
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(new ThreadStart(t_start));
            t.Start();
        }

        private void t_start()
        {
            cURL curl = new cURL("http://www.neopets.com/objects.phtml?type=inventory");
            string src = curl.post("http://www.neopets.com/objects.phtml?type=inventory");
            string r = @"openwin\(([0-9]+)\);""><... src=.http:/.images.neopets.com/"+Form1.REGEX_ITEM_NAME+"\" width=\"80\" height=\"80\" title=\""+Form1.REGEX_ITEM_NAME+"\" alt=\""+Form1.REGEX_ITEM_NAME+"\"  border=\"0\" class=\"neopointItem\"></A><BR>("+Form1.REGEX_ITEM_NAME+")";
            Regex reg = new Regex(r, RegexOptions.IgnoreCase);
            MatchCollection matches = reg.Matches(src);

            foreach (AuctionItem item in items)
            {
                foreach (Match m in matches)
                {
                    if (item.name.Equals(m.Groups[2].Value))
                    {
                        item.itemID = m.Groups[1].Value;
                    }
                }
            }

            Random random = new Random();
            foreach (AuctionItem i in items)
            {
                if (i.itemID == null)
                    continue;

                log("Creating auction for: "+i.name);

                curl.setReferer("http://www.neopets.com/iteminfo.phtml?obj_id="+i.itemID);
                
                curl.post("http://www.neopets.com/useobject.phtml", "obj_id="+i.itemID+"&action=auction");
                curl.post("http://www.neopets.com/add_auction.phtml", "start_price=" + i.startingPrice + "&min_increment=" + i.minIncrement + "&duration=1&obj_id="+i.itemID);
                Thread.Sleep(random.Next(0,3001));
            }

            log("Auctions are set");
        }


    }






    public class AuctionItem
    {
        public string name, startingPrice, minIncrement;
        public string itemID;

        public AuctionItem(string name, string sp, string minInc)
        {
            this.name = name;
            this.startingPrice = sp;
            this.minIncrement = minInc;
        }

        public override string ToString()
        {
            return this.name;
        }
    }
}
