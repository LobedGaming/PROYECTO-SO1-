using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SkyFire
{
    public partial class Form1 : Form
    {
        private int Estado;
        private int vida;
        private int HoraChoque;
        private int HoraDib;
        private int HoraFinD;
        private int level;

        private bool choco;
        private bool muerto;
        private bool ganar;
        private string seleccionado;

        private PCB User;
        private Cola Q;

        private List<PCB> ListaNaves;
        private List<PCB> ListaBalasU;
        private List<PCB> ListaBalasN;
        private List<PCB> ListaNavesCho;

        private Image Nave;
        private Image Enemigo;
        private Image BalaN;
        private Image BalaU;
        private Image Corazon;
        private Image Muerto;
        private Image win;
        private Image EnemigoRojo;
        private Image EnemigoAmar;
        private Image EnemigoAzu;
        public Form1()
        {
            seleccionado = @"GREEN.png";
            load();
            InitializeComponent();
        }
       
        private void Form1_Load(object sender, EventArgs e)
        {           
            this.Size = new Size(400, 500);
            label1.Location = new Point(315,10);
           
        }
        public static Image resizeImage(Image imgToResize, Size size)
        {
            return (Image)(new Bitmap(imgToResize, size));
        }

        private void load()
        {
            Estado = -1;
            level = 1;
            Q = new Cola();
            vida = 10;
            User = new PCB();
            choco = false;
            muerto = false;
            ganar = false;          
            ListaBalasN = new List<PCB>();
            ListaBalasU = new List<PCB>();
            ListaNaves = new List<PCB>();
            ListaNavesCho = new List<PCB>();
            Image img = Image.FromFile(seleccionado);
            Bitmap imgbitmap = new Bitmap(img);
            Nave = resizeImage(imgbitmap, new Size(80, 80));

            Enemigo = Image.FromFile(@"enemigo.png");
            EnemigoAmar = Image.FromFile(@"enemigo2.png");
            EnemigoRojo = Image.FromFile(@"enemigo3.png");
            EnemigoAzu = Image.FromFile(@"enemigo4.png");
            BalaN = Image.FromFile(@"bala.png");
            BalaU = Image.FromFile(@"bala.png");
            Corazon = Image.FromFile(@"corazon.png");
            Muerto = Image.FromFile(@"muerto.jpg");
            win = Image.FromFile(@"win.jpg");
        }
        private void InitJuego()
        {
            load();
            leerNivel(@"json.json");
            HoraDib = Environment.TickCount;
            HoraFinD = Environment.TickCount + 100;
            User.ANCHO = 80;
            User.ALTO = 30;
            User.X = (Width - User.ANCHO) / 2;
            User.Y = 320;
            Estado = 0;
            CicloJuego();
        }
        private void cargarNivel()
        {
            ListaNaves.Clear();
            ListaBalasN.Clear();
            ListaBalasU.Clear();
            ListaNavesCho.Clear();
            level++;
            if (level <= Constants.MaxLevel)
            {
                string j = @"json";
                string c = ".json";
                string l = level.ToString();
                leerNivel(j + l + c);
            }
            else
            {
                ganar = true;
            }

        }
        private void leerNivel(string path)
        {
            string JsonFile;
            using (var reader = new StreamReader(path))
            {
                JsonFile = reader.ReadToEnd();
            }
            ListaNaves = JsonConvert.DeserializeObject<List<PCB>>(JsonFile);
            foreach (var v in ListaNaves)
            {
                v.HORA = Environment.TickCount;
                Q.Meter(v);
            }
        }
        private void Dibujar()
        {                       
            Invalidate();
        }
        private int MaxX()
        {
            return Width - 1;
        }
        private int MaxY()
        {
            return Height-1;
        }
        private void CicloJuego()
        {
            Estado = 0;
            while (Estado == 0)
            {
                PlanificadorRR();
                Application.DoEvents();
            }
        }
        private void PlanificadorRR()
        {
            PCB PRUN = Q.Sacar();
            if (HoraDib < HoraFinD)
            {
                HoraDib = Environment.TickCount;
            }
            else
            {
                HoraDib = Environment.TickCount;
                HoraFinD = Environment.TickCount + 150;
                Dibujar();
            }

            if ((PRUN.HORA+PRUN.RETARDO)>Environment.TickCount)         
            {               
                Q.Meter(PRUN);              
            }
            else
            {
                switch (PRUN.TIPO)
                {
                    case Constants.NAVE: MoverNave(PRUN);
                        break;
                    case Constants.BALAN: MoverBalaN(PRUN);
                        break;
                    case Constants.BALAU: MoverBalaU(PRUN);
                        break;
                    case Constants.NAVEP:Parada(PRUN);
                        break;
                    case Constants.NAVEM: MoverNaveM(PRUN);
                        break;
                }
            }

        }    
        private void MoverNave(PCB PRUN)
        {         
            if (!PRUN.CHOCADo)
            {
                PCB P = new PCB();
                PRUN.X = PRUN.X + 5;
                if (PRUN.X > MaxX())
                {
                    PRUN.X = 0;
                }
                PRUN.HORA = Environment.TickCount;
                var random = new Random(Environment.TickCount);
                var value = random.Next(0, 20);
                Q.Meter(PRUN);
                if (value == 0)
                {
                    P.TIPO = Constants.BALAN;
                    P.ANCHO = 5;
                    P.ALTO = 10;
                    P.X = (PRUN.ANCHO - P.ANCHO) / 2 + PRUN.X;
                    P.Y = PRUN.Y + PRUN.ALTO + P.ALTO;
                    P.RETARDO = 25;
                    P.HORA = Environment.TickCount;
                    ListaBalasN.Add(P);
                    if (ListaBalasN.Count > 25)
                    {
                        ListaBalasN.Clear();
                    }

                    Q.Meter(P);
                }
            }
        }
        private void MoverBalaN(PCB PRUN)
        {
            PRUN.Y = PRUN.Y + 5;
            if (PRUN.Y < MaxY())
            {
                if (((PRUN.Y > User.Y) && (PRUN.Y < User.Y + User.ALTO)) && ((PRUN.X >= User.X) && (PRUN.X <= User.X + User.ANCHO)))
                {
                    choco = true;
                    HoraChoque = Environment.TickCount;
                    ListaBalasN.Clear();
                    return;
                }
                PRUN.HORA = Environment.TickCount;
                Q.Meter(PRUN);
            }
        }
        private void MoverBalaU(PCB PRUN)
        {
            PRUN.Y = PRUN.Y - 5;
            if (PRUN.Y >0)
            {
                for (int i = 0; i < ListaNaves.Count; i++)
                {
                    if (((PRUN.Y <= ListaNaves[i].Y)&&(PRUN.Y>=(ListaNaves[i].Y-ListaNaves[i].ALTO)))
                        && ((PRUN.X >= ListaNaves[i].X) && (PRUN.X <= ListaNaves[i].X + ListaNaves[i].ANCHO))&&(!ListaNaves[i].CHOCADo))
                    {                 
                        List<PCB> aux = new List<PCB>();
                        for (int j = 0; j < ListaNaves.Count; j++)
                        {
                            if (j != i)
                            {
                                aux.Add(ListaNaves[j]);
                            }
                            else 
                            {
                                ListaNaves[j].vida = ListaNaves[j].vida - 1;    
                                if (ListaNaves[j].vida==0)
                                {
                                    ListaNaves[j].CHOCADo = true;
                                    aux.Add(ListaNaves[j]);
                                    ListaNavesCho.Add(ListaNaves[j]);
                                }
                                else
                                {
                                    aux.Add(ListaNaves[j]);
                                }
                                                                                           
                            }
                        }
                        i = ListaNaves.Count;
                        ListaNaves = aux;
                        ListaBalasU.Clear();                 
                        if(ListaNavesCho.Count==ListaNaves.Count)
                        {
                            cargarNivel();
                        }
                        PRUN.HORA = Environment.TickCount+PRUN.RETARDO;
                        return;
                    }
                }                            
                PRUN.HORA = Environment.TickCount;               
                Q.Meter(PRUN);
            }
        }      
        private void Parada(PCB PRUN)
        {
            PRUN.HORA = Environment.TickCount;
            Q.Meter(PRUN);
        }
        private void MoverNaveM(PCB PRUN)
        {
            if (!PRUN.CHOCADo)
            {
                PCB P = new PCB();


                if (PRUN.PAREDD)
                {
                    PRUN.X = PRUN.X - 5;
                    if (PRUN.X < 0)
                    {
                        PRUN.PAREDD = false;
                        PRUN.PAREDIZ = true;
                    }
                }
                else if (PRUN.PAREDIZ)
                {
                    PRUN.X = PRUN.X + 5;
                    if (PRUN.X > MaxX() - PRUN.ANCHO)
                    {
                        PRUN.PAREDD = true;
                        PRUN.PAREDIZ = false;
                    }

                }
                if (PRUN.TECHO)
                {
                    PRUN.Y = PRUN.Y + 5;
                    if (PRUN.Y > MaxY() - 30)
                    {
                        PRUN.PISO = true;
                        PRUN.TECHO = false;
                    }
                }
                else if (PRUN.PISO)
                {
                    PRUN.Y = PRUN.Y - 5;
                    if (PRUN.Y < 0)
                    {
                        PRUN.PISO = false;
                        PRUN.TECHO = true;
                    }
                }

                PRUN.HORA = Environment.TickCount;
                var random = new Random(Environment.TickCount);
                var value = random.Next(0, 20);
                Q.Meter(PRUN);
                if (value == 0)
                {
                    P.TIPO = Constants.BALAN;
                    P.ANCHO = 5;
                    P.ALTO = 10;
                    P.X = (PRUN.ANCHO - P.ANCHO) / 2 + PRUN.X;
                    P.Y = PRUN.Y + PRUN.ALTO + P.ALTO;
                    P.RETARDO = 25;
                    P.HORA = Environment.TickCount;
                    ListaBalasN.Add(P);
                    if (ListaBalasN.Count > 25)
                    {
                        ListaBalasN.Clear();
                    }

                    Q.Meter(P);
                }
            }
        }
       
        private void Form1_Paint(object sender, PaintEventArgs e)
        {

            for (int i =0; i < ListaNaves.Count; i++)
            {
                if (!ListaNaves[i].CHOCADo)
                {
                    if (ListaNaves[i].vida == 4)
                    {
                        e.Graphics.DrawImage(EnemigoAzu, new Point(ListaNaves[i].X, ListaNaves[i].Y));
                    }
                    if (ListaNaves[i].vida==3)
                    {
                        e.Graphics.DrawImage(Enemigo, new Point(ListaNaves[i].X, ListaNaves[i].Y));
                    }
                    if (ListaNaves[i].vida == 2)
                    {
                        e.Graphics.DrawImage(EnemigoAmar, new Point(ListaNaves[i].X, ListaNaves[i].Y));
                    }
                    if (ListaNaves[i].vida == 1)
                    {
                        e.Graphics.DrawImage(EnemigoRojo, new Point(ListaNaves[i].X, ListaNaves[i].Y));
                    }


                }
            }
            for (int i=0; i<ListaBalasN.Count;i++)
            {
                e.Graphics.DrawImage(BalaN, new Point(ListaBalasN[i].X,ListaBalasN[i].Y));
            }
            for (int i = 0; i < ListaBalasU.Count; i++)
            {
                e.Graphics.DrawImage(BalaU, new Point(ListaBalasU[i].X, ListaBalasU[i].Y));
            }
            if(Estado==0&&(!muerto||!ganar))
            {
                e.Graphics.DrawImage(Nave, new Point(User.X, User.Y));
            }
            if(choco)
            {              
                choco = false;
                vida--;                          
            }
            if (vida == 0)
            {
                Estado = 1;
                muerto = true;
            }
            int num = 120;
            int num2 = 15;
            label1.Text = vida.ToString();
            for (int i=0; i<vida; i++)
            {                
                e.Graphics.DrawImage(Corazon, new Point(num, num2));
                num = num + num2;
            }   
            if(muerto)
            {
                e.Graphics.DrawImage(Muerto, new Point(-320, -200));
                Estado = 1;
                button1.Visible = true;
            }
            if(ganar)
            {
                e.Graphics.DrawImage(win, new Point(40, 0));
                Estado = 1;
                button1.Visible = true;
            }
        }     
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            PCB P = new PCB();
            switch (e.KeyData)
            {
                case Keys.A: User.X = User.X - 10;                                 
                    break;
                case Keys.D: User.X = User.X + 10;                   
                    break;
                case Keys.W: User.Y = User.Y - 10;                    
                    break;
                case Keys.S: User.Y = User.Y + 10;                   
                    break;
                case Keys.F:
                    P.TIPO = Constants.BALAU;
                    P.ANCHO = 5;
                    P.ALTO = 10;
                    P.X = (User.ANCHO - P.ANCHO) / 2 + User.X;
                    P.Y = User.Y- User.ALTO;
                    P.RETARDO = 25;
                    P.HORA = Environment.TickCount;                   
                    ListaBalasU.Add(P);                                   
                    if(ListaBalasU.Count>10)//SOLO PUEDE DISPARAR 10 VECES SEGUIDAS
                    {
                        ListaBalasU.Clear();
                    }
                    Q.Meter(P);                  
                    break;
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            titulo.Visible = false;

            button1.Visible = false;
            pictureBox1.Visible = false;
            pictureBox2.Visible = false;
            pictureBox3.Visible = false;
            button2.Visible = false;
            button3.Visible = false;
            button4.Visible = false;
            label3.Visible = false;
            label4.Visible = true;
            InitJuego();
        }

   

        private void button2_Click(object sender, EventArgs e)
        {
            seleccionado = @"GREEN.png";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            seleccionado = @"BLUE.png";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            seleccionado = @"RED.png";
        }

       
    }
}
