using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.IO;

namespace njutn
{
    class Point
    {
        public double x;
        public double y;

        public Point(double a, double b)
        {
            x = a;
            y = b;
        }
    }
    class Program
    {
        static readonly string PasswordHash = "P@@Sw0rd";
        static readonly string SaltKey = "S@LT&KEY";
        static readonly string VIKey = "@1B2c3D4e5F6g7H8";

        public static string Encrypt(string plainText)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            byte[] keyBytes = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(256 / 8);
            var symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC, Padding = PaddingMode.Zeros };
            var encryptor = symmetricKey.CreateEncryptor(keyBytes, Encoding.ASCII.GetBytes(VIKey));

            byte[] cipherTextBytes;

            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                    cryptoStream.FlushFinalBlock();
                    cipherTextBytes = memoryStream.ToArray();
                    cryptoStream.Close();
                }
                memoryStream.Close();
            }
            return Convert.ToBase64String(cipherTextBytes);
        }

        public static string Decrypt(string encryptedText)
        {
            byte[] cipherTextBytes = Convert.FromBase64String(encryptedText);
            byte[] keyBytes = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(256 / 8);
            var symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC, Padding = PaddingMode.None };

            var decryptor = symmetricKey.CreateDecryptor(keyBytes, Encoding.ASCII.GetBytes(VIKey));
            var memoryStream = new MemoryStream(cipherTextBytes);
            var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            byte[] plainTextBytes = new byte[cipherTextBytes.Length];

            int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            memoryStream.Close();
            cryptoStream.Close();
            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount).TrimEnd("\0".ToCharArray());
        }

        static public int party_size = 10;
        static public int min_unlock_size = 7;
        static public List<Point> Points;
        static public double[,] memory = new double[300,300]; //default value - 0
        static public bool[,] filled = new bool[300, 300]; // default value - false

        static double y(int index)
        {
            return Points[index].y;
        }

        static double x(int index)
        {
            return Points[index].x;
        }

        static double findCoeff(int down, int up)
        {
            if (down == up)
                return y(down);

            if (filled[down,up])
            {
                return memory[down,up];
            }

            if (up - down == 1)
            {
                memory[down,up] = (Points[up].y - Points[down].y) / (Points[up].x - Points[down].x);
                return memory[down,up];
            }

            return (findCoeff(down, up - 1) - findCoeff(down + 1, up)) / (x(down) - x(up));
        }
        static double function(double s)
        {
            double sum = 0;
            for (int i = 0; i < Points.Count; i++)
            {
                sum += precision(i, s);
            }

            return sum;
        }

        static double precision(int prec, double s)
        {
            if (prec == 0)
            {
                return y(0);
            }
            double result = findCoeff(0, prec);
            for (int i = 0; i < prec; i++)
            {
                result *= s - x(i);
            }
            return result;
        }

        static List<Point> GenerateRandomPoints(Random rnd, int number)
        {
            List<Point> points = new List<Point>();
            for(int i = 0; i < number; i++)
            {
                var t = new Point(rnd.Next(1, 1000), rnd.Next(-500, 500));
                /// za test
                //t.y = Math.Sqrt(2) * t.x;
                //t.y = 5 * t.x + 2;
                points.Add(t);
            }
            return points;
        }

        static void SendKeys(Random rnd)
        {
            string[] lines = System.IO.File.ReadAllLines(@"..\..\emails.txt");
            int k = 0;

            string passwords = "";

            for(int i = 0; i < lines.Length; i += 2)
            {
                try
                {
                    string jsonString;
                    // jsonString = JsonSerializer.Serialize(Points[i]);

                    bool successfull_parse = int.TryParse(lines[i + 1], out int priority);

                    List<Point> userPoints = new List<Point>();
                    userPoints.Add(Points[k++]);

                    for (int j = 0; j < priority - 1; j++)
                    {
                        userPoints.Add(GenerateNewPoint(rnd));
                    }

                    //string json = JsonConvert.SerializeObject(Points[k++]);
                    string json = JsonConvert.SerializeObject(userPoints);

                    string encryptedPoints = Encrypt(json);
                    string nazad = Decrypt(encryptedPoints); //provera dekripcije

                    MailAddress myemail = new MailAddress("secretsanta.rtc@gmail.com", "SECRET KEY MASTER");
                    //MailAddress mail_to = new MailAddress("aleksa@lukac.rs", "Receiver");
                    MailAddress mail_to = new MailAddress(lines[i], "Key holder " + (i+1));

                    string password = "Sant@123";

                    SmtpClient client_smtp = new SmtpClient("smtp.gmail.com", 587);
                    client_smtp.EnableSsl = true;
                    client_smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client_smtp.UseDefaultCredentials = false;
                    client_smtp.Credentials = new System.Net.NetworkCredential(myemail.Address, password);

                    MailMessage message = new MailMessage(myemail, mail_to);
                    message.Subject = "Your key";
                    message.Body = "Your key is " + encryptedPoints + "\n";


                    Console.WriteLine(encryptedPoints);
                    passwords += encryptedPoints + System.Environment.NewLine;

                    //OVU LINIJU ODKOMENTARISATI ZA DEMO SA MEJLOM
                    client_smtp.Send(message);
                    Console.WriteLine("Successfully sent email");


                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

            }
            System.IO.File.WriteAllText(@"..\..\password.txt", Encrypt(function(0).ToString()));
            System.IO.File.WriteAllText(@"..\..\keys.txt", passwords);

        }

        static Point GenerateNewPoint(Random rnd)
        {
            int r = rnd.Next(2, 10);
            double x = rnd.Next(0, 1000);
            x += (double)r / 10;
            Point p = new Point(x, function(x));
            if (!Points.Contains(p))
                return p;

            return GenerateNewPoint(rnd);
        }

        static void Main(string[] args)
        {
            Random rnd = new Random();
            string[] lines = System.IO.File.ReadAllLines(@"..\..\config.txt");

            bool successfullParse = int.TryParse(lines[1], out int actionCode);

            if(actionCode == 1)
            {
                successfullParse = int.TryParse(lines[2], out party_size);
                successfullParse = int.TryParse(lines[2], out min_unlock_size);

                ///Test rada njutnovog interpolacionog polinoma (treba da vrati -sqrt(2)):
                //Points = GenerateRandomPoints(rnd, 5);
                //Console.WriteLine(function(-1));

                Points = GenerateRandomPoints(rnd, min_unlock_size);

                //provera
                /*
                for(int i = 0; i < party_size; i++)
                {
                    if (i < Points.Count)
                        Console.WriteLine("( " + Points[i].x + " , " + Points[i].y + " ) Provera: " + function(Points[i].x));
                    else
                    {
                        double x = rnd.Next(0, 1000) + 0.5;
                        Console.WriteLine("( " + x + " , " + function(x) + " )");
                        Points.Add(new Point(x, function(x)));
                    }
                }*/

                Console.WriteLine();
                SendKeys(rnd);
            }
            else if(actionCode == 2)
            {
                string[] passwords = System.IO.File.ReadAllLines(@"..\..\keys.txt");
                Points = new List<Point>();
                foreach(var line in passwords)
                {
                    var fetch = JsonConvert.DeserializeObject<Point[]>(Decrypt(line));
                    foreach(var pnt in fetch)
                    {
                        Points.Add(pnt);
                    }
                    //var fileList = fetch.First(); // here we have a single FileList object
                }

                Console.WriteLine(Encrypt(function(0).ToString()));
            }
        }
    }
}
