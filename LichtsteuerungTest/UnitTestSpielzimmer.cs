using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lichtsteuerung;
using System;
using System.Threading;

namespace LichtsteuerungTest
{
    [TestClass]
    public class UnitTestSpielzimmer
    {

        private void Initialize()
        {
            SteuerungLogic.Instance.IsDebug = true; //kann ganz am anfang aktiviert werden
            SteuerungLogic.Instance.Start(); //führt dann den konstruktor aus

            InitializeDefaultValues();

        }

        private void InitializeDefaultValues()
        {
            //Werte auf Standard retoursetzen damit die Tests immer gleich laufen
            SteuerungLogic.Instance.JemandZuhause.DebugSetStatus(false);
            SteuerungLogic.Instance.LichtsteuerungSpielzimmer.RaumBewegung.DebugSetStatus(false);         
            SteuerungLogic.Instance.LichtsteuerungSpielzimmer.RaumLicht.ZielStatus = false;
        }

        private void JemandZuhauseAktivieren()
        {
            SteuerungLogic.Instance.JemandZuhause.DebugSetStatus(true);
            SteuerungLogic.Instance.JemandZuhause.RaiseDataChange(true);
            if (SteuerungLogic.Instance.LichtsteuerungSpielzimmer.StateMachine.CurrentState != JusiBase.State.Aus)
            {
                Console.WriteLine("Fehler bei Standard, müsste auf ReadyForAction sein");
            }
        }

        private void StandardEinschalten()
        {
            //licht schaltet ein
            SteuerungLogic.Instance.LichtsteuerungSpielzimmer.RaumLicht.ZielStatus = true;
            SteuerungLogic.Instance.LichtsteuerungSpielzimmer.RaumLicht.RaiseDataChange(true);
            if (SteuerungLogic.Instance.LichtsteuerungSpielzimmer.StateMachine.CurrentState != JusiBase.State.Action)
            {
                Console.WriteLine("Fehler bei Standard, müsste auf Action sein");
            }           
        }

        [TestMethod]
        public void SpielzimmerLichtAusManuell()
        {


            try
            {

                Initialize();
                JemandZuhauseAktivieren();
                StandardEinschalten();

                //nun wieder aussschalten, dafür restlaufzeit min übersteuern
                SteuerungLogic.Instance.LichtsteuerungSpielzimmer.RaumLicht.ZielStatus = false;
                SteuerungLogic.Instance.LichtsteuerungSpielzimmer.RaumLicht.RaiseDataChange(true);
                if (SteuerungLogic.Instance.LichtsteuerungSpielzimmer.StateMachine.CurrentState != JusiBase.State.Aus)
                {
                    Console.WriteLine("Fehler bei AnkleideStandard, müsste auf Aus sein");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler bei TestMethod1", ex);
                //throw;
            }
        }

        [TestMethod]
        public void SpielzimmerLichtAusBewegung()
        {
            try
            {

                Initialize();
                JemandZuhauseAktivieren();
                StandardEinschalten();


                           
                SteuerungLogic.Instance.LichtsteuerungSpielzimmer.RaumBewegung.DebugSetStatus(true);
                SteuerungLogic.Instance.LichtsteuerungSpielzimmer.RaumBewegung.RaiseDataChange(true);

                if (SteuerungLogic.Instance.LichtsteuerungSpielzimmer.StateMachine.CurrentState != JusiBase.State.Action)
                {
                    Console.WriteLine("Fehler bei SpielzimmerStandard, müsste auf Action sein");
                }

                Thread.Sleep(10000);

                SteuerungLogic.Instance.LichtsteuerungSpielzimmer.RaumBewegung.DebugSetStatus(false);
                SteuerungLogic.Instance.LichtsteuerungSpielzimmer.RaumBewegung.RaiseDataChange(true);

                if (SteuerungLogic.Instance.LichtsteuerungSpielzimmer.StateMachine.CurrentState != JusiBase.State.Action)
                {
                    Console.WriteLine("Fehler bei SpielzimmerStandard, müsste auf Action sein");
                }

                //1 minute warten
                Thread.Sleep(45000);

                if (SteuerungLogic.Instance.LichtsteuerungSpielzimmer.StateMachine.CurrentState != JusiBase.State.Aus)
                {
                    Console.WriteLine("Fehler bei SpielzimmerStandard, müsste auf Aus sein");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler bei TestMethod1", ex);
                //throw;
            }
        }

        [TestMethod]
        public void SpielzimmerLichtAusNieBewegung()
        {           
            try
            {

                Initialize();
                JemandZuhauseAktivieren();
                StandardEinschalten();

                
                //nun wieder aussschalten             
                //SteuerungLogic.Instance.LichtsteuerungSpielzimmer.SpielzimmerBewegung.DebugSetStatus(false);
                //SteuerungLogic.Instance.LichtsteuerungSpielzimmer.SpielzimmerBewegung.RaiseDataChange(true);

                //1 minute warten
                Thread.Sleep(45000);

                if (SteuerungLogic.Instance.LichtsteuerungSpielzimmer.StateMachine.CurrentState != JusiBase.State.Aus)
                {
                    Console.WriteLine("Fehler bei SpielzimmerStandard, müsste auf Aus sein");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler bei TestMethod1", ex);
                //throw;
            }
        }
        
      
    }
}
