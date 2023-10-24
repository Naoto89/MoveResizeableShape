using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BaseShape
{
    public partial class BasicShape : Label
    {
        protected virtual void InitMenu()
        {
            _menu.Items.Clear();

            var item1 = new ToolStripMenuItem("色変更");
            item1.Click += (s,e) => 
            {
                using (var dlg = new ColorDialog())
                {
                    dlg.Color = BackColor;
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        BackColor = dlg.Color;
                        Invalidate();
                    }
                }
            };

            var item2 = new ToolStripMenuItem("フォント変更");
            item2.Click += (s, e) =>
            {
                using (var dlg = new FontDialog())
                {
                    dlg.Font = Font;
                    dlg.ShowColor = true;
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Font = dlg.Font;
                        ForeColor = dlg.Color;
                        Invalidate();
                    }
                }
            };
            var item3 = new ToolStripMenuItem("名称変更");
            item3.Click += (s, e) =>
            {
                using (var dlg = new FormText())
                {
                    dlg.DispText = Text;
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Text = dlg.DispText;
                        Invalidate();
                    }
                }
            };

            _menu.Items.Add(item1);
            _menu.Items.Add(item2);
            _menu.Items.Add(item3);
            ContextMenuStrip = _menu;
        }
    }
}
