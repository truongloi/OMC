using System;
using System.Windows.Forms;
using DevExpress.XtraBars;

// goi them thu vien
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.Drawing;
using Telnet_OMC.WS_CSKH;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;

namespace Telnet_OMC
{
    public partial class frmMain : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        private bool _preventExpand = false;
        private DateTime _lastMouseDown = DateTime.Now;
        frmLogin _frm;

        public frmMain(frmLogin frm)
        {
            InitializeComponent();
            _frm = frm;
            splashScreenManager1 = new DevExpress.XtraSplashScreen.SplashScreenManager(this, typeof(WaitForm1), true, true);
        }
        // Load form
        private void frmMain_Load(object sender, EventArgs e)
        {
            // Đóng các dock
            dockPanel_CAM_TAMBINH_Dsach.Close();
            dockPanel_CAM_BINHMINH_Dsach.Close();
            dockPanel_CAM_VUNGLIEM_Dsach.Close();
            //dockPanelLOG.Close();

            // Ket noi file ini
            INIFile inif = new INIFile(@"d:\OMC CAMERA\ConfigFile\Config.ini");
            string menu = inif.Read("CONFIG", "LOAD");
            string[] list_menu = tachChuoi(menu);
            foreach(string m in list_menu)
            {
                if (m == "BM")
                {
                    ribbonPageGroupBinhMinh.Visible = true;
                    //barButtonItemCAMBINHMINH_ItemClick(null, null);
                }
                if (m == "TB")
                {
                    ribbonPageGroupTamBinh.Visible = true;
                    //barButtonItemCAMTAMBINH_ItemClick(null, null);
                }
                if (m == "VL")
                {
                    ribbonPageGroupVungLiem.Visible = true;
                    //barButtonItemCAMVUNGLIEM_ItemClick(null, null);
                }
            }

            //this.TopMost = true;
            //this.FormBorderStyle = FormBorderStyle.None;
            // full màn hình
            this.WindowState = FormWindowState.Maximized;
        }
        private void barButtonItemCAMTAMBINH_ItemClick(object sender, ItemClickEventArgs e)
        {
            splashScreenManager1.ShowWaitForm();
            // TAM BINH
            // Ket noi file ini
            INIFile inif = new INIFile(@"d:\OMC CAMERA\ConfigFile\Conf_CAM_TAMBINH.ini");
            Debug.WriteLine("LOAD DANH SÁCH TAM BÌNH");
            //txtLOG.Text += "LOAD DANH SÁCH TAM BÌNH" + "\r\n";
            // Load thông tin kết nối từ file
            loadCam(treeView_CAM_TAMBINH_Dsach, inif);
            // Set thời gian timeout
            timer1.Interval = int.Parse(inif.Read("CONFIG", "TIMEOUT"));
            // Mở dock
            dockPanel_CAM_TAMBINH_Dsach.Close();
            dockPanel_CAM_TAMBINH_Dsach.Show();
            splashScreenManager1.CloseWaitForm();
        }

        private void barButtonItemCAMBINHMINH_ItemClick(object sender, ItemClickEventArgs e)
        {
            splashScreenManager1.ShowWaitForm();
            // BINH MINH
            // Ket noi file ini
            INIFile inif_bm = new INIFile(@"d:\OMC CAMERA\ConfigFile\Conf_CAM_BINHMINH.ini");
            Debug.WriteLine("LOAD DANH SÁCH BÌNH MINH");
            //txtLOG.Text += "LOAD DANH SÁCH BÌNH MINH" + "\r\n";
            // Load thông tin kết nối từ file
            loadCam(treeView_CAM_BINHMINH_Dsach, inif_bm);
            // Set thời gian timeout
            timer2.Interval = int.Parse(inif_bm.Read("CONFIG", "TIMEOUT"));
            // Mở dock
            dockPanel_CAM_BINHMINH_Dsach.Close();
            dockPanel_CAM_BINHMINH_Dsach.Show();
            TreeNode tnParent = new TreeNode();
            splashScreenManager1.CloseWaitForm();
        }
        private void barButtonItemCAMVUNGLIEM_ItemClick(object sender, ItemClickEventArgs e)
        {
            splashScreenManager1.ShowWaitForm();
            // VUNG LIEM
            // Ket noi file ini
            INIFile inif_bm = new INIFile(@"d:\OMC CAMERA\ConfigFile\Conf_CAM_VUNGLIEM.ini");
            Debug.WriteLine("LOAD DANH SÁCH VŨNG LIÊM");
            //txtLOG.Text += "LOAD DANH SÁCH VŨNG LIÊM" + "\r\n";
            // Load thông tin kết nối từ file
            loadCam(treeView_CAM_VUNGLIEM_Dsach, inif_bm);
            // Set thời gian timeout
            timer3.Interval = int.Parse(inif_bm.Read("CONFIG", "TIMEOUT"));
            // Mở dock
            dockPanel_CAM_VUNGLIEM_Dsach.Close();
            dockPanel_CAM_VUNGLIEM_Dsach.Show();
            TreeNode tnParent = new TreeNode();
            splashScreenManager1.CloseWaitForm();
        }
        private void barButtonItem2_ItemClick(object sender, ItemClickEventArgs e)
        {
            splashScreenManager1.ShowWaitForm();
            dockPanelLOG.Close();
            dockPanelLOG.Show();
            splashScreenManager1.CloseWaitForm();
        }
        // Check 1 node
        private void checkManual(TreeView node, string tentram, INIFile inif)
        {
            // Ping
            Ping ping = new Ping();
            PingReply pingresult = ping.Send(inif.Read(bientoancuc.TenTram, "IP"));
            DateTime now = DateTime.Now;
            cskh WS = new cskh();
            if (pingresult.Status.ToString() == "Success")
            {
                Debug.WriteLine(bientoancuc.TenTram + ". Success, IP: " + pingresult.Address.ToString() + ": bytes=" + pingresult.Buffer.Length
                    + " time=" + pingresult.RoundtripTime + "ms TTL=" + pingresult.Options.Ttl);
                txtLOG.Text += bientoancuc.TenTram + ". Success, IP: " + pingresult.Address.ToString() + ": bytes=" + pingresult.Buffer.Length
                    + " time=" + pingresult.RoundtripTime + "ms TTL=" + pingresult.Options.Ttl + "\r\n";
                if(inif.Read(bientoancuc.TenTram, "Connect") == "0")
                {
                    // Set màu
                    SearchTree(node.Nodes, bientoancuc.TenTram).BackColor = Color.FromArgb(192, 255, 192);
                    inif.Write(bientoancuc.TenTram, "Connect", "1");
                    string sdt = inif.Read(bientoancuc.TenTram, "SDT");
                    if (inif.Read("CONFIG", "MESSAGE_AGAIN") == "1")
                    {
                        foreach (String s in tachChuoi(sdt))
                        {
                            string nd = "KET NOI LAI: " + bientoancuc.TenTram + " ip: " + replaceIP(inif.Read(bientoancuc.TenTram, "IP")) + " vi tri: " + inif.Read(bientoancuc.TenTram, "Name");
                            string rs = WS.SendSMS6_VNPT(s, nd, "vnpthaiphong_cskh_pttb");
                            txtLOG.Text += "SDT: " + s + "   " + nd + "\r\n";
                            logFileSMS(now.ToString(), s, nd);
                            thongKe(node, inif);
                        }
                    }
                }
            }

            else
            {
                Debug.WriteLine(bientoancuc.TenTram + ". Error: " + pingresult.Status.ToString() + "; IP: " + inif.Read(bientoancuc.TenTram, "IP")
                    + "; Điểm: " + inif.Read(bientoancuc.TenTram, "Name"));
                txtLOG.Text += bientoancuc.TenTram + ". Error: " + pingresult.Status.ToString() + "; IP: " + inif.Read(bientoancuc.TenTram, "IP")
                    + "; Điểm: " + inif.Read(bientoancuc.TenTram, "Name") + "\r\n";
                if (inif.Read(bientoancuc.TenTram, "Connect") == "1")
                {
                    // Set màu
                    SearchTree(node.Nodes, bientoancuc.TenTram).BackColor = Color.FromArgb(255, 128, 128);
                    inif.Write(bientoancuc.TenTram, "Connect", "0");
                    string sdt = inif.Read(bientoancuc.TenTram, "SDT");
                    if (inif.Read("CONFIG", "MESSAGE") == "1")
                    {
                        foreach (String s in tachChuoi(sdt))
                        {
                            string nd = "MAT KET NOI: " + bientoancuc.TenTram + " ip: " + replaceIP(inif.Read(bientoancuc.TenTram, "IP")) + " vi tri: " + inif.Read(bientoancuc.TenTram, "Name");
                            string rs = WS.SendSMS6_VNPT(s, nd, "vnpthaiphong_cskh_pttb");
                            txtLOG.Text += "SDT: " + s + "   " + nd + "\r\n";
                            logFileSMS(now.ToString(), s, nd);
                            thongKe(node, inif);
                        }
                    }
                }
            }
        }
        // Checkbox tự động
        private void checkEditAuto_TAMBINH_CheckStateChanged(object sender, EventArgs e)
        {
            // Ket noi file ini
            INIFile inif = new INIFile(@"d:\OMC CAMERA\ConfigFile\Conf_CAM_TAMBINH.ini");
            DateTime now = DateTime.Now;
            if (checkEditAuto_TAMBINH.CheckState == CheckState.Checked)
            {
                Debug.WriteLine("-- " + now.ToString() + ":   CHECK TỰ ĐỘNG TAM BÌNH");
                txtLOG.Text += "-- " + now.ToString() + ":   CHECK TỰ ĐỘNG TAM BÌNH: " + (int.Parse(inif.Read("CONFIG", "TIMEOUT"))/60000).ToString() + " phút \r\n";
                logFile(now.ToString(), "CHECK TỰ ĐỘNG TAM BÌNH: " + (int.Parse(inif.Read("CONFIG", "TIMEOUT")) / 60000).ToString() + " phút");
                //checkAuto(treeView_CAM_TAMBINH_Dsach, inif);
                //thongKe(treeView_CAM_TAMBINH_Dsach, inif);
                // Bật check tự động
                timer1.Enabled = true;
            }
            else
            {
                Debug.WriteLine("-- " + now.ToString() + ":   TẮT CHECK TỰ ĐỘNG TAM BÌNH");
                txtLOG.Text += "-- " + now.ToString() + ":   TẮT CHECK TỰ ĐỘNG TAM BÌNH \r\n";
                logFile(now.ToString(), "TẮT CHECK TỰ ĐỘNG TAM BÌNH");
                // Tắt check tự động
                timer1.Enabled = false;
            }
        }
        // Checkbox tự động
        private void checkEditAuto_BINHMINH_CheckStateChanged(object sender, EventArgs e)
        {
            // Ket noi file ini
            INIFile inif = new INIFile(@"d:\OMC CAMERA\ConfigFile\Conf_CAM_BINHMINH.ini");
            DateTime now = DateTime.Now;

            if (checkEditAuto_BINHMINH.CheckState == CheckState.Checked)
            {
                Debug.WriteLine("-- " + now.ToString() + ":   CHECK TỰ ĐỘNG BÌNH MINH");
                txtLOG.Text += "-- " + now.ToString() + ":   CHECK TỰ ĐỘNG BÌNH MINH: " + (int.Parse(inif.Read("CONFIG", "TIMEOUT"))/60000).ToString() + " phút \r\n";
                logFile(now.ToString(), "CHECK TỰ ĐỘNG BÌNH MINH: " + (int.Parse(inif.Read("CONFIG", "TIMEOUT")) / 60000).ToString() + " phút");
                //checkAuto(treeView_CAM_BINHMINH_Dsach, inif);
                //thongKe(treeView_CAM_BINHMINH_Dsach, inif);
                // Bật check tự động
                timer2.Enabled = true;
            }
            else
            {
                Debug.WriteLine("-- " + now.ToString() + ":   TẮT CHECK TỰ ĐỘNG BÌNH MINH");
                txtLOG.Text += "-- " + now.ToString() + ":   TẮT CHECK TỰ ĐỘNG BÌNH MINH \r\n";
                logFile(now.ToString(), "TẮT CHECK TỰ ĐỘNG BÌNH MINH");
                // Tắt check tự động
                timer2.Enabled = false;
            }
        }
        // Checkbox tự động
        private void checkEditAuto_VUNGLIEM_CheckStateChanged(object sender, EventArgs e)
        {
            // Ket noi file ini
            INIFile inif = new INIFile(@"d:\OMC CAMERA\ConfigFile\Conf_CAM_VUNGLIEM.ini");
            DateTime now = DateTime.Now;

            if (checkEditAuto_VUNGLIEM.CheckState == CheckState.Checked)
            {
                Debug.WriteLine("-- " + now.ToString() + ":   CHECK TỰ ĐỘNG VŨNG LIÊM");
                txtLOG.Text += "-- " + now.ToString() + ":   CHECK TỰ ĐỘNG VŨNG LIÊM: " + (int.Parse(inif.Read("CONFIG", "TIMEOUT")) / 60000).ToString() + " phút \r\n";
                logFile(now.ToString(), "CHECK TỰ ĐỘNG VŨNG LIÊM: " + (int.Parse(inif.Read("CONFIG", "TIMEOUT")) / 60000).ToString() + " phút");
                //checkAuto(treeView_CAM_VUNGLIEM_Dsach, inif);
                //thongKe(treeView_CAM_VUNGLIEM_Dsach, inif);
                // Bật check tự động
                timer3.Enabled = true;
            }
            else
            {
                Debug.WriteLine("-- " + now.ToString() + ":   TẮT CHECK TỰ ĐỘNG VŨNG LIÊM");
                txtLOG.Text += "-- " + now.ToString() + ":   TẮT CHECK TỰ ĐỘNG VŨNG LIÊM \r\n";
                logFile(now.ToString(), "TẮT CHECK TỰ ĐỘNG VŨNG LIÊM");
                // Tắt check tự động
                timer3.Enabled = false;
            }
        }
        // Hiện thị tất cả
        public void CheckAllNodes(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                node.Expand();
            }
        }
        // Thu gọn
        public void UncheckAllNodes(TreeNodeCollection nodes)
        {
            foreach (TreeNode node in nodes)
            {
                node.Collapse();
            }
        }
        // Tìm kiếm node
        private TreeNode SearchTree(TreeNodeCollection nodes, string searchtext)
        {
            TreeNode n_found_node = null;
            bool b_node_found = false;
            foreach (TreeNode node in nodes)
            {
                if (node.Name == searchtext)
                {
                    b_node_found = true;
                    n_found_node = node;
                }
                if (!b_node_found)
                {
                    foreach (TreeNode node_level2 in node.Nodes)
                    {
                        if (node_level2.Name == searchtext)
                        {
                            b_node_found = true;
                            n_found_node = node_level2;
                        }
                    }
                }
            }
            return n_found_node;
        }
        // Check kết nối
        private void checkAuto(TreeView node, INIFile inif)
        {
            // DS kết nối lại
            var list_knl = new List<Data>();
            // DS mất kết nối
            var list_mkn = new List<Data>();
            Debug.WriteLine("BĐ=============================");
            txtLOG.Text += "BĐ=============================" + "\r\n";
            foreach (TreeNode node_level1 in node.Nodes)
            {
                // Ping
                Ping ping = new Ping();
                PingReply pingresult = ping.Send(inif.Read(node_level1.Name, "IP"));
                if (pingresult.Status.ToString() == "Success")
                {
                    Debug.WriteLine("+ " + node_level1.Name + ". Success, IP: " + pingresult.Address.ToString() + ": bytes=" + pingresult.Buffer.Length
                        + " time=" + pingresult.RoundtripTime + "ms TTL=" + pingresult.Options.Ttl);
                    //txtLOG.Text += "+ " + node_level1.Name + ". Success, IP: " + pingresult.Address.ToString() + ": bytes=" + pingresult.Buffer.Length
                    //    + " time=" + pingresult.RoundtripTime + "ms TTL=" + pingresult.Options.Ttl + "\r\n";
                    SearchTree(node.Nodes, node_level1.Name).BackColor = Color.FromArgb(192, 255, 192);
                    // Có kết nối lại
                    if(inif.Read(node_level1.Name, "Connect") == "0")
                    {
                        string node1 = node_level1.Name + " ip: " + replaceIP(inif.Read(node_level1.Name, "IP")) + " vi tri: " + inif.Read(node_level1.Name, "Name") + "; ";
                        string sdt = inif.Read(node_level1.Name, "SDT");
                        foreach (String s in tachChuoi(sdt))
                        {
                            list_knl.Add(new Data(s, node1));
                        }

                        inif.Write(node_level1.Name, "Connect", "1");
                        inif.Write(node_level1.Name, "Disconnect", "0");
                    }
                }
                else
                {
                    Debug.WriteLine("+ " + node_level1.Name + ". Error: " + pingresult.Status.ToString() + "; IP: " + inif.Read(node_level1.Name, "IP")
                        + "; Điểm: " + inif.Read(node_level1.Name, "Name"));
                    txtLOG.Text += "+ " + node_level1.Name + ". Error: " + pingresult.Status.ToString() + "; IP: " + inif.Read(node_level1.Name, "IP")
                        + "; Điểm: " + inif.Read(node_level1.Name, "Name") + "\r\n";
                    SearchTree(node.Nodes, node_level1.Name).BackColor = Color.FromArgb(255, 128, 128);
                    inif.Write(node_level1.Name, "Disconnect", (int.Parse(inif.Read(node_level1.Name, "Disconnect")) + 1).ToString());

                    string node1 = node_level1.Name + " ip: " + replaceIP(inif.Read(node_level1.Name, "IP")) + " vi tri: " + inif.Read(node_level1.Name, "Name") + "; ";
                    string sdt = inif.Read(node_level1.Name, "SDT");
                    foreach (String s in tachChuoi(sdt))
                    {
                        list_mkn.Add(new Data(s, node1));
                    }

                    inif.Write(node_level1.Name, "Connect", "0");
                    // Rớt n lần trở lên
                    //if (int.Parse(inif.Read(node_level1.Name, "Disconnect")) >= int.Parse(inif.Read("CONFIG", "DISCONNECT")))
                    //{
                        
                    //}
                }
                foreach (TreeNode node_level2 in node_level1.Nodes)
                {
                    Ping ping1 = new Ping();
                    PingReply pingresult1 = ping1.Send(inif.Read(node_level2.Name, "IP"));
                    if (pingresult1.Status.ToString() == "Success")
                    {
                        Debug.WriteLine("         - " + node_level2.Name + ". Success, IP: " + pingresult1.Address.ToString() + ": bytes=" + pingresult1.Buffer.Length
                            + " time=" + pingresult1.RoundtripTime + "ms TTL=" + pingresult1.Options.Ttl);
                        //txtLOG.Text += "         - " + node_level2.Name + ". Success, IP: " + pingresult1.Address.ToString() + ": bytes=" + pingresult1.Buffer.Length
                        //    + " time=" + pingresult1.RoundtripTime + "ms TTL=" + pingresult1.Options.Ttl + "\r\n";
                        SearchTree(node_level1.Nodes, node_level2.Name).BackColor = Color.FromArgb(192, 255, 192);
                        // Có kết nối lại
                        if (inif.Read(node_level2.Name, "Connect") == "0")
                        {
                            string node2 = node_level2.Name + " ip: " + replaceIP(inif.Read(node_level2.Name, "IP")) + " vi tri: " + inif.Read(node_level2.Name, "Name") + "; ";
                            string sdt = inif.Read(node_level2.Name, "SDT");
                            foreach (String s in tachChuoi(sdt))
                            {
                                list_knl.Add(new Data(s, node2));
                            }

                            inif.Write(node_level2.Name, "Connect", "1");
                            inif.Write(node_level2.Name, "Disconnect", "0");
                        }
                    }

                    else
                    {
                        Debug.WriteLine("         - " + node_level2.Name + ". Error: " + pingresult1.Status.ToString() + "; IP: " + inif.Read(node_level2.Name, "IP")
                            + "; Điểm: " + inif.Read(node_level2.Name, "Name"));
                        txtLOG.Text += "         - " + node_level2.Name + ". Error: " + pingresult1.Status.ToString() + "; IP: " + inif.Read(node_level2.Name, "IP")
                            + "; Điểm: " + inif.Read(node_level2.Name, "Name") + "\r\n";
                        SearchTree(node_level1.Nodes, node_level2.Name).BackColor = Color.FromArgb(255, 128, 128);
                        inif.Write(node_level2.Name, "Disconnect", (int.Parse(inif.Read(node_level2.Name, "Disconnect")) + 1).ToString());

                        string node2 = node_level2.Name + " ip: " + replaceIP(inif.Read(node_level2.Name, "IP")) + " vi tri: " + inif.Read(node_level2.Name, "Name") + "; ";
                        string sdt = inif.Read(node_level2.Name, "SDT");
                        foreach (String s in tachChuoi(sdt))
                        {
                            list_mkn.Add(new Data(s, node2));
                        }

                        inif.Write(node_level2.Name, "Connect", "0");
                        // Rớt n lần trở lên
                        //if (int.Parse(inif.Read(node_level2.Name, "Disconnect")) >= int.Parse(inif.Read("CONFIG", "DISCONNECT")))
                        //{
                            
                        //}
                    }
                }
            }
            Debug.WriteLine("KT=============================");
            txtLOG.Text += "KT=============================" + "\r\n";

            cskh WS = new cskh();
            // Gửi tin nhắn kết nối lại
            List<string> listSDT_knl = list_knl.Select(x => x.SDT).Distinct().ToList();
            if (listSDT_knl.Count > 0)
            {
                if (inif.Read("CONFIG", "MESSAGE_AGAIN") == "1")
                {
                    foreach (string sdt in listSDT_knl)
                    {
                        string sms = "KET NOI LAI: ";
                        int num = 0;
                        foreach (Data note in list_knl)
                        {
                            if (note.SDT == sdt)
                            {
                                ++num;
                                sms = sms + num.ToString() + ". " + note.content;
                            }
                        }
                        DateTime now = DateTime.Now;
                        DateTime today = DateTime.Today;
                        DateTime bd_sang = today.AddHours(7);
                        DateTime kt_sang = today.AddHours(11);
                        DateTime bd_chieu = today.AddHours(13);
                        DateTime kt_chieu = today.AddHours(17);

                        if (betweenDate(now, bd_sang, kt_sang) || betweenDate(now, bd_chieu, kt_chieu))
                        {
                            string rs = WS.SendSMS6_VNPT(sdt, sms, "vnpthaiphong_cskh_pttb");
                            txtLOG.Text += "SDT: " + sdt + "   " + sms + "\r\n";
                            logFileSMS(now.ToString(), sdt, sms);
                        }
                    }
                }
            }
            // Gửi tin nhắn mất kết nối
            List<string> listSDT_mkn = list_mkn.Select(x => x.SDT).Distinct().ToList();
            if (listSDT_mkn.Count > 0)
            {
                if (inif.Read("CONFIG", "MESSAGE") == "1")
                {
                    foreach (string sdt in listSDT_mkn)
                    {
                        string sms = "MAT KET NOI: ";
                        int num = 0;
                        foreach (Data note in list_mkn)
                        {
                            if (note.SDT == sdt)
                            {
                                ++num;
                                sms = sms + num.ToString() + ". " + note.content;
                            }
                        }
                        DateTime now = DateTime.Now;
                        DateTime today = DateTime.Today;
                        DateTime bd_sang = today.AddHours(7);
                        DateTime kt_sang = today.AddHours(11);
                        DateTime bd_chieu = today.AddHours(13);
                        DateTime kt_chieu = today.AddHours(17);

                        if (betweenDate(now, bd_sang, kt_sang) || betweenDate(now, bd_chieu, kt_chieu))
                        {
                            string rs = WS.SendSMS6_VNPT(sdt, sms, "vnpthaiphong_cskh_pttb");
                            txtLOG.Text += "SDT: " + sdt + "   " + sms + "\r\n";
                            logFileSMS(now.ToString(), sdt, sms);
                        }
                    }
                }
            }
        }
        private bool betweenDate(DateTime input, DateTime date1, DateTime date2)
        {
            return (input > date1 && input < date2);
        }
        // Load kết nối từ file
        private void loadCam(TreeView node, INIFile inif)
        {
            node.Nodes.Clear();
            for (int n = 1; n <= int.Parse(inif.Read("CONFIG", "GW")); n++)
            {
                // add nodes GW
                TreeNode tnParent = new TreeNode();
                tnParent.Text = inif.Read("GW_" + n.ToString(), "Name");
                tnParent.Name = "GW_" + n.ToString();
                node.Nodes.Add(tnParent);
                // add nodes DG
                for (int m = 1; m <= int.Parse(inif.Read("CONFIG", "DG")); m++)
                {
                    if("GW_" + n.ToString() == inif.Read("DG_" + m.ToString(), "PARENT"))
                    {
                        TreeNode child = new TreeNode();

                        child.Text = "Dau ghi " + m.ToString();
                        child.Name = "DG_" + m.ToString();
                        tnParent.Nodes.Add(child);
                    }
                }
                for (int k = 1; k <= int.Parse(inif.Read("CONFIG", "CAM")); k++)
                {
                    if ("GW_" + n.ToString() == inif.Read("CAM_" + k.ToString(), "PARENT"))
                    {
                        TreeNode child = new TreeNode();

                        child.Text = "CAM " + k.ToString();
                        child.Name = "CAM_" + k.ToString();
                        tnParent.Nodes.Add(child);
                    }
                }
            }
            
            foreach (TreeNode node_level1 in node.Nodes)
            {
                if(inif.Read(node_level1.Name, "Connect") == "1")
                {
                    SearchTree(node.Nodes, node_level1.Name).BackColor = Color.FromArgb(192, 255, 192);
                }
                else if(inif.Read(node_level1.Name, "Connect") == "0")
                {
                    SearchTree(node.Nodes, node_level1.Name).BackColor = Color.FromArgb(255, 128, 128);
                }
                foreach (TreeNode node_level2 in node_level1.Nodes)
                {
                    if (inif.Read(node_level2.Name, "Connect") == "1")
                    {
                        SearchTree(node_level1.Nodes, node_level2.Name).BackColor = Color.FromArgb(192, 255, 192);
                    }
                    else if (inif.Read(node_level2.Name, "Connect") == "0")
                    {
                        SearchTree(node_level1.Nodes, node_level2.Name).BackColor = Color.FromArgb(255, 128, 128);
                    }
                }
            }
            thongKe(node, inif);
        }
        public struct Data
        {
            public Data(string intValue, string strValue)
            {
                SDT = intValue;
                content = strValue;
            }

            public string SDT { get; private set; }
            public string content { get; private set; }
        }
        private string replaceIP(string ip)
        {
            string str1 = Regex.Replace(ip.Substring(0, 7), @"\d", "X");
            string str2 = ip.Substring(7, ip.Length - 7);
            return str1 + str2;
        }
        private string [] tachChuoi(string sdt)
        {
            char[] spearator = { ',', ' ' };
            Int32 count = 3;
            return sdt.Split(spearator, count);
        }
        // Gửi tin nhắn từ file
        private void sendMessageFromFile(TreeView node, INIFile inif)
        {
            var list = new List<Data>();
            foreach (TreeNode node_level1 in node.Nodes)
            {
                if (inif.Read(node_level1.Name, "Connect") == "0")
                {
                    string node1 = node_level1.Name + " ip: " + replaceIP(inif.Read(node_level1.Name, "IP")) + " vi tri: " + inif.Read(node_level1.Name, "Name") + "; ";
                    string sdt = inif.Read(node_level1.Name, "SDT");
                    
                    foreach (String s in tachChuoi(sdt))
                    {
                        list.Add(new Data(s, node1));
                    }
                }
                foreach (TreeNode node_level2 in node_level1.Nodes)
                {
                    if (inif.Read(node_level2.Name, "Connect") == "0")
                    {
                        string node2 = node_level2.Name + " ip: " + replaceIP(inif.Read(node_level2.Name, "IP")) + " vi tri: " + inif.Read(node_level2.Name, "Name") + "; ";
                        string sdt = inif.Read(node_level2.Name, "SDT");
                        foreach (String s in tachChuoi(sdt))
                        {
                            list.Add(new Data(s, node2));
                        }
                    }
                }
            }
            List<string> listSDT = list.Select(x => x.SDT).Distinct().ToList();
            if (listSDT.Count > 0) {
                if (inif.Read("CONFIG", "MESSAGE") == "1")
                {
                    cskh WS = new cskh();
                    foreach (string sdt in listSDT)
                    {
                        string sms = "MAT KET NOI: ";
                        int num = 0;
                        foreach (Data note in list)
                        {
                            if (note.SDT == sdt)
                            {
                                ++ num;
                                sms = sms + num.ToString() + ". " + note.content;
                            }
                        }
                        DateTime now = DateTime.Now;
                        string rs = WS.SendSMS6_VNPT(sdt, sms, "vnpthaiphong_cskh_pttb");
                        txtLOG.Text += "SDT: " + sdt + "   " + sms + "\r\n";
                        logFileSMS(now.ToString(), sdt, sms);
                    }
                }
            }
        }
        // Ghi log nhắn tin
        private void logFileSMS(string time, string sdt, string sms)
        {
            try
            {
                //String filepath = Application.StartupPath + @"\log\log_sms.log";// đường dẫn của file
                String filepath = @"d:\OMC CAMERA\log\log_sms.log";// đường dẫn của file
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine("-- " + time + " SĐT: " + sdt + "   " + sms);
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
                txtLOG.Text += ex.ToString();
            }
        }
        private void logFile(string time, string noidung)
        {
            try
            {
                //String filepath = Application.StartupPath + @"\log\log_sms.log";// đường dẫn của file
                String filepath = @"d:\OMC CAMERA\log\log_sms.log";// đường dẫn của file
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine("-- " + time + ": " + noidung);
                    sw.Close();
                }
            }
            catch(Exception ex)
            {
                txtLOG.Text += ex.ToString();
            }
        }
        // Check tự động
        private void timer1_Tick(object sender, EventArgs e)
        {
            if(timer1.Enabled == true)
            {
                splashScreenManager1.ShowWaitForm();
                //TAM BINH
                // Ket noi file ini
                INIFile inif = new INIFile(@"d:\OMC CAMERA\ConfigFile\Conf_CAM_TAMBINH.ini");
                
                Debug.WriteLine("TAM BÌNH");
                txtLOG.Text += "TAM BÌNH" + "\r\n";
                checkAuto(treeView_CAM_TAMBINH_Dsach, inif);
                thongKe(treeView_CAM_TAMBINH_Dsach, inif);
                splashScreenManager1.CloseWaitForm();
            }
        }
        // Check tự động
        private void timer2_Tick(object sender, EventArgs e)
        {
            if (timer2.Enabled == true)
            {
                splashScreenManager1.ShowWaitForm();
                //BINH MINH
                // Ket noi file ini
                INIFile inif = new INIFile(@"d:\OMC CAMERA\ConfigFile\Conf_CAM_BINHMINH.ini");
                
                Debug.WriteLine("BÌNH MINH");
                txtLOG.Text += "BÌNH MINH" + "\r\n";
                checkAuto(treeView_CAM_BINHMINH_Dsach, inif);
                thongKe(treeView_CAM_BINHMINH_Dsach, inif);
                splashScreenManager1.CloseWaitForm();
            }
        }
        // Check tự động
        private void timer3_Tick(object sender, EventArgs e)
        {
            if (timer3.Enabled == true)
            {
                splashScreenManager1.ShowWaitForm();
                //VUNG LIEM
                // Ket noi file ini
                INIFile inif = new INIFile(@"d:\OMC CAMERA\ConfigFile\Conf_CAM_VUNGLIEM.ini");

                Debug.WriteLine("VŨNG LIÊM");
                txtLOG.Text += "VŨNG LIÊM" + "\r\n";
                checkAuto(treeView_CAM_VUNGLIEM_Dsach, inif);
                thongKe(treeView_CAM_VUNGLIEM_Dsach, inif);
                splashScreenManager1.CloseWaitForm();
            }
        }
        private void hyperlinkLabelSellectAll_TamBinh_Click(object sender, EventArgs e)
        {
            CheckAllNodes(treeView_CAM_TAMBINH_Dsach.Nodes);
        }

        private void hyperlinkLabelUnsellectAll_TamBinh_Click(object sender, EventArgs e)
        {
            UncheckAllNodes(treeView_CAM_TAMBINH_Dsach.Nodes);
        }
        

        private void hyperlinkLabelSellectAll_BinhMinh_Click(object sender, EventArgs e)
        {
            CheckAllNodes(treeView_CAM_BINHMINH_Dsach.Nodes);
        }

        private void hyperlinkLabelUnsellectAll_BinhMinh_Click(object sender, EventArgs e)
        {
            UncheckAllNodes(treeView_CAM_BINHMINH_Dsach.Nodes);
        }
        private void hyperlinkLabelSellectAll_VungLiem_Click(object sender, EventArgs e)
        {
            CheckAllNodes(treeView_CAM_VUNGLIEM_Dsach.Nodes);
        }
        private void hyperlinkLabelUnsellectAll_VungLiem_Click(object sender, EventArgs e)
        {
            UncheckAllNodes(treeView_CAM_VUNGLIEM_Dsach.Nodes);
        }
        // Check 1 node
        private void treeView_CAM_TAMBINH_Dsach_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            bientoancuc.TenTram = e.Node.Name; //lay bien de tìm trong file ini
            // Ket noi file ini
            INIFile inif = new INIFile(@"d:\OMC CAMERA\ConfigFile\Conf_CAM_TAMBINH.ini");

            Debug.WriteLine("TAM BÌNH");
            txtLOG.Text += "TAM BÌNH" + "\r\n";
            checkManual(treeView_CAM_TAMBINH_Dsach, bientoancuc.TenTram, inif);
            thongKe(treeView_CAM_TAMBINH_Dsach, inif);
        }
        // Check 1 node
        private void treeView_CAM_BINHMINH_Dsach_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            bientoancuc.TenTram = e.Node.Name; //lay bien de tìm trong file ini
            // Ket noi file ini
            INIFile inif = new INIFile(@"d:\OMC CAMERA\ConfigFile\Conf_CAM_BINHMINH.ini");

            Debug.WriteLine("BÌNH MINH");
            txtLOG.Text += "BÌNH MINH" + "\r\n";
            checkManual(treeView_CAM_BINHMINH_Dsach, bientoancuc.TenTram, inif);
            thongKe(treeView_CAM_BINHMINH_Dsach, inif);
        }
        // Check 1 node
        private void treeView_CAM_VUNGLIEM_Dsach_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            bientoancuc.TenTram = e.Node.Name; //lay bien de tìm trong file ini
            // Ket noi file ini
            INIFile inif = new INIFile(@"d:\OMC CAMERA\ConfigFile\Conf_CAM_VUNGLIEM.ini");

            Debug.WriteLine("VŨNG LIÊM");
            txtLOG.Text += "VŨNG LIÊM" + "\r\n";
            checkManual(treeView_CAM_VUNGLIEM_Dsach, bientoancuc.TenTram, inif);
            thongKe(treeView_CAM_VUNGLIEM_Dsach, inif);
        }
        // Check all node
        private void btnCheckAllTB_Click(object sender, EventArgs e)
        {
            splashScreenManager1.ShowWaitForm();
            // TAM BINH
            // Ket noi file ini
            INIFile inif = new INIFile(@"d:\OMC CAMERA\ConfigFile\Conf_CAM_TAMBINH.ini");

            Debug.WriteLine("TAM BÌNH");
            txtLOG.Text += "TAM BÌNH" + "\r\n";
            checkAuto(treeView_CAM_TAMBINH_Dsach, inif);
            thongKe(treeView_CAM_TAMBINH_Dsach, inif);
            splashScreenManager1.CloseWaitForm();
        }
        // Check all node
        private void btnCheckAllBM_Click(object sender, EventArgs e)
        {
            splashScreenManager1.ShowWaitForm();
            // BINH MINH
            // Ket noi file ini
            INIFile inif = new INIFile(@"d:\OMC CAMERA\ConfigFile\Conf_CAM_BINHMINH.ini");

            Debug.WriteLine("BÌNH MINH");
            txtLOG.Text += "BÌNH MINH" + "\r\n";
            checkAuto(treeView_CAM_BINHMINH_Dsach, inif);
            thongKe(treeView_CAM_BINHMINH_Dsach, inif);
            splashScreenManager1.CloseWaitForm();
        }
        // Check all node
        private void btnCheckAllVL_Click(object sender, EventArgs e)
        {
            splashScreenManager1.ShowWaitForm();
            // VUNG LIEM
            // Ket noi file ini
            INIFile inif = new INIFile(@"d:\OMC CAMERA\ConfigFile\Conf_CAM_VUNGLIEM.ini");

            Debug.WriteLine("VŨNG LIÊM");
            txtLOG.Text += "VŨNG LIÊM" + "\r\n";
            checkAuto(treeView_CAM_VUNGLIEM_Dsach, inif);
            thongKe(treeView_CAM_VUNGLIEM_Dsach, inif);
            splashScreenManager1.CloseWaitForm();
        }
        // Set thống kê
        private void thongKe(TreeView node, INIFile inif)
        {
            int tong = 0;
            int tc = 0;
            int tb = 0;
            foreach (TreeNode node_level1 in node.Nodes)
            {
                if (inif.Read(node_level1.Name, "Connect") == "1")
                {
                    tc++;
                }
                else if (inif.Read(node_level1.Name, "Connect") == "0")
                {
                    tb++;
                }
                tong++;
                foreach (TreeNode node_level2 in node_level1.Nodes)
                {
                    if (inif.Read(node_level2.Name, "Connect") == "1")
                    {
                        tc++;
                    }
                    else if (inif.Read(node_level2.Name, "Connect") == "0")
                    {
                        tb++;
                    }
                    tong++;
                }
            }
            if (node == treeView_CAM_TAMBINH_Dsach)
            {
                lblTKTB.Text = "Tổng: " + tong.ToString() + "; TC: " + tc.ToString() + "; TB: " + tb.ToString();
            }
            else if (node == treeView_CAM_BINHMINH_Dsach)
            {
                lblTKBM.Text = "Tổng: " + tong.ToString() + "; TC: " + tc.ToString() + "; TB: " + tb.ToString();
            }
            else if (node == treeView_CAM_VUNGLIEM_Dsach)
            {
                lblTKVL.Text = "Tổng: " + tong.ToString() + "; TC: " + tc.ToString() + "; TB: " + tb.ToString();
            }
        }
        // Nhắn tin
        private void btnNhanTinTB_Click(object sender, EventArgs e)
        {
            splashScreenManager1.ShowWaitForm();
            // TAM BINH
            // Ket noi file ini
            INIFile inif = new INIFile(@"d:\OMC CAMERA\ConfigFile\Conf_CAM_TAMBINH.ini");
            Debug.WriteLine("NHẮN TIN TAM BÌNH");
            txtLOG.Text += "NHẮN TIN TAM BÌNH" + "\r\n";
            sendMessageFromFile(treeView_CAM_TAMBINH_Dsach, inif);
            splashScreenManager1.CloseWaitForm();
        }
        // Nhắn tin
        private void btnNhanTinBM_Click(object sender, EventArgs e)
        {
            splashScreenManager1.ShowWaitForm();
            // BINH MINH
            // Ket noi file ini
            INIFile inif = new INIFile(@"d:\OMC CAMERA\ConfigFile\Conf_CAM_BINHMINH.ini");
            Debug.WriteLine("NHẮN TIN BÌNH MINH");
            txtLOG.Text += "NHẮN TIN BÌNH MINH" + "\r\n";
            sendMessageFromFile(treeView_CAM_BINHMINH_Dsach, inif);
            splashScreenManager1.CloseWaitForm();
        }
        // Nhắn tin
        private void btnNhanTinVL_Click(object sender, EventArgs e)
        {
            splashScreenManager1.ShowWaitForm();
            // VUNG LIEM
            // Ket noi file ini
            INIFile inif = new INIFile(@"d:\OMC CAMERA\ConfigFile\Conf_CAM_VUNGLIEM.ini");
            Debug.WriteLine("NHẮN TIN VŨNG LIÊM");
            txtLOG.Text += "NHẮN TIN VŨNG LIÊM" + "\r\n";
            sendMessageFromFile(treeView_CAM_VUNGLIEM_Dsach, inif);
            splashScreenManager1.CloseWaitForm();
        }
        // Show ip
        private void treeView_CAM_TAMBINH_Dsach_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            bientoancuc.TenTram = e.Node.Name; //lay bien de tìm trong file ini
            // Ket noi file ini
            INIFile inif = new INIFile(@"d:\OMC CAMERA\ConfigFile\Conf_CAM_TAMBINH.ini");
            string ip = inif.Read(bientoancuc.TenTram, "IP");
            toolTip1.SetToolTip(lblIPTB, ip);
        }
        // Show ip
        private void treeView_CAM_BINHMINH_Dsach_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            bientoancuc.TenTram = e.Node.Name; //lay bien de tìm trong file ini
            // Ket noi file ini
            INIFile inif = new INIFile(@"d:\OMC CAMERA\ConfigFile\Conf_CAM_BINHMINH.ini");
            string ip = inif.Read(bientoancuc.TenTram, "IP");
            toolTip1.SetToolTip(lblIPBM, ip);
        }
        // Show ip
        private void treeView_CAM_VUNGLIEM_Dsach_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            bientoancuc.TenTram = e.Node.Name; //lay bien de tìm trong file ini
            // Ket noi file ini
            INIFile inif = new INIFile(@"d:\OMC CAMERA\ConfigFile\Conf_CAM_VUNGLIEM.ini");
            string ip = inif.Read(bientoancuc.TenTram, "IP");
            toolTip1.SetToolTip(lblIPVL, ip);
        }

        private void treeView_CAM_TAMBINH_Dsach_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = _preventExpand;
            _preventExpand = false;
        }

        private void treeView_CAM_TAMBINH_Dsach_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = _preventExpand;
            _preventExpand = false;
        }

        private void treeView_CAM_BINHMINH_Dsach_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = _preventExpand;
            _preventExpand = false;
        }

        private void treeView_CAM_BINHMINH_Dsach_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = _preventExpand;
            _preventExpand = false;
        }
        private void treeView_CAM_VUNGLIEM_Dsach_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = _preventExpand;
            _preventExpand = false;
        }
        private void treeView_CAM_VUNGLIEM_Dsach_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = _preventExpand;
            _preventExpand = false;
        }
        private void treeView_CAM_TAMBINH_Dsach_MouseDown(object sender, MouseEventArgs e)
        {
            int delta = (int)DateTime.Now.Subtract(_lastMouseDown).TotalMilliseconds;
            _preventExpand = (delta < SystemInformation.DoubleClickTime);
            _lastMouseDown = DateTime.Now;
        }

        private void treeView_CAM_BINHMINH_Dsach_MouseDown(object sender, MouseEventArgs e)
        {
            int delta = (int)DateTime.Now.Subtract(_lastMouseDown).TotalMilliseconds;
            _preventExpand = (delta < SystemInformation.DoubleClickTime);
            _lastMouseDown = DateTime.Now;
        }
        private void treeView_CAM_VUNGLIEM_Dsach_MouseDown(object sender, MouseEventArgs e)
        {
            int delta = (int)DateTime.Now.Subtract(_lastMouseDown).TotalMilliseconds;
            _preventExpand = (delta < SystemInformation.DoubleClickTime);
            _lastMouseDown = DateTime.Now;
        }
        private void barButtonItemLogout_ItemClick(object sender, ItemClickEventArgs e)
        {
            _frm.Show();
            this.Hide();
            DateTime now = DateTime.Now;
            logFile(now.ToString(), "Dang xuat");
        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(bientoancuc.close == false)
            {
                // Cho hiện notifyIcon
                notifyIcon1.Visible = true;
                // Chọn ẩn
                this.Hide();
                // Thu nhỏ
                WindowState = FormWindowState.Minimized;

                e.Cancel = true;
            }
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                // Ẩn notifyIcon đi
                notifyIcon1.Visible = false;
                // Phóng to
                WindowState = FormWindowState.Normal;
                // Hoặc
                this.Show();
            }
        }

        private void ToolStripMenuItemThoat_Click(object sender, EventArgs e)
        {
            bientoancuc.close = true;
            Application.Exit();
        }
    }

    public class bientoancuc
    {
        public static string TenTram;
        public static WaitForm1 waitting;
        public static bool close = false;
    }
}