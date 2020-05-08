using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Algorithm2.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PathController : ControllerBase
    {
        [HttpGet]
        public string Gat()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
            var cords = new Coordinates[400];
            var weights = new int[400];
            var lines = System.IO.File.ReadAllLines("test.txt");
            var style = NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign;
            for (int i = 0; i < 400; i++)
            {
                var line = lines[i + 1].Split(' ');
                cords[i] = new Coordinates(double.Parse(line[0], style), double.Parse(line[1], style));
                weights[i] = int.Parse(line[2]);
            }

            return "asdf";
        }
    }
}