﻿using Avalonia.Collections;

namespace UotanToolbox.Common
{
    internal class Global
    {
        public static bool checkdevice = true;
        public static string runpath = null;
        public static string bin_path = null;
        public static string tmp_path = null;
        public static string log_path = null;
        public static string backup_path = null;
        public static string System = "Windows";
        public static AvaloniaList<string> deviceslist;
        public static string thisdevice = null;
        public static ZipInfo Zipinfo;
        public static int mb_exitcode, cpio_exitcode, load_times;
        //分区表储存
        public static string sdatable = "";
        public static string sdbtable = "";
        public static string sdctable = "";
        public static string sddtable = "";
        public static string sdetable = "";
        public static string sdftable = "";
        public static string emmcrom = "";

        //工具箱版本
        public static string currentVersion = "819a867";
    }
    public class BootInfo
    {
        public static string SHA1;
        public static string tmp_path;
        public static bool userful;
        public static bool gki2;
        public static string version;
        public static string kmi;
        public static string os_version;
        public static string patch_level;
        public static bool have_ramdisk;
        public static bool have_kernel;
        public static bool have_dtb;
        public static string dtb_name;
        public static string arch;
    }
    public class ZipInfo
    {
        public string Path { get; set; }
        public string SHA1 { get; set; }
        public string Version { get; set; }
        public string KMI { get; set; }
        public string TempPath { get; set; }
        public bool IsUseful { get; set; }
        public PatchMode Mode { get; set; }
        public string SubSHA1 { get; set; }

        public ZipInfo(string path, string sha1, string version, string kmi, string tempPath, bool isUseful, PatchMode mode, string subSHA1)
        {
            Path = path;
            SHA1 = sha1;
            Version = version;
            KMI = kmi;
            TempPath = tempPath;
            IsUseful = isUseful;
            Mode = mode;
            SubSHA1 = subSHA1;
        }
    }
    public class EnvironmentVariable
    {
        public static string KEEPVERITY = "true";
        public static string KEEPFORCEENCRYPT = "true";
        public static string PATCHVBMETAFLAG = "false";
        public static string RECOVERYMODE = "false";
        public static string LEGACYSAR = "true";
    }

    public static class GlobalData
    {
        public static MainViewModel MainViewModelInstance { get; set; }
    }
    public enum PatchMode
    {
        Magisk,
        Apatch,
        KernelSU,
        None
    }

}