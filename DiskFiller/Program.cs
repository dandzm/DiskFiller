using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DiskFiller
{
    class Program
    {
        static void Main(string[] args)
        {
            ShowDrives();//显示每个盘的信息
            List<FillingDrive> userInputFillingDrive = CollectUserInput();//获取输入的每个盘和要设置的可用空间
            Console.WriteLine("填充中");
            List<string> filledFilePathList = FillDataToDrive(userInputFillingDrive);//往磁盘填充数据
            Console.WriteLine("填充完成");
            Console.WriteLine("是否删除填充的数据(输入y表示删除):");
            if (Console.ReadLine().ToLower() == "y")
            {
                DeleteUselessData(filledFilePathList); //删除填充的数据
            }
        }

        /// <summary>
        /// 显示每个盘的信息
        /// </summary>
        public static void ShowDrives()
        {
            IEnumerable<DriveInfo> fixedDiskDrives = GetFixedDiskDrives();
            foreach (var drive in fixedDiskDrives)
            {
                Console.WriteLine("盘符:" + drive.Name + "  可用空间:" + (drive.TotalFreeSpace >> 20).ToString() + "MB");
            }
        }

        /// <summary>
        /// 获取输入的每个盘和要设置的可用空间
        /// </summary>
        /// <returns></returns>
        public static List<FillingDrive> CollectUserInput()
        {
            List<FillingDrive> fillingDriveList = new List<FillingDrive>();
            Console.WriteLine("输入的格式为  盘符,设置的可用空间  \n例如： c,15    表示要设置C盘，并且设置C盘的最后可用空间是15MB ");
            while (true)
            {
                Console.WriteLine("请输入,输入quit结束输入:");
                string input = Console.ReadLine();
                if (input.ToLower() == "quit")
                {
                    break;
                }
                string[] inputArray = input.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                fillingDriveList.Add(new FillingDrive() //这里没判断输入的格式是否正确
                    {
                        DriveName = inputArray[0],
                        NewFreeSpace = Convert.ToInt64(inputArray[1]) << 20
                    }
                    );
            }
            return fillingDriveList;
        }

        /// <summary>
        /// 往磁盘填充数据
        /// </summary>
        /// <param name="fillingDrives"></param>
        /// <returns></returns>
        public static List<string> FillDataToDrive(List<FillingDrive> fillingDrives)
        {
            List<string> filledFilePathList = new List<string>();
            foreach (var item in fillingDrives)
            {
                DriveInfo drive = new DriveInfo(item.DriveName);
                string filledFileName = DateTime.Now.ToString("yyyyMMddHHmmssfffffff") + ".my1111";//填充数据的文件
                string filledFilePath = Path.Combine(drive.RootDirectory.FullName, filledFileName);
                using (FileStream fs = new FileStream(filledFilePath, FileMode.OpenOrCreate))
                {
                    //减去4096是因为设置系统盘时，最后的结果总是多填充了4096字节，所以这里少填充4096字节
                    long fillingSpace = drive.TotalFreeSpace - item.NewFreeSpace -4096;
                    fs.SetLength(fillingSpace);

                } 
                filledFilePathList.Add(filledFilePath);
            }
            return filledFilePathList;
        }

        /// <summary>
        /// 删除填充的数据
        /// </summary>
        /// <param name="filledFilePathList"></param>
        public static void DeleteUselessData(List<string> filledFilePathList)
        {
            foreach (var path in filledFilePathList)
            {
                File.Delete(path);
            }
        }

        /// <summary>
        /// 获取磁盘
        /// </summary>
        /// <returns></returns>
        internal static IEnumerable<DriveInfo> GetFixedDiskDrives()
        {
            DriveInfo[] totalDrives = DriveInfo.GetDrives();
            IEnumerable<DriveInfo> fixedDiskDrives = totalDrives.Where(p => p.DriveType == DriveType.Fixed);
            return fixedDiskDrives;
        }

        public class FillingDrive
        {
            /// <summary>
            /// 盘符
            /// </summary>
            public string DriveName { get; set; }

            /// <summary>
            /// 填充到指定大小
            /// </summary>
            public long NewFreeSpace { get; set; }
        }
    }
}
