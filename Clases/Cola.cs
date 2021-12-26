using System;
using System.Collections.Generic;
using System.Text;

namespace SkyFire
{
    
    static class Constants
    {
        public const int MAX = 200;
        public const int MaxLevel = 2;
        public const int NAVE  = 0;
        public const int BALAU = 1;
        public const int BALAN = 2;
        public const int NAVEP = 3;
        public const int NAVEM = 4;
       
    }
    class Cola
    {
        private PCB[] V = new PCB[200];
        private int ini;
        private int fin;
        public Cola()
        {
            Init();
        }
        public void Init()
        {
            ini = 0;
            fin = 0;
        }
        public bool Vacia()
        {
            return ini == 0;
        }
        public bool Llena()
        {
            return (Length()==Constants.MAX);
            
        }
        public int Length()
        {
           if(fin==0)
            {
                return 0;
            }
            else
            {
                if(ini<=fin)
                {
                    return fin - ini + 1;
                }
                else
                {
                    return fin + (Constants.MAX -ini + 1);
                }
            }

        }
        public void Meter(PCB P)
        {
            if (!Llena())
            {
                if (Vacia())
                {
                    ini = 1;
                    fin = 1;
                }
                else
                {
                    fin = siguiente(fin);
                }
                V[fin] = P;
            }
        }
        public PCB Sacar()
        {
          
                
            PCB aux = V[ini];         
           
            if(ini==fin)
            {
                Init();
                return aux;
            }
            else
            {
                ini = siguiente(ini);
            }
            return aux;


        }
        public int Cant(int Tipo)
        {
            int cont = 0;
            foreach (PCB i in V)
            {
                if(i.TIPO==Tipo)
                {
                    cont++;
                }
            }
            return cont;
        }
        int siguiente(int e)
        {
            if(e+1==Constants.MAX)
            {
                return 1;
            }
            return e + 1;

        }
    }
}
