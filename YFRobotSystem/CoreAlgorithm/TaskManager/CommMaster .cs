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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using SocketHelper;

namespace CoreAlgorithm.TaskManager
{
    
    public class CommMaster
    { 
        TasksManager tm;
        INIClass ini = new INIClass(System.AppDomain.CurrentDomain.BaseDirectory + "Config.ini");
        string localip = "", plcip = "", sprayip = "";//400PLC ip
        int localportr = 0, plcportr = 0, plcports = 0, sprayportr = 0, sprayports = 0;//400PLC端口
         public static SocketServer PLC_Server = null;
        YFHelper YF_Helper = new YFHelper();
        public CommMaster()
        {
            tm = new TasksManager();
        }

        public void do_SendMessage(object objTh)
        {
            while (true)
            {if (Program.MessageStop == 1)
                    break;
                Thread.Sleep(1000);
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
                                sql = "update S_TFlag set Flag=0 where ID=2";
                                tm.MultithreadExecuteNonQuery(sql);
                                Program.MessageFlg = 3;
                                byte[] sendArray = Enumerable.Repeat((byte)0x0, 94).ToArray();
                                byte[] byteArray1 = BitConverter.GetBytes(Program.MessageFlg);
                                Buffer.BlockCopy(byteArray1, 0, sendArray, 0, byteArray1.Length);
                                if (sendArray.Length > 0)
                                {
                                    PLCSocketServer.senddata(sendArray);
                                    string str = Program.MessageFlg.ToString();
                                    sql = string.Format("INSERT INTO SENDLOG(REC_CREATE_TIME,CONTENT) VALUES ('{0}','{1}')", DateTime.Now.ToString(("yyyy-MM-dd HH:mm:ss")), "plc数据请求失败"+str);
                                    tm.MultithreadExecuteNonQuery(sql);
                                }
                            }
                            else
                            {
                                Program.MessageFlg = 2;
                                Send_SignsMessage();
                            }
                        
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
                YFHelper.LabelData PLClable;
                double MAXRECID = 0;// PLANIDNow = 0; 
                double REC_ID = 0;// PLANIDNow = 0; 
                string sql = "select MAX(REC_ID) AS REC_ID from TLabelContent WHERE IMP_FINISH=31 or IMP_FINISH=32 or IMP_FINISH=33";
                DbDataReader dr = null;
                dr = tm.MultithreadDataReader(sql);
                while (dr.Read())
                {
                    if (dr["REC_ID"] != DBNull.Value)
                        MAXRECID = Convert.ToDouble(dr["REC_ID"].ToString());
                }
                dr.Close();
                sql = string.Format("select top 1 REC_ID,merge_sinbar,gk,heat_no,mtrl_no,spec,wegith,num_no,print_date,classes from TLabelContent WHERE REC_ID>{0} AND IMP_FINISH=0 order by REC_ID ASC", MAXRECID);
                DataTable dt = tm.MultithreadDataTable(sql);
                for (int i = 0; i<dt.Rows.Count; i++)
                {
                    PLClable.merge_sinbar = dt.Rows[i]["merge_sinbar"].ToString();
                    PLClable.gk = dt.Rows[i]["gk"].ToString();
                    PLClable.heat_no = dt.Rows[i]["heat_no"].ToString();
                    PLClable.mtrl_no = dt.Rows[i]["mtrl_no"].ToString();
                    PLClable.spec = dt.Rows[i]["spec"].ToString();
                    PLClable.wegith = int.Parse(dt.Rows[i]["wegith"].ToString());
                    PLClable.num_no= int.Parse(dt.Rows[i]["num_no"].ToString());
                    PLClable.print_date = dt.Rows[i]["print_date"].ToString();
                    PLClable.classes= dt.Rows[i]["classes"].ToString();
                    REC_ID = double.Parse(dt.Rows[i]["REC_ID"].ToString());

                    byte[] sendArray = Enumerable.Repeat((byte)0x0, 201).ToArray();
                    byte[] byteArray1 = BitConverter.GetBytes(Program.MessageFlg);
                    byte[] byteArray2 = BitConverter.GetBytes(Program.PrintNum);
                    byte[] byteArray3 = Encoding.ASCII.GetBytes(REC_ID.ToString());
                    byte[] byteArray4 = Encoding.ASCII.GetBytes(PLClable.merge_sinbar);
                    byte[] byteArray5=  Encoding.ASCII.GetBytes(PLClable.gk);
                    byte[] byteArray6 = BitConverter.GetBytes(PLClable.wegith);
                    byte[] byteArray7 = Encoding.ASCII.GetBytes(PLClable.print_date);
                    byte[] byteArray8= Encoding.ASCII.GetBytes(PLClable.mtrl_no);
                    byte[] byteArray9 = Encoding.ASCII.GetBytes(PLClable.spec);
                    byte[] byteArray10 = Encoding.ASCII.GetBytes(PLClable.merge_sinbar);
                    byte[] byteArray11 = BitConverter.GetBytes(PLClable.num_no);
                    byte[] byteArray12 = Encoding.ASCII.GetBytes(PLClable.classes);
                                        
                    Buffer.BlockCopy(byteArray1, 0, sendArray, 0, byteArray1.Length);
                    Buffer.BlockCopy(byteArray2, 0, sendArray, 2, byteArray2.Length);
                    Buffer.BlockCopy(byteArray3, 0, sendArray, 4, byteArray3.Length);
                    Buffer.BlockCopy(byteArray4, 0, sendArray, 16, byteArray4.Length);
                    Buffer.BlockCopy(byteArray5, 0, sendArray, 46, byteArray5.Length);
                    Buffer.BlockCopy(byteArray6, 0, sendArray, 31, byteArray6.Length);
                    Buffer.BlockCopy(byteArray7, 0, sendArray, 69, byteArray7.Length);
                    Buffer.BlockCopy(byteArray8, 0, sendArray, 88, byteArray8.Length);
                    Buffer.BlockCopy(byteArray9, 0, sendArray, 118, byteArray9.Length);
                    Buffer.BlockCopy(byteArray10, 0, sendArray, 148, byteArray10.Length);
                    Buffer.BlockCopy(byteArray11, 0, sendArray, 168, byteArray11.Length);
                    Buffer.BlockCopy(byteArray12, 0, sendArray, 176, byteArray12.Length);

                    if (sendArray.Length > 0)
                    {
                        KeyValuePair<string, Socket> kvp = PLC_Server.dict.FirstOrDefault();
                        PLC_Server.SendToSomeone(sendArray,kvp.Key);
                        string str ="发送到PLC"+ Program.MessageFlg.ToString() + " " + MAXRECID.ToString()  + PLClable.heat_no;
                        sql = string.Format("INSERT INTO SENDLOG(REC_CREATE_TIME,CONTENT) VALUES ('{0}','{1}')", DateTime.Now.ToString(("yyyy-MM-dd HH:mm:ss")), str);
                        tm.MultithreadExecuteNonQuery(sql);
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
     
        /// <summary>
        /// </summary>
        /// <param name="serverModlue"></param>
        public void RunSINGenerate()
        {
            try
            {      
                
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
                    }

                }
                dr.Close();

                Thread thS = new Thread(new System.Threading.ParameterizedThreadStart(do_SendMessage));
                thS.Start(null);
                PLC_Server = new SocketServer(localip, localportr);
                PLC_Server.StarServer(YF_Helper.Recv);

                //PLCSocketServer PLCServer = new PLCSocketServer();
                //PLCServer.CreateSocket(localip,localportr);                
            }
            catch (Exception ex)
            {
                log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString() + "::" + MethodBase.GetCurrentMethod().ToString());
                Log.addLog(log, LogType.ERROR, ex.Message);
                Log.addLog(log, LogType.ERROR, ex.StackTrace);
            }
            return;
        }
      
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

    
}
