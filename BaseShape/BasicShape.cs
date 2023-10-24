using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BaseShape
{
    /// <summary>
    /// フォームに自由に配置してリサイズして移動できるラベルコントロール
    /// </summary>
    public partial class BasicShape : Label
    {
        /// <summary>
        /// ピンチの方向定義と、その方向に適したカーソルを返すためのクラス
        /// </summary>
        public class Pinch
        {
            public enum Direction
            {
                Top_Left = 0,
                Top_Mid,
                Top_Right,
                Mid_Left,
                Mid_Right,
                Bot_Left,
                Bot_Mid,
                Bot_Right,

                None,
            }

            /// <summary>
            /// 方向に適したカーソルを返す
            /// </summary>
            /// <param name="dir"></param>
            /// <returns></returns>
            public static Cursor GetCursor(Direction dir)
            {
                switch (dir)
                {
                    case Direction.Top_Left:
                    case Direction.Bot_Right:
                        {
                            return Cursors.SizeNWSE;
                        }
                    case Direction.Top_Right:
                    case Direction.Bot_Left:
                        {
                            return Cursors.SizeNESW;
                        }
                    case Direction.Top_Mid:
                    case Direction.Bot_Mid:
                        {
                            return Cursors.SizeNS;
                        }
                    case Direction.Mid_Right:
                    case Direction.Mid_Left:
                        {
                            return Cursors.SizeWE;
                        }
                }
                return Cursors.Default;
            }
        }

        /// <summary>
        /// ピンチサイズpx
        /// </summary>
        public int PinchSize = 10;

        /// <summary>
        /// ピンチつまみの座標、サイズ
        /// </summary>
        public Rectangle[] Pinches = new Rectangle[8];

        /// <summary>
        /// 選択状態（ピンチ表示に関わるもの）
        /// </summary>
        public bool IsSelected = false;

        /// <summary>
        /// 
        /// </summary>
        public string ID = "";

        public List<BasicShape> ConnectInputPin = new List<BasicShape>();

        public List<BasicShape> ConnectOutputPin = new List<BasicShape>();

        /// <summary>
        /// 選択中のインスタンス情報
        /// </summary>
        public List<BasicShape> SelectInstances = new List<BasicShape>();

        /// <summary>
        /// 最小サイズ
        /// </summary>
        public Size MinSize = new Size(100, 100);

        /// <summary>
        /// マウスクリックした時のクライアント座標
        /// </summary>
        protected Point _curPt = new Point();

        /// <summary>
        /// マウスクリックした時の本コントロールのサイズ
        /// </summary>
        protected Size _curSize = new Size();

        /// <summary>
        /// つまんだピンチの方向
        /// </summary>
        protected Pinch.Direction _curPinch = Pinch.Direction.None;

        /// <summary>
        /// 右クリックメニュー
        /// </summary>
        protected ContextMenuStrip _menu = new ContextMenuStrip();

        protected Color PaintBackColor = Color.SkyBlue;
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BasicShape()
        {
            InitMenu();
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            FlatStyle = FlatStyle.Popup;
            //_menu.Click
        }

        public Rectangle GetSmallBigRectangle(Rectangle rect, int amount)
        {
            return new Rectangle(
                rect.Left   - amount,
                rect.Top    - amount,
                rect.Width  + amount * 2,
                rect.Height + amount * 2
                );
        }

        /// <summary>
        /// 初期化
        /// </summary>
        public void Init()
        {
            int left = 0;
            int midl = (Width - PinchSize) / 2;
            int rigt = Width - PinchSize;

            int top = 0;
            int midlt = (Height - PinchSize) / 2;
            int bottm = Height - PinchSize;

            Pinches[0] = new Rectangle(left, top  , PinchSize, PinchSize);
            Pinches[1] = new Rectangle(midl, top  , PinchSize, PinchSize);
            Pinches[2] = new Rectangle(rigt, top  , PinchSize, PinchSize);
            Pinches[3] = new Rectangle(left, midlt, PinchSize, PinchSize);
            Pinches[4] = new Rectangle(rigt, midlt, PinchSize, PinchSize);
            Pinches[5] = new Rectangle(left, bottm, PinchSize, PinchSize);
            Pinches[6] = new Rectangle(midl, bottm, PinchSize, PinchSize);
            Pinches[7] = new Rectangle(rigt, bottm, PinchSize, PinchSize);

            //int radius = 20;
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

        /// <summary>
        /// つまんだピンチの方向を返す
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        private Pinch.Direction GetPinchDir(Point pt)
        {
            for (int i = 0; i < Pinches.Length; ++i)
            {
                if (Pinches[i].Contains(pt))
                {
                    return (Pinch.Direction)i;
                }
            }
            return Pinch.Direction.None;

        }

        /// <summary>
        /// つまんだピンチの位置に適したカーソルの取得
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        private Cursor GetCursor(Point pt)
        {
            for (int i = 0; i < Pinches.Length; ++i)
            {
                if (Pinches[i].Contains(pt))
                {
                    return Pinch.GetCursor((Pinch.Direction)i);
                }
            }
            return Pinch.GetCursor(Pinch.Direction.None);
        }

        /// <summary>
        /// コントロールのサイズ変更時
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            Init();
        }
        protected override void OnResize(EventArgs e)
        {
            Console.WriteLine("on resize");
        }
        /// <summary>
        /// 描画処理
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            //e.Graphics.Clear(Color.Transparent);
            var ownRect = new Rectangle(0, 0, Width, Height);
            //e.Graphics.FillRectangle(Brushes.Transparent, ownRect);
            if (IsSelected)
            {

                e.Graphics.FillRectangles(Brushes.Black, Pinches);

                var pen = new Pen(Color.Black);
                float[] dashVals = { 5, 5, 5, 5 };
                pen.DashPattern = dashVals;
                ownRect.Width = ownRect.Width - PinchSize;
                ownRect.Height = ownRect.Height - PinchSize;
                ownRect.X = PinchSize / 2;
                ownRect.Y = ownRect.X;

                e.Graphics.DrawRectangle(pen, ownRect);
                pen.Dispose();
            }


            // 背景の描画
            var backBr = new SolidBrush(PaintBackColor);
            var r = GetSmallBigRectangle(new Rectangle(0, 0, Width, Height), -20);
            var path = GetRoundRect(r, 10);
            //e.Graphics.FillRectangle(backBr, r);
            e.Graphics.FillPath(backBr, path);

            // 文字列の描画
            var rectStr = GetSmallBigRectangle(r,-5);
            var foreBr = new SolidBrush(ForeColor);
            e.Graphics.DrawString(Text, Font, foreBr, rectStr);

            e.Dispose();
            backBr.Dispose();
            foreBr.Dispose();
            //base.OnPaint(e);

        }
        /// <summary>
        /// 本インスタンスの選択状態を変更して再描画
        /// </summary>
        /// <param name="isSelect"></param>
        public void ChangeSelectStatus(bool isSelect, bool isRefrash = false)
        {
            IsSelected = isSelect;
            
            if(IsSelected)
            {
                SelectInstances.Add(this);
            }
            else
            {
                Cursor = Cursors.Default;
                SelectInstances.Remove(this);
            }

            if(isRefrash)
            {
                //Refresh();
                Invalidate();
            }
        }

        /// <summary>
        /// マウスクリックした時
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if(e.Button == MouseButtons.Left)
            {
                if ((ModifierKeys & Keys.Control) != Keys.Control)
                {
                    // Ctrlキーが押されていないので、
                    // それらのインスタンスを非選択状態にして
                    // 選択リストをクリアする
                    for(int i = 0; i < SelectInstances.Count; ++i)
                    {
                        SelectInstances[i].ChangeSelectStatus(false,  true);
                    }
                    SelectInstances.Clear();
                }
                ChangeSelectStatus(true,false);
                _curPt = new Point(e.X, e.Y);

                _curPinch = GetPinchDir(e.Location);
                _curSize = new Size(Width, Height);
                if (e.Button == MouseButtons.Left)
                {
                    // ピンチを選択していなかったら移動カーソルに変更
                    // ピンチを選択していたら適切なカーソルに変更
                    Cursor = (_curPinch == Pinch.Direction.None ?
                        Cursors.SizeAll : Pinch.GetCursor(_curPinch));
                }

                Focus();
                //Refresh();
                Invalidate();
            }
            else if (e.Button == MouseButtons.Right)
            {
                //_menu.Show(e.Location);
                _menu.Show(MousePosition);
                //ContextMenuStrip.Show(e.Location);

            }
        }

        /// <summary>
        /// マウスムーブした時
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (e.Button == MouseButtons.Left && _curPinch == Pinch.Direction.None)
            {
                // 移動処理
                Location = new Point(
                     Location.X + e.X - _curPt.X
                    , Location.Y + e.Y - _curPt.Y
                    );
                //Refresh();
                Invalidate();
            }
            else if (e.Button == MouseButtons.Left && _curPinch != Pinch.Direction.None)
            {
                // リサイズ処理
                ResizeByPinch(e);
                //Refresh();
                Invalidate();
            }
            else if(IsSelected)
            {
                // カーソル変更表示
                Cursor = GetCursor(e.Location);
            }

        }

        /// <summary>
        /// マウスアップした時
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            Cursor = Cursors.Default;
        }


        /// <summary>
        /// ピンチをつまんだ事によるリサイズ処理
        /// </summary>
        /// <param name="e"></param>
        protected void ResizeByPinch(MouseEventArgs e)
        {
            // リサイズ処理
            if (_curPinch == Pinch.Direction.Top_Left
                || _curPinch == Pinch.Direction.Top_Right
                || _curPinch == Pinch.Direction.Bot_Left
                || _curPinch == Pinch.Direction.Bot_Right
                )
            {
                // 縦横サイズ変更
                int x = _curPt.X - e.X;
                int y = _curPt.Y - e.Y;


                if (_curPinch == Pinch.Direction.Top_Left)
                {
                    int tmpW = Width + x;
                    Width = Math.Max(tmpW, MinSize.Width);
                    if (tmpW >= MinSize.Width)
                    {
                        Left -= x;
                    }

                    int tmpH = Height + y;
                    Height = Math.Max(tmpH, MinSize.Height);
                    if (tmpH >= MinSize.Height)
                    {
                        Top -= y;
                    }
                }
                else if (_curPinch == Pinch.Direction.Top_Right)
                {
                    Width = Math.Max(_curSize.Width - x, MinSize.Width);
                    int tmpH = Height + y;
                    Height = Math.Max(tmpH, MinSize.Height);
                    if (tmpH >= MinSize.Height)
                    {
                        Top -= y;
                    }
                }
                else if (_curPinch == Pinch.Direction.Bot_Left)
                {
                    Height = Math.Max(_curSize.Height - y, MinSize.Height);
                    int tmpW = Width + x;
                    Width = Math.Max(tmpW, MinSize.Width);
                    if (tmpW >= MinSize.Width)
                    {
                        Left -= x;
                    }
                }
                else
                {
                    Height = Math.Max(_curSize.Height - y, MinSize.Height);
                    Width = Math.Max(_curSize.Width - x, MinSize.Width);
                }
            }
            else if (_curPinch == Pinch.Direction.Top_Mid
                   || _curPinch == Pinch.Direction.Bot_Mid)
            {
                // 縦サイズ変更
                int y = _curPt.Y - e.Y;
                if (_curPinch == Pinch.Direction.Top_Mid)
                {
                    int tmpH = Height + y;
                    Height = Math.Max(tmpH, MinSize.Height);
                    if (tmpH >= MinSize.Height)
                    {
                        Top -= y;
                    }
                }
                else
                {
                    Height = Math.Max(_curSize.Height - y, MinSize.Height);
                }
            }
            else
            {
                // 横サイズ変更
                int x = _curPt.X - e.X;

                if (_curPinch == Pinch.Direction.Mid_Left)
                {
                    int tmpW = Width + x;
                    Width = Math.Max(tmpW, MinSize.Width);
                    if (tmpW >= MinSize.Width)
                    {
                        Left -= x;
                    }
                }
                else
                {
                    Width = Math.Max(_curSize.Width - x, MinSize.Width);
                }

            }
        }

        /// <summary>
        /// キーダウン時
        /// </summary>
        /// <param name="e"></param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (IsSelected)
            {
                // 選択時に、上下左右キーを押下すると平行移動する
                int shift = 10;
                if (e.Control)
                {
                    shift = 1;//Ctrlを同時押しすると1pxずつの平行移動
                }
                switch (e.KeyCode)
                {
                    case Keys.Up: Location = new Point(Location.X + 0, Location.Y - shift); break;
                    case Keys.Down: Location = new Point(Location.X + 0, Location.Y + shift); break;
                    case Keys.Left: Location = new Point(Location.X - shift, Location.Y + 0); break;
                    case Keys.Right: Location = new Point(Location.X + shift, Location.Y + 0); break;
                }
                Refresh();
            }
        }

        /// <summary>
        /// 角丸長方形のGraphicsPathを作成
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public GraphicsPath GetRoundRect(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();

            path.StartFigure();

            // 左上の角丸
            path.AddArc(rect.Left, rect.Top, radius * 2, radius * 2,
                180, 90);
            // 上の線
            path.AddLine(rect.Left + radius, rect.Top, rect.Right - radius, rect.Top);
            // 右上の角丸
            path.AddArc(rect.Right - radius * 2, rect.Top, radius * 2, radius * 2, 270, 90);
            // 右の線
            path.AddLine(rect.Right, rect.Top + radius, rect.Right, rect.Bottom - radius);
            // 右下の角丸
            path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
            // 下の線
            path.AddLine(rect.Right - radius, rect.Bottom, rect.Left + radius, rect.Bottom);
            // 左下の角丸
            path.AddArc(rect.Left, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
            // 左の線
            path.AddLine(rect.Left, rect.Bottom - radius, rect.Left, rect.Top + radius);

            path.CloseFigure();

            return path;
        }

    }
}
