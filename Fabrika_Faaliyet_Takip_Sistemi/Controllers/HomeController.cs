using Microsoft.AspNetCore.Mvc;
using Fabrika_Faaliyet_Takip_Sistemi.Models;


namespace Fabrika_Faaliyet_Takip_Sistemi.Controllers
{
    public class HomeController : Controller
    {
        // loglar için liste oluþturuyoruz
        private static List<LogEntry> loglar = new List<LogEntry>();
        // aktif islem için deðiþken
        private static LogEntry aktifLog = null;
        // Index sayfasýna gittiðinde tüm loglarý göster
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

            // aktif iþlemi mola saatinde durdur
            if (aktifLog != null)
            {
                if ((currentHour == 10 && currentMinute == 0) ||
                    (currentHour == 12 && currentMinute == 0) ||
                    (currentHour == 15 && currentMinute == 0) ||
                    (currentHour == 0 && currentMinute == 30))
                {
                    aktifLog.DurusZamani = DateTime.Now;

                    if (currentHour == 10)
                        aktifLog.DurusNedeni = "Çay Molasý";
                    else if (currentHour == 12)
                        aktifLog.DurusNedeni = "Yemek Molasý";
                    else if (currentHour == 15)
                        aktifLog.DurusNedeni = "2. Çay Molasý";
                    else if (currentHour == 0 && currentMinute == 30)
                        aktifLog.DurusNedeni = "Gece Çayý Molasý";

                    aktifLog.KayitNo = kayitSayaci++;
                    loglar.Add(aktifLog);

                    aktifLog = null; // aktif logu boþaltýyoruzki bir sonraki iþleme hazýr hale gelsiin
                }
            }
            // Eðer sistem duruyorsa ve mola süresi bittiyse otomatik baþlat
            else if (loglar.Count > 0)
            {
                var sonLog = loglar.LastOrDefault();
                if (sonLog != null && sonLog.DurusZamani.HasValue && !string.IsNullOrEmpty(sonLog.DurusNedeni))
                {
                    var gecenSure = DateTime.Now - sonLog.DurusZamani.Value;

                    if ((sonLog.DurusNedeni == "Çay Molasý" && gecenSure.TotalMinutes >= 15) ||
                        (sonLog.DurusNedeni == "2. Çay Molasý" && gecenSure.TotalMinutes >= 15) ||
                        (sonLog.DurusNedeni == "Yemek Molasý" && gecenSure.TotalMinutes >= 30) ||
                        (sonLog.DurusNedeni == "Gece Çayý Molasý" && gecenSure.TotalMinutes >= 1))
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
                    aktifLog.DurusNedeni = "Çay Molasý";
                }
                else if (yemek.Contains(aktifLog.DurusZamani.Value.Hour))
                {
                    aktifLog.DurusNedeni = "Yemek Molasý";
                }
                else if (cay2.Contains(aktifLog.DurusZamani.Value.Hour))
                {
                    aktifLog.DurusNedeni = "2. Çay Molasý";
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
