using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using System.IO;

namespace RGDCallPredictor
{
    public class OpenWeatherMapProxy
    {
        public async static Task<RootObject> GetWeather()
        {
            var http = new HttpClient();
            var response = await http.GetAsync("http://api.openweathermap.org/data/2.5/weather?id=5037649&APPID=fb095d6eef5d6ff1ef6a8d087ceba1d2&units=imperial");
            var result = await response.Content.ReadAsStringAsync();
            var serializer = new DataContractJsonSerializer(typeof(RootObject));

            var ms = new MemoryStream(Encoding.UTF8.GetBytes(result));
            var data = (RootObject)serializer.ReadObject(ms);

            return data;
        }
    }

    [DataContract]
    public class Weather
    {
        [DataMember]
        public string icon { get; set; }
    }

    [DataContract]
    public class Main
    {
        [DataMember]
        public double temp_min { get; set; }

        [DataMember]
        public double temp_max { get; set; }
    }

    [DataContract]
    public class RootObject
    {
        [DataMember]
        public List<Weather> weather { get; set; }

        [DataMember]
        public Main main { get; set; }
    } 
}
