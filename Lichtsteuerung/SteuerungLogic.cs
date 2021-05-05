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

        //automatisches ausschalten
        public LichtsteuerungAutoAus LichtsteuerungSpielzimmer;
        public LichtsteuerungAutoAus LichtsteuerungPhilomenaStehlampe;
        

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

            LichtsteuerungSpielzimmer = new LichtsteuerungAutoAus("LichtsteuerungSpielzimmer","zigbee.0.00158d0004abd3aa.occupancy", "shelly.0.SHSW-25#D8BFC01A2B2A#1.Relay1.Switch",8);
            LichtsteuerungPhilomenaStehlampe = new LichtsteuerungAutoAus("LichtsteuerungPhilomenaStehlampe", "zigbee.0.00158d000504e521.occupancy", "zigbee.0.588e81fffef59c5d.state", 15);


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
            LichtsteuerungSpielzimmer.Initialize();


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
