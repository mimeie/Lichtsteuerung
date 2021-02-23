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


        public bool IsDebug;

        public SensorBool JemandZuhause; //master
        //master2


        //Sublogiken
        public LichtsteuerungAnkleidezimmer LichtsteuerungAnkleidezimmer;
        public LichtsteuerungGarderobe LichtsteuerungGarderobe;




        private SteuerungLogic()
        {
                  
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
            Console.WriteLine("Steuerungsobjekte initieren");
            LichtsteuerungAnkleidezimmer = new LichtsteuerungAnkleidezimmer();
            LichtsteuerungGarderobe = new LichtsteuerungGarderobe();

            if (IsDebug == false)
            {
                JemandZuhause = new SensorBool("0_userdata.0.IsAnybodyHome");
            }
            else
            {
                JemandZuhause = new SensorBool("0_userdata.0.DebugIsAnybodyHome");
            }
            
            //braucht es im moment nicht JemandZuhause.DataChange += DoDataChange; 

            Console.WriteLine("Steuerung starten");

            JemandZuhause.Update();

            LichtsteuerungAnkleidezimmer.Initialize();
            LichtsteuerungGarderobe.Initialize();



            Console.WriteLine("JobManager wurde initialisiert");
            Console.WriteLine("Steuerung gestartet");

            //zum testen           
           
        }


       

        public void Stop()
        {
        }

        private void DoDataChange(object sender, Objekt source) //im moment im singleton nicht gebraucht
        {
            try
            {                
                source.Update();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler beim DoDataChange", ex);
                //throw;
            }
        }



    }
}
