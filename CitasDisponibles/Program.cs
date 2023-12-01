using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace CitasDisponibles
{
    internal class Program
    {
        List<SkeletorCitas> Citas;
        string diaConsultar;
        DateTime horaFinalDia = DateTime.ParseExact("17:00", "HH:mm", null);
        static void Main(string[] args)
        {
            Program program = new Program();
            var continuar = "si";
            program.LecturaJson();
            while(continuar.ToLower() == "si" || continuar.ToLower() == "sí")
            {
                Console.WriteLine("¿Cuál día de la semana quiere consultar?");
                program.diaConsultar = Console.ReadLine();

                if (program.diaConsultar.ToLower()=="lunes" || program.diaConsultar.ToLower() == "martes" || program.diaConsultar.ToLower() == "miércoles" || program.diaConsultar.ToLower() == "jueves" || program.diaConsultar.ToLower() == "viernes")
                {
                    Console.WriteLine($"Total de espacios disponibles {program.CuposDisponibles(program.diaConsultar)}");
                    Console.WriteLine("¿Quiere consultar otro día? (Sí - No)");
                    continuar = Console.ReadLine();
                }
                else
                {
                    Console.WriteLine("El día que escribió no es correcto.");
                }
            }
        }
        private void LecturaJson()
        {
            
            string jsonCitas = "https://luegopago.blob.core.windows.net/luegopago-uploads/Pruebas%20LuegoPago/data.json";
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string jsonContent = client.GetStringAsync(jsonCitas).Result;

                    Citas = JsonConvert.DeserializeObject<List<SkeletorCitas>>(jsonContent);

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al leer el archivo JSON: {ex.Message}");
                }
            }
        }
        private class SkeletorCitas
        {
            public string Day { get; set; }
            public string Hour { get; set; }
            public string Duration { get; set; }
        }
        private int CuposDisponibles(string diaCita)
        {
            List<SkeletorCitas> diaConsultadoCitas = Citas.Where(cita => cita.Day.ToLower() == diaCita.ToLower()).ToList();
            var citasOrdenadasPorHora = diaConsultadoCitas.OrderBy(cita => cita.Hour).ToList();
            DateTime horaComparacion = DateTime.ParseExact("09:00", "HH:mm", null);
            int cuposDisponibles = 0;
            foreach (var horariosOcupados in citasOrdenadasPorHora)
            {
                if ((DateTime.ParseExact(horariosOcupados.Hour, "HH:mm", null)< horaFinalDia))
                {
                    var minutosDisponibles = (DateTime.ParseExact(horariosOcupados.Hour, "HH:mm", null) - horaComparacion).TotalMinutes;
                    cuposDisponibles = minutosDisponibles / 30 >= 1 ? cuposDisponibles + (int)Math.Floor(minutosDisponibles / 30) : cuposDisponibles;
                    horaComparacion = (DateTime.ParseExact(horariosOcupados.Hour, "HH:mm", null).AddMinutes(Convert.ToUInt16(horariosOcupados.Duration)));
                }
            }
            if (horaComparacion <= horaFinalDia)
            {
                var minutosDisponibles = (horaFinalDia - horaComparacion).TotalMinutes;
                cuposDisponibles = minutosDisponibles / 30 > 1 ? cuposDisponibles + (int)Math.Floor(minutosDisponibles / 30) : cuposDisponibles;
            }
            return cuposDisponibles;
        }
    }
}
