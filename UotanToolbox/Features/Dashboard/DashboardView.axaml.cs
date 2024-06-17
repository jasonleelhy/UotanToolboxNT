﻿using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using SukiUI.Controls;
using System;
using System.Diagnostics;
using System.IO;
using UotanToolbox.Common;
using UotanToolbox.Features.Components;

namespace UotanToolbox.Features.Dashboard;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
    }

    private async void OpenUnlockFile(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open File",
            AllowMultiple = false
        });
        if (files.Count >= 1)
        {
            UnlockFile.Text = StringHelper.FilePath(files[0].Path.ToString());
        }
    }

    private async void OpenRecFile(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open File",
            AllowMultiple = false
        });
        if (files.Count >= 1)
        {
            RecFile.Text = StringHelper.FilePath(files[0].Path.ToString());
        }
    }

    private async void OpenMagiskFile(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open File",
            AllowMultiple = false
        });
        if (files.Count >= 1)
        {
            MagiskFile.Text = StringHelper.FilePath(files[0].Path.ToString());
            Global.magisk_tmp = Path.Combine(Global.tmp_path,"Magisk-"+StringHelper.RandomString(8));
            bool istempclean =FileHelper.ClearFolder(Global.magisk_tmp);
            if (istempclean)
            {
                string outputzip = await CallExternalProgram.SevenZip($"x \"{MagiskFile.Text}\" -o\"{Global.magisk_tmp}\" -y");
                string pattern_MAGISK_VER = @"MAGISK_VER='([^']+)'";
                string pattern_MAGISK_VER_CODE = @"MAGISK_VER_CODE=(\d+)";
                string Magisk_sh_path = Path.Combine(Global.magisk_tmp, "assets", "util_functions.sh");
                string MAGISK_VER = StringHelper.FileRegex(Magisk_sh_path, pattern_MAGISK_VER, 1);
                string MAGISK_VER_CODE = StringHelper.FileRegex(Magisk_sh_path, pattern_MAGISK_VER_CODE, 1);
                if ((MAGISK_VER != null) &(MAGISK_VER_CODE != null))
                {
                    string BOOT_PATCH_PATH = Path.Combine(Global.magisk_tmp, "assets", "boot_patch.sh");
                    string md5 = FileHelper.Md5Hash(BOOT_PATCH_PATH);
                    //SukiHost.ShowDialog(new ConnectionDialog(md5), allowBackgroundClose: true);
                    bool Magisk_Valid = StringHelper.Magisk_Validation(md5, MAGISK_VER, MAGISK_VER_CODE);
                }
                else
                {
                    SukiHost.ShowDialog(new ConnectionDialog("未能获取到有效Magisk版本号"), allowBackgroundClose: true);
                }
            }
            else
            {
                SukiHost.ShowDialog(new ConnectionDialog("清理临时目录出错"), allowBackgroundClose: true);
            }
        }
    }

    private async void OpenBootFile(object sender, RoutedEventArgs args)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open File",
            AllowMultiple = false
        });
        if (files.Count >= 1)
        {
            BootFile.Text = StringHelper.FilePath(files[0].Path.ToString());
            Global.boot_tmp = Path.Combine(Global.tmp_path, "Boot-"+StringHelper.RandomString(8));
            string workpath =Global.boot_tmp;
            if (FileHelper.ClearFolder(workpath))
            {
                (string mb_output,Global.mb_exitcode) = await CallExternalProgram.MagiskBoot($"unpack \"{BootFile.Text}\"", Global.boot_tmp);
                if (mb_output.Contains("error")) 
                {
                    SukiHost.ShowDialog(new ConnectionDialog("解包失败"), allowBackgroundClose: true);
                    return;
                }
                string cpio_path = Path.Combine(Global.boot_tmp, "ramdisk.cpio");
                string ramdisk = Path.Combine(Global.boot_tmp, "ramdisk");
                if (Global.System != "Windows")
                {
                    workpath = Path.Combine(Global.boot_tmp, "ramdisk");
                    Directory.CreateDirectory(workpath);
                }
                (string outputcpio,Global.cpio_exitcode) = await CallExternalProgram.MagiskBoot($"cpio \"{cpio_path}\" extract",workpath);
                //SukiHost.ShowDialog(new ConnectionDialog($"cpio \"{cpio_path}\" extract ./ \"{ramdisk}\""), allowBackgroundClose: true);
                string init_info = await CallExternalProgram.File($"\"{Path.Combine(ramdisk, "init")}\"");
                //SukiHost.ShowDialog(new ConnectionDialog(init_info), allowBackgroundClose: true);
                if (init_info.Contains("ARM aarch64"))
                {
                    SukiHost.ShowDialog(new ConnectionDialog("检测到可用AArch64镜像"), allowBackgroundClose: true);
                    ArchList.SelectedItem = "aarch64";
                }else if (init_info.Contains("X86-64"))
                {
                    SukiHost.ShowDialog(new ConnectionDialog("检测到可用X86-64镜像"), allowBackgroundClose: true);
                    ArchList.SelectedItem = "X86-64";
                }else if (init_info.Contains("ARM,"))
                {
                    SukiHost.ShowDialog(new ConnectionDialog("检测到可用ARM镜像"), allowBackgroundClose: true);
                    ArchList.SelectedItem = "armeabi";
                }else if (init_info.Contains(" Intel 80386"))
                {
                    SukiHost.ShowDialog(new ConnectionDialog("检测到可用X86镜像"), allowBackgroundClose: true);
                    ArchList.SelectedItem = "X86";
                }
                init_info = await CallExternalProgram.File($"\"{Path.Combine(ramdisk, "system","bin","init")}\"");
                if (init_info.Contains("ARM aarch64"))
                {
                    SukiHost.ShowDialog(new ConnectionDialog("检测到可用AArch64镜像"), allowBackgroundClose: true);
                    ArchList.SelectedItem = "aarch64";
                }
                else if (init_info.Contains("X86-64"))
                {
                    SukiHost.ShowDialog(new ConnectionDialog("检测到可用X86-64镜像"), allowBackgroundClose: true);
                    ArchList.SelectedItem = "X86-64";
                }
                else if (init_info.Contains("ARM,"))
                {
                    SukiHost.ShowDialog(new ConnectionDialog("检测到可用ARM镜像"), allowBackgroundClose: true);
                    ArchList.SelectedItem = "armeabi";
                }
                else if (init_info.Contains(" Intel 80386"))
                {
                    SukiHost.ShowDialog(new ConnectionDialog("检测到可用X86镜像"), allowBackgroundClose: true);
                    ArchList.SelectedItem = "X86";
                }
            }
        }
    }

    private async void OpenAFDI(object sender, RoutedEventArgs args)
    {
        if (Global.System == "Windows")
        {
            Process.Start(@"drive\adb.exe");
        }
        else
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                SukiHost.ShowDialog(new ConnectionDialog("当前设备无需进行此操作！"), allowBackgroundClose: true);
            });
        }
    }

    private async void Open9008DI(object sender, RoutedEventArgs args)
    {
        if (Global.System == "Windows")
        {
            Process.Start(@"drive\Qualcomm_HS-USB_Driver.exe");
        }
        else
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                SukiHost.ShowDialog(new ConnectionDialog("当前设备无需进行此操作！"), allowBackgroundClose: true);
            });
        }
    }

    private async void OpenUSBP(object sender, RoutedEventArgs args)
    {
        if (Global.System == "Windows")
        {
            string cmd = @"drive\USB3.bat";
            ProcessStartInfo? cmdshell = null;
            cmdshell = new ProcessStartInfo(cmd)
            {
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process? f = Process.Start(cmdshell);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                SukiHost.ShowDialog(new ConnectionDialog("执行完成！"), allowBackgroundClose: true);
            });
        }
        else
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                SukiHost.ShowDialog(new ConnectionDialog("当前设备无需进行此操作！"), allowBackgroundClose: true);
            });
        }
    }
}