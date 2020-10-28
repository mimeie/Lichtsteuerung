using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using JusiBase;

namespace Lichtsteuerung
{
    [ApiController]
    [Route("api/[controller]")]
    public class LichtsteuerungController : ControllerBase
    {
       

        private readonly ILogger<LichtsteuerungController> _logger;

        public LichtsteuerungController(ILogger<LichtsteuerungController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{id}", Name = "Get")]
        public ResponseTrigger Get(string id)
        {

            try
            {
                Console.WriteLine("getter Aufruf");

                SteuerungLogic.Instance.Update();

                return new ResponseTrigger
                {
                    ReturnCode = 0,
                    ReturnState = SteuerungLogic.Instance.StateMachine.CurrentState.ToString()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler bei LichtsteuerungAufruf: " + ex);

                return new ResponseTrigger
                {
                    ReturnCode = 0,
                    ReturnState = "Fehler"
                };

            }


        }
    }
}
