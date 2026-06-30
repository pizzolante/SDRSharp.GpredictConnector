using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace SDRSharp.GpredictConnector
{
    public partial class Controlpanel : UserControl
    {
        

        public Controlpanel()
        {
            InitializeComponent();
            this.checkBoxEnable.CheckedChanged += CheckBoxEnable_CheckedChanged;
            TcpServer_Connected_Changed(false); // init state is disconnected.

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            this.labelVersion.Text = "v"+fvi.FileMajorPart+"."+fvi.FileMinorPart;
        }

        public void TcpServer_Enabled_Changed(bool enabled)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<bool>(TcpServer_Enabled_Changed), new object[] { enabled });
                return;
            }
            else
            {
                enabled_ = enabled;
                SetDescriptionOfServerState();
            }
        }

        public void TcpServer_Connected_Changed(bool connected)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<bool>(TcpServer_Connected_Changed), new object[] { connected });
                return;
            }
            else
            {
                connected_ = connected;
                SetDescriptionOfServerState();
            }
        }

        public void SetDescriptionOfServerState()
        {
            if (enabled_)
            {
                labelDescserverStat.ForeColor = Color.White;
                if (connected_)
                {
                    this.labelStatus.Text = "connected";
                    labelStatus.ForeColor = Color.Green;
                }
                else
                {
                    this.labelStatus.Text = "listening on port 4532";
                    labelStatus.ForeColor = Color.White;
                }
            }
            else
            {
                labelDescserverStat.ForeColor = Color.White;
                this.labelStatus.Text = "disabled";
                labelStatus.ForeColor = Color.White;
            }
        }

        private void CheckBoxEnable_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkbox = sender as CheckBox;
            if (checkbox != null)
            {
                if (checkbox.Checked)
                {
                    ServerStart?.Invoke();
                    
                } else
                {
                    ServerStop?.Invoke();
                    
                }
            }
        }

        private bool enabled_ = false;
        private bool connected_ = false;

        public event Action ServerStop;
        public event Action ServerStart;

        
    }
}
