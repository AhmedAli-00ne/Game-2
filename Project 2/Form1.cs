using System.Security.Cryptography.X509Certificates;

namespace Project_2
{
    public class BGLayer
    {
        public Bitmap Img;
        public int X;
        public int NextX;
        public bool StartNext;
        public BGLayer(Bitmap img, int x, int nextX, bool startNext)
        {
            Img = img;
            X = x;
            NextX = nextX;
            StartNext = startNext;
        }
    }
    public class Player
    {
        public Bitmap Img;
        public int ImgNo = 1;
        public string State = "Idle";
        public string Facing = "Right";
        public int X;
        public int Y;
        public int Speed = 2;
        public int Health = 100;
        public int CoinsCollected = 0;
        public int GemsCollected = 0;
        public int Gravity = 1;
        public bool EvolveStatus = false;
        public bool JumpStatus = false;
        public bool LandingStatus = false;
        public bool IdleStatus = true;
        public bool RunRightStatus = false;
        public bool RunLeftStatus = false;
        public Player(Bitmap img, int x, int y)
        {
            Img = img;
            X = x;
            Y = y;
        }
    }
    public class Coin
    {
        public Bitmap Img;
        public int X;
        public int Y;
        public bool Collected = false;
        public Coin(Bitmap img, int x, int y)
        {
            Img = img;
            X = x;
            Y = y;
        }
    }
    public partial class Form1 : Form
    {
        Bitmap offImage;
        System.Windows.Forms.Timer T = new System.Windows.Forms.Timer();
        List<BGLayer> BGLayers = new List<BGLayer>();
        Bitmap bg = new Bitmap("2dh2tcn(6).png");
        Player Player = new Player(new Bitmap("Player/PrimitivePlayerIdleRight/PlayerIdle (1).png"), 100, 100);
        Coin coin = new Coin(new Bitmap("Coin/1.png"), 470, 375-15);
        Rectangle playerRect;
        List<Rectangle> rects = new List<Rectangle>();
        int x = 0;
        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
            this.Paint += Form1_Paint;
            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;
            T.Tick += T_Tick;
        }

        private void Form1_KeyUp(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                Player.JumpStatus = false;
            }
            else if (e.KeyCode == Keys.Right)
            {
                Player.RunRightStatus = false;
            }
            else if (e.KeyCode == Keys.Left)
            {
                Player.RunLeftStatus = false;
            }
        }

        private void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                Player.JumpStatus = true;
                Player.IdleStatus = false;
                Player.ImgNo = 1;
            }
            else if(e.KeyCode == Keys.Right)
            {
                Player.Facing = "Right";
                Player.RunRightStatus = true;
                Player.IdleStatus = false;
                if(Player.ImgNo>8)
                {
                    Player.ImgNo = 1;
                }
            }
            else if (e.KeyCode == Keys.Left)
            {
                Player.Facing = "Left";
                Player.RunLeftStatus = true;
                Player.IdleStatus = false;
                if (Player.ImgNo > 8)
                {
                    Player.ImgNo = 1;
                }
            }
        }

        private void Form1_Paint(object? sender, PaintEventArgs e)
        {
            DoubleBuffer(e.Graphics);
        }

        private void T_Tick(object? sender, EventArgs e)
        {
            CoinCollision();
            AnimatePlayer();
            CollisionDetection();
            DoubleBuffer(this.CreateGraphics());
        }

        private void Form1_Load(object? sender, EventArgs e)
        {
            offImage = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
            for(int i=1;i<=3;i++)
            {
                BGLayers.Add(new BGLayer(new Bitmap("Background/plx-" + i.ToString() + ".png"), 0, 0, false));
                BGLayers[i - 1].Img = new Bitmap(BGLayers[i - 1].Img, new Size(this.ClientSize.Width, this.ClientSize.Height));
            }
            rects.Add(new Rectangle(0, 375, 395, 70));
            rects.Add(new Rectangle(345, 360, 50, 15));
            rects.Add(new Rectangle(357, 348, 37, 12));
            rects.Add(new Rectangle(369, 336, 24, 12));
            rects.Add(new Rectangle(380, 323, 13, 12));
            rects.Add(new Rectangle(440, 375, 355, 70));
            T.Interval = 20;
            T.Start();
        }
        void DoubleBuffer(Graphics g)
        {
            Graphics offGraphics = Graphics.FromImage(offImage);
            DrawScene(offGraphics);
            g.DrawImage(offImage, 0, 0);
        }
        void DrawScene(Graphics g)
        {
            g.Clear(Color.White);
            for (int i = 0; i < BGLayers.Count; i++)
            {
                g.DrawImage(BGLayers[i].Img, BGLayers[i].X, 0);
                if (BGLayers[i].StartNext)
                {
                    g.DrawImage(BGLayers[i].Img, BGLayers[i].NextX, 0);
                }
            }
            g.DrawImage(bg, x, 0);
            g.DrawRectangles(Pens.Black, rects.ToArray());
            g.DrawImage(Player.Img, Player.X, Player.Y);
            g.DrawRectangle(Pens.Red, playerRect);
            if (!coin.Collected)
            {
                g.DrawImage(coin.Img, coin.X, coin.Y);
            }
        }

        void CoinCollision()
        {
            if (playerRect.IntersectsWith(new Rectangle(coin.X, coin.Y, coin.Img.Width, coin.Img.Height)))
            {
                coin.Collected = true;
                Player.CoinsCollected++;
            }
        }
        void CollisionDetection()
        {
            int min = 99999;
            int minpos = 0;
            for (int i = 0; i < rects.Count; i++)
            {
                if ((Player.X >= rects[i].X || Player.X + Player.Img.Width >= rects[i].X) && (Player.X + Player.Img.Width <= rects[i].X + rects[i].Width || Player.X <= rects[i].X + rects[i].Width))
                {
                    if (Player.Y + Player.Img.Height-5 < rects[i].Y)
                    {
                        if (rects[i].Y<min)
                        {
                            min = rects[i].Y;
                            minpos = i;
                        }
                    }
                }
            }
            Gravity(min);
            int MaxRight = 0; 
            int MaxLeft = 0;
            int minRight = 99999;
            int minLeft = 99999;
            for(int i = 0;i<rects.Count; i++)
            {
                if (Player.Y + Player.Img.Height > rects[i].Y)
                {
                    if (rects[i].X > Player.X)
                    {
                        if (minRight > rects[i].X)
                        {
                            minRight = rects[i].X;
                        }
                    }
                    else
                    {
                        if(minLeft > rects[i].X)
                        {
                            minLeft = rects[i].X + rects[i].Width;
                        }
                    }
                }
            }
            if(minRight == 99999)
            {
                MaxRight = this.ClientSize.Width;
            }
            else
            {
                MaxRight = minRight;
            }
            if (minLeft == 99999)
            {
                MaxLeft = 0;
            }
            else
            {
                MaxLeft = minLeft;
            }
            PlayerMovement(MaxRight, MaxLeft);
        }
        void Gravity(int pos)
        {
            if (Player.Y < pos-Player.Img.Height)
            {
                Player.LandingStatus = true;
                Player.Y += Player.Gravity;
            }
            else
            {
                Player.LandingStatus = false;
            }
            playerRect = new Rectangle(Player.X, Player.Y, Player.Img.Width, Player.Img.Height);
        }
        void PlayerMovement(int RightX, int LeftX)
        {
            if (Player.JumpStatus)
            {
                Player.Y -= Player.Speed+30;
                Player.JumpStatus = false;
            }
            else if (Player.RunRightStatus && !Player.RunLeftStatus && Player.X + Player.Img.Width < RightX)
            {
                if (Player.X >= 100)
                {
                    for (int i = 0; i < rects.Count; i++)
                    {
                        rects[i] = new Rectangle(rects[i].X - Player.Speed, rects[i].Y, rects[i].Width, rects[i].Height);
                    }
                    x -= Player.Speed;
                }
                else
                {
                    Player.X += Player.Speed;
                }
                coin.X -= Player.Speed;
            }
            else if(Player.RunLeftStatus && !Player.RunRightStatus && Player.X > LeftX)
            {
                if (x + Player.Speed < 0)
                {
                    for (int i = 0; i < rects.Count; i++)
                    {
                        rects[i] = new Rectangle(rects[i].X + Player.Speed, rects[i].Y, rects[i].Width, rects[i].Height);
                    }
                    x += Player.Speed;
                }
                else if (Player.X + Player.Speed > 0)
                {
                    Player.X -= Player.Speed;
                }
            }
            else if (!Player.RunRightStatus && !Player.RunLeftStatus && !Player.JumpStatus && !Player.LandingStatus)
            {
                Player.IdleStatus = true;
            }
            playerRect = new Rectangle(Player.X, Player.Y, Player.Img.Width, Player.Img.Height);
        }
        void AnimatePlayer()
        {
            if (Player.Facing == "Right")
            {
                if (Player.IdleStatus)
                {
                    Console.WriteLine("Player is Idle and facing Right - Photo Num: " + Player.ImgNo.ToString());
                    Player.Img = new Bitmap("Player/PrimitivePlayerIdleRight/PlayerIdle (" + Player.ImgNo.ToString() + ").png");
                    if (Player.ImgNo == 12)
                    {
                        Player.ImgNo = 1;
                    }
                    else
                    {
                        Player.ImgNo++;
                    
                    }
                }
                else if (Player.RunRightStatus && !Player.LandingStatus && !Player.RunLeftStatus)
                {
                    Player.Img = new Bitmap("Player/PrimitivePlayerRunRight/PlayerRun (" + Player.ImgNo.ToString() + ").png");
                    if (Player.ImgNo == 8 || Player.ImgNo > 8)
                    {
                        Player.ImgNo = 1;
                    }
                    else
                    {
                        Player.ImgNo++;
                    }
                }
                else if (Player.JumpStatus)
                {
                    Player.Img = new Bitmap("Player/PrimitivePlayerJumpRight/jump.png");
                }
                else if (Player.LandingStatus)
                {
                    Player.Img = new Bitmap("Player/PrimitivePlayerLandRight/landing.png");
                }
            }
            else
            {
                if (Player.IdleStatus)
                {
                    Console.WriteLine("Player is Idle and facing Left - Photo Num: " + Player.ImgNo.ToString());
                    Player.Img = new Bitmap("Player/PrimitivePlayerIdleLeft/PlayerIdle (" + Player.ImgNo.ToString() + ").png");
                    if (Player.ImgNo == 12)
                    {
                        Player.ImgNo = 1;
                    }
                    else
                    {
                        Player.ImgNo++;
                    }
                }
                else if (Player.RunLeftStatus && !Player.LandingStatus && !Player.RunRightStatus)
                {
                    Player.Img = new Bitmap("Player/PrimitivePlayerRunLeft/PlayerRun (" + Player.ImgNo.ToString() + ").png");
                    if (Player.ImgNo == 8 || Player.ImgNo > 8)
                    {
                        Player.ImgNo = 1;
                    }
                    else
                    {
                        Player.ImgNo++;
                    }
                }
                else if (Player.JumpStatus)
                {
                    Player.Img = new Bitmap("Player/PrimitivePlayerJumpLeft/jump.png");
                }
                else if (Player.LandingStatus)
                {
                    Player.Img = new Bitmap("Player/PrimitivePlayerLandLeft/landing.png");
                }
            }
        }
    }
}