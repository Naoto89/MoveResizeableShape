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
    public partial class FormDragResizer : Form
    {
        public FormDragResizer()
        {
            InitializeComponent();
        }

        private void FormDragResizer_Load(object sender, EventArgs e)
        {
            int radius = 20;
            int diameter = radius * 2;
            GraphicsPath gp = new GraphicsPath();

            // 左上
            gp.AddPie(0, 0, diameter, diameter, 180, 90);
            // 右上
            gp.AddPie(Width - diameter, 0, diameter, diameter, 270, 90);
            // 左下
            gp.AddPie(0, Height - diameter, diameter, diameter, 90, 90);
            // 右下
            gp.AddPie(Width - diameter, Height - diameter, diameter, diameter, 0, 90);
            // 中央
            gp.AddRectangle(new Rectangle(radius, 0, Width - diameter, Height));
            // 左
            gp.AddRectangle(new Rectangle(0, radius, radius, Height - diameter));
            // 右
            gp.AddRectangle(new Rectangle(Width - radius, radius, radius, Height - diameter));

            Region = new Region(gp);
        }
    }
    /// <summary>
    /// フォームの端をドラッグしてサイズ変更するクラス
    /// </summary>
    class FormResizer
    {
        // サイズ変更の対象となるフォーム
        Form resizeForm;

        /// <summary>
        /// サイズ変更の対象となる枠の位置
        /// </summary>
        public enum ResizeDirection
        {
            None = 0,
            Top = 1,
            Left = 2,
            Bottom = 4,
            Right = 8,
            All = 15
        }

        // サイズ変更が有効になる枠
        ResizeDirection resizeDirection;

        // サイズ変更中を表す状態
        ResizeDirection resizeStatus;

        // サイズ変更が有効になる範囲の幅
        int resizeAreaWidth;

        // 標準のカーソル
        Cursor defaultCursor;

        // マウスをクリックした位置
        Point lastMouseDownPoint;

        // マウスをクリックした時点のサイズ
        Size lastMouseDownSize;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="resizeForm">サイズ変更の対象となるフォーム</param>
        /// <param name="resizeDirection">サイズ変更が有効になる枠</param>
        /// <param name="resizeAreaWidth">サイズ変更が有効になる範囲の幅</param>
        public FormResizer(Form resizeForm, ResizeDirection resizeDirection, int resizeAreaWidth)
        {
            this.resizeForm = resizeForm;
            this.resizeDirection = resizeDirection;
            this.resizeAreaWidth = resizeAreaWidth;

            // 現時点でのカーソルを保存しておく
            defaultCursor = resizeForm.Cursor;

            // イベントハンドラを追加
            resizeForm.MouseDown += new MouseEventHandler(resizeForm_MouseDown);
            resizeForm.MouseMove += new MouseEventHandler(resizeForm_MouseMove);
            resizeForm.MouseUp += new MouseEventHandler(resizeForm_MouseUp);
        }

        /// <summary>
        /// マウスボタン押下イベントハンドラ
        /// </summary>
        void resizeForm_MouseDown(object sender, MouseEventArgs e)
        {
            // クリックしたポイントを保存する
            lastMouseDownPoint = e.Location;

            // クリックした時点でのフォームのサイズを保存する
            lastMouseDownSize = resizeForm.Size;

            // クリックした位置から、サイズ変更する方向を決める
            resizeStatus = ResizeDirection.None;

            // 上の判定
            if ((resizeDirection & ResizeDirection.Top) == ResizeDirection.Top)
            {
                Rectangle topRect = new Rectangle(0, 0, resizeForm.Width, resizeAreaWidth);
                if (topRect.Contains(e.Location))
                {
                    resizeStatus |= ResizeDirection.Top;
                }
            }

            // 左の判定
            if ((resizeDirection & ResizeDirection.Left) == ResizeDirection.Left)
            {
                Rectangle leftRect = new Rectangle(0, 0, resizeAreaWidth, resizeForm.Height);
                if (leftRect.Contains(e.Location))
                {
                    resizeStatus |= ResizeDirection.Left;
                }
            }

            // 下の判定
            if ((resizeDirection & ResizeDirection.Bottom) == ResizeDirection.Bottom)
            {
                Rectangle bottomRect = new Rectangle(0, resizeForm.Height - resizeAreaWidth, resizeForm.Width, resizeAreaWidth);
                if (bottomRect.Contains(e.Location))
                {
                    resizeStatus |= ResizeDirection.Bottom;
                }
            }

            // 右の判定
            if ((resizeDirection & ResizeDirection.Right) == ResizeDirection.Right)
            {
                Rectangle rightRect = new Rectangle(resizeForm.Width - resizeAreaWidth, 0, resizeAreaWidth, resizeForm.Height);
                if (rightRect.Contains(e.Location))
                {
                    resizeStatus |= ResizeDirection.Right;
                }
            }

            // サイズ変更の対象だったら、マウスキャプチャー
            if (resizeStatus != ResizeDirection.None)
            {
                resizeForm.Capture = true;
            }
        }

        /// <summary>
        /// マウス移動イベントハンドラ
        /// </summary>
        void resizeForm_MouseMove(object sender, MouseEventArgs e)
        {
            // サイズ変更が有効になる枠の上にカーソルが乗ったら
            // マウスカーソルをサイズ変更用のものに変更する

            // どの枠の上にカーソルが乗っているか
            ResizeDirection cursorPos = ResizeDirection.None;

            // 上の判定
            if ((resizeDirection & ResizeDirection.Top) == ResizeDirection.Top)
            {
                Rectangle topRect = new Rectangle(0, 0, resizeForm.Width, resizeAreaWidth);
                if (topRect.Contains(e.Location))
                {
                    cursorPos |= ResizeDirection.Top;
                }
            }

            // 左の判定
            if ((resizeDirection & ResizeDirection.Left) == ResizeDirection.Left)
            {
                Rectangle leftRect = new Rectangle(0, 0, resizeAreaWidth, resizeForm.Height);
                if (leftRect.Contains(e.Location))
                {
                    cursorPos |= ResizeDirection.Left;
                }
            }

            // 下の判定
            if ((resizeDirection & ResizeDirection.Bottom) == ResizeDirection.Bottom)
            {
                Rectangle bottomRect = new Rectangle(0, resizeForm.Height - resizeAreaWidth, resizeForm.Width, resizeAreaWidth);
                if (bottomRect.Contains(e.Location))
                {
                    cursorPos |= ResizeDirection.Bottom;
                }
            }

            // 右の判定
            if ((resizeDirection & ResizeDirection.Right) == ResizeDirection.Right)
            {
                Rectangle rightRect = new Rectangle(resizeForm.Width - resizeAreaWidth, 0, resizeAreaWidth, resizeForm.Height);
                if (rightRect.Contains(e.Location))
                {
                    cursorPos |= ResizeDirection.Right;
                }
            }

            // カーソルを変更

            // 左上（左上から右下への斜め矢印）
            if (((cursorPos & ResizeDirection.Left) == ResizeDirection.Left)
                && ((cursorPos & ResizeDirection.Top) == ResizeDirection.Top))
            {
                resizeForm.Cursor = Cursors.SizeNWSE;
            }
            // 右下（左上から右下への斜め矢印）
            else if (((cursorPos & ResizeDirection.Right) == ResizeDirection.Right)
                && ((cursorPos & ResizeDirection.Bottom) == ResizeDirection.Bottom))
            {
                resizeForm.Cursor = Cursors.SizeNWSE;
            }
            // 右上（右上から左下への斜め矢印）
            else if (((cursorPos & ResizeDirection.Right) == ResizeDirection.Right)
                && ((cursorPos & ResizeDirection.Top) == ResizeDirection.Top))
            {
                resizeForm.Cursor = Cursors.SizeNESW;
            }
            // 左下（右上から左下への斜め矢印）
            else if (((cursorPos & ResizeDirection.Left) == ResizeDirection.Left)
                && ((cursorPos & ResizeDirection.Bottom) == ResizeDirection.Bottom))
            {
                resizeForm.Cursor = Cursors.SizeNESW;
            }
            // 上（上下矢印）
            else if ((cursorPos & ResizeDirection.Top) == ResizeDirection.Top)
            {
                resizeForm.Cursor = Cursors.SizeNS;
            }
            // 左（左右矢印）
            else if ((cursorPos & ResizeDirection.Left) == ResizeDirection.Left)
            {
                resizeForm.Cursor = Cursors.SizeWE;
            }
            // 下（上下矢印）
            else if ((cursorPos & ResizeDirection.Bottom) == ResizeDirection.Bottom)
            {
                resizeForm.Cursor = Cursors.SizeNS;
            }
            // 右（左右矢印）
            else if ((cursorPos & ResizeDirection.Right) == ResizeDirection.Right)
            {
                resizeForm.Cursor = Cursors.SizeWE;
            }
            // どこにも属していない（デフォルト）
            else
            {
                resizeForm.Cursor = defaultCursor;
            }

            // ボタンを押していた場合は、サイズ変更を行う
            if (e.Button == MouseButtons.Left)
            {
                // ドラッグにより移動した距離を計算
                int diffX = e.X - lastMouseDownPoint.X;
                int diffY = e.Y - lastMouseDownPoint.Y;

                // 上
                if ((resizeStatus & ResizeDirection.Top) == ResizeDirection.Top)
                {
                    // まず、ドラッグした距離分だけサイズを変更する
                    // その後、フォームの位置を調整する
                    // （順番を逆にすると、ちょっとちらつく？）
                    int h = resizeForm.Height;
                    resizeForm.Height -= diffY;
                    resizeForm.Top += h - resizeForm.Height;
                }
                // 左
                if ((resizeStatus & ResizeDirection.Left) == ResizeDirection.Left)
                {
                    // 上と同じ
                    int w = resizeForm.Width;
                    resizeForm.Width -= diffX;
                    resizeForm.Left += w - resizeForm.Width;
                }
                // 下
                if ((resizeStatus & ResizeDirection.Bottom) == ResizeDirection.Bottom)
                {
                    // マウスクリックした時点のサイズを基点として、
                    // ドラッグした距離分だけサイズを変更する
                    resizeForm.Height = lastMouseDownSize.Height + diffY;
                }
                // 右
                if ((resizeStatus & ResizeDirection.Right) == ResizeDirection.Right)
                {
                    // マウスクリックした時点のサイズを基点として、
                    // ドラッグした距離分だけサイズを変更する
                    resizeForm.Width = lastMouseDownSize.Width + diffX;
                }
            }
        }

        /// <summary>
        /// マウスボタン押上イベントハンドラ
        /// </summary>
        void resizeForm_MouseUp(object sender, MouseEventArgs e)
        {
            // マウスキャプチャーを終了する
            resizeForm.Capture = false;
        }
    }




    /// <summary>
    /// フォームをドラッグして移動するクラス
    /// </summary>
    class FormMover
    {
        // 移動の対象となるフォーム
        Form moveForm;

        // 移動中を表す状態
        bool moveStatus;

        // ドラッグを無効とする幅（フォームの端をサイズ変更に使うときなど）
        int noDragAreaWidth;

        // 標準のカーソル
        Cursor defaultCursor;

        // マウスをクリックした位置
        Point lastMouseDownPoint;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="moveForm">移動の対象となるフォーム</param>
        /// <param name="noDragAreaWidth">ドラッグを無効とする幅</param>
        public FormMover(Form moveForm, int noDragAreaWidth)
        {
            this.moveForm = moveForm;
            this.noDragAreaWidth = noDragAreaWidth;

            // 現時点でのカーソルを保存しておく
            defaultCursor = moveForm.Cursor;

            // イベントハンドラを追加
            moveForm.MouseDown += new MouseEventHandler(moveForm_MouseDown);
            moveForm.MouseMove += new MouseEventHandler(moveForm_MouseMove);
            moveForm.MouseUp += new MouseEventHandler(moveForm_MouseUp);
        }

        /// <summary>
        /// マウスボタン押下イベントハンドラ
        /// </summary>
        void moveForm_MouseDown(object sender, MouseEventArgs e)
        {
            // 左クリック時のみ処理する。左クリックでなければ何もしない
            if ((e.Button & MouseButtons.Left) != MouseButtons.Left) return;

            // 移動が有効になる範囲
            // 例えばフォームの端から何ドットかをサイズ変更用の領域として使用する場合、
            // そこを避けるために使う。
            Rectangle moveArea = new Rectangle(
                noDragAreaWidth, noDragAreaWidth,
                moveForm.Width - (noDragAreaWidth * 2), moveForm.Height - (noDragAreaWidth * 2));

            // クリックした位置が移動が有効になる範囲であれば、移動中にする
            if (moveArea.Contains(e.Location))
            {
                // 移動中にする
                moveStatus = true;

                // クリックしたポイントを保存する
                lastMouseDownPoint = e.Location;

                // マウスキャプチャー
                moveForm.Capture = true;
            }
            else
            {
                moveStatus = false;
            }
        }

        /// <summary>
        /// マウス移動イベントハンドラ
        /// </summary>
        void moveForm_MouseMove(object sender, MouseEventArgs e)
        {
            // 移動中の場合のみ処理。移動中でなければ何もせず終わる
            if (moveStatus == false) return;

            // 左ボタン押下中のみ処理する。押下中ではないときは何もしない。
            if ((e.Button & MouseButtons.Left) != MouseButtons.Left) return;

            // マウスカーソルの変更
            moveForm.Cursor = Cursors.SizeAll;

            // フォームの移動
            //*//通常の場合
            moveForm.Left += e.X - lastMouseDownPoint.X;
            moveForm.Top += e.Y - lastMouseDownPoint.Y;
            //*/

            // 吸着の処理は後回し
        }

        /// <summary>
        /// マウスボタン押上イベントハンドラ
        /// </summary>
        void moveForm_MouseUp(object sender, MouseEventArgs e)
        {
            // 左ボタンのみ処理する。左ボタンではないときは何もしない。
            if ((e.Button & MouseButtons.Left) != MouseButtons.Left) return;

            // 移動を終了する
            moveStatus = false;

            // マウスキャプチャーを終了する
            moveForm.Capture = false;
        }
    }
}
