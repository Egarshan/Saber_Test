using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Linq;

namespace Saber_Test
{
    class ListNode
    {
        public ListNode Previous;
        public ListNode Next;
        public ListNode Random;
        public string Data;
    }

    class ListRandom
    {
        public ListNode Head;
        public ListNode Tail;
        public int Count;

        private Dictionary<ListNode, string> GetDictionary()
        {
            Dictionary<ListNode, string> dict = new Dictionary<ListNode, string>();
            ListNode curNode = this.Head;
            int i = 0;

            while (curNode != null)
            {
                dict.Add(curNode, String.Format("#{0}", i++));
                curNode = curNode.Next;
            }

            return dict;
        }
        public void AddNode(string data)
        {
            if (Count++ == 0)
            {
                Head = new ListNode();
                Tail = Head;

                Head.Next = null;
                Head.Previous = null;
                Head.Random = Head;
                Head.Data = data;
            }
            else
            {
                ListNode curNode = new ListNode();
                Random randomizer = new Random();
                Dictionary<ListNode, string> dict = GetDictionary();

                curNode.Next = null;
                curNode.Previous = Tail;
                curNode.Random = dict.Keys.ElementAt(randomizer.Next(Count-1));
                Tail.Next = curNode;

                Tail = curNode;
                curNode.Data = data;
            }   
        }
        public void Serialize(Stream s)
        {
            Dictionary<ListNode, string> dict = GetDictionary();
            ListNode curNode = this.Head;

            using (StreamWriter strW = new StreamWriter(s))
            {
                strW.WriteLine(@"<ListRandom>");
                
                strW.WriteLine("    <Head>{0}</Head>", (Head == null) ? "null" : dict[Head]);
                strW.WriteLine("    <Tail>{0}</Tail>", (Tail == null) ? "null" : dict[Tail]);
                strW.WriteLine("    <Count>{0}</Count>", Count);

                while (curNode != null)
                {
                    strW.WriteLine(String.Format(@"   <ListNode ID=""{0}"">", dict[curNode]));
                    strW.WriteLine(String.Format("    <Rand>{0}</Rand>", curNode.Random == null ? "null" : dict[curNode.Random]));
                    strW.WriteLine(String.Format("    <Data>{0}</Data>", curNode.Data));
                    strW.WriteLine(String.Format("   </ListNode>"));
                    curNode = curNode.Next;
                }

                strW.WriteLine(String.Format(@"</ListRandom>"));
            }
        }
        public void Deserialize(Stream s)
        {
            using (XmlReader xmlR = XmlReader.Create(s))
            {
                string head_ID = "", tail_ID = "";
                Dictionary<string, ListNode> dict = new Dictionary<string, ListNode>();
                Queue<string> identQueue = new Queue<string>();

                while (xmlR.Read())
                {

                    if (xmlR.IsStartElement() && xmlR.Name.Equals("Head")) head_ID = xmlR.ReadInnerXml();
                    if (xmlR.IsStartElement() && xmlR.Name.Equals("Tail")) tail_ID = xmlR.ReadInnerXml();
                    if (xmlR.IsStartElement() && xmlR.Name.Equals("Count")) Count = Convert.ToInt32(xmlR.ReadInnerXml());
                    if (xmlR.IsStartElement() && xmlR.Name.Equals("ListNode"))
                    {
                        ListNode newNode = new ListNode();
                        string ID = xmlR.GetAttribute("ID");

                        while (true)
                        {
                            xmlR.Read();
                            if (xmlR.IsStartElement() && xmlR.Name.Equals("Data")) newNode.Data = xmlR.ReadInnerXml();
                            if (xmlR.IsStartElement() && xmlR.Name.Equals("Rand")) identQueue.Enqueue(xmlR.ReadInnerXml());
                            if (xmlR.Name.Equals("ListNode") && xmlR.IsStartElement() != true) break;
                        }

                        dict.Add(ID, newNode);
                        if (Head == null)
                        {
                            Head = newNode;
                            Tail = Head;
                            Head.Next = null;
                            Head.Previous = null;
                        }
                        else
                        {
                            newNode.Next = null;
                            newNode.Previous = Tail;
                            Tail.Next = newNode;
                            Tail = newNode;
                        }
                    }
                }

                if (head_ID.Equals("null"))
                    Head = null;
                else Head = dict[head_ID];

                if (tail_ID.Equals("null"))
                    Tail = null;
                else Tail = dict[tail_ID];

                ListNode curNode = Head;
            }
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            ListRandom list = new ListRandom();

            for (int i = 0; i < 100; i++)
                list.AddNode(("Test data_" + i));

            list.Serialize(new FileStream("file.xml", FileMode.Create));
            ListRandom list_ = new ListRandom();
            list_.Deserialize(new FileStream("file.xml", FileMode.Open));
        }
    }
}
