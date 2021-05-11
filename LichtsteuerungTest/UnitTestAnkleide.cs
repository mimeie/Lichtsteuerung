using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lichtsteuerung;
using System;

namespace LichtsteuerungTest
{
    [TestClass]
    public class UnitTestRaum
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
            SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.RaumBewegung.DebugSetStatus(false);
            SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.RaumTuer.DebugSetStatus(true);
            SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.RaumHelligkeit.DebugSetHelligkeit(10);
            SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.RaumLicht.ZielStatus = false;
        }

        private void JemandZuhauseAktivieren()
        {
            SteuerungLogic.Instance.JemandZuhause.DebugSetStatus(true);
            SteuerungLogic.Instance.JemandZuhause.RaiseDataChange(true);
            if (SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.StateMachine.CurrentState != JusiBase.State.ReadyForAction)
            {
                Console.WriteLine("Fehler bei RaumStandard, müsste auf ReadyForAction sein");
            }
        }

        private void StandardEinschalten()
        {
            //tür öffnen
            SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.RaumTuer.DebugSetStatus(false);
            SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.RaumTuer.RaiseDataChange(true);
            if (SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.StateMachine.CurrentState != JusiBase.State.Action)
            {
                Console.WriteLine("Fehler bei RaumStandard, müsste auf Action sein");
            }

            //fehler provozieren, helligkeit verschiebt sich, licht darf nicht ausgeschaltet werden
            SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.RaumHelligkeit.DebugSetHelligkeit(8);
            SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.RaumHelligkeit.RaiseDataChange(true);
            if (SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.StateMachine.CurrentState != JusiBase.State.Action)
            {
                Console.WriteLine("Fehler bei RaumStandard, müsste auf Action sein");
            }

            //nun kommt bewegung hinzu
            SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.RaumBewegung.DebugSetStatus(true);
            SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.RaumBewegung.RaiseDataChange(true);
            if (SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.StateMachine.CurrentState != JusiBase.State.Action)
            {
                Console.WriteLine("Fehler bei RaumStandard, müsste auf Action sein");
            }
        }

        [TestMethod]
        public void RaumLichtAusBewegung()
        {

           
            try
            {

                Initialize();
                JemandZuhauseAktivieren();
                StandardEinschalten();

                //nun wieder aussschalten, dafür restlaufzeit min übersteuern
                SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.RaumBewegung.MinLaufzeitMinutes = 0;
                SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.RaumBewegung.DebugSetStatus(false);
                SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.RaumBewegung.RaiseDataChange(true);
                if (SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.StateMachine.CurrentState != JusiBase.State.ReadyForAction)
                {
                    Console.WriteLine("Fehler bei RaumStandard, müsste auf ReadyForAction sein");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler bei TestMethod1", ex);
                //throw;
            }
        }

        [TestMethod]
        public void RaumLichtHelligkeit()
        {
            try
            {
                Initialize();
                JemandZuhauseAktivieren();
                StandardEinschalten();


                //nun wieder aussschalten, dafür restlaufzeit min übersteuern
                SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.RaumHelligkeit.DebugSetHelligkeit(98);
                SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.RaumHelligkeit.RaiseDataChange(true);
                if (SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.StateMachine.CurrentState != JusiBase.State.Aus)
                {
                    Console.WriteLine("Fehler bei RaumStandard, müsste auf Aus sein");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler bei TestMethod1", ex);
                //throw;
            }
        }

        [TestMethod]
        public void RaumStatusHelligkeit()
        {
            try
            {
                Initialize();
                JemandZuhauseAktivieren();


                //nun wieder aussschalten, dafür restlaufzeit min übersteuern
                SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.RaumHelligkeit.DebugSetHelligkeit(98);
                SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.RaumHelligkeit.RaiseDataChange(true);
                if (SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.StateMachine.CurrentState != JusiBase.State.Aus)
                {
                    Console.WriteLine("Fehler bei RaumStandard, müsste auf Aus sein");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler bei TestMethod1", ex);
                //throw;
            }
        }



        [TestMethod]
        public void RaumLichtManuell()
        {
            try
            {
                Initialize();
                JemandZuhauseAktivieren();

                SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.RaumLicht.ZielStatus=true;
                SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.RaumLicht.RaiseDataChange(true);
                if (SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.StateMachine.CurrentState != JusiBase.State.Action)
                {
                    Console.WriteLine("Fehler bei RaumStandard, müsste auf Action bleiben");
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
