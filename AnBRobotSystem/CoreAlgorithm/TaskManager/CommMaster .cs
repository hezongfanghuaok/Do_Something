﻿using System;
using System.Runtime.InteropServices;
using SQLPublicClass;
using System.Data.Common;
using System.Threading;
using System.Reflection;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
namespace CoreAlgorithm.TaskManager
{
    public struct LabelData
    {
        public string ID_LOT_PROD ;
        public Int16 ID_PART_LOT ;
        public Int16 NUM_BDL ;
        public Int16 SEQ_LEN ;
        public Int16 SEQ_OPR ;
        public string DES_FIPRO_SECTION ;
        public string BAR_CODE ;
        public string NAME_PROD;
        public string NAME_STLGD;
        public string ID_CREW_CK;
        public string NAME_STND;
        public string ID_HEAT;
        public string TMSTP_WEIGH ;
        public float LA_BDL_ACT;
        public Int16 NUM_BAR;
        public double DIM_LEN;
    };
    public class CommMaster
    { 
        TasksManager tm;
        //private List<Thread> OrderSetSlabDesignThread;
        INIClass ini = new INIClass(System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
        LabelData Label;
        public CommMaster()
        {
            tm = new TasksManager();
            //OrderSetSlabDesignThread = new List<Thread>();
        }
        //int PLCSend = 0;
        Socket raw;
        
        public void SendSprayMessage(string msg)
        {
            byte[] msgBytes = Encoding.ASCII.GetBytes(msg);
            raw.Send(msgBytes);
            string Text = "SendSpray:";
            foreach (byte b in msgBytes)
            {
                Text += Convert.ToInt16(b).ToString("x").PadLeft(2, '0') + " ";
            }
            string sql = string.Format("INSERT INTO SENDLOG(REC_CREATE_TIME,CONTENT) VALUES ('{0}','{1}')", DateTime.Now.ToString(("yyyy-MM-dd HH:mm:ss")), Text);
            tm.MultithreadExecuteNonQuery(sql);
        }
        private byte SendSpray()
        {
            byte rownum = 0;
            string sql = "SELECT PARAMETER_VALUE FROM SYSPARAMETER where PARAMETER_ID=9";
            DbDataReader dr  = tm.MultithreadDataReader(sql);
            while (dr.Read())
            {
                rownum = Convert.ToByte(dr["PARAMETER_VALUE"]);
            }
            dr.Close();
            double MAXRECID = 0;// PLANIDNow = 0;                
            sql = "select MAX(REC_ID) AS REC_ID from TLabelContent WHERE IMP_FINISH=31 or IMP_FINISH=32 or IMP_FINISH=33";
            dr = null;
            dr = tm.MultithreadDataReader(sql);
            while (dr.Read())
            {
                MAXRECID = Convert.ToDouble(dr["REC_ID"].ToString());
            }
            dr.Close();

            for(int i=1;i<=rownum;i++)
            {
                Thread.Sleep(500);
                if (i == 1)
                {
                    sql = "select RTDATA_VALUE from REALTIMETASKDATA where TASK_ID=1";
                    dr = tm.MultithreadDataReader(sql);
                    string ProductIDA = "";
                    while (dr.Read())
                    {
                        if (dr["RTDATA_VALUE"] != "")
                        {
                            ProductIDA = dr["RTDATA_VALUE"].ToString();
                            ProductIDA = ProductIDA.Substring(0, (ProductIDA.Length - 1));
                        }
                    }
                    string Product = "";
                    if (ProductIDA != "")
                    {
                        sql = string.Format("select top 1 " + ProductIDA + "  from TLabelContent WHERE REC_ID>{0} AND IMP_FINISH=0 order by REC_ID ASC", MAXRECID);
                        dr = tm.MultithreadDataReader(sql);

                        while (dr.Read())
                        {
                            if (ProductIDA.Contains("ID_HEAT") && dr["ID_HEAT"] != DBNull.Value)
                                Product = Product + dr["ID_HEAT"].ToString() + "   ";
                            if (ProductIDA.Contains("ID_LOT_PROD") && dr["ID_LOT_PROD"] != DBNull.Value)
                                Product = Product + dr["ID_LOT_PROD"].ToString() + "   ";
                            if (ProductIDA.Contains("NAME_STLGD") && dr["NAME_STLGD"] != DBNull.Value)
                                Product = Product + dr["NAME_STLGD"].ToString() + "   ";
                            if (ProductIDA.Contains("DIM_LEN") && dr["DIM_LEN"] != DBNull.Value)
                                Product = Product + dr["DIM_LEN"].ToString() + "   ";
                            if (ProductIDA.Contains("DES_FIPRO_SECTION") && dr["DES_FIPRO_SECTION"] != DBNull.Value)
                                Product = Product + dr["DES_FIPRO_SECTION"].ToString() + "   ";
                        }
                        Product = Product.Substring(0, (Product.Length - 3));
                    }
                    string msg = "external_field string " + i.ToString() + " \"" + Product + "\"\r\n";
                    try
                    {
                        SendSprayMessage(msg);
                    }
                    catch (Exception ex)
                    {
                        Program.MessageFlg = 23;

                        log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString() + "::" + MethodBase.GetCurrentMethod().ToString());
                        Log.addLog(log, LogType.ERROR, ex.Message);
                        Log.addLog(log, LogType.ERROR, ex.StackTrace);
                    }
                }
                if(i == 1)
                {
                    sql = "select RTDATA_VALUE from REALTIMETASKDATA where TASK_ID=2";
                    dr = tm.MultithreadDataReader(sql);
                    string ProductIDA = "";
                    while (dr.Read())
                    {
                        if (dr["RTDATA_VALUE"] != "")
                        {
                            ProductIDA = dr["RTDATA_VALUE"].ToString();
                            ProductIDA = ProductIDA.Substring(0, (ProductIDA.Length - 1));
                        }
                    }
                    string Product = "";
                    if (ProductIDA != "")
                    {
                        sql = string.Format("select top 1 " + ProductIDA + "  from TLabelContent WHERE REC_ID>{0} AND IMP_FINISH=0 order by REC_ID ASC", MAXRECID);
                        dr = tm.MultithreadDataReader(sql);
                        while (dr.Read())
                        {
                            if (ProductIDA.Contains("ID_HEAT") && dr["ID_HEAT"] != DBNull.Value)
                                Product = Product + dr["ID_HEAT"].ToString() + "   ";
                            if (ProductIDA.Contains("ID_LOT_PROD") && dr["ID_LOT_PROD"] != DBNull.Value)
                                Product = Product + dr["ID_LOT_PROD"].ToString() + "   ";
                            if (ProductIDA.Contains("NAME_STLGD") && dr["NAME_STLGD"] != DBNull.Value)
                                Product = Product + dr["NAME_STLGD"].ToString() + "   ";
                            if (ProductIDA.Contains("DIM_LEN") && dr["DIM_LEN"] != DBNull.Value)
                                Product = Product + dr["DIM_LEN"].ToString() + "   ";
                            if (ProductIDA.Contains("DES_FIPRO_SECTION") && dr["DES_FIPRO_SECTION"] != DBNull.Value)
                                Product = Product + dr["DES_FIPRO_SECTION"].ToString() + "   ";

                        }
                        Product = Product.Substring(0, (Product.Length - 3));
                    }
                    string msg = "external_field string " + i.ToString() + " \"" + Product + "\"\r\n";
                    try
                    {
                        SendSprayMessage(msg);
                    }
                    catch (Exception ex)
                    {
                        Program.MessageFlg = 23;
                        log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString() + "::" + MethodBase.GetCurrentMethod().ToString());
                        Log.addLog(log, LogType.ERROR, ex.Message);
                        Log.addLog(log, LogType.ERROR, ex.StackTrace);
                    }
                }
                if (i == 2)
                {
                    sql = "select RTDATA_VALUE from REALTIMETASKDATA where TASK_ID=3";
                    dr = tm.MultithreadDataReader(sql);
                    string ProductIDA = "";
                    while (dr.Read())
                    {
                        if (dr["RTDATA_VALUE"] != "")
                        {
                            ProductIDA = dr["RTDATA_VALUE"].ToString();
                            ProductIDA = ProductIDA.Substring(0, (ProductIDA.Length - 1));
                        }
                    }
                    string Product = "";
                    if (ProductIDA != "")
                    {
                        sql = string.Format("select top 1 " + ProductIDA + "  from TLabelContent WHERE REC_ID>{0} AND IMP_FINISH=0 order by REC_ID ASC", MAXRECID);
                        dr = tm.MultithreadDataReader(sql);
                        while (dr.Read())
                        {
                            if (ProductIDA.Contains("ID_HEAT") && dr["ID_HEAT"] != DBNull.Value)
                                Product = Product + dr["ID_HEAT"].ToString() + "   ";
                            if (ProductIDA.Contains("ID_LOT_PROD") && dr["ID_LOT_PROD"] != DBNull.Value)
                                Product = Product + dr["ID_LOT_PROD"].ToString() + "   ";
                            if (ProductIDA.Contains("NAME_STLGD") && dr["NAME_STLGD"] != DBNull.Value)
                                Product = Product + dr["NAME_STLGD"].ToString() + "   ";
                            if (ProductIDA.Contains("DIM_LEN") && dr["DIM_LEN"] != DBNull.Value)
                                Product = Product + dr["DIM_LEN"].ToString() + "   ";
                            if (ProductIDA.Contains("DES_FIPRO_SECTION") && dr["DES_FIPRO_SECTION"] != DBNull.Value)
                                Product = Product + dr["DES_FIPRO_SECTION"].ToString() + "   ";

                        }
                        Product = Product.Substring(0, (Product.Length - 3));
                    }
                    string msg = "external_field string " + i.ToString() + " \"" + Product + "\"\r\n";
                    try
                    {
                        SendSprayMessage(msg);
                    }
                    catch (Exception ex)
                    {
                        Program.MessageFlg = 23;
                        log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString() + "::" + MethodBase.GetCurrentMethod().ToString());
                        Log.addLog(log, LogType.ERROR, ex.Message);
                        Log.addLog(log, LogType.ERROR, ex.StackTrace);
                    }
                }
            }
            return rownum;
}
        public void do_SendMessage(object objTh)
        {


            while (true)
            {if (Program.MessageStop == 1)
                    break;
                Thread.Sleep(1000);
                /*if (PLCSend == 1)
                {
                    Send_SignsMessage();
                    PLCSend = 0;
                }*/
                //}
                try
                { 
                if (Program.MessageFlg == 1)
                {
                    double MAXRECID = 0;// PLANIDNow = 0;                
                    string sql = "select MAX(REC_ID) AS REC_ID from TLabelContent WHERE IMP_FINISH=31 or IMP_FINISH=32 or IMP_FINISH=33";
                    DbDataReader dr = null;
                    dr = tm.MultithreadDataReader(sql);
                    while (dr.Read())
                    {
                        if(dr["REC_ID"]!=DBNull.Value)
                        MAXRECID = Convert.ToDouble(dr["REC_ID"].ToString());
                    }
                    dr.Close();
                    int count = 0;
                    sql = string.Format("select count(*) as count from TLabelContent WHERE REC_ID>{0} AND IMP_FINISH=0", MAXRECID);
                    DataTable dt = tm.MultithreadDataTable(sql);
                    for (int i = 0; i < dt.Rows.Count; i++)
                        count = Convert.ToInt32(dt.Rows[i]["count"].ToString());

                    lock (Program.gllock)
                    {
                        if (count == 0)
                        {
                            Program.MessageFlg = 3;
                            byte[] sendArray = Enumerable.Repeat((byte)0x0, 92).ToArray();
                            byte[] byteArray1 = BitConverter.GetBytes(Program.MessageFlg);
                            Buffer.BlockCopy(byteArray1, 0, sendArray, 0, byteArray1.Length);
                            if (sendArray.Length > 0)
                            {
                                PLCSocketServer.senddata(sendArray);
                                string str = Program.MessageFlg.ToString();
                                sql = string.Format("INSERT INTO SENDLOG(REC_CREATE_TIME,CONTENT) VALUES ('{0}','{1}')", DateTime.Now.ToString(("yyyy-MM-dd HH:mm:ss")), str);
                                tm.MultithreadExecuteNonQuery(sql);
                            }
                        }
                        else
                        {
                            Program.MessageFlg = 2;
                        }
                        Send_SignsMessage();
                    }
                }
                if (Program.MessageFlg == 11 || Program.MessageFlg == 14)
                {
                    lock (Program.gllock)
                    {
                        FormPrint PrintNow = new FormPrint();
                        string PrintNO = "";
                        if (Program.PrintNum == 1)
                            PrintNO = "Print1";
                        if (Program.PrintNum == 2)
                            PrintNO = "Print2";
                        PrintNow.button_handprinnt_Click(PrintNO);
                        if (Program.MessageFlg == 14)
                        {
                            if (Program.PrintNum == 1)
                            {
                                Program.PrintNum = 2;
                                PrintNO = "Print2";
                            }
                            else
                            {
                                Program.PrintNum = 1;
                                PrintNO = "Print1";
                            }
                        }
                        if (Program.MessageFlg == 13)
                            Send_SignsMessage();
                    }
                }
                if (Program.MessageFlg == 21)
                {
                    lock (Program.gllock)
                    {
                        Program.MessageFlg = 22;
                        int rownum = SendSpray();
                       
                        Send_SignsMessage();
                    }
                }
            }
                catch (Exception ex)
                {
                    Program.MessageFlg = 23;
                    log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString() + "::" + MethodBase.GetCurrentMethod().ToString());
                    Log.addLog(log, LogType.ERROR, ex.Message);
                    Log.addLog(log, LogType.ERROR, ex.StackTrace);
                }
            }
        }
           
        public void Send_SignsMessage()
        {
            try
            {
                double MAXRECID = 0;// PLANIDNow = 0;                
                string sql = "select MAX(REC_ID) AS REC_ID from TLabelContent WHERE IMP_FINISH=31 or IMP_FINISH=32 or IMP_FINISH=33";
                DbDataReader dr = null;
                dr = tm.MultithreadDataReader(sql);
                while (dr.Read())
                {
                    if (dr["REC_ID"] != DBNull.Value)
                        MAXRECID = Convert.ToDouble(dr["REC_ID"].ToString());
                }
                dr.Close();
                sql = string.Format("select top 1 ID_LOT_PROD,ID_PART_LOT,NUM_BDL,SEQ_LEN,SEQ_OPR,DES_FIPRO_SECTION,BAR_CODE from TLabelContent WHERE REC_ID>{0} AND IMP_FINISH=0 order by REC_ID ASC", MAXRECID);
                string ID_LOT_PROD = "";    
                Int16 ID_PART_LOT = 0;   
                Int16 NUM_BDL = 0;
                Int16 SEQ_LEN= 0;
                Int16 SEQ_OPR = 0;
                string DES_FIPRO_SECTION = "";
                string BAR_CODE = "";
                DataTable dt = tm.MultithreadDataTable(sql);
                for (int i = 0; i<dt.Rows.Count; i++)
                {
                    ID_LOT_PROD = dt.Rows[i]["ID_LOT_PROD"].ToString();
                    ID_PART_LOT = Int16.Parse(dt.Rows[i]["ID_PART_LOT"].ToString());
                    NUM_BDL = Int16.Parse(dt.Rows[i]["NUM_BDL"].ToString());
                    SEQ_LEN = Int16.Parse(dt.Rows[i]["SEQ_LEN"].ToString());
                    SEQ_OPR = Int16.Parse(dt.Rows[i]["SEQ_OPR"].ToString());
                    DES_FIPRO_SECTION = dt.Rows[i]["DES_FIPRO_SECTION"].ToString();
                    BAR_CODE = dt.Rows[i]["BAR_CODE"].ToString();  
                }
                    byte[] sendArray = Enumerable.Repeat((byte)0x0, 92).ToArray();
                    byte[] byteArray1 = BitConverter.GetBytes(Program.MessageFlg);
                    byte[] byteArray2 = System.Text.Encoding.ASCII.GetBytes(ID_LOT_PROD);
                    byte[] byteArray3 = BitConverter.GetBytes(ID_PART_LOT);
                    byte[] byteArray4= BitConverter.GetBytes(NUM_BDL);
                    byte[] byteArray5 = BitConverter.GetBytes(SEQ_LEN);
                    byte[] byteArray6 = BitConverter.GetBytes(SEQ_OPR);
                    byte[] byteArray7= System.Text.Encoding.ASCII.GetBytes(DES_FIPRO_SECTION);
                    byte[] byteArray8 = System.Text.Encoding.ASCII.GetBytes(BAR_CODE);
                    byte[] byteArray10 = BitConverter.GetBytes(Program.PrintNum);
                    if (Program.MessageFlg == 22)
                    {
                        byte rownum = 0;
                        sql = "SELECT PARAMETER_VALUE FROM SYSPARAMETER where PARAMETER_ID=9";
                        dr = tm.MultithreadDataReader(sql);
                        while (dr.Read())
                        {
                            rownum = Convert.ToByte(dr["PARAMETER_VALUE"]);
                        }
                        dr.Close();
                        byte[] byteArray9 = BitConverter.GetBytes(rownum);
                        Buffer.BlockCopy(byteArray9, 0, sendArray, 3, byteArray9.Length);
                        Buffer.BlockCopy(byteArray10, 0, sendArray, 2, byteArray10.Length);
                    }
                    Buffer.BlockCopy(byteArray1, 0, sendArray, 0, byteArray1.Length);
                    Buffer.BlockCopy(byteArray2, 0, sendArray, 4, byteArray2.Length);
                    Buffer.BlockCopy(byteArray3, 0, sendArray, 14, byteArray3.Length);
                    Buffer.BlockCopy(byteArray4, 0, sendArray, 16, byteArray4.Length);
                    Buffer.BlockCopy(byteArray5, 0, sendArray, 18, byteArray5.Length);
                    Buffer.BlockCopy(byteArray6, 0, sendArray, 20, byteArray6.Length);
                    Buffer.BlockCopy(byteArray7, 0, sendArray, 22, byteArray7.Length);
                    Buffer.BlockCopy(byteArray8, 0, sendArray, 62, byteArray8.Length);
                    if (sendArray.Length > 0)
                    {
                        //SocketClient.senddata(PDResult);连接相机恢复
                        PLCSocketServer.senddata(sendArray);
                        string str = Program.MessageFlg.ToString() + " " + ID_LOT_PROD + " "  + ID_PART_LOT.ToString() + " " + NUM_BDL.ToString() + " " + SEQ_LEN.ToString() + " " + SEQ_OPR.ToString() + " "+ DES_FIPRO_SECTION + " " + BAR_CODE ;
                        //sqlTemp.Add("INSERT INTO [EquipmentAttributeValues]([EquipmentAttribute_id],[AttributeValues_Time],[AttributeValues_Value]) VALUES ({0},'{1}','{2}')");
                        sql = string.Format("INSERT INTO SENDLOG(REC_CREATE_TIME,CONTENT) VALUES ('{0}','{1}')", DateTime.Now.ToString(("yyyy-MM-dd HH:mm:ss")), str);
                        tm.MultithreadExecuteNonQuery(sql);
                    }
            
            }
            catch (Exception ex)
            {
                log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString() + "::" + MethodBase.GetCurrentMethod().ToString());
                Log.addLog(log, LogType.ERROR, ex.Message);
                Log.addLog(log, LogType.ERROR, ex.StackTrace);
            }//sendArray[38] = 50;(byte)'1';
            
        }
        public void MsgToTextBox(string msg)
        {
            /*if (textBox2.InvokeRequired)
            {
                Action<string> actionDelegate = (x) => { this.textBox1.Text += x.ToString(); };
                // 或者
                // Action<string> actionDelegate = delegate(string txt) { this.label2.Text = txt; };
                this.textBox1.Invoke(actionDelegate, msg);
            }
            else
            {
                textBox1.Text += msg;
            }*/
        }
        private void ListenRecall()
        {
            try
            {
                while (0 == 0)
                {
                    if (Program.MessageStop == 1)
                        break;
                    Thread.Sleep(1000);
                    byte[] buffer = new byte[1024];
                    int byteCount = raw.Receive(buffer);
                    if (byteCount == 0)
                    {
                        break;
                    }
                    else
                    {
                        string msg = Encoding.Default.GetString(buffer);
                        if (msg.Contains("1234567890abcdefghijklmn"))
                        Program.MessageFlg = 23;
                            MsgToTextBox(msg);
                    }
                }
            }
            catch (Exception ex)
            {
                log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString() + "::" + MethodBase.GetCurrentMethod().ToString());
                Log.addLog(log, LogType.ERROR, ex.Message);
                Log.addLog(log, LogType.ERROR, ex.StackTrace);
            }
        }
        public void CreateRawSocket(string sprayip , int sprayports)
        {
            //IPAddress ip = IPAddress.Parse("127.0.0.1");
            //int port = 15786;
        IPAddress ip = IPAddress.Parse(sprayip);
            try
            {
                raw = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                raw.Connect(new IPEndPoint(ip, sprayports));
            }
            catch (Exception ex)
            {
                log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString() + "::" + MethodBase.GetCurrentMethod().ToString());
                Log.addLog(log, LogType.ERROR, ex.Message);
                Log.addLog(log, LogType.ERROR, ex.StackTrace);
            }
        }
        /// <summary>
        /// </summary>
        /// <param name="serverModlue"></param>
        public void RunSINGenerate()
        {try
        {       /*string sql = "SELECT PARAMETER_VALUE FROM SYSPARAMETER where PARAMETER_ID=2";
                DbDataReader dr = null;
                dr = tm.MultithreadDataReader(sql);
                while (dr.Read())
                {PLCSend = Convert.ToInt16(dr["PARAMETER_VALUE"]);
                }
                dr.Close();*/
                string localip = "", plcip = "", sprayip = "";//400PLC ip
                int localportr = 0, plcportr = 0, plcports = 0, sprayportr = 0, sprayports = 0;//400PLC端口
                string sql = "SELECT ACQUISITIONCONFIG_ID,DATAACQUISITION_IP,DATAACQUISITION_PORTR,DATAACQUISITION_PORTS FROM ACQUISITIONCONFIG where ACQUISITIONCONFIG_ID=1 or ACQUISITIONCONFIG_ID=4 or ACQUISITIONCONFIG_ID=15";// ";
                DbDataReader dr = tm.MultithreadDataReader(sql);
                while (dr.Read())
                {if (Convert.ToInt16(dr["ACQUISITIONCONFIG_ID"]) == 1)
                    {
                        plcip = Convert.ToString(dr["DATAACQUISITION_IP"]);
                        plcportr = Convert.ToInt32(dr["DATAACQUISITION_PORTR"]);
                        plcports = Convert.ToInt32(dr["DATAACQUISITION_PORTS"]);
                    }
                    else if (Convert.ToInt16(dr["ACQUISITIONCONFIG_ID"]) == 4)
                    {
                        localip = Convert.ToString(dr["DATAACQUISITION_IP"]);
                        localportr = Convert.ToInt32(dr["DATAACQUISITION_PORTR"]);
                        //plcports = Convert.ToInt32(dr["DATAACQUISITION_PORTS"]);
                    }
                    else
                    {
                        sprayip = Convert.ToString(dr["DATAACQUISITION_IP"]);
                        sprayportr = Convert.ToInt32(dr["DATAACQUISITION_PORTR"]);
                        sprayports = Convert.ToInt32(dr["DATAACQUISITION_PORTS"]);
                    }
                }
                dr.Close();
                //PLCSocketClient PLCClient = new PLCSocketClient();
                //SocketClient MESClient = new SocketClient();
                //PLCClient.CreateConnect(plcip,plcports);
                //MESClient.CreateConnect(mesip, mesports);
                PLCSocketServer PLCServer = new PLCSocketServer();
                //PLCSocketServer MESServer = new PLCSocketServer();
                PLCServer.CreateSocket(localip,localportr);
                //MESServer.CreateSocket(mesportr);
                CreateRawSocket(sprayip , sprayports);
                //创建监听进程
                Thread receiveThread = new Thread(ListenRecall);
                receiveThread.Start();
                //Thread th = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(do_UpdateProduct));
                    //OrderSetSlabDesignThread.Add(th);
                    //th.Start(null);
                Thread thS = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(do_SendMessage));
                //OrderSetSlabDesignThread.Add(th);
                thS.Start(null);
               
                //}
            }
            catch (Exception ex)
            {
                log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString() + "::" + MethodBase.GetCurrentMethod().ToString());
                Log.addLog(log, LogType.ERROR, ex.Message);
                Log.addLog(log, LogType.ERROR, ex.StackTrace);
            }
        return;
        }


        /// <summary>
        /// </summary>
        /// <param name="serverModlue"></param>
      
   }


    public class DllInvoke
    {
        #region Win API
        [DllImport("kernel32.dll")]
        private extern static IntPtr LoadLibrary(string path);
        [DllImport("kernel32.dll")]
        private extern static IntPtr GetProcAddress(IntPtr lib, string funcName);
        [DllImport("kernel32.dll")]
        private extern static bool FreeLibrary(IntPtr lib);
        #endregion
        private IntPtr hLib;
        public DllInvoke(String DLLPath)
        {
            hLib = LoadLibrary(DLLPath);
        }

        public bool IDisposable()
        {
            return FreeLibrary(hLib);
        }
        //将要执行的函数转换为委托
        public Delegate Invoke(string APIName, Type t)
        {
            IntPtr api = GetProcAddress(hLib, APIName);
            return (Delegate)Marshal.GetDelegateForFunctionPointer(api, t);
        }
    }

    public class ParaManager
    {
        //准备参数

        //获取取参路径

        //读取参数

        //制作参数xml
    }
}
