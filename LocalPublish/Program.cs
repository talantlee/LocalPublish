using DataAccessLayers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LocalPublish
{
    static class Program
    {
        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            SqlHelper.DbConnectionStrings = new Dictionary<string, string>();
            //    SqlHelper.DbConnectionStrings.Add("sysdata", "Data Source=192.168.88.53;Initial Catalog=SysData;user id=mis002;password=dongguan;Connect Timeout=90;");
            for(int i=0;i< System.Configuration.ConfigurationManager.ConnectionStrings.Count; i++)
            {
                SqlHelper.DbConnectionStrings.Add(System.Configuration.ConfigurationManager.ConnectionStrings[i].Name, DecryptString(System.Configuration.ConfigurationManager.ConnectionStrings[i].ConnectionString));
            }

           // AppConfig.BinPath = System.Configuration.ConfigurationManager.AppSettings.Get("BinPath");
            AppConfig.HostServer = System.Configuration.ConfigurationManager.AppSettings.Get("HostServer");
            AppConfig.ServiceId = System.Configuration.ConfigurationManager.AppSettings.Get("ServiceId");
            AppConfig.ClusterId = System.Configuration.ConfigurationManager.AppSettings.Get("ClusterId");
            // SqlHelper.DbConnectionStrings.Add("default", "Data Source=192.168.88.53;Initial Catalog=SysData;user id=mis002;password=dongguan;Connect Timeout=90;");
            Application.Run(new Form1());
        }
        internal static string DecryptString(string enpstr)
        {
            try
            {

                byte[] enpstrt = Convert.FromBase64String(enpstr);

                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048))
                {
                    try
                    {
                        rsa.FromXmlString("<RSAKeyValue><Modulus>wFlLk1ySTPKFqIASpaHcBsuq4vnkRgOFfpISCJF8lsKRSVvae/KwwSJz5jpMRUdMusR1DmEsRB5Eoer4YXr1IYhfjixMNtyi2c3IFU6jb+5+XgWp2wFMnu8J5r5sFgiDOc6JEO80dKOgpwP9KyJ11fd9STMIJei68ggxZvl6/MPtEDZkpRuyIxQUVcPQuEaOlDU2kycKvQ+dEg/H6/LLR4HPmnOU47M8XOe8PznK/OO7BvPbQvq33OgZQMsGSS0L7DwfkSEFaJcP72BWMvhHxqz/RepRqwAbt6xhKdaF76tkdKUPffhKNWTv+SKf91kuShZg7msJmPm6rM/x0R6roQ==</Modulus><Exponent>AQAB</Exponent><P>wuzDxAqKkQA5FqsZAeTRCcdmiRln9ocyOHD2OpI5g4Ov4rjNAAKpq2NFjivO5u+S+NLdoCcggLzBRG9vwZDRoj5jaT5c7K1ZTToLK512h6fqBNYEILywWYrM/In4zrQcwPa1BLhrsWgyCiO5Ehz1FxvDHm6yOhw27NjZvYw7R98=</P><Q>/J3mtLiqDR2bGjBBlkQkE+tKe/SxPgiOO326MGn/uginEKwtoT/cZajDjKF3bvMI00UBs6d34k7gHZc6dS3REyn3ii+V0T/iThRppHCSUmbXnoUUn1U4S9OKly/TNJk9yaC+bVKHpzde2Q6RKlwhxdDOU4ItdgI8nih37UvafH8=</Q><DP>RG0U89a3edtYLwr1BmMqtoIXDn2qrzIagRG2fi0in3Y04iP5Ys2MfiulGRlr0km7kDmFr1jM4iRgvANaedq6nHfGMgXAXR0lQiTkEvV9zHU3g2cVn+BdD7HX77cHbphvl5WDShrrQyXOmxybNS6RIwGN5zr4ucl5xn8BQLbfgIU=</DP><DQ>FU7cXvIUpfaRVqwrXBlzUZrPNZV2Pd1K4gnA4sSOJYsFTIq5bpiMZqbNl9cm09z8KZXwhs7hdd1hWh2feLLcp9kqYCWTZYsV87AVGSHFtd2m4mAnVSlDDmJRll8aWZ1zcMZ2SZJkjt5EydpItGIBF0Z9oN2iKAoYQt72/rUfQNE=</DQ><InverseQ>sXS5pBaoah8myp7f81ZF4DLRFoDvbVgpUJJpfXF8sTL+DuM+43/UvhaHNMdvpPGS3kbpwsuQv6JxXONVyNHv3AL/vmfwu58qseI2sXuUN282VYEiC7KXWPFrs7fOEzkFCKG+7yJpvmeuAjGp7KRpLGCgYtl+EvvuMC09sV2mxWs=</InverseQ><D>WHx7xQXwE8/55uNMMMA+XS3ypko5VnAmSGzgOOrc+un1Nl84Ko8h+ydjVAV7st8zEDRyeTQAzjFyd16F9yo+fLek5d3BDfgAPtxo8Exl6Yc4wP4v/p0hZ3gmAO2XAafW3pACBDVW11WzAUJXhDFtLgqVq0f7tpzS66cTOwEIMCEQGgCdeiX40tVqS5T+PQhFOqm6sjAEWIGzNh+XvHyLFBCKj1reEUabR6+za52JQJbY90llawgVZtT2U5pmf6BWXoMG8ARJRBl1lNXktzciLVrrxSG1rkzZ2AyoGaPsJsm1AAnkGdarYLbVTeCcHKhbj/8ohqFFf5UddvI3qjcLVQ==</D></RSAKeyValue>");
                        byte[] ss1 = rsa.Decrypt(enpstrt, false);
                        return System.Text.Encoding.UTF8.GetString(ss1);
                    }
                    catch (Exception)
                    {
                        //fail();
                        return enpstr;
                    }
                }

            }
            catch (Exception)
            {
                return enpstr;
            }


        }
    }
}
