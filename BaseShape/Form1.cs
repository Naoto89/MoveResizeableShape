using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BaseShape
{
    public partial class Form1 : Form
    {
        private BasicShape _label1 = null;
        private BasicShape _label2 = null;
        private List<BasicShape> _shapes = new List<BasicShape>();

        private BasicShape BS1 = new BasicShape();
        private BasicShape BS2 = new BasicShape();

        private FormResizer fmres = null;
        private FormMover fmmov = null;

        private FormResizer fmres2 = null;
        private FormMover fmmov2 = null;


        //private FormMobe

        FormDragResizer fm = new FormDragResizer();
        FormDragResizer fm2 = new FormDragResizer();
        FormBase fmBase = new FormBase(FormBase.ResizeDirection.All,8);
        public Form1()
        {
            InitializeComponent();
            _label1 = label1 as BasicShape;
            _label2 = label2 as BasicShape;
            _label1.SelectInstances = _shapes;
            _label2.SelectInstances = _shapes;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            BS1.BackColor = Color.Transparent;
            BS2.BackColor = Color.Transparent;
            BS1.Width = 200;
            BS1.Height = 200;
            BS2.Width = 200;
            BS2.Height = 200;

            pictureBox1.Controls.Add(BS1);
            pictureBox1.Controls.Add(BS2);
            //Label1の位置をPictureBox1内の位置に変更する
            //BS1.Top = BS1.Top - pictureBox1.Top;
            //BS1.Left = BS1.Left - pictureBox1.Left;

            //BS2.Top = BS2.Top - pictureBox1.Top;
            //BS2.Left = BS2.Left - pictureBox1.Left;
            fmres = new FormResizer(fm, FormResizer.ResizeDirection.All, 8);
            fmmov = new FormMover(fm, 8);

            fmres2 = new FormResizer(fm2, FormResizer.ResizeDirection.All, 8);
            fmmov2 = new FormMover(fm2, 8);


            fm.TopLevel = false;
            fm2.TopLevel = false;
            fm2.BackColor = Color.SkyBlue;
            Controls.Add(fm);
            Controls.Add(fm2);
            Controls.Add(fmBase);
            //fm.Show();
            //fm2.Show();
            fmBase.Show();
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            _label1.ChangeSelectStatus(false,true);
            _label2.ChangeSelectStatus(false,true);
        }
    }

}
