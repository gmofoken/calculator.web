using calculator.web.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace calculator.web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CalculatorController : ControllerBase
    {
        private readonly ICalculator _Calculator;

        public CalculatorController(ICalculator calculator)
        {
            _Calculator = calculator;
        }

        [HttpGet("calculate")]
        public ActionResult Get(string input)
        {
            try
            {
                var results = _Calculator.Parse(input);
                return Ok(results);
            }
            catch (System.Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}