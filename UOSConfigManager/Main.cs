using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UOSConfigManager
{
    public partial class Main : Form
    {
        private FileInfo[] profileBackups;
        private FileInfo[] profiles;
        #region delegates
        private Program.ConsoleMessage cmessage = new Program.ConsoleMessage(Program.CMSG);
        private Program.LoadProfileDelegate loadProfile = new Program.LoadProfileDelegate(Program.LoadProfile);
        private Program.LoadLauncherDelegate loadLauncher = new Program.LoadLauncherDelegate(Program.LoadLauncher);
        private Action LoadConfig = new Action(Program.LoadConfig);
        private Action SaveConfig = new Action(Program.SaveConfig);
        private Program.SaveFileDelegate SaveFile = new Program.SaveFileDelegate(Program.SaveFile);
        private Program.DialogDelegate ShowDialog = new Program.DialogDelegate(Program.Dialog);
        #endregion
        private UOS.XML.Profile.Profile currentProfile;
        private UOS.XML.Launcher.Launcher launcher;
        private int ProfileID;
        private LargeAddressAware.LAA.LaaFile client;
        private Changed ChangedElements;
        private bool LoadedProfile;
        
        public Main()
        {
            InitializeComponent();
            ChangedElements = Changed.None;
            LoadedProfile = false;
        }

        [Flags]
        public enum Changed
        {
            None = 0,
            Friends = 1,
            Hotkeys = 2,
            Macros = 4,
            Scavenger = 8,
            Counters = 16,
            Dresslist = 32,
            Object = 64,
            Client = 128
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ProfileID = Program.Config.DefaultProfileID;
            LoadProfile(ProfileID);
            launcher = loadLauncher();
            LoadData(true);
        }

        private void LoadData(bool loadLauncher = false)
        {
            grid_Friends.Rows.Clear();
            grid_Hotkeys.Rows.Clear();
            grid_Dress.Rows.Clear();
            grid_Counters.Rows.Clear();
            grid_Objects.Rows.Clear();
            grid_Scavenger.Rows.Clear();
            grid_Profiles.Rows.Clear();

            if (currentProfile.Friends == null)
                cmessage.Invoke("Could not find friends");
            else
                currentProfile.Friends.Friend.ForEach(f => grid_Friends.Rows.Add(f.Text, f.Name));

            if (currentProfile.Hotkeys == null)
                cmessage.Invoke("Could not find hotkeys");
            else
                currentProfile.Hotkeys.Hotkey.ForEach(f => grid_Hotkeys.Rows.Add(f.Key, Program.ConvertVirtualKey(f.Key), f.Pass, f.Action, f.Param));

            if (loadLauncher)
            {
                if (launcher.Servers == null)
                    cmessage.Invoke("Could not find shards");
                else
                {
                    UOS.XML.Launcher.Shard shard = launcher.Servers.Shard.Find(s => !string.IsNullOrEmpty(s.Last));

                    if (shard == null)
                    {
                        cmessage.Invoke($"Could not find last shard!{Environment.NewLine}Using shard id 0");
                        shard = launcher.Servers.Shard[0];
                    }

                    tb_host.Text = shard.Login;
                    tb_Port.Text = shard.Port.ToString();

                    int indexOf = launcher.Clients.Path.FindIndex(p => !string.IsNullOrEmpty(p.Last) && bool.Parse(p.Last));
                    tb_ClientExe.Text = launcher.Clients.Path[indexOf].Text;
                    cb_ClientExeEncryptionEnabled.Checked = bool.Parse(launcher.Clients.Path[indexOf].RemoveEncryption);
                    indexOf = launcher.Clients.Path.FindIndex(p => !string.IsNullOrEmpty(p.Last) && bool.Parse(p.Last));
                    tb_ClientFolder.Text = launcher.Clients.Path[indexOf].Text;
                }
            }

            cmessage.Invoke("Checking for client exe");
            if (File.Exists(tb_ClientExe.Text))
            {
                cmessage.Invoke("Found client exe, checking for large address aware...");
                client = new LargeAddressAware.LAA.LaaFile(tb_ClientExe.Text);

                cmessage.Invoke("Large address aware enabled: " + client.LargeAddressAware);
                cb_LAAEnabled.Checked = client.LargeAddressAware;
            }
            else
            {
                b_SwitchLAA.Visible = false;
                cmessage.Invoke("Client exe not found");
            }
            
            if (currentProfile.Macros == null)
                cmessage.Invoke("Could not find macros");
            else
                currentProfile.Macros.Macro.ForEach(m => lb_macros.Items.Add(m.Name));

            if (currentProfile.Counters == null)
                cmessage.Invoke("Could not find counters");
            else
                currentProfile.Counters.Counter.ForEach(c => grid_Counters.Rows.Add(c.Format, c.Name, c.Graphic, c.Enabled, c.Image));


            if (currentProfile.Scavenger == null)
                cmessage.Invoke("Could not find scavenger");
            else
            {
                if (currentProfile.Scavenger.Scavenge == null)
                    cmessage.Invoke("Could not find scavenger items");
                else
                    currentProfile.Scavenger.Scavenge.ForEach(s => grid_Scavenger.Rows.Add(s.Enabled, s.Graphic, s.Color));

                cb_ScavengerEnabled.Checked = bool.Parse(currentProfile.Scavenger.Enabled);
            }
            
            if (currentProfile.Objects == null)
                cmessage.Invoke("Could not find objects");
            else
                currentProfile.Objects.Obj.ForEach(o => grid_Objects.Rows.Add(o.Text, o.Name));

            if (currentProfile.Dresslist == null)
                cmessage.Invoke("Could not find dresslist");
            else
                currentProfile.Dresslist.ForEach(d => lb_Dresslist.Items.Add(d.Name));

            NUD_ProfileDefaultID.Value = Convert.ToDecimal(Program.Config.DefaultProfileID);
            tb_UOSFolder.Text = Program.Config.UOSPath;

            for (int i = 0; i < Program.profiles.Count(); i++)
                grid_Profiles.Rows.Add(i, Program.profiles[i].Name, Program.profiles[i].FullName);


            LoadedProfile = true;
        }

        public void LoadProfile(int ProfileID)
        {
            currentProfile = loadProfile(ProfileID);
        }

        private async Task SetDefaultPath()
        {
            Program.Config.UOSPath = "null";
            SaveConfig();
        }

        private void setDefaultPathToolStripMenuItem_Click(object sender, EventArgs e)
            => Task.Run(async () => await SetDefaultPath());

        private void b_SetClientExe_Click(object sender, EventArgs e)
        {
            KeyValuePair<DialogResult, string> dresult = ShowDialog(Program.DialogType.File);

            if (dresult.Key != DialogResult.OK && dresult.Key != DialogResult.Yes)
                return;

            tb_ClientExe.Text = dresult.Value;
        }

        private void b_ClientFolderSet_Click(object sender, EventArgs e)
        {
            KeyValuePair<DialogResult, string> dresult = ShowDialog(Program.DialogType.Folder);

            if (dresult.Key != DialogResult.OK && dresult.Key != DialogResult.Yes)
                return;

            tb_ClientFolder.Text = dresult.Value;
        }

        private void b_setVM_Click(object sender, EventArgs e)
        {
            tb_host.Text = Program.VMHost;
            tb_Port.Text = Program.VMPort.ToString();
        }

        private void lb_macros_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!LoadedProfile)
                return;
            try
            {
                UOS.XML.Profile.Macro macro = currentProfile.Macros.Macro.First(f => f.Name.Equals(lb_macros.SelectedItem.ToString()));
                rtb_macro.Text = macro.Text;
            }
            catch (Exception ex)
            {
                cmessage(ex.ToString());
            }
        }

        private void lb_Dresslist_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!LoadedProfile)
                return;
            try
            {
                grid_Dress.Rows.Clear();
                UOS.XML.Profile.Dresslist dresslist = currentProfile.Dresslist.Find(d => d.Name.Equals(lb_Dresslist.SelectedItem.ToString()));
                dresslist.Item.ForEach(i => grid_Dress.Rows.Add(i.Text, i.Layer, i.Graphic, i.Amount));
                rtb_Dresslist.Text = string.Format("Name:{0}{1}{0}ContainerID:{0}{2}", Environment.NewLine, dresslist.Name, dresslist.Container);
            }
            catch (Exception ex)
            {
                cmessage(ex.ToString());
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                KeyValuePair<DialogResult, string> dr = ShowDialog(Program.DialogType.File);

                Task.Run(() =>
                {
                    if (dr.Key != DialogResult.Yes && dr.Key != DialogResult.OK)
                        return;

                    string path = dr.Value;
                    string toSave = "VirtualKey | Key | Pass | Action | Param" + Environment.NewLine;

                    for (int i = 0; i < grid_Hotkeys.Rows.Count; i++)
                    {
                        DataGridViewRow row = grid_Hotkeys.Rows[i];
                        for (int x = 0; x < row.Cells.Count; x++)
                        {
                            DataGridViewCell cell = row.Cells[x];
                            toSave += (cell.Value == null ? "" : cell.Value.ToString()) + " | ";
                        }
                        toSave += Environment.NewLine;
                    }

                    SaveFile(path, toSave);
                }).Wait();
            }
            catch (Exception ex)
            {
                cmessage(ex.ToString());
            }
        }

        private void b_ProfileReload_Click(object sender, EventArgs e)
        {
            cmessage("Reloading profile");
            LoadedProfile = false;
            Program.Config.DefaultProfileID = Convert.ToInt32(NUD_ProfileDefaultID.Value);
            SaveConfig();
            cmessage($"Loading profile {Program.Config.DefaultProfileID}");
            ProfileID = Program.Config.DefaultProfileID;
            LoadProfile(ProfileID);
            LoadData();
        }
        
        private void b_UOSSetPath_Click(object sender, EventArgs e)
        {
            if (!LoadedProfile)
                return;
            try
            {
                KeyValuePair<DialogResult, string> result = ShowDialog(Program.DialogType.Folder);

                if (result.Key != DialogResult.OK && result.Key != DialogResult.Yes)
                    return;

                Program.Config.UOSPath = result.Value;
                SaveConfig();
                tb_UOSFolder.Text = result.Value;
            }
            catch (Exception ex)
            {
                cmessage(ex.ToString());
            }
        }

        private void b_SwitchLAA_Click(object sender, EventArgs e)
        {
            if (client == null)
                return;

            cmessage.Invoke($"Switching large address aware from {client.LargeAddressAware} to {!client.LargeAddressAware}");

            if (client.LargeAddressAware)
                client.WriteCharacteristics(false);
            else
                client.WriteCharacteristics(true);

            cb_LAAEnabled.Checked = client.LargeAddressAware;
        }

        private void grid_Friends_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (!LoadedProfile)
                return;
            if (!ChangedElements.HasFlag(Changed.Friends))
                ChangedElements |= Changed.Friends;

            Task.Run(async () =>
            {
                currentProfile.Friends.Friend.Clear();
                foreach (DataGridViewRow row in grid_Friends.Rows)
                    currentProfile.Friends.Friend.Add(new UOS.XML.Profile.Friend() { Text = row.Cells[0].Value.ToString(), Name = row.Cells[1].Value.ToString() });
            });
        }

        private void grid_Hotkeys_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (!LoadedProfile)
                return;
            if (!ChangedElements.HasFlag(Changed.Hotkeys))
                ChangedElements |= Changed.Hotkeys;

            Task.Run(async () =>
            {
                currentProfile.Hotkeys.Hotkey.Clear();
                foreach (DataGridViewRow row in grid_Hotkeys.Rows)
                    currentProfile.Hotkeys.Hotkey.Add(new UOS.XML.Profile.Hotkey() { Key = row.Cells[0].Value.ToString(), Pass = row.Cells[2].Value.ToString(), Action = row.Cells[3].Value.ToString(), Param = row.Cells[4].Value.ToString() });
            });
        }

        private void grid_Scavenger_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (!LoadedProfile)
                return;
            if (!ChangedElements.HasFlag(Changed.Scavenger))
                ChangedElements |= Changed.Scavenger;

            Task.Run(async () =>
            {
                currentProfile.Scavenger.Scavenge.Clear();
                foreach (DataGridViewRow row in grid_Scavenger.Rows)
                    currentProfile.Scavenger.Scavenge.Add(new UOS.XML.Profile.Scavenge() { Enabled = row.Cells[0].Value.ToString(), Graphic = row.Cells[1].Value.ToString(), Color = row.Cells[2].Value.ToString() });
            });
        }

        private void grid_Counters_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (!LoadedProfile)
                return;
            if (!ChangedElements.HasFlag(Changed.Counters))
                ChangedElements |= Changed.Counters;

            Task.Run(async () =>
            {
                currentProfile.Counters.Counter.Clear();

                foreach (DataGridViewRow row in grid_Objects.Rows)
                    currentProfile.Counters.Counter.Add(new UOS.XML.Profile.Counter() { Format = row.Cells[0].Value.ToString(), Name = row.Cells[1].Value.ToString(), Graphic = row.Cells[2].Value.ToString(), Color = row.Cells[3].Value.ToString(), Enabled = row.Cells[4].Value.ToString(), Image = row.Cells[5].Value.ToString() });
            });
        }

        private void grid_Dress_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (!LoadedProfile)
                return;
            int indexOf = currentProfile.Dresslist.FindIndex(dl => dl.Name.Equals(lb_Dresslist.SelectedValue.ToString()));

            if (indexOf == -1)
                return;

            if (!ChangedElements.HasFlag(Changed.Dresslist))
                ChangedElements |= Changed.Dresslist;

            Task.Run(async () =>
            {
                currentProfile.Dresslist[indexOf].Item.Clear();

                for (int i = 0; i < grid_Dress.RowCount; i++)
                {
                    DataGridViewRow row = grid_Dress.Rows[i];
                    currentProfile.Dresslist[indexOf].Item.Add(new UOS.XML.Profile.Item()
                    {
                        Text = row.Cells[0].Value.ToString(),
                        Layer = row.Cells[1].Value.ToString(),
                        Graphic = row.Cells[2].Value.ToString(),
                        Amount = row.Cells[3].Value.ToString()
                    });
                }
            });
        }

        private void grid_Objects_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (!LoadedProfile)
                return;
            if (!ChangedElements.HasFlag(Changed.Object))
                ChangedElements |= Changed.Object;
            
            Task.Run(async () =>
            {
                currentProfile.Objects.Obj.Clear();

                foreach (DataGridViewRow row in grid_Objects.Rows)
                    currentProfile.Objects.Obj.Add(new UOS.XML.Profile.Obj() { Text = row.Cells[0].Value.ToString(), Name = row.Cells[1].Value.ToString() });
            });
        }

        private void rtb_macro_TextChanged(object sender, EventArgs e)
        {
            if (!LoadedProfile)
                return;
            int indexOf = currentProfile.Macros.Macro.FindIndex(m => m.Name.Equals(lb_macros.SelectedValue.ToString()));

            if (indexOf == -1)
                return;

            if (!ChangedElements.HasFlag(Changed.Macros))
                ChangedElements |= Changed.Macros;

            Task.Run(async () =>
            {
                string macro = rtb_macro.Text.Replace(Environment.NewLine, "");

                currentProfile.Macros.Macro[indexOf].Text = macro;
            });
        }

        private void cb_ScavengerEnabled_CheckedChanged(object sender, EventArgs e)
        {
            if (!LoadedProfile)
                return;
            if (!ChangedElements.HasFlag(Changed.Scavenger))
                ChangedElements |= Changed.Scavenger;

            currentProfile.Scavenger.Enabled = cb_ScavengerEnabled.Checked.ToString();
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if ((int)ChangedElements == 0)
                return;

            DialogResult result = MessageBox.Show(
                "Exit without saving?",
                "Are you sure?",
                MessageBoxButtons.YesNo);

            e.Cancel = (result == DialogResult.No);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                UOS.UOSWriter writer;

                if (ChangedElements.HasFlag(Changed.Client))
                {
                    writer = new UOS.UOSWriter(launcher, 0, UOS.UOSReader.ConfigType.Launcher);
                    Task.Run(async () =>
                    {
                        cmessage.Invoke("Saving profile " + ProfileID);
                        writer.Write();
                        cmessage.Invoke("Saved profile " + ProfileID);
                    });
                }
                else if (ChangedElements == Changed.None)
                    return;
                else
                {
                    writer = new UOS.UOSWriter(currentProfile, ProfileID);
                    Task.Run(async () =>
                    {
                        cmessage.Invoke("Saving config");
                        writer.Write();
                        cmessage.Invoke("Saved config");
                    });
                }
                ChangedElements = Changed.None;
                MessageBox.Show("Successfully saved!", "Save State");
            }
            catch (Exception ex)
            {
                cmessage.Invoke(ex.ToString());
            }
        }

        private void tb_host_TextChanged(object sender, EventArgs e)
        {
            if (!LoadedProfile)
                return;
            if (!ChangedElements.HasFlag(Changed.Client))
                ChangedElements |= Changed.Client;

            int indexOf = launcher.Servers.Shard.FindIndex(s => !string.IsNullOrEmpty(s.Last) && bool.Parse(s.Last));

            if (indexOf == -1)
                return;

            launcher.Servers.Shard[indexOf].Login = tb_host.Text;
        }

        private void tb_Port_TextChanged(object sender, EventArgs e)
        {
            if (!LoadedProfile)
                return;
            if (!ChangedElements.HasFlag(Changed.Client))
                ChangedElements |= Changed.Client;

            int indexOf = launcher.Servers.Shard.FindIndex(s => !string.IsNullOrEmpty(s.Last) && bool.Parse(s.Last));

            if (indexOf == -1)
                return;

            launcher.Servers.Shard[indexOf].Port = tb_Port.Text;
        }

        private void tb_ClientFolder_TextChanged(object sender, EventArgs e)
        {
            if (!LoadedProfile)
                return;
            if (!ChangedElements.HasFlag(Changed.Client))
                ChangedElements |= Changed.Client;

            int indexOf = launcher.Installs.Path.FindIndex(s => !string.IsNullOrEmpty(s.Last) && bool.Parse(s.Last));

            if (indexOf == -1)
                return;

            launcher.Installs.Path[indexOf].Text = tb_ClientFolder.Text;
        }

        private void tb_ClientExe_TextChanged(object sender, EventArgs e)
        {
            if (!LoadedProfile)
                return;
            if (!ChangedElements.HasFlag(Changed.Client))
                ChangedElements |= Changed.Client;

            int indexOf = launcher.Clients.Path.FindIndex(s => !string.IsNullOrEmpty(s.Last) && bool.Parse(s.Last));

            if (indexOf == -1)
                return;

            launcher.Clients.Path[indexOf].Text = tb_ClientExe.Text;
        }

        private void cb_ClientExeEncryptionEnabled_CheckedChanged(object sender, EventArgs e)
        {
            if (!LoadedProfile)
                return;
            if (!ChangedElements.HasFlag(Changed.Client))
                ChangedElements |= Changed.Client;

            int indexOf = launcher.Clients.Path.FindIndex(s => !string.IsNullOrEmpty(s.Last) && bool.Parse(s.Last));

            if (indexOf == -1)
                return;

            launcher.Clients.Path[indexOf].RemoveEncryption = cb_ClientExeEncryptionEnabled.Checked.ToString();
        }
    }
}
