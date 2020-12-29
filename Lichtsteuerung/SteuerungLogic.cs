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



        public SensorBool JemandZuhause; //master


        //Sublogiken
        public LichtsteuerungAnkleidezimmer LichtsteuerungAnkleidezimmer;
        public LichtsteuerungGarderobe LichtsteuerungGarderobe;




        private SteuerungLogic()
        {

            


            LichtsteuerungAnkleidezimmer = new LichtsteuerungAnkleidezimmer();
            LichtsteuerungGarderobe = new LichtsteuerungGarderobe();


            JemandZuhause = new SensorBool("0_userdata.0.IsAnybodyHome");
            JemandZuhause.DataChange += DoDataChange;



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

        private void DoDataChange(object sender, Objekt source)
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
