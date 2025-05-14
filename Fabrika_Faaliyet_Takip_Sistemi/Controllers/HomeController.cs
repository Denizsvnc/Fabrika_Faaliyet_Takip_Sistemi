using Microsoft.AspNetCore.Mvc;
using Fabrika_Faaliyet_Takip_Sistemi.Models;


namespace Fabrika_Faaliyet_Takip_Sistemi.Controllers
{
    public class HomeController : Controller
    {
        // loglar i�in liste olu�turuyoruz
        private static List<LogEntry> loglar = new List<LogEntry>();
        // aktif islem i�in de�i�ken
        private static LogEntry aktifLog = null;
        // Index sayfas�na gitti�inde t�m loglar� g�ster
        private static int kayitSayaci = 1;
        public IActionResult Index()
        {
            OtomatikKontrol();
            return View(loglar);

        }
        [HttpPost]
        private void OtomatikKontrol()
        {

            var now = DateTime.Now;
            int currentHour = now.Hour;
            int currentMinute = now.Minute;

            // aktif i�lemi mola saatinde durdur
            if (aktifLog != null)
            {
                if ((currentHour == 10 && currentMinute == 0) ||
                    (currentHour == 12 && currentMinute == 0) ||
                    (currentHour == 15 && currentMinute == 0) ||
                    (currentHour == 0 && currentMinute == 30))
                {
                    aktifLog.DurusZamani = DateTime.Now;

                    if (currentHour == 10)
                        aktifLog.DurusNedeni = "�ay Molas�";
                    else if (currentHour == 12)
                        aktifLog.DurusNedeni = "Yemek Molas�";
                    else if (currentHour == 15)
                        aktifLog.DurusNedeni = "2. �ay Molas�";
                    else if (currentHour == 0 && currentMinute == 30)
                        aktifLog.DurusNedeni = "Gece �ay� Molas�";

                    aktifLog.KayitNo = kayitSayaci++;
                    loglar.Add(aktifLog);

                    aktifLog = null; // aktif logu bo�alt�yoruzki bir sonraki i�leme haz�r hale gelsiin
                }
            }
            // E�er sistem duruyorsa ve mola s�resi bittiyse otomatik ba�lat
            else if (loglar.Count > 0)
            {
                var sonLog = loglar.LastOrDefault();
                if (sonLog != null && sonLog.DurusZamani.HasValue && !string.IsNullOrEmpty(sonLog.DurusNedeni))
                {
                    var gecenSure = DateTime.Now - sonLog.DurusZamani.Value;

                    if ((sonLog.DurusNedeni == "�ay Molas�" && gecenSure.TotalMinutes >= 15) ||
                        (sonLog.DurusNedeni == "2. �ay Molas�" && gecenSure.TotalMinutes >= 15) ||
                        (sonLog.DurusNedeni == "Yemek Molas�" && gecenSure.TotalMinutes >= 30) ||
                        (sonLog.DurusNedeni == "Gece �ay� Molas�" && gecenSure.TotalMinutes >= 1))
                    {
                        aktifLog = new LogEntry()
                        {
                            BaslangicZamani = DateTime.Now
                        };
                    }
                }
            }
        }

        public IActionResult Basla()
        {
            if (aktifLog == null)
            {
                aktifLog = new LogEntry()
                {
                    BaslangicZamani = DateTime.Now
                };

            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult Dur(string durusNedeni)
        {
            if (aktifLog != null)
            {
                aktifLog.DurusZamani = DateTime.Now;
                var cay = new[] { 10 };
                var yemek = new[] { 12 };
                var cay2 = new[] { 15 };

                if (cay.Contains(aktifLog.DurusZamani.Value.Hour))
                {
                    aktifLog.DurusNedeni = "�ay Molas�";
                }
                else if (yemek.Contains(aktifLog.DurusZamani.Value.Hour))
                {
                    aktifLog.DurusNedeni = "Yemek Molas�";
                }
                else if (cay2.Contains(aktifLog.DurusZamani.Value.Hour))
                {
                    aktifLog.DurusNedeni = "2. �ay Molas�";
                }
                else
                {
                    aktifLog.DurusNedeni = string.IsNullOrWhiteSpace(durusNedeni) ? "" : durusNedeni;
                }


                aktifLog.KayitNo = kayitSayaci++;
                loglar.Add(aktifLog);

                aktifLog = null;

                Basla();
            }
            return RedirectToAction("Index");
        }
    }
}
