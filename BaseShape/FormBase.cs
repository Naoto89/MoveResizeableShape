using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BaseShape
{
    public partial class FormBase : Form
    {
        /// <summary>
        /// サイズ変更の対象となる枠の位置
        /// ビット判定するために1,2,4,...と分けている
        /// </summary>
        public enum ResizeDirection
        {
            None   = 0x0,
            Top    = 0x1,
            Left   = 0x2,
            Bottom = 0x4,
            Right  = 0x8,
            All    = 0xF
        }

        /// <summary>
        /// サイズ変更が有効になる枠
        /// </summary>        
        protected ResizeDirection _enableResizeDirection = ResizeDirection.All;

        /// <summary>
        /// サイズ変更中を表す状態
        /// </summary>        
        protected ResizeDirection _resizeStatus = ResizeDirection.None;

        /// <summary>
        /// サイズ変更が有効になる範囲の幅
        /// </summary>        
        protected int _resizeAreaWidth = 8;

        /// <summary>
        /// マウスをクリックした位置
        /// </summary>        
        protected Point _lastMouseDownPoint = new Point();

        /// <summary>
        /// マウスをクリックした時点のサイズ
        /// </summary>
        protected Size _lastMouseDownSize = new Size();

        /// <summary>
        ///  移動中を表す状態
        /// </summary>
        protected bool _isMoving = false;

        /// <summary>
        /// 選択状態（ピンチ表示に関わるもの）
        /// </summary>
        public bool IsSelected = false;

        public FormBase()
        {
            InitializeComponent();
            TopLevel = false;
        }

        public FormBase(ResizeDirection enableResizeDir, int resizeAreaWidth):this()
        {
            _enableResizeDirection = enableResizeDir;
            _resizeAreaWidth = resizeAreaWidth;

        }
        private void FormBase_Load(object sender, EventArgs e)
        {
            TransparencyKey = BackColor;
            //int radius = 4;
            //int diameter = radius * 2;
            //GraphicsPath gp = new GraphicsPath();

            //// 左上
            //gp.AddPie(0, 0, diameter, diameter, 180, 90);
            //// 右上
            //gp.AddPie(Width - diameter, 0, diameter, diameter, 270, 90);
            //// 左下
            //gp.AddPie(0, Height - diameter, diameter, diameter, 90, 90);
            //// 右下
            //gp.AddPie(Width - diameter, Height - diameter, diameter, diameter, 0, 90);
            //// 中央
            //gp.AddRectangle(new Rectangle(radius, 0, Width - diameter, Height));
            //// 左
            //gp.AddRectangle(new Rectangle(0, radius, radius, Height - diameter));
            //// 右
            //gp.AddRectangle(new Rectangle(Width - radius, radius, radius, Height - diameter));

            //Region = new Region(gp);
        }

        protected ResizeDirection GetResizeDirection(MouseEventArgs e)
        {
            // サイズ変更が有効になる枠の上にカーソルが乗ったら
            // マウスカーソルをサイズ変更用のものに変更する

            // どの枠の上にカーソルが乗っているか
            ResizeDirection cursorPos = ResizeDirection.None;

            // 上の判定
            if ((_enableResizeDirection & ResizeDirection.Top) == ResizeDirection.Top)
            {
                Rectangle topRect = new Rectangle(0, 0, Width, _resizeAreaWidth);
                if (topRect.Contains(e.Location))
                {
                    cursorPos |= ResizeDirection.Top;
                }
            }

            // 左の判定
            if ((_enableResizeDirection & ResizeDirection.Left) == ResizeDirection.Left)
            {
                Rectangle leftRect = new Rectangle(0, 0, _resizeAreaWidth, Height);
                if (leftRect.Contains(e.Location))
                {
                    cursorPos |= ResizeDirection.Left;
                }
            }

            // 下の判定
            if ((_enableResizeDirection & ResizeDirection.Bottom) == ResizeDirection.Bottom)
            {
                Rectangle bottomRect = new Rectangle(0, Height - _resizeAreaWidth, Width, _resizeAreaWidth);
                if (bottomRect.Contains(e.Location))
                {
                    cursorPos |= ResizeDirection.Bottom;
                }
            }

            // 右の判定
            if ((_enableResizeDirection & ResizeDirection.Right) == ResizeDirection.Right)
            {
                Rectangle rightRect = new Rectangle(Width - _resizeAreaWidth, 0, _resizeAreaWidth, Height);
                if (rightRect.Contains(e.Location))
                {
                    cursorPos |= ResizeDirection.Right;
                }
            }

            return cursorPos;
        }


        private void FormBase_MouseDown(object sender, MouseEventArgs e)
        {
            // クリックしたポイントを保存する
            _lastMouseDownPoint = e.Location;

            // クリックした時点でのフォームのサイズを保存する
            _lastMouseDownSize = Size;

            // クリックされたので選択状態
            IsSelected = true;

            // クリックした位置から、サイズ変更する方向を決める
            _resizeStatus = GetResizeDirection(e);


            // サイズ変更の対象だったら、マウスキャプチャー
            if (_resizeStatus != ResizeDirection.None)
            {
                Capture = true;
            }

            // 左クリック時のみ処理する。左クリックでなければ何もしない
            if ((e.Button & MouseButtons.Left) != MouseButtons.Left)
            {
                return;
            }

            // 移動が有効になる範囲
            // 例えばフォームの端から何ドットかをサイズ変更用の領域として使用する場合、
            // そこを避けるために使う。
            Rectangle moveArea = new Rectangle(_resizeAreaWidth, _resizeAreaWidth,
                Width - (_resizeAreaWidth * 2), Height - (_resizeAreaWidth * 2));

            // クリックした位置が移動が有効になる範囲であれば、移動中にする
            if (moveArea.Contains(e.Location))
            {
                // 移動中にする
                _isMoving = true;

                // マウスキャプチャー
                Capture = true;
            }
            else
            {
                _isMoving = false;
            }

        }

        protected void ChangeCursor(ResizeDirection dir)
        {
            // カーソルを変更

            // 左上（左上から右下への斜め矢印）
            if (((dir & ResizeDirection.Left) == ResizeDirection.Left)
                && ((dir & ResizeDirection.Top) == ResizeDirection.Top))
            {
                Cursor = Cursors.SizeNWSE;
            }
            // 右下（左上から右下への斜め矢印）
            else if (((dir & ResizeDirection.Right) == ResizeDirection.Right)
                && ((dir & ResizeDirection.Bottom) == ResizeDirection.Bottom))
            {
                Cursor = Cursors.SizeNWSE;
            }
            // 右上（右上から左下への斜め矢印）
            else if (((dir & ResizeDirection.Right) == ResizeDirection.Right)
                && ((dir & ResizeDirection.Top) == ResizeDirection.Top))
            {
                Cursor = Cursors.SizeNESW;
            }
            // 左下（右上から左下への斜め矢印）
            else if (((dir & ResizeDirection.Left) == ResizeDirection.Left)
                && ((dir & ResizeDirection.Bottom) == ResizeDirection.Bottom))
            {
                Cursor = Cursors.SizeNESW;
            }
            // 上（上下矢印）
            else if ((dir & ResizeDirection.Top) == ResizeDirection.Top)
            {
                Cursor = Cursors.SizeNS;
            }
            // 左（左右矢印）
            else if ((dir & ResizeDirection.Left) == ResizeDirection.Left)
            {
                Cursor = Cursors.SizeWE;
            }
            // 下（上下矢印）
            else if ((dir & ResizeDirection.Bottom) == ResizeDirection.Bottom)
            {
                Cursor = Cursors.SizeNS;
            }
            // 右（左右矢印）
            else if ((dir & ResizeDirection.Right) == ResizeDirection.Right)
            {
                Cursor = Cursors.SizeWE;
            }
            // どこにも属していない（デフォルト）
            else
            {
                Cursor = Cursors.Default;
            }
        }


        private void FormBase_MouseMove(object sender, MouseEventArgs e)
        {
            // 移動中の場合のみ処理。移動中でなければ何もせず終わる
            // 左ボタン押下中のみ処理する。押下中ではないときは何もしない。
            if (_isMoving && ((e.Button & MouseButtons.Left) == MouseButtons.Left))
            {
                // マウスカーソルの変更
                Cursor = Cursors.SizeAll;

                // フォームの移動
                Left += e.X - _lastMouseDownPoint.X;
                Top += e.Y - _lastMouseDownPoint.Y;

                Invalidate();
                return;
            }

            if(_resizeStatus == ResizeDirection.None)
            {
                // サイズ変更が有効になる枠の上にカーソルが乗ったら
                // マウスカーソルをサイズ変更用のものに変更する

                // どの枠の上にカーソルが乗っているか
                ResizeDirection cursorPos = GetResizeDirection(e);
                ChangeCursor(cursorPos);
            }
            // ボタンを押していた場合は、サイズ変更を行う
            if (e.Button == MouseButtons.Left)
            {
                // ドラッグにより移動した距離を計算
                int diffX = e.X - _lastMouseDownPoint.X;
                int diffY = e.Y - _lastMouseDownPoint.Y;

                // 上
                if ((_resizeStatus & ResizeDirection.Top) == ResizeDirection.Top)
                {
                    // まず、ドラッグした距離分だけサイズを変更する
                    // その後、フォームの位置を調整する
                    // （順番を逆にすると、ちょっとちらつく？）
                    int h = Height;
                    Height -= diffY;
                    Top += h - Height;
                }
                // 左
                if ((_resizeStatus & ResizeDirection.Left) == ResizeDirection.Left)
                {
                    // 上と同じ
                    int w = Width;
                    Width -= diffX;
                    Left += w - Width;
                }
                // 下
                if ((_resizeStatus & ResizeDirection.Bottom) == ResizeDirection.Bottom)
                {
                    // マウスクリックした時点のサイズを基点として、
                    // ドラッグした距離分だけサイズを変更する
                    Height = _lastMouseDownSize.Height + diffY;
                }
                // 右
                if ((_resizeStatus & ResizeDirection.Right) == ResizeDirection.Right)
                {
                    // マウスクリックした時点のサイズを基点として、
                    // ドラッグした距離分だけサイズを変更する
                    Width = _lastMouseDownSize.Width + diffX;
                }
                Invalidate();

            }

        }

        private void FormBase_MouseUp(object sender, MouseEventArgs e)
        {
            // 移動を終了する
            _isMoving = false;
            Cursor = Cursors.Default;
            Capture = false;
            _resizeStatus = ResizeDirection.None;

        }

        /// <summary>
        /// 一回り大きいor小さい矩形を返す
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public Rectangle GetSmallBigRectangle(Rectangle rect, int amount)
        {
            return new Rectangle(
                rect.Left   - amount,
                rect.Top    - amount,
                rect.Width  + amount * 2,
                rect.Height + amount * 2
                );
        }

        private void FormBase_Paint(object sender, PaintEventArgs e)
        {
            var rect = new Rectangle(0, 0, Width, Height);
            var srect = GetSmallBigRectangle(rect, (int)-30);
            e.Graphics.FillRectangle(Brushes.Transparent, rect);
            var brushBack = new SolidBrush(BackColor);
            e.Graphics.FillRectangle(brushBack, srect);
            if (IsSelected)
            {
                var pen = new Pen(Color.Black);
                pen.Width = 3;
                float[] dashVals = { 5, 5, 5, 5 };
                pen.DashPattern = dashVals;
                var ownRect = GetSmallBigRectangle(rect, (int)-pen.Width);
                e.Graphics.DrawRectangle(pen, ownRect);
                pen.Dispose();
            }
            brushBack.Dispose();
            e.Dispose();

        }
    }
}
