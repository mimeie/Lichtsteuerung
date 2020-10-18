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

        [HttpGet("{messageType}", Name = "Get")]
        public ResponseTrigger Get(string messageType, string subject, string text)
        {

            try
            {
                Console.WriteLine("getter Aufruf");
                return new ResponseTrigger
                {
                    ReturnCode = 0,
                    ReturnState = "unbekannt"
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
