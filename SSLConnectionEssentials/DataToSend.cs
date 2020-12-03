using System;
using System.Text;

namespace SSLConnectionEssentials
{
    public class DataToSend
    {
        public string Fname { get; set; }
        public string Lname { get; set; }
        public string Email { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Genres { get; set; }
        public string Version { get; set; }
        public string SendTime { get; set; }
        public string SendYear { get; set; }

        public string Artist { get; set; }

        public DataToSend(string name, string lname, string email, string city, string state, string zipCode,string artist, string genres)
        {
            Fname = name ;
            Lname = lname;
            Email = email ;
            City = city;
            State = state;
            ZipCode = zipCode;
            Genres = genres;
            Version = "1.0.0";
            SendTime = DateTime.Now.ToString("h:mm:ss tt");
            SendYear = DateTime.Now.Year.ToString();
            Artist= artist;
        }
        public DataToSend()
        {
            //Version = "1.0.0";
            //SendTime = DateTime.Now.ToString("h:mm:ss tt");
            //Random fuck = new Random();
            //SendYear = DateTime.Now.Year.ToString()+fuck.Next(5);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(Fname);
            builder.AppendLine(Lname);
            builder.AppendLine(Email);
            builder.AppendLine(City);
            builder.AppendLine(State);
            builder.AppendLine(ZipCode);
            builder.AppendLine(Genres);
            builder.AppendLine(Artist);
            builder.AppendLine(Version);
            builder.AppendLine(SendTime);
            builder.AppendLine(SendYear);

            return builder.ToString();
        }

        public void Dispose()
        {
            Fname = null;
            Lname = null;
            Email = null;
            City = null;
            State = null;         
            Genres = null;
            ZipCode = null;
            Version = null;
            SendTime = null;
            SendYear = null;
            Artist = null;
        }
    }
}
