using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lichtsteuerung;
using System;

namespace LichtsteuerungTest
{
    [TestClass]
    public class UnitTestAnkleide
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
            SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.AnkleideBewegung.DebugSetStatus(false);
            SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.AnkleideTuer.DebugSetStatus(true);
            SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.AnkleideHelligkeit.DebugSetHelligkeit(10);
            SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.LichtAnkleide.ZielStatus = false;
        }

        private void JemandZuhauseAktivieren()
        {
            SteuerungLogic.Instance.JemandZuhause.DebugSetStatus(true);
            SteuerungLogic.Instance.JemandZuhause.RaiseDataChange(true);
            if (SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.StateMachine.CurrentState != JusiBase.State.ReadyForAction)
            {
                Console.WriteLine("Fehler bei AnkleideStandard, müsste auf ReadyForAction sein");
            }
        }

        private void StandardEinschalten()
        {
            //tür öffnen
            SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.AnkleideTuer.DebugSetStatus(false);
            SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.AnkleideTuer.RaiseDataChange(true);
            if (SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.StateMachine.CurrentState != JusiBase.State.Action)
            {
                Console.WriteLine("Fehler bei AnkleideStandard, müsste auf Action sein");
            }

            //fehler provozieren, helligkeit verschiebt sich, licht darf nicht ausgeschaltet werden
            SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.AnkleideHelligkeit.DebugSetHelligkeit(8);
            SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.AnkleideHelligkeit.RaiseDataChange(true);
            if (SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.StateMachine.CurrentState != JusiBase.State.Action)
            {
                Console.WriteLine("Fehler bei AnkleideStandard, müsste auf Action sein");
            }

            //nun kommt bewegung hinzu
            SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.AnkleideBewegung.DebugSetStatus(true);
            SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.AnkleideBewegung.RaiseDataChange(true);
            if (SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.StateMachine.CurrentState != JusiBase.State.Action)
            {
                Console.WriteLine("Fehler bei AnkleideStandard, müsste auf Action sein");
            }
        }

        [TestMethod]
        public void AnkleideLichtAusBewegung()
        {

           
            try
            {

                Initialize();
                JemandZuhauseAktivieren();
                StandardEinschalten();

                //nun wieder aussschalten, dafür restlaufzeit min übersteuern
                SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.AnkleideBewegung.MinLaufzeitMinutes = 0;
                SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.AnkleideBewegung.DebugSetStatus(false);
                SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.AnkleideBewegung.RaiseDataChange(true);
                if (SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.StateMachine.CurrentState != JusiBase.State.ReadyForAction)
                {
                    Console.WriteLine("Fehler bei AnkleideStandard, müsste auf ReadyForAction sein");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler bei TestMethod1", ex);
                //throw;
            }
        }

        [TestMethod]
        public void AnkleideLichtHelligkeit()
        {
            try
            {
                Initialize();
                JemandZuhauseAktivieren();
                StandardEinschalten();


                //nun wieder aussschalten, dafür restlaufzeit min übersteuern
                SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.AnkleideHelligkeit.DebugSetHelligkeit(98);
                SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.AnkleideHelligkeit.RaiseDataChange(true);
                if (SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.StateMachine.CurrentState != JusiBase.State.Aus)
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
        public void AnkleideStatusHelligkeit()
        {
            try
            {
                Initialize();
                JemandZuhauseAktivieren();


                //nun wieder aussschalten, dafür restlaufzeit min übersteuern
                SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.AnkleideHelligkeit.DebugSetHelligkeit(98);
                SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.AnkleideHelligkeit.RaiseDataChange(true);
                if (SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.StateMachine.CurrentState != JusiBase.State.Aus)
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
        public void AnkleideLichtManuell()
        {
            try
            {
                Initialize();
                JemandZuhauseAktivieren();

                SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.LichtAnkleide.ZielStatus=true;
                SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.LichtAnkleide.RaiseDataChange(true);
                if (SteuerungLogic.Instance.LichtsteuerungAnkleidezimmer.StateMachine.CurrentState != JusiBase.State.Action)
                {
                    Console.WriteLine("Fehler bei AnkleideStandard, müsste auf Action bleiben");
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
