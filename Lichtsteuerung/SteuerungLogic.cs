using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

using JusiBase;
//Update-Package

namespace Lichtsteuerung
{
    public sealed class SteuerungLogic
    {
        private static volatile SteuerungLogic _instance;
        private static object _syncRoot = new object();


       


        //Sublogiken
        public LichtsteuerungAnkleidezimmer LichtsteuerungAnkleidezimmer;
        public LichtsteuerungGarderobe LichtsteuerungGarderobe;




        private SteuerungLogic()
        {

            


            LichtsteuerungAnkleidezimmer = new LichtsteuerungAnkleidezimmer();
            LichtsteuerungGarderobe = new LichtsteuerungGarderobe();






        }

     

        public static SteuerungLogic Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new SteuerungLogic();
                        }
                    }
                }

                return _instance;
            }
        }


        public void Start()
        {
            Console.WriteLine("Steuerung starten");

            LichtsteuerungAnkleidezimmer.Initialize();
            LichtsteuerungGarderobe.Initialize();



            Console.WriteLine("JobManager wurde initialisiert");
            Console.WriteLine("Steuerung gestartet");

            //zum testen           
           
        }


       

        public void Stop()
        {
        }



    }
}
