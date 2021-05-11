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
        public LichtsteuerungGarderobe LichtsteuerungGarderobe;


        public LichtsteuerungAuto LichtsteuerungAnkleidezimmer;
        public LichtsteuerungAuto LichtsteuerungWaschraum;
        public LichtsteuerungAuto LichtsteuerungKeller;

        //automatisches ausschalten
        //public List<SensorBool> SpielzimmerBewegungen;
        //public SensorBool SpielzimmerBewegung;


        public LichtsteuerungAutoAus LichtsteuerungSpielzimmer;
        public LichtsteuerungAutoAus LichtsteuerungPhilomenaStehlampe;
        public LichtsteuerungAutoAus LichtsteuerungKueche;
        public LichtsteuerungAutoAus LichtsteuerungWohnzimmer;


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
            LichtsteuerungAnkleidezimmer = new LichtsteuerungAuto("Lichtsteuerung Ankleide", "zigbee.0.00158d00063a6d54.occupancy", "shelly.0.SHSW-25#D8BFC01A2B2A#1.Relay0.Switch", "zigbee.0.00158d00063a6d54.illuminance", "zigbee.0.00158d00025d978b.contact",55,4);
            LichtsteuerungWaschraum = new LichtsteuerungAuto("Lichtsteuerung Waschraum", "zigbee.0.00158d0005228c10.occupancy", "zigbee.0.842e14fffe1f104c.state", "zigbee.0.00158d0005228c10.illuminance", "zigbee.0.00158d0002a70010.contact", 80, 4);
            LichtsteuerungWaschraum = new LichtsteuerungAuto("Lichtsteuerung Waschraum", "zigbee.0.00158d0005228c10.occupancy", "zigbee.0.842e14fffe1f104c.state", "zigbee.0.00158d0005228c10.illuminance", 80, 4);


            LichtsteuerungGarderobe = new LichtsteuerungGarderobe();


            //SpielzimmerBewegung = new SensorBool("zigbee.0.00158d0004abd3aa.occupancy");
            //SpielzimmerBewegung.MinLaufzeitMinutes = 8;
            //SpielzimmerBewegungen.Add(SpielzimmerBewegung);
            LichtsteuerungSpielzimmer = new LichtsteuerungAutoAus("LichtsteuerungSpielzimmer","zigbee.0.00158d0004abd3aa.occupancy", SourceType.TrueFalse, "shelly.0.SHSW-25#D8BFC01A2B2A#1.Relay1.Switch",8);
            LichtsteuerungPhilomenaStehlampe = new LichtsteuerungAutoAus("LichtsteuerungPhilomenaStehlampe", "zigbee.0.00158d000504e521.occupancy", SourceType.TrueFalse, "zigbee.0.588e81fffef59c5d.state", 15);
            LichtsteuerungKueche = new LichtsteuerungAutoAus("Lichtsteuerung Küche", "zwave2.0.Node_004.Basic.currentValue",SourceType.Integer, "shelly.0.SHSW-25#D8BFC01A263A#1.Relay1.Switch", 10);
            LichtsteuerungWohnzimmer = new LichtsteuerungAutoAus("Lichtsteuerung Küche", "zigbee.0.00158d000504e6df.occupancy",SourceType.TrueFalse, "shelly.0.SHBDUO-1#D0CB57#1.lights.Switch", 60);


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
