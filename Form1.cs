using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading; //error at line 180

namespace NP_Shop_Wizardry
{
    public partial class Form1 : Form
    {
        private Item itemLookedUp;
        private const int SHOP_WIZ_INDEX = 23000;
        System.Collections.ArrayList myThreads = new System.Collections.ArrayList();
        System.Threading.Timer[] myTimers = new System.Threading.Timer[2];
        private ListItem[] listItems;
        public static string REGEX_ITEM_NAME = "[a-zA-Z0-9*&!#+:/™�®.,? _()-]+";
        



        public Form1()
        {
            InitializeComponent();
            readList();
        }

        private void readList()
        {
            try
            {
                string[] lines = System.IO.File.ReadAllLines(@"mainlist.txt");
                if (lines[lines.Length-1].Equals(""))
                    this.listItems = new ListItem[lines.Length - 1];
                else
                    this.listItems = new ListItem[lines.Length];

                for (int i = 0; i < listItems.Length; i++ )
                {
                    string[] s = Regex.Split(lines[i],"@=@");
                    listItems[i] = new ListItem(s[0],s[1]);
                }
                log("Loaded "+listItems.Length+" items");
            }
            catch (FileNotFoundException e)
            { return; }
        }

        private bool checkConnection()
        {
            if (cURL.cookieJar == null)
            {
                MessageBox.Show("You must login first!");
                return false;
            }
            return true;
        }

        private void log(string data)
        {
            if (dataBox.InvokeRequired)
            {
                this.Invoke(new updateTextDelegate(log),data);
                return;
            }
            if (dataBox.TextLength > 20000)
                dataBox.Clear();

            dataBox.AppendText(data+"\n");
            dataBox.ScrollToCaret();
        }
        private void log2(string data)
        {
            if (dataBox2.InvokeRequired)
            {
                this.Invoke(new updateTextDelegate(log2), data);
                return;
            }
            if (dataBox2.TextLength > 10000)
                dataBox2.Clear();

            dataBox2.AppendText(data + "\n");
            dataBox2.ScrollToCaret();
        }

        private void debug(string data, bool found)
        {
            if ((DebuggerItemFound.Checked == true && found) || (DebuggerItemNotFound.Checked == true && !found))
            {
                TextWriter tw = new StreamWriter("debug\\debug " + DateTime.Now.ToString("s").Replace(":", " ") + ".html");
                tw.Write(data);
                tw.Close();
            }
        }

        public delegate void updateTextDelegate(string t);
        private void setState(string state)
        {
            if (labelState.InvokeRequired)
            {
                this.Invoke(new updateTextDelegate(setState),state);
            } else
            labelState.Text = state;
        }

        public delegate void updateTimerDelegate(int t, string s);
        private void updateTimer(int index, string state)
        {
            if (listBoxTimers.InvokeRequired)
            {
                this.Invoke(new updateTimerDelegate(updateTimer), index, state);
            }
            else
                listBoxTimers.Items[index] = state;
        }
        public delegate void updateTimerObjDelegate(Object s);
        private void updateTimer(Object state)
        {
            if (listBoxTimers.InvokeRequired)
            {
                this.Invoke(new updateTimerObjDelegate(updateTimer), state);
            }
            else
            {
                timerObj tObj = (timerObj)state;
                listBoxTimers.Items[tObj.shopIndex] = "Waiting " + tObj.timeleft + " seconds";
                tObj.timeleft -= 1;
            }
        }


        private bool checkLoggedIn()
        {
            cURL curl = new cURL("http://www.neopets.com/objects.phtml?type=inventory");
            string response = curl.post("http://www.neopets.com/objects.phtml?type=inventory");
            string needle = "href=\"/logout.phtml\"><b>Logout";
            if (response == null) //timed out
            {
                log("Connection Time-out");
                return checkLoggedIn();
            }
            if (Regex.Match(response, needle, RegexOptions.IgnoreCase).Success)
                return true;
            else
            {
                return false;
            }
        }

        private Item lookupItem(string item, int numSearches, bool changeState)
        {
            string data = String.Format("type=process_wizard&feedset=0&shopwizard={0}&table=shop&criteria=exact&min_price=0&max_price=99999", item);
            cURL curl = new cURL("http://www.neopets.com/market.phtml?type=wizard");

            int lowest = 100000;
            string seller = "";
            string objID = "";
            Regex reg = new Regex("browseshop.phtml.owner=([a-zA-Z0-9_-]+)&buy_obj_info_id=([0-9]+)&buy_cost_neopoints=([0-9]+)", RegexOptions.IgnoreCase);
            for (int i = 1; i <= numSearches; i++)
            {
                if (changeState)
                    setState("Searching " + i + "/"+numSearches);
                curl.setReferer("http://www.neopets.com/market.phtml?type=wizard");
                string src = curl.post("http://www.neopets.com/market.phtml", data);
                if (src == null) //timed out
                {
                    log("Connection Time-out");
                    continue;
                }

                //int myI = Regex.Match(src, "<a href=\"/browseshop.phtml", RegexOptions.IgnoreCase).Index;

                //string match = cURL.textBetween(src, "<a href=\"/browseshop.phtml?owner=", "\"><b>", SHOP_WIZ_INDEX); //bike1331&buy_obj_info_id=12756&buy_cost_neopoints=1889
                Match match = reg.Match(src, SHOP_WIZ_INDEX);
                if (!match.Success)
                {
                    log(item + " was not found!");
                    continue;
                }

                //string[] splite = match.Split('=');

                int price = Convert.ToInt32(match.Groups[3].Value);
                log("Found "+item+" for: "+price);
                if (i == 0 || price < lowest)
                {
                    lowest = price;
                    seller = match.Groups[1].Value;
                    objID = match.Groups[2].Value;
                    continue;
                }
            }
            if (lowest != 100000)
                return new Item(item, seller, objID, lowest);
            else
                return null;
        }

        private bool shopNonHaggle(string urlToShop, string referer, string processURL, Regex reg) //returns true if something in shop, false if nothing in shop
        {
            cURL curl = new cURL(referer, 1000);
            string src = curl.post(urlToShop);
            if (src == null) //timed out
            {
                log("Connection Time-out");
                return shopNonHaggle(urlToShop, referer, processURL, reg);
            }
            MatchCollection matches = reg.Matches(src);
            
            if (matches.Count == 0)
            {
                log("[" + DateTime.Now.ToString("T") + "] Checked Igloo Garage [0]");
                debug(src, false);
                //check if logged in
                if (!Regex.Match(src, "href=\"/logout.phtml\"><b>Logout", RegexOptions.IgnoreCase).Success)
                {
                    log("Program was logged out, logging back in.");
                    buttonLogin_Click(null, null);
                    return true; //prevent uneccesary extra delay in thread
                }
                if (Regex.Match(src, "you cannot get any more items from the Igloo Garage Sale today").Success)
                {
                    updateTimer(0, "Maxed");
                    log2("Igloo Shop is maxed out.");
                    Thread.Sleep(60000*60*24);
                }
                return false; //no items found in shop
            }

            Item[] profitableItems = new Item[matches.Count];
            int count = 0;
            foreach (Match match in matches)
            {
                int listIndex;
                if ((listIndex = binarySearch(match.Groups[2].Value)) != -1)
                {
                    int shopPrice = Convert.ToInt32(match.Groups[3].Value);
                    int profit = listItems[listIndex].price - shopPrice;
                    if (profit >= Convert.ToInt32(textBoxMinProfit.Text))
                    {
                        profitableItems[count++] = new Item(match.Groups[1].Value, match.Groups[2].Value, shopPrice, profit);
                    }
                }
            }

            if (count == 0) //no profitable items
            {
                log("[" + DateTime.Now.ToString("T") + "] Checked Igloo Garage ["+matches.Count+" NP]");
                debug(src, true);
                return true;
            }

            profitableItems = sortByProfit(profitableItems, count);
            foreach(Item item in profitableItems)
            {
                curl.post(processURL+"?obj_info_id="+item.objID);
                log2("["+DateTime.Now.ToString("t")+"] "+item.name+" ("+item.price+") profit="+item.profit);
            }
            log("[" + DateTime.Now.ToString("T") + "] Checked Igloo Garage [" + matches.Count + "]");
            debug(src, true);
            return true;
        }







        private void buttonLogin_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(textBoxUsername.Text) || String.IsNullOrEmpty(textBoxPassword.Text))
            {
                MessageBox.Show("Enter your login credentials.");
                return;
            }

            string postData = String.Format("destination=%2Findex.phtml&username={0}&password={1}", textBoxUsername.Text, textBoxPassword.Text);
            cURL curl = new cURL("http://www.neopets.com/index.phtml");

            string response = curl.post("http://www.neopets.com/login.phtml", postData);
            string needle = "href=\"/logout.phtml\"><b>Logout";

            if (Regex.Match(response, needle, RegexOptions.IgnoreCase).Success)
            {
                setState("Logged In!");
                log("Program has successfully logged in.");
            }
            else
            {
                MessageBox.Show("Invalid login credentials!");
                curl = null;
            }
        }

        private void buttonLookup_Click(object sender, EventArgs e)
        {
            if (!checkConnection())
                return;

            Thread t = new Thread(new ThreadStart(this.tLookup));
            t.Start();
        }

        private void buttonUpdateShop_Click(object sender, EventArgs e)
        {
            if (!checkConnection())
                return;

            Thread t2 = new Thread(new ThreadStart(this.tUpdateMyShop));
            myThreads.Add(t2);
            t2.Start();
        }

        private void buttonCreateList_Click(object sender, EventArgs e)
        {
            Thread t3 = new Thread(new ThreadStart(this.tCreateList));
            myThreads.Add(t3);
            t3.Start();
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            if (!checkConnection() || !checkLoggedIn())
                return;

            buttonStart.Enabled = false;
            setState("Running");
            
            foreach (Object checkedItem in checkedListBoxShops.CheckedItems)
            {
                Thread t, t2;

                switch (checkedItem.ToString())
                {
                    case "Igloo Garage" :
                        t = new Thread(new ThreadStart(this.tShop_Igloo_Garage));
                        break;
                    case "Abandoned Attic" :
                        t = new Thread(new ThreadStart(this.tShop_Abandoned_Attic));
                        //t2 = new Thread(new ThreadStart(this.tShop_Abandoned_Attic));
                        //myThreads.Add(t2);
                        //t2.Start();
                        break;
                    default:
                        continue;
                }
                myThreads.Add(t);
                t.Start();
            }
        }

        private void buttonStop_Click(object sender, EventArgs e)
        { 
            buttonStart.Enabled = true;
            setState("Stopped");
            foreach (Thread t in myThreads)
                t.Abort();
            foreach (System.Threading.Timer t in myTimers)
                if (t != null)
                    t.Dispose();
            for (int i = 0; i < listBoxTimers.Items.Count; i++ )
                listBoxTimers.Items[i] = "Stopped";
            myThreads.Clear();
        }

        private void toolStripShowCookies_Click(object sender, EventArgs e)
        {
            if (cURL.cookieJar == null)
            {
                MessageBox.Show("No cookies to display");
                return;
            }
            CookieCollection cookies = cURL.cookieJar.GetCookies(new Uri("http://www.neopets.com"));
            if(cookies.Count == 0) 
            {
				MessageBox.Show("No cookies to display");
				return;
			}
            string data = "";
            for (int j = 0; j < cookies.Count; j++)
                data += cookies[j].ToString() + "\n";

            CookiesForm cookiesForm = new CookiesForm(data);
            cookiesForm.Show();
        }

        private void toolStripMenuItemCheckIfLoggedIn_Click(object sender, EventArgs e)
        {
            if (cURL.cookieJar == null || !checkLoggedIn())
            {
                log("Program is currently logged out.");
            }
            else
                log("Program is currently logged in.");
        }

        private void DebuggerItemFound_Click(object sender, EventArgs e)
        {
            DebuggerItemFound.Checked = !DebuggerItemFound.Checked;
        }

        private void DebuggerItemNotFound_Click(object sender, EventArgs e)
        {
            DebuggerItemNotFound.Checked = !DebuggerItemNotFound.Checked;
        }

        private void buttonClearDatabox_Click(object sender, EventArgs e)
        {
            dataBox.Clear();
            dataBox2.Clear();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox about = new AboutBox();
            about.Show();
        }

        private void buyToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void sellToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormAuction formAuction = new FormAuction();
            formAuction.Show();
        }

    }
}
