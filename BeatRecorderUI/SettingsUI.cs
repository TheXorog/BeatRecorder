using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BeatRecorderUI;
public partial class SettingsUI : Form
{
    bool beTopMost = false;

    public SettingsUI(bool topMost)
    {
        InitializeComponent();

        beTopMost = topMost;
    }

    private void SettingsUI_Shown(object sender, EventArgs e)
    {
        this.TopMost = beTopMost;
    }
}
