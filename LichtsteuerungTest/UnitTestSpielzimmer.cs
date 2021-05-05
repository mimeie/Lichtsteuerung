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
            SteuerungLogic.Instance.Start(); //f�hrt dann den konstruktor aus

            InitializeDefaultValues();

        }

        private void InitializeDefaultValues()
        {
            //Werte auf Standard retoursetzen damit die Tests immer gleich laufen
            SteuerungLogic.Instance.JemandZuhause.DebugSetStatus(false);
            SteuerungLogic.Instance.LichtsteuerungSpielzimmer.ZimmerBewegung.DebugSetStatus(false);         
            SteuerungLogic.Instance.LichtsteuerungSpielzimmer.ZimmerLicht.ZielStatus = false;
        }

        private void JemandZuhauseAktivieren()
        {
            SteuerungLogic.Instance.JemandZuhause.DebugSetStatus(true);
            SteuerungLogic.Instance.JemandZuhause.RaiseDataChange(true);
            if (SteuerungLogic.Instance.LichtsteuerungSpielzimmer.StateMachine.CurrentState != JusiBase.State.Aus)
            {
                Console.WriteLine("Fehler bei Standard, m�sste auf ReadyForAction sein");
            }
        }

        private void StandardEinschalten()
        {
            //licht schaltet ein
            SteuerungLogic.Instance.LichtsteuerungSpielzimmer.ZimmerLicht.ZielStatus = true;
            SteuerungLogic.Instance.LichtsteuerungSpielzimmer.ZimmerLicht.RaiseDataChange(true);
            if (SteuerungLogic.Instance.LichtsteuerungSpielzimmer.StateMachine.CurrentState != JusiBase.State.Action)
            {
                Console.WriteLine("Fehler bei Standard, m�sste auf Action sein");
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

                //nun wieder aussschalten, daf�r restlaufzeit min �bersteuern
                SteuerungLogic.Instance.LichtsteuerungSpielzimmer.ZimmerLicht.ZielStatus = false;
                SteuerungLogic.Instance.LichtsteuerungSpielzimmer.ZimmerLicht.RaiseDataChange(true);
                if (SteuerungLogic.Instance.LichtsteuerungSpielzimmer.StateMachine.CurrentState != JusiBase.State.Aus)
                {
                    Console.WriteLine("Fehler bei AnkleideStandard, m�sste auf Aus sein");
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


                           
                SteuerungLogic.Instance.LichtsteuerungSpielzimmer.ZimmerBewegung.DebugSetStatus(true);
                SteuerungLogic.Instance.LichtsteuerungSpielzimmer.ZimmerBewegung.RaiseDataChange(true);

                if (SteuerungLogic.Instance.LichtsteuerungSpielzimmer.StateMachine.CurrentState != JusiBase.State.Action)
                {
                    Console.WriteLine("Fehler bei SpielzimmerStandard, m�sste auf Action sein");
                }

                Thread.Sleep(10000);

                SteuerungLogic.Instance.LichtsteuerungSpielzimmer.ZimmerBewegung.DebugSetStatus(false);
                SteuerungLogic.Instance.LichtsteuerungSpielzimmer.ZimmerBewegung.RaiseDataChange(true);

                if (SteuerungLogic.Instance.LichtsteuerungSpielzimmer.StateMachine.CurrentState != JusiBase.State.Action)
                {
                    Console.WriteLine("Fehler bei SpielzimmerStandard, m�sste auf Action sein");
                }

                //1 minute warten
                Thread.Sleep(45000);

                if (SteuerungLogic.Instance.LichtsteuerungSpielzimmer.StateMachine.CurrentState != JusiBase.State.Aus)
                {
                    Console.WriteLine("Fehler bei SpielzimmerStandard, m�sste auf Aus sein");
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
                    Console.WriteLine("Fehler bei SpielzimmerStandard, m�sste auf Aus sein");
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
