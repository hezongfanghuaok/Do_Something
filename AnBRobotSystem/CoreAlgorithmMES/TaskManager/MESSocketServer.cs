﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Runtime.InteropServices;
using log4net;
using SQLPublicClass;
using System.Reflection;
using System.Data.Common;
namespace CoreAlgorithm.TaskManager
{   
    class MESSocketServer
    { [DllImport("kernel32.dll")]
    private static extern bool SetLocalTime(ref SYSTEMTIME time);
        [StructLayout(LayoutKind.Sequential)]
    private struct SYSTEMTIME
    {
        public ushort year;
        public ushort month;
        public ushort dayOfWeek;
        public ushort day;
        public ushort hour;
        public ushort minute;
        public ushort second;
        public ushort milliseconds;
    }
        public struct LabelData
        {
            public string MACHINE_NO;// 打包机组号
            public string ID_LOT_PROD;//生产批号
            public Int16 ID_PART_LOT; //分批号
            public Int16 NUM_BDL;//捆号
            public Int16 SEQ_LEN;//长度顺序号
            public Int16 SEQ_OPR;//操作顺序号
            public double DIM_LEN; //米长
            public string IND_FIXED;// 定尺标志
            public double SEQ_SEND;// 下发顺序号
            public Int16 NUM_BAR;// 捆内支数
            public Int16 SEQ_LIST;// 排列序号
            public double LA_BDL_ACT;// 重量
            public string NO_LICENCE;// 许可证号
            public string NAME_PROD; //产品名称
            public string NAME_STLGD;// 执行标准
            public string ID_HEAT; //熔炼号
            public string NAME_STND; //钢牌号
            public string DES_FIPRO_SECTION; //断面规格描述
            public string ID_CREW_RL;// 轧制班别
            public string ID_CREW_CK;// 检查班别
            public string TMSTP_WEIGH;// 生产日期
            public string BAR_CODE; //条码内容
            public Int16 NUM_HEAD;//头签个数
            public Int16 NUM_TAIL;// 尾签个数
            public string TMSTP_SEND;// 发送时间
};
        public static bool SetDate(DateTime dt)
        {
            SYSTEMTIME st;

            st.year = (ushort)dt.Year;
            st.month = (ushort)dt.Month;
            st.dayOfWeek = (ushort)dt.DayOfWeek;
            st.day = (ushort)dt.Day;
            st.hour = (ushort)dt.Hour;
            st.minute = (ushort)dt.Minute;
            st.second = (ushort)dt.Second;
            st.milliseconds = (ushort)dt.Millisecond;
            bool rt = SetLocalTime(ref st);
            return rt;
        }
        static Socket socketServer;
        static Socket socketWatch;
        static TasksManager tm;
        public enum EncodingType { UTF7, UTF8, UTF32, Unicode, BigEndianUnicode, ASCII, GB2312, GBK };
        public static string GetString(byte[] myByte, EncodingType encodingType)
        {
            string str = null;
            switch (encodingType)
            {
                //将要加密的字符串转换为指定编码的字节数组
                case EncodingType.UTF7:
                    str = Encoding.UTF7.GetString(myByte);
                    break;
                case EncodingType.UTF8:
                    str = Encoding.UTF8.GetString(myByte);
                    break;
                case EncodingType.UTF32:
                    str = Encoding.UTF32.GetString(myByte);
                    break;
                case EncodingType.Unicode:
                    str = Encoding.Unicode.GetString(myByte);
                    break;
                case EncodingType.BigEndianUnicode:
                    str = Encoding.BigEndianUnicode.GetString(myByte);
                    break;
                case EncodingType.ASCII:
                    str = Encoding.ASCII.GetString(myByte);
                    break;
                case EncodingType.GB2312:
                    str = Encoding.Default.GetString(myByte);
                    break;
                case EncodingType.GBK:
                    str = System.Text.Encoding.GetEncoding("GBK").GetString(myByte);
                    break;
            }
            return str;
        }


        // <summary>  
        /// 定位指定的 System.Byte[] 在此实例中的第一个匹配项的索引。  
        /// </summary>  
        /// <param name="srcBytes">源数组</param>  
        /// <param name="searchBytes">查找的数组</param>  
        /// <returns>返回的索引位置；否则返回值为 -1。</returns>  
        public static List<byte[]> nByteIndexOf(byte[] srcBytes, byte[] searchBytes)
        {
            List<byte[]> HeadIndex = new List<byte[]>();
            int starindex = 0;
            for (int i = 0; i < srcBytes.Length; i++)//- searchBytes.Length- searchBytes.Length
            {
                if (srcBytes[i] == searchBytes[0])
                {
                    if (searchBytes.Length == 1)
                    {
                        if (starindex == 0)
                        {
                            HeadIndex.Add(srcBytes.Skip(i).Take(i - starindex).ToArray());
                            starindex = i;
                        }
                        else
                        {
                            HeadIndex.Add(srcBytes.Skip(i).Take(i - starindex).ToArray());
                            starindex = i + searchBytes.Length;
                        }

                    }
                    else
                    {
                        bool flag = true;
                        for (int j = 1; j < searchBytes.Length; j++)
                        {
                            if (srcBytes[i + j] != searchBytes[j])
                            {
                                flag = false;
                                break;
                            }
                        }
                        if (flag)
                        {
                            if (starindex == 0)
                            {
                                HeadIndex.Add(srcBytes.Skip(starindex).Take(i - starindex).ToArray());
                                starindex = i;
                            }
                            else
                            {
                                HeadIndex.Add(srcBytes.Skip(starindex + searchBytes.Length).Take(i - starindex - searchBytes.Length).ToArray());
                                starindex = i;
                            }
                        }
                    }
                }
            }
            return HeadIndex;
        }
        public static List<int> ByteIndexOf(byte[] srcBytes, byte[] searchBytes)
        {
            List<int> HeadIndex = new List<int>();
            /*if (srcBytes == null) { return -1; }
            if (searchBytes == null) { return -1; }
            if (srcBytes.Length == 0) { return -1; }
            if (searchBytes.Length == 0) { return -1; }
            if (srcBytes.Length < searchBytes.Length) { return -1; }*/
            for (int i = 0; i < srcBytes.Length- searchBytes.Length; i++)
            {
                if (srcBytes[i] == searchBytes[0])
                {
                    if (searchBytes.Length == 1) { HeadIndex.Add(i); }
                    bool flag = true;
                    for (int j = 1; j < searchBytes.Length; j++)
                    {
                        if (srcBytes[i + j] != searchBytes[j])
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag) { HeadIndex.Add(i); }
                }
            }
            return HeadIndex;
        }
        public static void CreateSocket(string mesip, int port)
        {
            //当点击开始监听的时候 在服务器端创建一个负责监听IP地址跟端口号的Socket
            socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //创建端口号对象
            IPEndPoint iep = new IPEndPoint(IPAddress.Parse(mesip), port);
            //监听
            socketWatch.Bind(iep);
            tm = new TasksManager();
            socketWatch.Listen(10);//队列排队
            Console.WriteLine("监听Socket创建完成，准备进入监听程序。");
            Thread receiveThread = new Thread(ListenRecall);
            receiveThread.Start();
        }
        static private void ListenRecall()
        {
            //等待客户端的链接并创建一个负责通信的Socket
            while (true)
            {
                Thread.Sleep(1000);
                try
                {
                    socketServer = socketWatch.Accept();
                    #region 方法一：启用带参数的线程接收数据
                    Console.WriteLine(socketServer.RemoteEndPoint.ToString() + ":连接成功");
                    //开启一个新线程不停的接受客户端发过来的数据
                    //启动线程池里得一个线程(队列的方式，如线程池暂时没空闲线程，则进入队列排队)
                    ThreadPool.QueueUserWorkItem(new WaitCallback(Recv), socketServer);
                    #endregion
                    #region 方法二：启用带参数的线程接收数据
                    //ParameterizedThreadStart pts = new ParameterizedThreadStart(Recv);
                    //Thread thread = new Thread(pts)
                    //{
                    //    IsBackground = true
                    //};
                    //thread.Start(socketServer);
                    #endregion
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return;
                }
            }
        }

 
        public static void Recv(object SocketClient)
        {
            Socket connect = SocketClient as Socket;
            while (true)
            {
                Thread.Sleep(1000);
                //创建一个内存缓冲区，其大小为1024*1024字节  即1M     
                byte[] arrServerRecMsg = new byte[1024];
                try
                {
                    //将接收到的信息存入到内存缓冲区，并返回其字节数组的长度   
                    int length = connect.Receive(arrServerRecMsg);
                    if (length > 0)
                    {
                        byte[] buffer = new byte[length];
                        Array.Copy(arrServerRecMsg, buffer, length);
                        byte[] searchBytes = new byte[] { 0x7F, 0x26 };
                        List<byte[]> HeadIndex = new List<byte[]>();
                        HeadIndex = nByteIndexOf(buffer, searchBytes);
                        string MessageFlg = GetString(HeadIndex[0], EncodingType.ASCII);//System.Text.Encoding.ASCII.GetString(buffer.Skip(0).Take(HeadIndex + 1).ToArray());
                        string sql = "";

                        if (MessageFlg == "L3PR000")//心跳信息，校准时间
                        {
                            string MesTime = GetString(HeadIndex[1], EncodingType.ASCII);//System.Text.Encoding.ASCII.GetString(buffer.Skip(0).Take(HeadIndex + 1).ToArray());
                            bool ret = SetDate(Convert.ToDateTime(MesTime));
                            string str = MessageFlg.ToString() + " " + MesTime;
                            sql = string.Format("INSERT INTO MESRECVLOG(REC_CREATE_TIME,RECV_CONTENT) VALUES ('{0}','{1}')", DateTime.Now.ToString(("yyyy-MM-dd HH:mm:ss")), str);
                            tm.MultithreadExecuteNonQuery(sql);
                        }
                        if (MessageFlg == "L3PR001")//接收标签数据信息并反馈
                        {
                            LabelData LabelDataRecv;
                            LabelDataRecv.MACHINE_NO = GetString(HeadIndex[1], EncodingType.ASCII);
                            LabelDataRecv.ID_LOT_PROD = GetString(HeadIndex[2], EncodingType.ASCII);
                            LabelDataRecv.ID_PART_LOT = Convert.ToInt16(GetString(HeadIndex[3], EncodingType.ASCII));
                            LabelDataRecv.NUM_BDL = Convert.ToInt16(GetString(HeadIndex[4], EncodingType.ASCII));
                            LabelDataRecv.SEQ_LEN = Convert.ToInt16(GetString(HeadIndex[5], EncodingType.ASCII));
                            LabelDataRecv.SEQ_OPR = Convert.ToInt16(GetString(HeadIndex[6], EncodingType.ASCII));
                            LabelDataRecv.DIM_LEN = Convert.ToDouble(GetString(HeadIndex[7], EncodingType.ASCII));
                            LabelDataRecv.IND_FIXED = GetString(HeadIndex[8], EncodingType.ASCII);
                            LabelDataRecv.SEQ_SEND = Convert.ToDouble((HeadIndex[9], EncodingType.ASCII));
                            LabelDataRecv.NUM_BAR = Convert.ToInt16(GetString(HeadIndex[10], EncodingType.ASCII));
                            LabelDataRecv.SEQ_LIST = Convert.ToInt16(GetString(HeadIndex[11], EncodingType.ASCII));
                            LabelDataRecv.LA_BDL_ACT = Convert.ToDouble((HeadIndex[12], EncodingType.ASCII));
                            LabelDataRecv.NO_LICENCE = GetString(HeadIndex[13], EncodingType.ASCII);
                            LabelDataRecv.NAME_PROD = GetString(HeadIndex[14], EncodingType.ASCII);
                            LabelDataRecv.NAME_STLGD = GetString(HeadIndex[15], EncodingType.GBK);
                            LabelDataRecv.ID_HEAT = GetString(HeadIndex[16], EncodingType.ASCII);
                            LabelDataRecv.NAME_STND = GetString(HeadIndex[17], EncodingType.ASCII);
                            LabelDataRecv.DES_FIPRO_SECTION = GetString(HeadIndex[18], EncodingType.ASCII);
                            LabelDataRecv.ID_CREW_RL = GetString(HeadIndex[19], EncodingType.GBK);
                            LabelDataRecv.ID_CREW_CK = GetString(HeadIndex[20], EncodingType.GBK);
                            LabelDataRecv.TMSTP_WEIGH = GetString(HeadIndex[21], EncodingType.ASCII);
                            LabelDataRecv.BAR_CODE = GetString(HeadIndex[22], EncodingType.ASCII);
                            LabelDataRecv.NUM_HEAD = Convert.ToInt16(GetString(HeadIndex[23], EncodingType.ASCII));
                            LabelDataRecv.NUM_TAIL = Convert.ToInt16(GetString(HeadIndex[24], EncodingType.ASCII));
                            LabelDataRecv.TMSTP_SEND = GetString(HeadIndex[25], EncodingType.ASCII);
                            sql = "select PARAMETER_VALUE from SYSPARAMETER where PARAMETER_ID=10";
                            DbDataReader dr = tm.MultithreadDataReader(sql);
                            double ProductIDA = 0;
                            while (dr.Read())
                            {
                                if (dr["PARAMETER_VALUE"] != DBNull.Value)
                                    ProductIDA = Convert.ToDouble(dr["PARAMETER_VALUE"]) + 1;
                            }
                            dr.Close();

                            sql = string.Format("insert into HLabelContent(MACHINE_NO,ID_LOT_PROD,ID_PART_LOT,NUM_BDL,SEQ_LEN,SEQ_OPR,DIM_LEN,IND_FIXED,SEQ_SEND,NUM_BAR,SEQ_LIST,LA_BDL_ACT,NO_LICENCE,NAME_PROD,NAME_STND,ID_HEAT,NAME_STLGD,DES_FIPRO_SECTION,ID_CREW_RL,ID_CREW_CK,TMSTP_WEIGH,BAR_CODE,NUM_HEAD,NUM_TAIL,L3TMSTP_SEND,REC_ID,REC_CREATE_TIME) values('{0}','{1}',{2},{3},{4},{5},{6},'{7}',{8},{9},{10},{11},'{12}','{13}','{14}','{15}','{16}','{17}','{18}','{19}','{20}','{21}',{22},{23},'{24}',{25},'{26}')", LabelDataRecv.MACHINE_NO, LabelDataRecv.ID_LOT_PROD, LabelDataRecv.ID_PART_LOT, LabelDataRecv.NUM_BDL, LabelDataRecv.SEQ_LEN, LabelDataRecv.SEQ_OPR, LabelDataRecv.DIM_LEN, LabelDataRecv.IND_FIXED, LabelDataRecv.SEQ_SEND, LabelDataRecv.NUM_BAR, LabelDataRecv.SEQ_LIST, LabelDataRecv.LA_BDL_ACT, LabelDataRecv.NO_LICENCE, LabelDataRecv.NAME_PROD, LabelDataRecv.NAME_STND, LabelDataRecv.ID_HEAT, LabelDataRecv.NAME_STLGD, LabelDataRecv.DES_FIPRO_SECTION, LabelDataRecv.ID_CREW_RL, LabelDataRecv.ID_CREW_CK, LabelDataRecv.TMSTP_WEIGH, LabelDataRecv.BAR_CODE, LabelDataRecv.NUM_HEAD, LabelDataRecv.NUM_TAIL, LabelDataRecv.TMSTP_SEND, ProductIDA, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            tm.MultithreadExecuteNonQuery(sql);
                            sql = string.Format("insert into TLabelContent(MACHINE_NO,ID_LOT_PROD,ID_PART_LOT,NUM_BDL,SEQ_LEN,SEQ_OPR,DIM_LEN,IND_FIXED,SEQ_SEND,NUM_BAR,SEQ_LIST,LA_BDL_ACT,NO_LICENCE,NAME_PROD,NAME_STND,ID_HEAT,NAME_STLGD,DES_FIPRO_SECTION,ID_CREW_RL,ID_CREW_CK,TMSTP_WEIGH,BAR_CODE,NUM_HEAD,NUM_TAIL,L3TMSTP_SEND,REC_ID,REC_CREATE_TIME) values('{0}','{1}',{2},{3},{4},{5},{6},'{7}',{8},{9},{10},{11},'{12}','{13}','{14}','{15}','{16}','{17}','{18}','{19}','{20}','{21}',{22},{23},'{24}',{25},'{26}')", LabelDataRecv.MACHINE_NO, LabelDataRecv.ID_LOT_PROD, LabelDataRecv.ID_PART_LOT, LabelDataRecv.NUM_BDL, LabelDataRecv.SEQ_LEN, LabelDataRecv.SEQ_OPR, LabelDataRecv.DIM_LEN, LabelDataRecv.IND_FIXED, LabelDataRecv.SEQ_SEND, LabelDataRecv.NUM_BAR, LabelDataRecv.SEQ_LIST, LabelDataRecv.LA_BDL_ACT, LabelDataRecv.NO_LICENCE, LabelDataRecv.NAME_PROD, LabelDataRecv.NAME_STND, LabelDataRecv.ID_HEAT, LabelDataRecv.NAME_STLGD, LabelDataRecv.DES_FIPRO_SECTION, LabelDataRecv.ID_CREW_RL, LabelDataRecv.ID_CREW_CK, LabelDataRecv.TMSTP_WEIGH, LabelDataRecv.BAR_CODE, LabelDataRecv.NUM_HEAD, LabelDataRecv.NUM_TAIL, LabelDataRecv.TMSTP_SEND, ProductIDA, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            tm.MultithreadExecuteNonQuery(sql);
                            sql = string.Format("UPDATE SYSPARAMETER SET PARAMETER_VALUE={0},PARAMETER_TIME='{1}' where PARAMETER_ID=10", ProductIDA, DateTime.Now.ToString(("yyyy-MM-dd HH:mm:ss")));
                            tm.MultithreadExecuteNonQuery(sql);
                            string str = MessageFlg.ToString() + " " + LabelDataRecv.MACHINE_NO + " " + LabelDataRecv.ID_LOT_PROD + " " + LabelDataRecv.ID_PART_LOT.ToString() + " " + LabelDataRecv.NUM_BDL.ToString() + " " + LabelDataRecv.SEQ_LEN.ToString() + " " + LabelDataRecv.SEQ_OPR.ToString() + " " + LabelDataRecv.DIM_LEN.ToString() + " " + LabelDataRecv.IND_FIXED + " " + LabelDataRecv.SEQ_SEND.ToString() + " " + LabelDataRecv.NUM_BAR.ToString() + " " + LabelDataRecv.SEQ_LIST.ToString() + " " + LabelDataRecv.LA_BDL_ACT.ToString() + " " + LabelDataRecv.NO_LICENCE + " " + LabelDataRecv.NAME_PROD + " " + LabelDataRecv.NAME_STND + " " + LabelDataRecv.ID_HEAT + " " + LabelDataRecv.NAME_STLGD + " " + LabelDataRecv.DES_FIPRO_SECTION + " " + LabelDataRecv.ID_CREW_RL + " " + LabelDataRecv.ID_CREW_CK + " " + LabelDataRecv.TMSTP_WEIGH + " " + LabelDataRecv.BAR_CODE + " " + LabelDataRecv.NUM_HEAD.ToString() + " " + LabelDataRecv.NUM_TAIL.ToString() + " " + LabelDataRecv.TMSTP_SEND;
                            sql = string.Format("INSERT INTO MESRECVLOG(REC_CREATE_TIME,SEND_CONTENT) VALUES ('{0}','{1}')", DateTime.Now.ToString(("yyyy-MM-dd HH:mm:ss")), str);
                            tm.MultithreadExecuteNonQuery(sql);

                            //反馈接收数据标签信息                        
                            string MessageHead = "PRL3001";                            
                            byte[] sendArray1 = Enumerable.Repeat((byte)0x20, length-19-2).ToArray(); //
                            Array.Copy(buffer, sendArray1, length-19-2);
                            byte[] byteArray1 = Encoding.ASCII.GetBytes(MessageHead);//应答头
                            Buffer.BlockCopy(byteArray1, 0, sendArray1, 0, byteArray1.Length);

                            string appendmsg = "1 &" + " &" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+" &"+ ProductIDA.ToString()+ " &";
                            //byte[] appendArray = System.Text.Encoding.ASCII.GetBytes(appendmsg);
                            byte OldBytes = 0x20;
                            byte NewBytes = 0x7F;
                            byte[] sendArray2 = StripIronNum.ByteReplace(Encoding.ASCII.GetBytes(appendmsg), OldBytes, NewBytes);

                            byte[] sendArray = Enumerable.Repeat((byte)0x20, sendArray1.Length+sendArray2.Length).ToArray();
                            Array.Copy(sendArray1, sendArray, sendArray1.Length);
                            Buffer.BlockCopy(sendArray2, 0, sendArray, sendArray1.Length, sendArray2.Length);

                            MESSocketClient.senddata(sendArray);
                            string strsend = MessageHead + " &" + str + "1" + " &" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " &";
                            string sqlsend = string.Format("INSERT INTO MESSENDLOG(REC_CREATE_TIME,SEND_CONTENT) VALUES ('{0}','{1}')", DateTime.Now.ToString(("yyyy-MM-dd HH:mm:ss")), strsend);
                            tm.MultithreadExecuteNonQuery(sqlsend);
                        }
                        if (MessageFlg == "L3PR02A")//标签结果应答
                        {
                            string MACHINE_NO = GetString(HeadIndex[1], EncodingType.ASCII);
                            string ID_LOT_PROD = GetString(HeadIndex[2], EncodingType.ASCII);
                            short ID_PART_LOT = Convert.ToInt16(GetString(HeadIndex[3], EncodingType.ASCII));
                            short NUM_BDL = Convert.ToInt16(GetString(HeadIndex[4], EncodingType.ASCII));
                            short SEQ_LEN = Convert.ToInt16(GetString(HeadIndex[5], EncodingType.ASCII));
                            short SEQ_OPR = Convert.ToInt16(GetString(HeadIndex[6], EncodingType.ASCII));
                            double SEQ_SEND= Convert.ToDouble(GetString(HeadIndex[7], EncodingType.ASCII));
                            double SEQ_L2 = Convert.ToDouble(GetString(HeadIndex[8], EncodingType.ASCII));
                            short ACK = Convert.ToInt16(GetString(HeadIndex[9], EncodingType.ASCII));
                            string REASON = GetString(HeadIndex[10], EncodingType.ASCII);
                            string TMSTP_SEND = GetString(HeadIndex[11], EncodingType.ASCII);
                            sql = string.Format("UPDATE TLabelContent SET MODELSWTMSTP_SEND='{0}',MODELSWACK={1},MODELSWREASON='{2}' WHERE ID_LOT_PROD='{3}' and ID_PART_LOT={4} and NUM_BDL={5} and SEQ_LEN={6} and SEQ_OPR={7} and SEQ_SEND={8} and SEQ_L2={9}", TMSTP_SEND, ACK, REASON, ID_LOT_PROD, ID_PART_LOT, NUM_BDL, SEQ_LEN, SEQ_OPR, SEQ_SEND, SEQ_L2);
                            tm.MultithreadExecuteNonQuery(sql);
                            string str = MessageFlg.ToString() + " " + ACK + " " + REASON + " " + TMSTP_SEND + " " + ID_LOT_PROD + " " + ID_PART_LOT.ToString() + " " + NUM_BDL.ToString() + " " + SEQ_LEN.ToString() + " " + SEQ_OPR.ToString()+SEQ_SEND.ToString()+SEQ_L2.ToString();
                            sql = string.Format("INSERT INTO MESRECVLOG(REC_CREATE_TIME,SEND_CONTENT) VALUES ('{0}','{1}')", DateTime.Now.ToString(("yyyy-MM-dd HH:mm:ss")), str);
                            tm.MultithreadExecuteNonQuery(sql);
                        }


                    }
                }
                catch (Exception ex)
                {
                    log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString() + "::" + MethodBase.GetCurrentMethod().ToString());
                    Log.addLog(log, LogType.ERROR, ex.Message);
                }
            }
        }
    }
}