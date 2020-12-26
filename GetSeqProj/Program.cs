using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;

namespace GetSeqProj
{
    class Program
    {
        private static string ConnString = @"data source=.\sqlexpress;initial catalog=YiIms;integrated security=True;Trusted_Connection=True;MultipleActiveResultSets=true";
        /// <summary>
        /// 其中一個執行緒 Thread1，靠DB 鎖取號，
        ///   另一個執行行 Thread2，是靠 執行緒鎖取號，
        ///   
        /// 然後將取來的號碼填入AAA的資料表當中，觀察 AAA的資料可以判斷取號是否有重複
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            List<Thread> Threads = new List<Thread>();//建立執行緒list
            //load thread
            for (int i = 0; i < 10; i++)
            {
                Threads.Add(new Thread(Thread1));//這裡可以切換thread1 thread2
            }
            //run thread
            foreach (var item in Threads)
            {
                item.Start();
            }
            //並沒有實作判斷執行緒是否結束
        }
        //靠db 鎖取號並填入AAA的執行緒
        private static void Thread1()
        {
            InsertToAAA(取號靠DB鎖());
        }
        //靠thread鎖並取號並填入AAA的執行緒
        private static void Thread2()
        {
            InsertToAAA(取號靠執行緒鎖());
        }

        #region 取號靠DB鎖
        /// <summary>
        /// 取號靠DB鎖
        /// </summary>
        /// <returns></returns>
        private static long 取號靠DB鎖()
        {
            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                conn.Open();
                IsolationLevel isolationLevel = IsolationLevel.Serializable;
                //repeat
                //  Unspecified
                //  ReadCommitted
                //  ReadUncommitted

                //exception
                //  Serializable - like deadlock
                //  Chaos - unknown
                //SqlTransaction transaction = conn.BeginTransaction("SampleTransaction");
                SqlTransaction transaction = conn.BeginTransaction(isolationLevel, "SampleTransaction");
                //transaction.IsolationLevel
                SqlCommand command = conn.CreateCommand();
                command.Connection = conn;
                command.Transaction = transaction;

                try
                {
                    //讀取seq
                    command.CommandText = "select SeqNo from SeqTable where [SeqId]=@SeqId";
                    command.Parameters.AddWithValue("@SeqId", "Doc1");
                    long SeqNo = (long)command.ExecuteScalar();
                    //更新seq
                    command.CommandText = "update SeqTable set SeqNo = @SeqNo where [SeqId]=@SeqId";
                    //command.Parameters.AddWithValue("@SeqId", "Doc1");
                    command.Parameters.AddWithValue("@SeqNo", SeqNo + 1);
                    command.ExecuteNonQuery();
                    //commit
                    transaction.Commit();
                    return SeqNo;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Commit Exception Type: {0}", ex.GetType());
                    Console.WriteLine("  Message: {0}", ex.Message);

                    // Attempt to roll back the transaction.
                    try
                    {
                        transaction.Rollback();
                    }
                    catch (Exception ex2)
                    {
                        // This catch block will handle any errors that may have occurred
                        // on the server that would cause the rollback to fail, such as
                        // a closed connection.
                        Console.WriteLine("Rollback Exception Type: {0}", ex2.GetType());
                        Console.WriteLine("  Message: {0}", ex2.Message);
                        throw;
                    }
                    throw;
                }
            }
        }
        #endregion

        #region 取號靠執行緒鎖
        /// <summary>
        /// 取號靠執行緒鎖
        /// </summary>
        private readonly static object __lockObj = new object();
        private static long 取號靠執行緒鎖()
        {
            lock (__lockObj)
            {
                using (SqlConnection conn = new SqlConnection(ConnString))
                {
                    conn.Open();
                    SqlCommand command = conn.CreateCommand();
                    //讀取seq
                    command.CommandText = "select SeqNo from SeqTable where [SeqId]=@SeqId";
                    command.Parameters.AddWithValue("@SeqId", "Doc1");
                    long SeqNo = (long)command.ExecuteScalar();
                    //更新seq
                    command.CommandText = "update SeqTable set SeqNo = @SeqNo where [SeqId]=@SeqId";
                    command.Parameters.AddWithValue("@SeqNo", SeqNo + 1);
                    command.ExecuteNonQuery();
                    return SeqNo;
                }
            }
        }
        #endregion
 
        #region 將資料insert到 AAA 資料表
        private static void InsertToAAA(long SeqNo)
        {
            using (SqlConnection conn = new SqlConnection(ConnString))
            {
                conn.Open();
                SqlCommand command = conn.CreateCommand();
                command.Connection = conn;
                command.CommandText = "insert into AAA (SeqNo) values (@SeqNo)";
                command.Parameters.AddWithValue("@SeqNo", SeqNo);
                command.ExecuteNonQuery();
            }
        } 
        #endregion

    }
}
