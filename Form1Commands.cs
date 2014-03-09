using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;
using System.Net;

namespace NP_Shop_Wizardry
{
    public partial class Form1 : Form
    {
        private void tLookup()
        {
            this.itemLookedUp = lookupItem(textBoxItemLookup.Text, Convert.ToInt32(nUDNumSearches.Value), true);
            if (this.itemLookedUp != null)
                log("The lowest price for " + itemLookedUp.name + " is: " + itemLookedUp.price + ". Buy it at \nhttp://www.neopets.com/browseshop.phtml?owner=" + itemLookedUp.seller + "&buy_obj_info_id=" + itemLookedUp.objID + "&buy_cost_neopoints=" + itemLookedUp.price);
            else
                log(textBoxItemLookup.Text + " was not found in any shops!");
            setState("Search Complete");
        }

        private void tUpdateMyShop()
        {
            cURL curl = new cURL("http://www.neopets.com/objects.phtml?type=inventory");
            string src = curl.post("http://www.neopets.com/market.phtml?type=your"); 
            System.Collections.ArrayList myItems = new System.Collections.ArrayList();

            Regex reg = new Regex(
               "(" + REGEX_ITEM_NAME + ")</b></td><td align=center width=[0-9]+><img src='" + REGEX_ITEM_NAME + "' height=[0-9]+ width=[0-9]+></td><td width=[0-9]+ bgcolor='#[0-9a-z]+' align=center><b>([0-9]+)</b></td><td bgcolor='#[0-9a-z]+'><b>" + REGEX_ITEM_NAME + "</b></td><input type='hidden'  name='obj_id_[0-9]+' value='([0-9]+)'><input type='hidden' name='oldcost_[0-9]+'  value='([0-9]+)",
               RegexOptions.IgnoreCase
                );
            MatchCollection matches = reg.Matches(src);

            Regex reg2 = new Regex("Items Stocked : <b>([0-9]+)</b>", RegexOptions.IgnoreCase);
            Match match2 = reg2.Match(src);

            int stockCount = 0;
            foreach(Match match in matches)
            {
                myItems.Add(new Item(match.Groups[1].Value, match.Groups[3].Value, match.Groups[4].Value, match.Groups[2].Value));
                stockCount += Convert.ToInt32(match.Groups[2].Value);
            }

            if (stockCount != Convert.ToInt32(match2.Groups[1].Value))
            {
                MessageBox.Show("SERIOUS ERROR: # shop items != # items parsed");
                debug(src, false);
                return;
            }

            for (int i = 0; i < matches.Count; i++)
            {
                setState("Searching item "+(i+1)+"/"+myItems.Count);
                Item search = lookupItem(((Item)myItems[i]).name, Convert.ToInt32(nUDNumSearchesMyShop.Value), false);

                if (search == null)
                {
                    if (((Item)myItems[i]).price == 0)
                        ((Item)myItems[i]).price = 99999;
                    continue;
                }

                if (search.seller.ToLower().Equals(textBoxUsername.Text.ToLower()) || (((Item)myItems[i]).price < search.price && ((Item)myItems[i]).price > 0))
                    continue; //this is my own guy, price is obviously already lowest or my price is lower than lowest price found

                if (search.price > 11000)
                    ((Item)myItems[i]).price = search.price - 500;
                else if (search.price > 400)
                    ((Item)myItems[i]).price = search.price - 100;
                else if (search.price > 30)
                    ((Item)myItems[i]).price = search.price - 20;
                else
                    ((Item)myItems[i]).price = 1;
            }

            string data = "type=update_prices&order_by=&view=";
            for (int i = 0; i < myItems.Count; i++)
            {
                int k = i + 1;
                data += "&obj_id_" + k + "=" + ((Item)myItems[i]).objID + "&oldcost_" + k + "=" + ((Item)myItems[i]).oldPrice + "&cost_" + k + "=" + ((Item)myItems[i]).price + "&back_to_inv[" + ((Item)myItems[i]).objID + "]=0";
                //log((i + 1) + " " + ((Item)myItems[i]).name + " " + ((Item)myItems[i]).objID + " " + ((Item)myItems[i]).oldPrice + " " + ((Item)myItems[i]).price);
            }
            data += "&lim=30&obj_name=&submit=Update";

            for (int i = 0; i < myItems.Count; i++)
            {
                log2((i + 1) + " " + ((Item)myItems[i]).name + " " + ((Item)myItems[i]).oldPrice + "->" + ((Item)myItems[i]).price);
            }
            curl.post("http://www.neopets.com/process_market.phtml", data);
            debug(src, true);
        }

        private void tCreateList()
        {
            cURL curl = new cURL("http://items.jellyneo.net");
            string src = curl.post("http://items.jellyneo.net/index.php?go=show_items&name=&name_type=partial&desc=&desc_type=partial&pic=&pic_type=partial&notes=&notes_type=partial&op_cat=0&cat=0&r1=&r2=&op_spec=0&specialcat=0&status=0&sortby=price&sortby_type=desc&numitems=75&p1="+textBoxListMinCost.Text+"&p2="+textBoxListMaxCost.Text+"&start=0&ncoff=1");
            System.Collections.ArrayList newListItems = new System.Collections.ArrayList();

            Regex reg = new Regex(" Page <b>1</b> of <b>([0-9]+)</b> ", RegexOptions.IgnoreCase);
            Match match = reg.Match(src);
            if (!match.Success)
                return;
            int lastPage = Convert.ToInt32(match.Groups[1].Value);
            log2(match.Groups[1].Value+" "+( (lastPage*75)-75 ));

            reg = new Regex("showitem=[0-9]+\">(" + REGEX_ITEM_NAME + ")</a><br><b>([0-9,]+) NP", RegexOptions.Compiled);

            //look through the pages of site and store all items
            for (int i = 1; i <= lastPage; i++)
            {
                setState("Processing page: "+i+"/"+lastPage);
                src = curl.post("http://items.jellyneo.net/index.php?go=show_items&name=&name_type=partial&desc=&desc_type=partial&pic=&pic_type=partial&notes=&notes_type=partial&op_cat=0&cat=0&r1=&r2=&op_spec=0&specialcat=0&status=0&sortby=price&sortby_type=desc&numitems=75&p1=" + textBoxListMinCost.Text + "&p2=" + textBoxListMaxCost.Text + "&start=" + ((i * 75) - 75) + "&ncoff=1");
                MatchCollection matches = reg.Matches(src);
                foreach (Match m in matches)
                {
                    newListItems.Add(new ListItem(m.Groups[1].Value, m.Groups[2].Value.Replace(",", "")));
                }
                if (matches.Count != 75 && i != lastPage)
                    log("Page "+i+" had an error (inform programmer)");
            }

            //following code is an addition to get artificats of rarity 200-250
            src = curl.post("http://items.jellyneo.net/index.php?go=show_items&name=&name_type=partial&desc=&desc_type=partial&pic=&pic_type=partial&notes=&notes_type=partial&op_cat=0&cat=0&r1=181&r2=300&op_spec=0&specialcat=0&status=0&sortby=rarity&sortby_type=desc&numitems=75&p1=&p2=&start=0&ncoff=1");
            System.Collections.ArrayList artifactList = new System.Collections.ArrayList();

            reg = new Regex(" Page <b>1</b> of <b>([0-9]+)</b> ", RegexOptions.IgnoreCase);
            match = reg.Match(src);
            if (!match.Success)
                return;
            lastPage = Convert.ToInt32(match.Groups[1].Value);
            log2(match.Groups[1].Value + " " + ((lastPage * 75) - 75));

            reg = new Regex("showitem=[0-9]+\">(" + REGEX_ITEM_NAME + ")</a>", RegexOptions.Compiled);

            //look through the pages of site and store all items
            for (int i = 1; i <= lastPage; i++)
            {
                setState("Artifact page: " + i + "/" + lastPage);
                src = curl.post("http://items.jellyneo.net/index.php?go=show_items&name=&name_type=partial&desc=&desc_type=partial&pic=&pic_type=partial&notes=&notes_type=partial&op_cat=0&cat=0&r1=181&r2=300&op_spec=0&specialcat=0&status=0&sortby=rarity&sortby_type=desc&numitems=75&p1=&p2=&start=" + ((i * 75) - 75) + "&ncoff=1");
                MatchCollection matches = reg.Matches(src);
                foreach (Match m in matches)
                {
                    artifactList.Add(new ListItem(m.Groups[1].Value, "90000000"));
                }
                if (matches.Count != 75 && i != lastPage)
                    log("Page " + i + " had an error (inform programmer)");
            }



            //combine lists, just old and new.... NOT including artifacts
            setState("Updating List...");
            System.Collections.ArrayList itemsToBeAdded = new System.Collections.ArrayList();
            foreach (ListItem i in newListItems)
            {
                int iIndex = binarySearch(i.name);
                if (iIndex == -1)
                    itemsToBeAdded.Add(new ListItem(i.name, i.price));
                else
                    listItems[iIndex].price = i.price;
                if (i.name.Equals("Tall Dark Vase")) log2("["+iIndex+"]"+i.name+" at "+i.price+","+listItems[iIndex].price) ;
            }

            setState("Sorting list...");
            ListItem[] newListItemsArray = new ListItem[listItems.Length + itemsToBeAdded.Count];
            int a = 0;
            while (a < listItems.Length)
            {
                newListItemsArray[a] = listItems[a];
                a++;
            }
            for (int i = 0; i < itemsToBeAdded.Count; i++, a++)
            {
                newListItemsArray[a] = (ListItem)itemsToBeAdded[i];
            }
            mergesort(newListItemsArray);
            this.listItems = newListItemsArray; //update main item list to this new one so we can sort artifacts correctly.



            setState("Sorting list with artifacts...");
            for (int i = 0; i < artifactList.Count; i++)
            {
                ListItem artifact = (ListItem)artifactList[i];
                int iIndex = binarySearch(artifact.name);
                if (iIndex != -1)
                {
                    artifactList.RemoveAt(i);
                    i--;
                }
            }
            ListItem[] finalList = new ListItem[newListItemsArray.Length + artifactList.Count];
            a = 0;
            while (a < newListItemsArray.Length)
            {
                finalList[a] = newListItemsArray[a];
                a++;
            }
            for (int i = 0; i < artifactList.Count; i++, a++)
            {
                ListItem artifact = (ListItem)artifactList[i];
                int iIndex = binarySearch(artifact.name);
                if (iIndex == -1)
                    finalList[a] = artifact;
            }
            mergesort(finalList);

            TextWriter tw = new StreamWriter("mainlist.txt");
            
            int count = 1;
            foreach (ListItem listItem in finalList)
            {
                setState("Writing: " + (count++) + "/" + finalList.Length);
                tw.WriteLine(listItem.name + "@=@" + listItem.price);
            }
            setState("Writing complete");
            tw.Close();

            log2("Created mainlist with " + finalList.Length + " items");
            readList();
        }






        private void tShop_Igloo_Garage()
        {
            int iglooIndex = 0;
            Regex reg = new Regex("process_igloo.phtml.obj_info_id=([0-9]+)['\"] onclick=['\"]if . !confirm.'Are you sure you wish to buy (" + REGEX_ITEM_NAME + ") at ([0-9,]+) NP", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            Random random = new Random();
            int delay = Convert.ToInt32(textBoxRefreshDelay.Text);
            int randomizer = Convert.ToInt32(textBoxRandomizer.Text);
            bool hasItems = true;
            while (true)
            {
                updateTimer(iglooIndex, "Refreshing");
                bool currentlyHasItems = shopNonHaggle("http://www.neopets.com/winter/igloo2.phtml", "http://www.neopets.com/winter/igloo.phtml", "http://www.neopets.com/winter/process_igloo.phtml", reg);
                int randomNumber = random.Next(-1 * (randomizer), randomizer + 1);

                if (checkBoxAddIglooDelay.Checked && !hasItems && currentlyHasItems) //then it guaranteed just updated, so have a much bigger delay
                {
                    hasItems = true;
                    randomNumber += 120000; //extra 2 minutes
                }
                else if (checkBoxAddIglooDelay.Checked && hasItems && !currentlyHasItems)
                    hasItems = false;

                updateTimer(iglooIndex, "Waiting "+ (delay+randomNumber) +"ms");
                Thread.Sleep(delay+randomNumber);
            }
        }

        private void tShop_Abandoned_Attic()
        {
            
            int atticIndex = 1; 
            //Regex reg = new Regex("<input type=['\"]?hidden['\"]? name=['\"]?oii['\"]? value=['\"]?([0-9]+)['\"]?>[ \n\r\t]+<input type=['\"]?hidden['\"]? name=['\"]?neopets['\"]? value=['\"]?([a-zA-Z0-9]+)['\"]?>.+?[ \n\r\t]+.+?[ \n\r\t]+<b>(" + REGEX_ITEM_NAME + ")</b><br>([0-9]+) in stock<br>Cost: ([0-9,]+) NP<br><br></form>", RegexOptions.IgnoreCase);
            Regex reg = new Regex(@"<input type='hidden' name='oii' value='([0-9]+)'>[ \n\r\t]{0,}<input type='hidden' name='neopets' value='([0-9]+)'>[ \n\r\t]{0,}.+[ \n\r\t]{0,}<b>(" + REGEX_ITEM_NAME + ")</b><br>[0-9]+ in stock<br>Cost: ([0-9,]+) NP<br><br></form>", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Multiline);
            Random random = new Random();
            //int delay = 500;
            int randomizer = 1000;
            cURL curl = new cURL("http://www.neopets.com/halloween/garage.phtml", 1000);
            cURL curlPrivate = new cURL("http://www.sunnyneo.com/attictimer/getTimes.php", true);
            long nextRestock;

            // Create an inferred delegate that invokes methods for the timer.
            System.Threading.TimerCallback tcb = new System.Threading.TimerCallback(updateTimer);

            while (true)
            {
                string psrc = curlPrivate.post("http://www.sunnyneo.com/attictimer/getTimes.php");
                int divIndex = psrc.IndexOf('|');
                if (divIndex == -1)
                {
                    MessageBox.Show("Unable to connect to the website that shows next Attic Restock, Trying again in 2 minutes.");
                    updateTimer(atticIndex, "Waiting 2 mins");
                    Thread.Sleep(120000);
                    continue;
                }
                psrc = psrc.Substring(divIndex + 1);
                nextRestock = Convert.ToInt64(psrc);
                long timestamp = getTimestamp();

                int timeleft = (int)(nextRestock - timestamp); //seconds
                if (timeleft > 6)
                {
                    timerObj tObj = new timerObj(atticIndex,timeleft-5);
                    myTimers[atticIndex] = new System.Threading.Timer(tcb, tObj, 0, 998);
                    //updateTimer(atticIndex, "Waiting "+(timeleft)+" seconds");
                    Thread.Sleep(((timeleft- 5) * 1000));
                    myTimers[atticIndex].Dispose();
                }
                
                updateTimer(atticIndex, "Refreshing");

                while (timestamp < nextRestock + 9)
                {
                    timestamp = getTimestamp();
                    
                    //System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
                    
                    string src = curl.post("http://www.neopets.com/halloween/garage.phtml");
                    if (src == null) //timed out
                    {
                        log("Attic Connection Time-out");
                        continue;
                    }
                    MatchCollection matches = reg.Matches(src);

                    //if there are items, check each if profitable and buy
                    if (matches.Count > 0)
                    {
                        Item[] profitableItems = new Item[matches.Count];
                        int count = 0;

                        //buy draig eggs and morphing potions ASAP
                        foreach (Match match in matches)
                        {
                            int draikIndex = 0;
                            if ((draikIndex = match.Groups[3].Value.IndexOf("Draik ")) != -1)
                            {
                                if ((match.Groups[3].Value[draikIndex + 6] == 'M' && match.Groups[3].Value[draikIndex + 7] == 'o') ||
                                      (match.Groups[3].Value[draikIndex + 6] == 'E' && match.Groups[3].Value[draikIndex + 7] == 'g') ||
                                      (match.Groups[3].Value[draikIndex + 6] == 'T' && match.Groups[3].Value[draikIndex + 7] == 'r'))
                                {
                                    //one of these exists, BUY!
                                    curl.post("http://www.neopets.com/halloween/garage.phtml", "purchase=1&oii=" + match.Groups[1].Value + "&neopets=" + match.Groups[2].Value);
                                    log2("Found " + match.Groups[3].Value);
                                }
                            }
                        }

                        foreach (Match match in matches)
                        {
                            int listIndex;
                            if ((listIndex = binarySearch(match.Groups[3].Value)) != -1)
                            {
                                int shopPrice = Convert.ToInt32(match.Groups[4].Value.Replace(",", ""));
                                int profit = listItems[listIndex].price - shopPrice;

                                if (profit > 2900000)
                                {
                                    //instant buy
                                    curl.post("http://www.neopets.com/halloween/garage.phtml", "purchase=1&oii=" + match.Groups[1].Value + "&neopets=" + match.Groups[2].Value);
                                    log2("[" + DateTime.Now.ToString("T") + "] " + match.Groups[3].Value + " (" + shopPrice + ") profit=" + profit);
                                }
                                else if (profit >= Convert.ToInt32(textBoxMinAtticProfit.Text))
                                {
                                    profitableItems[count++] = new Item(match.Groups[1].Value, match.Groups[3].Value, shopPrice, profit,  match.Groups[2].Value);
                                }
                            }
                        }

                        if (count == 0) //no profitable items
                        {
                            continue; // delete this line and uncomment the rest of non randomizers
                            //log("[" + DateTime.Now.ToString("T") + "] Checked AA [" + matches.Count + "]["+sw.ElapsedMilliseconds+"]");
                           // debug(src, true);
                            // int randomNumber = random.Next(-1 * (randomizer), randomizer + 1);
                            //int randomNumber = random.Next(0, randomizer + 1);
                            //Thread.Sleep(randomNumber);
                           // continue;
                        }


                        profitableItems = sortByProfit(profitableItems, count);
                        foreach (Item item in profitableItems)
                        {
                            curl.post("http://www.neopets.com/halloween/garage.phtml", "purchase=1&oii=" + item.objID + "&neopets=" + item.hash);
                            log2("[" + DateTime.Now.ToString("T") + "] " + item.name + " (" + item.price + ") profit=" + item.profit);
                        }
                        //log("[" + DateTime.Now.ToString("T") + "] Checked AA [" + matches.Count + "][" + sw.ElapsedMilliseconds + "]");
                        debug(src, true);
                        if (matches.Count > 10 && matches.Count < 18)
                            break;
                    }
                  /*  else
                    {
                        log("[" + DateTime.Now.ToString("T") + "] Checked AA [0][" + sw.ElapsedMilliseconds + "]");
                        debug(src, false);
                    }*/
                    
                    //int randomNumber2 = random.Next(-1 * (randomizer), randomizer + 1);
                   // int randomNumber2 = random.Next(100, randomizer + 1);
                    //Thread.Sleep(randomNumber2);
                }
                Thread.Sleep(3000);

                //updateTimer(atticIndex, "Waiting " + (delay + randomNumber) + "ms");
                
            }
        }

        private void tShop_Abandoned_Attic2()
        {
            //Regex reg = new Regex("<input type=['\"]?hidden['\"]? name=['\"]?oii['\"]? value=['\"]?([0-9]+)['\"]?>[ \n\r\t]+<input type=['\"]?hidden['\"]? name=['\"]?neopets['\"]? value=['\"]?([a-zA-Z0-9]+)['\"]?>.+?[ \n\r\t]+.+?[ \n\r\t]+<b>(" + REGEX_ITEM_NAME + ")</b><br>([0-9]+) in stock<br>Cost: ([0-9,]+) NP<br><br></form>", RegexOptions.IgnoreCase);
            Regex reg = new Regex(@"<input type='hidden' name='oii' value='([0-9]+)'>[ \n\r\t]{0,}<input type='hidden' name='neopets' value='([0-9]+)'>[ \n\r\t]{0,}.+[ \n\r\t]{0,}<b>(" + REGEX_ITEM_NAME + ")</b><br>[0-9]+ in stock<br>Cost: ([0-9,]+) NP<br><br></form>", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Multiline);
            Random random = new Random();
            //int delay = 500;
            int randomizer = 1000;
            cURL curl = new cURL("http://www.neopets.com/halloween/garage.phtml", 1000);
            cURL curlPrivate = new cURL("http://www.sunnyneo.com/attictimer/getTimes.php", true);
            long nextRestock;

            // Create an inferred delegate that invokes methods for the timer.
            System.Threading.TimerCallback tcb = new System.Threading.TimerCallback(updateTimer);

            while (true)
            {
                string psrc = curlPrivate.post("http://www.sunnyneo.com/attictimer/getTimes.php");
                int divIndex = psrc.IndexOf('|');
                if (divIndex == -1)
                {
                    MessageBox.Show("Unable to connect to the website that shows next Attic Restock, Trying again in 2 minutes.");
                    Thread.Sleep(120000);
                    continue;
                }
                psrc = psrc.Substring(divIndex + 1);
                nextRestock = Convert.ToInt64(psrc);
                long timestamp = getTimestamp();

                int timeleft = (int)(nextRestock - timestamp); //seconds
                if (timeleft > 6)
                {
                    Thread.Sleep(((timeleft - 5) * 1000)+100);
                }
                while (timestamp < nextRestock + 9)
                {
                    timestamp = getTimestamp();
                    //System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

                    string src = curl.post("http://www.neopets.com/halloween/garage.phtml");
                    if (src == null) //timed out
                    {
                        log("Attic Connection Time-out");
                        continue;
                    }
                    MatchCollection matches = reg.Matches(src);

                    //if there are items, check each if profitable and buy
                    if (matches.Count > 0)
                    {
                        Item[] profitableItems = new Item[matches.Count];
                        int count = 0;

                        //buy draig eggs and morphing potions ASAP
                        foreach (Match match in matches)
                        {
                            int draikIndex = 0;
                            if ((draikIndex = match.Groups[3].Value.IndexOf("Draik ")) != -1)
                            {
                                if ((match.Groups[3].Value[draikIndex + 6] == 'M' && match.Groups[3].Value[draikIndex + 7] == 'o') ||
                                      (match.Groups[3].Value[draikIndex + 6] == 'E' && match.Groups[3].Value[draikIndex + 7] == 'g') ||
                                      (match.Groups[3].Value[draikIndex + 6] == 'T' && match.Groups[3].Value[draikIndex + 7] == 'r'))
                                {
                                    //one of these exists, BUY!
                                    curl.post("http://www.neopets.com/halloween/garage.phtml", "purchase=1&oii=" + match.Groups[1].Value + "&neopets=" + match.Groups[2].Value);
                                    log2("Found " + match.Groups[3].Value);
                                }
                            }
                        }

                        foreach (Match match in matches)
                        {
                            int listIndex;
                            if ((listIndex = binarySearch(match.Groups[3].Value)) != -1)
                            {
                                int shopPrice = Convert.ToInt32(match.Groups[4].Value.Replace(",", ""));
                                int profit = listItems[listIndex].price - shopPrice;

                                if (profit > 2900000)
                                {
                                    //instant buy
                                    curl.post("http://www.neopets.com/halloween/garage.phtml", "purchase=1&oii=" + match.Groups[1].Value + "&neopets=" + match.Groups[2].Value);
                                    log2("[" + DateTime.Now.ToString("T") + "] " + match.Groups[3].Value + " (" + shopPrice + ") profit=" + profit);
                                }
                                else if (profit >= Convert.ToInt32(textBoxMinAtticProfit.Text))
                                {
                                    profitableItems[count++] = new Item(match.Groups[1].Value, match.Groups[3].Value, shopPrice, profit, match.Groups[2].Value);
                                }
                            }
                        }

                        if (count == 0) //no profitable items
                        {
                            continue; //delete this line and uncomment everything else besides randomizers
                            //log("[" + DateTime.Now.ToString("T") + "] Checked AA [" + matches.Count + "][" + sw.ElapsedMilliseconds + "]");
                            //debug(src, true);
                            // int randomNumber = random.Next(-1 * (randomizer), randomizer + 1);
                            //int randomNumber = random.Next(0, randomizer + 1);
                            //Thread.Sleep(randomNumber);
                            //continue;
                        }


                        profitableItems = sortByProfit(profitableItems, count);
                        foreach (Item item in profitableItems)
                        {
                            curl.post("http://www.neopets.com/halloween/garage.phtml", "purchase=1&oii=" + item.objID + "&neopets=" + item.hash);
                            log2("[" + DateTime.Now.ToString("T") + "] " + item.name + " (" + item.price + ") profit=" + item.profit);
                        }
                        //log("[" + DateTime.Now.ToString("T") + "] Checked AA [" + matches.Count + "][" + sw.ElapsedMilliseconds + "]");
                        debug(src, true);
                        if (matches.Count > 10 && matches.Count < 18)
                            break;
                    }
                  /*  else
                    {
                        log("[" + DateTime.Now.ToString("T") + "] Checked AA [0][" + sw.ElapsedMilliseconds + "]");
                        debug(src, false);
                    }*/

                    //int randomNumber2 = random.Next(-1 * (randomizer), randomizer + 1);
                    // int randomNumber2 = random.Next(100, randomizer + 1);
                    //Thread.Sleep(randomNumber2);
                }
                Thread.Sleep(3000);
                //updateTimer(atticIndex, "Waiting " + (delay + randomNumber) + "ms");

            }
        }

        private void tShop_Abandoned_Attic3()
        {
            //Regex reg = new Regex("<input type=['\"]?hidden['\"]? name=['\"]?oii['\"]? value=['\"]?([0-9]+)['\"]?>[ \n\r\t]+<input type=['\"]?hidden['\"]? name=['\"]?neopets['\"]? value=['\"]?([a-zA-Z0-9]+)['\"]?>.+?[ \n\r\t]+.+?[ \n\r\t]+<b>(" + REGEX_ITEM_NAME + ")</b><br>([0-9]+) in stock<br>Cost: ([0-9,]+) NP<br><br></form>", RegexOptions.IgnoreCase);
            Regex reg = new Regex(@"<input type='hidden' name='oii' value='([0-9]+)'>[ \n\r\t]{0,}<input type='hidden' name='neopets' value='([0-9]+)'>[ \n\r\t]{0,}.+[ \n\r\t]{0,}<b>(" + REGEX_ITEM_NAME + ")</b><br>[0-9]+ in stock<br>Cost: ([0-9,]+) NP<br><br></form>", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Multiline);
            Random random = new Random();
            //int delay = 500;
            int randomizer = 1000;
            cURL curl = new cURL("http://www.neopets.com/halloween/garage.phtml", 1000);
            cURL curlPrivate = new cURL("http://www.sunnyneo.com/attictimer/getTimes.php", true);
            long nextRestock;

            // Create an inferred delegate that invokes methods for the timer.
            System.Threading.TimerCallback tcb = new System.Threading.TimerCallback(updateTimer);

            while (true)
            {
                string psrc = curlPrivate.post("http://www.sunnyneo.com/attictimer/getTimes.php");
                int divIndex = psrc.IndexOf('|');
                if (divIndex == -1)
                {
                    MessageBox.Show("Unable to connect to the website that shows next Attic Restock, Trying again in 2 minutes.");
                    Thread.Sleep(120000);
                    continue;
                }
                psrc = psrc.Substring(divIndex + 1);
                nextRestock = Convert.ToInt64(psrc);
                long timestamp = getTimestamp();

                int timeleft = (int)(nextRestock - timestamp); //seconds
                if (timeleft > 6)
                {
                    Thread.Sleep(((timeleft - 5) * 1000) +100);
                }
                while (timestamp < nextRestock + 9)
                {
                    timestamp = getTimestamp();
                    //System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

                    string src = curl.post("http://www.neopets.com/halloween/garage.phtml");
                    if (src == null) //timed out
                    {
                        log("Attic Connection Time-out");
                        continue;
                    }
                    MatchCollection matches = reg.Matches(src);

                    //if there are items, check each if profitable and buy
                    if (matches.Count > 0)
                    {
                        Item[] profitableItems = new Item[matches.Count];
                        int count = 0;

                        //buy draig eggs and morphing potions ASAP
                        foreach (Match match in matches)
                        {
                            int draikIndex = 0;
                            if ((draikIndex = match.Groups[3].Value.IndexOf("Draik ")) != -1)
                            {
                                if ((match.Groups[3].Value[draikIndex + 6] == 'M' && match.Groups[3].Value[draikIndex + 7] == 'o') ||
                                      (match.Groups[3].Value[draikIndex + 6] == 'E' && match.Groups[3].Value[draikIndex + 7] == 'g') ||
                                      (match.Groups[3].Value[draikIndex + 6] == 'T' && match.Groups[3].Value[draikIndex + 7] == 'r'))
                                {
                                    //one of these exists, BUY!
                                    curl.post("http://www.neopets.com/halloween/garage.phtml", "purchase=1&oii=" + match.Groups[1].Value + "&neopets=" + match.Groups[2].Value);
                                    log2("Found " + match.Groups[3].Value);
                                }
                            }
                        }

                        foreach (Match match in matches)
                        {
                            int listIndex;
                            if ((listIndex = binarySearch(match.Groups[3].Value)) != -1)
                            {
                                int shopPrice = Convert.ToInt32(match.Groups[4].Value.Replace(",", ""));
                                int profit = listItems[listIndex].price - shopPrice;

                                if (profit > 2900000)
                                {
                                    //instant buy
                                    curl.post("http://www.neopets.com/halloween/garage.phtml", "purchase=1&oii=" + match.Groups[1].Value + "&neopets=" + match.Groups[2].Value);
                                    log2("[" + DateTime.Now.ToString("T") + "] " + match.Groups[3].Value + " (" + shopPrice + ") profit=" + profit);
                                }
                                else if (profit >= Convert.ToInt32(textBoxMinAtticProfit.Text))
                                {
                                    profitableItems[count++] = new Item(match.Groups[1].Value, match.Groups[3].Value, shopPrice, profit, match.Groups[2].Value);
                                }
                            }
                        }

                        if (count == 0) //no profitable items
                        {
                            continue;
                            //log("[" + DateTime.Now.ToString("T") + "] Checked AA [" + matches.Count + "][" + sw.ElapsedMilliseconds + "]");
                          //  debug(src, true);
                            // int randomNumber = random.Next(-1 * (randomizer), randomizer + 1);
                            //int randomNumber = random.Next(0, randomizer + 1);
                            //Thread.Sleep(randomNumber);
                            //continue;
                        }


                        profitableItems = sortByProfit(profitableItems, count);
                        foreach (Item item in profitableItems)
                        {
                            curl.post("http://www.neopets.com/halloween/garage.phtml", "purchase=1&oii=" + item.objID + "&neopets=" + item.hash);
                            log2("[" + DateTime.Now.ToString("T") + "] " + item.name + " (" + item.price + ") profit=" + item.profit);
                        }
                        //log("[" + DateTime.Now.ToString("T") + "] Checked AA [" + matches.Count + "][" + sw.ElapsedMilliseconds + "]");
                        debug(src, true);
                        if (matches.Count > 10 && matches.Count < 18)
                            break;
                    }
                   /* else
                    {
                        log("[" + DateTime.Now.ToString("T") + "] Checked AA [0][" + sw.ElapsedMilliseconds + "]");
                        debug(src, false);
                    }*/

                    //int randomNumber2 = random.Next(-1 * (randomizer), randomizer + 1);
                    // int randomNumber2 = random.Next(100, randomizer + 1);
                    //Thread.Sleep(randomNumber2);
                }
                Thread.Sleep(3000);
                //updateTimer(atticIndex, "Waiting " + (delay + randomNumber) + "ms");

            }
        }









        public long getTimestamp()
        {//Find unix timestamp (seconds since 01/01/1970)
            long ticks = DateTime.UtcNow.Ticks - DateTime.Parse("01/01/1970 00:00:00").Ticks;
            ticks /= 10000000; //Convert windows ticks to seconds
            return ticks;
        }

        public int binarySearch(string target)
        {
            int lo = 0, hi = listItems.Length - 1;
            while (lo <= hi)
            {
                int mid = (lo + hi) / 2;
                if (target.Equals(listItems[mid].name))
                {
                    return mid;
                }
                if (target.CompareTo(listItems[mid].name) < 0)
                {
                    hi = mid - 1;
                }
                else
                {
                    lo = mid + 1;
                }
            }
            return -1;
        }

        private void mergesort(ListItem[] list)
        {
            ListItem[] scratch = new ListItem[list.Length];
            mergesort(list, 0, list.Length - 1, scratch);
        }

        // recursive mergesort method, sorts the slice list[start..end] using scratch array space for merging
        private void mergesort(ListItem[] list, int start, int end, ListItem[] scratch)
        {
            if (end > start)
            { // there are at least 2 things in the array slice
                // find halfway point
                int mid = (start + end) / 2;
                // recursively sort left half: list[start..mid]
                mergesort(list, start, mid, scratch);
                // recursively sort right half: list[mid+1..mid]
                mergesort(list, mid + 1, end, scratch);
                // merge the sorted halves
                merge(list, start, mid, end, scratch);
            }
        }

        // merge two sorted array slices, list[start..mid] and list[mid+1..end] using scratch array space
        private void merge(ListItem[] list, int start, int mid, int end, ListItem[] scratch)
        {
            // set indices to track through each of the slices, and one to track through scratch space
            int f1 = start, f2 = mid + 1, s = 0;
            while (f1 <= mid && f2 <= end)
            {
                if (list[f1].name.CompareTo(list[f2].name) < 0)
                {
                    scratch[s] = list[f1];
                    f1++;
                }
                else
                {
                    scratch[s] = list[f2];
                    f2++;
                }
                s++;
            }
            // if there are left over entries in one of the lists, copy them over to the scratch output
            while (f1 <= mid)
            {
                scratch[s] = list[f1];
                f1++; s++;
            }
            while (f2 <= end)
            {
                scratch[s] = list[f2];
                f2++; s++;
            }
            // copy merged output from scratch space to 
            for (int i = 0; i < s; i++)
            {
                list[start + i] = scratch[i];
            }
        }

        private Item[] sortByProfit(Item[] items, int count)
        {
            Item[] sortedItems = new Item[count];
            for (int i = 0; i < count; i++)
            {
                int highest = 0;
                for (int k = 1; k < count; k++)
                {
                    if (items[k] == null)
                        continue;

                    if (items[highest] == null || items[k].profit > items[highest].profit)
                        highest = k;
                }
                sortedItems[i] = items[highest];
                items[highest] = null;
            }
            return sortedItems;
        }
    }





    public class timerObj
    {
        public int timeleft;
        public int shopIndex;
        public timerObj(int shopIndex, int timeleft)
        {
            this.timeleft = timeleft;
            this.shopIndex = shopIndex;
        }
    }
}
