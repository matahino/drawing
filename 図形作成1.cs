using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Collections.Generic;

namespace TYWinforms.Day13
{
    public class Exercise : Form
    {
        private ToolBar tlbStandard = new ToolBar();
        private string strFunction;
        private Point ptStartClick;
        private Point ptEndClick;
        private Point ptPreviousClick;
        private bool blnDrag = false;
        private Pen objPen;
        private Bitmap imgWorking;
        private Graphics objG;
        private List<Rectangle> rectangles = new List<Rectangle>();
        private List<PointF[]> arrows = new List<PointF[]>();
        private int currentArrowIndex = -1;
        private int currentRectangleIndex = -1;
        private bool isResizing = false;
        private Color currentColor = Color.Black;

        public Exercise()
        {
            Size maxScreenSize = Screen.PrimaryScreen.Bounds.Size;
            imgWorking = new Bitmap(maxScreenSize.Width, maxScreenSize.Height, PixelFormat.Format32bppArgb);
            objG = Graphics.FromImage(imgWorking);
            objG.Clear(Color.White);

            objPen = new Pen(currentColor);
            objG.SmoothingMode = SmoothingMode.AntiAlias;

            ToolBarButton tbbPen = new ToolBarButton();
            tbbPen.Text = "鉛筆";
            tlbStandard.Buttons.Add(tbbPen);

            ToolBarButton tbbStamp = new ToolBarButton();
            tbbStamp.Text = "スタンプ";
            tlbStandard.Buttons.Add(tbbStamp);

            ToolBarButton tbbColor = new ToolBarButton();
            tbbColor.Text = "カラー";
            tlbStandard.Buttons.Add(tbbColor);

            ToolBarButton tbbArrow = new ToolBarButton();
            tbbArrow.Text = "矢印";
            tlbStandard.Buttons.Add(tbbArrow);
            
            ToolBarButton tbbHand = new ToolBarButton();
            tbbHand.Text = "ハンド";
            tlbStandard.Buttons.Add(tbbHand);

            tlbStandard.ButtonClick += new ToolBarButtonClickEventHandler(this.MyToolBarHandler);
            this.Controls.Add(tlbStandard);

            this.BackgroundImage = imgWorking;
            this.Text = "Exercise";
            this.DoubleBuffered = true;
            this.Size = new Size(800, 600);
            this.BackColor = Color.White;

            this.MouseWheel += Exercise_MouseWheel;
        }

        private void Exercise_MouseWheel(object sender, MouseEventArgs e)
        {
            if (currentArrowIndex != -1 && strFunction == "矢印")
            {
                PointF[] currentArrow = arrows[currentArrowIndex];
                PointF start = currentArrow[0];
                PointF end = currentArrow[1];

                Vector2 direction = new Vector2(end.X - start.X, end.Y - start.Y);
                direction = Vector2.Normalize(direction);

                float change = e.Delta * 0.1f;

                end.X += direction.X * change;
                end.Y += direction.Y * change;

                arrows[currentArrowIndex] = new PointF[] { start, end };
                this.Invalidate();
            }
        }

        private void MyToolBarHandler(Object Sender, ToolBarButtonClickEventArgs e)
        {
            switch (e.Button.Text)
            {
                case "鉛筆":
                    strFunction = "鉛筆";
                    break;
                case "スタンプ":
                    strFunction = "スタンプ";
                    Point center = new Point(this.ClientSize.Width / 2, this.ClientSize.Height / 2);
                    Rectangle rect = new Rectangle(center.X - 50, center.Y - 50, 100, 100);
                    rectangles.Add(rect);
                    this.Invalidate();
                    break;
                case "カラー":
                    ColorDialog colorDialog = new ColorDialog();
                    if (colorDialog.ShowDialog() == DialogResult.OK)
                    {
                        currentColor = colorDialog.Color;
                        objPen.Color = currentColor;
                    }
                    break;
                case "矢印":
                    strFunction = "矢印";
                    Point arrowStart = new Point(this.ClientSize.Width / 2, this.ClientSize.Height / 2);
                    Point arrowEnd = new Point(this.ClientSize.Width / 2 + 50, this.ClientSize.Height / 2);
                    arrows.Add(new PointF[] { arrowStart, arrowEnd });
                    this.Invalidate();
                    break;
                case "ハンド":
                    strFunction = "ハンド";
                    break;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            ptStartClick = new Point(e.X, e.Y);
            ptPreviousClick = ptStartClick;

            if (strFunction == "スタンプ" && e.Button == MouseButtons.Left)
            {
                for (int i = 0; i < rectangles.Count; i++)
                {
                    if (rectangles[i].Contains(ptStartClick))
                    {
                        currentRectangleIndex = i;
                        break;
                    }
                }

                if (currentRectangleIndex != -1)
                {
                    Rectangle rect = rectangles[currentRectangleIndex];
                    if (Math.Abs(e.X - rect.Right) <= 10 && Math.Abs(e.Y - rect.Bottom) <= 10)
                    {
                        isResizing = true;
                    }
                }
            }
            else if (strFunction == "矢印" && e.Button == MouseButtons.Left)
            {
                for (int i = 0; i < arrows.Count; i++)
                {
                    PointF start = arrows[i][0];
                    PointF end = arrows[i][1];

                    if (DistanceToLine(start, end, ptStartClick) <= 10)
                    {
                        currentArrowIndex = i;
                        break;
                    }
                }
            }
            else if (strFunction == "ハンド" && e.Button == MouseButtons.Left)
            {
                for (int i = 0; i < rectangles.Count; i++)
                {
                    if (rectangles[i].Contains(ptStartClick))
                    {
                        currentRectangleIndex = i;
                        break;
                    }
                }

                for (int i = 0; i < arrows.Count; i++)
                {
                    PointF start = arrows[i][0];
                    PointF end = arrows[i][1];

                    if (DistanceToLine(start, end, ptStartClick) <= 10)
                    {
                        currentArrowIndex = i;
                        break;
                    }
                }
            }

            blnDrag = true;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            blnDrag = false;
            currentRectangleIndex = -1;
            currentArrowIndex = -1;
            isResizing = false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            ptEndClick = new Point(e.X, e.Y);
            if (blnDrag && e.Button == MouseButtons.Left)
            {
                if (strFunction == "スタンプ")
                {
                    if (currentRectangleIndex != -1)
                    {
                        if (isResizing)
                        {
                            Rectangle rect = rectangles[currentRectangleIndex];
                            rect.Width += e.X - ptPreviousClick.X;
                            rect.Height += e.Y - ptPreviousClick.Y;
                            rectangles[currentRectangleIndex] = rect;
                        }
                        else
                        {
                            Rectangle rect = rectangles[currentRectangleIndex];
                            int changeX = e.X - ptPreviousClick.X;
                            int changeY = e.Y - ptPreviousClick.Y;
                            rect.Offset(changeX, changeY);
                            rectangles[currentRectangleIndex] = rect;
                        }
                    }
                }
                else if (strFunction == "矢印")
                {
                    if (currentArrowIndex != -1)
                    {
                        PointF start = arrows[currentArrowIndex][0];
                        PointF end = arrows[currentArrowIndex][1];

                        Vector2 direction = new Vector2(end.X - start.X, end.Y - start.Y);
                        direction = Vector2.Normalize(direction);

                        float changeX = e.X - ptPreviousClick.X;
                        float changeY = e.Y - ptPreviousClick.Y;

                        start.X += changeX;
                        start.Y += changeY;
                        end.X += changeX;
                        end.Y += changeY;

                        arrows[currentArrowIndex] = new PointF[] { start, end };
                    }
                }
                else if (strFunction == "ハンド")
                {
                    if (currentRectangleIndex != -1)
                    {
                        Rectangle rect = rectangles[currentRectangleIndex];
                        int changeX = e.X - ptPreviousClick.X;
                        int changeY = e.Y - ptPreviousClick.Y;
                        rect.Offset(changeX, changeY);
                        rectangles[currentRectangleIndex] = rect;
                    }
                    else if (currentArrowIndex != -1)
                    {
                        PointF start = arrows[currentArrowIndex][0];
                        PointF end = arrows[currentArrowIndex][1];

                        float changeX = e.X - ptPreviousClick.X;
                        float changeY = e.Y - ptPreviousClick.Y;

                        start.X += changeX;
                        start.Y += changeY;
                        end.X += changeX;
                        end.Y += changeY;

                        arrows[currentArrowIndex] = new PointF[] { start, end };
                    }
                }
                else if (strFunction == "鉛筆")
                {
                    DrawIt(objG);
                }

                this.Invalidate();
                ptPreviousClick = ptEndClick;
            }
        }

        private void DrawIt(Graphics objGraphics)
        {
            switch (strFunction)
            {
                case "鉛筆":
                    objGraphics.DrawLine(objPen, ptPreviousClick, ptEndClick);
                    ptPreviousClick = ptEndClick;
                    break;
            }
        }

        private float DistanceToLine(PointF pointStart, PointF pointEnd, Point point)
        {
            float num = (pointEnd.X - pointStart.X) * (pointStart.Y - point.Y) - (pointStart.X - point.X) * (pointEnd.Y - pointStart.Y);
            return Math.Abs(num) / (float)Math.Sqrt((pointEnd.X - pointStart.X) * (pointEnd.X - pointStart.X) + (pointEnd.Y - pointStart.Y) * (pointEnd.Y - pointStart.Y));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(imgWorking, 0, 0);
            foreach (var rect in rectangles)
            {
                e.Graphics.DrawRectangle(new Pen(currentColor), rect);
            }
            foreach (var arrow in arrows)
            {
                e.Graphics.DrawLine(new Pen(currentColor), arrow[0], arrow[1]);
                AdjustedDrawArrow(e.Graphics, new Pen(currentColor), arrow[0], arrow[1]);
            }
        }

        private void AdjustedDrawArrow(Graphics g, Pen pen, PointF start, PointF end)
        {
            const float headSize = 10.0f;
            Vector2 direction = new Vector2(end.X - start.X, end.Y - start.Y);
            direction = Vector2.Normalize(direction);
            Vector2 normal = new Vector2(-direction.Y, direction.X);
            PointF arrowHead1 = new PointF(end.X - direction.X * headSize - normal.X * headSize, end.Y - direction.Y * headSize - normal.Y * headSize);
            PointF arrowHead2 = new PointF(end.X - direction.X * headSize + normal.X * headSize, end.Y - direction.Y * headSize + normal.Y * headSize);
            g.DrawLine(pen, end, arrowHead1);
            g.DrawLine(pen, end, arrowHead2);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (this.WindowState == FormWindowState.Minimized)
            {
                return;
            }
            this.Invalidate();
        }

        public struct Vector2
        {
            public float X, Y;
            public Vector2(float x, float y)
            {
                X = x;
                Y = y;
            }
            public static Vector2 Normalize(Vector2 value)
            {
                float val = 1.0f / (float)Math.Sqrt((value.X * value.X) + (value.Y * value.Y));
                return new Vector2(value.X * val, value.Y * val);
            }
        }

        public static void Main()
        {
            Application.Run(new Exercise());
        }
    }
}
