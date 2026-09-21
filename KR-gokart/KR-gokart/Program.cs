using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace KR_gokart
{
    //  Egy versenyzőt leíró osztály
    class Versenyzo
    {
        public string Vezeteknev { get; set; }
        public string Keresztnev { get; set; }
        public DateTime SzuletesiIdo { get; set; }

        // Kiszámolja, hogy a versenyző elmúlt-e 18 éves az aktuális dátumhoz képest
        public bool ElmultTizennyolc
        {
            get
            {
                DateTime ma = DateTime.Now.Date;
                int kor = ma.Year - SzuletesiIdo.Year;
                if (ma < SzuletesiIdo.Date.AddYears(kor))
                    kor--;
                return kor >= 18;
            }
        }

        // Versenyző-azonosító: GO- | Teljes név ékezet nélkül | - | Születési dátum (yyyyMMdd)
        public string Azonosito
        {
            get
            {
                string teljesNev = Program.EkezetNelkul(Vezeteknev) + Program.EkezetNelkul(Keresztnev);
                return $"GO-{teljesNev}-{SzuletesiIdo:yyyyMMdd}";
            }
        }

        // Email cím: vezeteknev.keresztnev@gmail.com (kisbetűvel, ékezet nélkül)
        public string Email
        {
            get
            {
                string v = Program.EkezetNelkul(Vezeteknev).ToLower(new CultureInfo("hu-HU"));
                string k = Program.EkezetNelkul(Keresztnev).ToLower(new CultureInfo("hu-HU"));
                return $"{v}.{k}@gmail.com";
            }
        }

        public override string ToString()
        {
            return $"{Vezeteknev} {Keresztnev} | Szül.: {SzuletesiIdo:yyyy.MM.dd.} | " +
                   $"Elmúlt 18 éves: {ElmultTizennyolc} | Azonosító: {Azonosito} | Email: {Email}";
        }
    }

    // ------------------------------------------------------------
    //  Egy időpontfoglalást (1 versenyző, 1 óra, 1 nap) leíró osztály
    // ------------------------------------------------------------
    class Foglalas
    {
        public string VersenyzoAzonosito { get; set; }
        public DateTime Datum { get; set; }   // csak a dátum rész számít
        public int OraKezdet { get; set; }    // pl. 15 -> 15:00-16:00-ig tartó sáv

        public override string ToString()
        {
            return $"{Datum:yyyy.MM.dd.} {OraKezdet:00}:00-{OraKezdet + 1:00}:00 | Versenyző: {VersenyzoAzonosito}";
        }
    }

    // ------------------------------------------------------------
    //  Fő program
    // ------------------------------------------------------------
    internal class Program
    {
        // ---- Helyszín adatai ----
        const string HelyszinNev = "KR Gokartpálya";
        const string HelyszinCim = "6767 Narnia, Csaba út 67.";
        const string HelyszinTelefon = "+36-30-676-1367";
        const string HelyszinWeboldal = "kr-gokart.hu";

        // ---- Projekt adatai ----
        const string ProjektNev = "Gokart időpontfoglaló - Egyéni kisprojekt";
        const string KeszitoMonogram = "KR";
        const string KezdetiDatum = "2026.09.07.";

        // ---- Pályabérlés szabályai ----
        const int NyitasOra = 8;
        const int ZarasOra = 19;
        const int MinFoPalyan = 8;
        const int MaxFoPalyan = 20;
        const int MinOraFoglalas = 1;
        const int MaxOraFoglalas = 2;

        static List<Versenyzo> versenyzok = new List<Versenyzo>();
        static List<Foglalas> foglalasok = new List<Foglalas>();
        static Random rnd = new Random();

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            //KR
            //2026.09.07
            //Gokart időpontfoglaló - Egyéni kisprojekt
            Console.WriteLine("============================================================");
            Console.WriteLine($" Készítő monogramja : {KeszitoMonogram}");
            Console.WriteLine($" Kezdeti dátum       : {KezdetiDatum}");
            Console.WriteLine($" Projekt neve        : {ProjektNev}");
            Console.WriteLine("============================================================");
            Console.WriteLine($" Helyszín: {HelyszinNev}");
            Console.WriteLine($" Cím     : {HelyszinCim}");
            Console.WriteLine($" Telefon : {HelyszinTelefon}");
            Console.WriteLine($" Weboldal: {HelyszinWeboldal}");
            Console.WriteLine("============================================================\n");

            VersenyzokGeneralasa();
            AlapFoglalasokGeneralasa();   // 3-4 alapból foglalt időpont

            bool kilep = false;
            while (!kilep)
            {
                Console.WriteLine("\n--------------------- FŐMENÜ ---------------------");
                Console.WriteLine("1 - Versenyzők listázása");
                Console.WriteLine("2 - Szabad időpontok megjelenítése (hónap végéig)");
                Console.WriteLine("3 - Foglalás manuális be-/átállítása versenyző-azonosító alapján");
                Console.WriteLine("4 - Kilépés");
                Console.Write("Választás: ");
                string valasztas = Console.ReadLine();

                switch (valasztas)
                {
                    case "1":
                        VersenyzokListazasa();
                        break;
                    case "2":
                        IdoszalagMegjelenites();
                        break;
                    case "3":
                        ManualisFoglalasBeallitas();
                        break;
                    case "4":
                        kilep = true;
                        break;
                    default:
                        Console.WriteLine("Érvénytelen választás, próbáld újra!");
                        break;
                }
            }

            Console.WriteLine("\nViszlát a KR Gokartpályán!");
        }

        // ------------------------------------------------------------
        //  Ékezetek eltávolítása (azonosító és email képzéséhez)
        // ------------------------------------------------------------
        public static string EkezetNelkul(string szoveg)
        {
            if (string.IsNullOrEmpty(szoveg)) return szoveg;

            var csereTabla = new Dictionary<char, char>
            {
                {'á','a'}, {'é','e'}, {'í','i'}, {'ó','o'}, {'ö','o'}, {'ő','o'}, {'ú','u'}, {'ü','u'}, {'ű','u'},
                {'Á','A'}, {'É','E'}, {'Í','I'}, {'Ó','O'}, {'Ö','O'}, {'Ő','O'}, {'Ú','U'}, {'Ü','U'}, {'Ű','U'}
            };

            var sb = new StringBuilder(); //új string-objektum
            foreach (char c in szoveg)
            {
                if (csereTabla.ContainsKey(c)) //ékezetek szűrése
                    sb.Append(csereTabla[c]);
                else if (!char.IsWhiteSpace(c)) // szóközök eltávolítása
                    sb.Append(c);
            }
            return sb.ToString();
        }

        // ------------------------------------------------------------
        //  Névlisták betöltése fájlból (soronként egy név)
        // ------------------------------------------------------------
        static List<string> NevlistaBetoltese(string fajlNev)
        {
            var alapertelmezett = new List<string> { "Kovács", "Nagy", "Tóth", "Szabó", "Horváth" };

            try
            {
                if (File.Exists(fajlNev))
                {
                    var sorok = File.ReadAllLines(fajlNev, Encoding.UTF8)
                                     .Select(s => s.Trim()) // szóközök eltávolítása
                                     .Where(s => !string.IsNullOrWhiteSpace(s)) // üres sorok kiszűrése
                                     .ToList();
                    if (sorok.Count > 0)
                        return sorok;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hiba a(z) {fajlNev} fájl beolvasásakor: {ex.Message}");
            }

            Console.WriteLine($"Figyelem: a(z) '{fajlNev}' fájl nem található/üres, alapértelmezett névlista kerül felhasználásra.");
            return alapertelmezett;
        }

        // ------------------------------------------------------------
        //  Véletlenszámú (1-150 közötti darabszámú) versenyző generálása
        // ------------------------------------------------------------
        static void VersenyzokGeneralasa()
        {
            List<string> vezeteknevek = NevlistaBetoltese("vezeteknevek.txt");
            List<string> keresztnevek = NevlistaBetoltese("keresztnevek.txt");

            int darabszam = rnd.Next(1, 151); // 1-150 (mindkét oldalon zárt)

            for (int i = 0; i < darabszam; i++)
            {
                string vezeteknev = vezeteknevek[rnd.Next(vezeteknevek.Count)];
                string keresztnev = keresztnevek[rnd.Next(keresztnevek.Count)];

                int ev = rnd.Next(1945, DateTime.Now.Year); // kb. 5-80 év közötti életkor
                int honap = rnd.Next(1, 13);
                int nap = rnd.Next(1, 29); // biztonságosan érvényes nap minden hónapban

                var versenyzo = new Versenyzo
                {
                    Vezeteknev = vezeteknev,
                    Keresztnev = keresztnev,
                    SzuletesiIdo = new DateTime(ev, honap, nap)
                };

                versenyzok.Add(versenyzo);
            }

            Console.WriteLine($"{darabszam} db versenyző generálva.");
        }

        // ------------------------------------------------------------
        //  3-4 előre foglalt időpont generálása (a mai naptól hónap végéig)
        //  Minden versenyző csak egy sávban szerepel, így a manuális
        //  átállítás (ami a korábbi foglalást törli) konzisztens marad.
        // ------------------------------------------------------------
        static void AlapFoglalasokGeneralasa()
        {
            DateTime ma = DateTime.Today;
            int napokSzama = DateTime.DaysInMonth(ma.Year, ma.Month) - ma.Day + 1;

            int savDarab = rnd.Next(3, 7); // 3 vagy 4 foglalt sáv
            var hasznaltSavok = new HashSet<(DateTime, int)>();
            var kevertVersenyzok = versenyzok.OrderBy(v => rnd.Next()).ToList();
            int index = 0;

            for (int i = 0; i < savDarab && index < kevertVersenyzok.Count; i++)
            {
                DateTime nap;
                int ora;

                // Olyan (nap, óra) párt keresünk, ami még nem foglalt
                do
                {
                    nap = ma.AddDays(rnd.Next(napokSzama));
                    ora = rnd.Next(NyitasOra, ZarasOra);
                } while (!hasznaltSavok.Add((nap, ora)));

                // Létszám: MinFoPalyan-MaxFoPalyan között, de legfeljebb ahány versenyző még szabad
                int fo = rnd.Next(MinFoPalyan, MaxFoPalyan + 1);
                fo = Math.Min(fo, kevertVersenyzok.Count - index);

                for (int j = 0; j < fo; j++)
                {
                    foglalasok.Add(new Foglalas
                    {
                        VersenyzoAzonosito = kevertVersenyzok[index++].Azonosito,
                        Datum = nap.Date,
                        OraKezdet = ora
                    });
                }
            }

            Console.WriteLine($"{hasznaltSavok.Count} db alapból foglalt időpont létrehozva.");
        }

        //  Versenyzők listázása

        static void VersenyzokListazasa()
        {
            Console.WriteLine("\n------------------- VERSENYZŐK LISTÁJA -------------------");
            for (int i = 0; i < versenyzok.Count; i++)
            {
                Console.WriteLine($"{i + 1,4}. {versenyzok[i]}");
            }
            Console.WriteLine($"Összesen: {versenyzok.Count} versenyző.");
        }

        // ------------------------------------------------------------
        //  Egy szöveget adott szélességre középre igazít (a táblázat
        //  celláinak/fejlécének egységes megjelenítéséhez)
        // ------------------------------------------------------------
        static string Kozepre(string szoveg, int szelesseg)
        {
            if (szoveg.Length >= szelesseg)
                return szoveg.Substring(0, szelesseg);

            int balPad = (szelesseg - szoveg.Length) / 2;
            int jobbPad = szelesseg - szoveg.Length - balPad;
            return new string(' ', balPad) + szoveg + new string(' ', jobbPad);
        }

        // ------------------------------------------------------------
        //  Szabad/foglalt időpontok megjelenítése táblázatos rácsban:
        //  a sorokban a napok (aktuális naptól hónap végéig),
        //  az oszlopokban az órasávok (pl. 08-09, 09-10, ...) szerepelnek
        // ------------------------------------------------------------
        const int DatumOszlopSzelesseg = 12;
        const int OraOszlopSzelesseg = 7;

        static void IdoszalagMegjelenites()
        {
            DateTime ma = DateTime.Today;
            int honapUtolsoNapja = DateTime.DaysInMonth(ma.Year, ma.Month);
            DateTime honapVege = new DateTime(ma.Year, ma.Month, honapUtolsoNapja);

            Console.WriteLine($"\n---------- SZABAD IDŐPONTOK ({ma:yyyy.MM.dd.} - {honapVege:yyyy.MM.dd.}) ----------\n");

            // ---- Fejléc sor: órasávok ----
            Console.Write(Kozepre("Dátum", DatumOszlopSzelesseg) + "|");
            for (int ora = NyitasOra; ora < ZarasOra; ora++)
            {
                Console.Write(Kozepre($"{ora:00}-{ora + 1:00}", OraOszlopSzelesseg) + "|");
            }
            Console.WriteLine();

            // ---- Elválasztó vonal ----
            int oraOszlopokSzama = ZarasOra - NyitasOra;
            int teljesSzelesseg = DatumOszlopSzelesseg + 1 + oraOszlopokSzama * (OraOszlopSzelesseg + 1);
            Console.WriteLine(new string('-', teljesSzelesseg));

            // ---- Napi sorok ----
            for (DateTime nap = ma; nap <= honapVege; nap = nap.AddDays(1))
            {
                Console.Write(Kozepre($"{nap:yyyy.MM.dd.}", DatumOszlopSzelesseg) + "|");

                for (int ora = NyitasOra; ora < ZarasOra; ora++)
                {
                    int foglaltFo = foglalasok.Count(f => f.Datum.Date == nap.Date && f.OraKezdet == ora);
                    bool szabad = foglaltFo == 0;

                    string cellaSzoveg = szabad ? "OK" : $"{foglaltFo}fő";

                    Console.ForegroundColor = szabad ? ConsoleColor.Green : ConsoleColor.Red;
                    Console.Write(Kozepre(cellaSzoveg, OraOszlopSzelesseg));
                    Console.ResetColor();
                    Console.Write("|");
                }
                Console.WriteLine();
            }

            Console.WriteLine("\nJelmagyarázat: zöld = szabad (OK), piros = foglalt (X fő).");
        }

        // ------------------------------------------------------------
        //  Manuális foglalás be-/átállítása versenyző-azonosító alapján
        // ------------------------------------------------------------
        static void ManualisFoglalasBeallitas()
        {
            VersenyzokListazasa();

            Console.Write("\nAdd meg a versenyző-azonosítót (pl. GO-KovacsDenes-19741204): ");
            string azonosito = Console.ReadLine()?.Trim();

            Versenyzo kivalasztott = versenyzok.FirstOrDefault(v => v.Azonosito.Equals(azonosito, StringComparison.OrdinalIgnoreCase)); // kis- és nagybetűtől független keresés
            if (kivalasztott == null)
            {
                Console.WriteLine("Nincs ilyen azonosítójú versenyző!");
                return;
            }

            Console.WriteLine($"Kiválasztott versenyző: {kivalasztott}");

            DateTime ma = DateTime.Today;
            int honapUtolsoNapja = DateTime.DaysInMonth(ma.Year, ma.Month);
            DateTime honapVege = new DateTime(ma.Year, ma.Month, honapUtolsoNapja);

            Console.Write($"Add meg a dátumot (hh.nn., {ma:MM.dd.} és {honapVege:MM.dd.} között): ");
            string datumSzoveg = Console.ReadLine()?.Trim();
            bool datumOk = DateTime.TryParseExact(datumSzoveg, "MM.dd.",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime datum)
                || DateTime.TryParse(datumSzoveg, out datum);

            if (!datumOk)
            {
                Console.WriteLine("Érvénytelen dátumformátum!");
                return;
            }

            if (datum.Date < ma || datum.Date > honapVege.Date)
            {
                Console.WriteLine("A dátum a jelenlegi hónapon/napon kívül esik!");
                return;
            }

            Console.Write($"Add meg a kezdő órát ({NyitasOra}-{ZarasOra - 1} között, pl. 15): ");
            if (!int.TryParse(Console.ReadLine(), out int kezdoOra) || kezdoOra < NyitasOra || kezdoOra > ZarasOra - 1)
            {
                Console.WriteLine("Érvénytelen kezdő óra!");
                return;
            }

            Console.Write($"Hány órára foglalsz ({MinOraFoglalas}-{MaxOraFoglalas})? ");
            if (!int.TryParse(Console.ReadLine(), out int oraszam) || oraszam < MinOraFoglalas || oraszam > MaxOraFoglalas)
            {
                Console.WriteLine("Érvénytelen óraszám!");
                return;
            }

            if (kezdoOra + oraszam > ZarasOra)
            {
                Console.WriteLine("A foglalás túllépné a nyitvatartási időt!");
                return;
            }

            // Az összefüggő órasávok (kezdoOra, kezdoOra+1, ... , kezdoOra+oraszam-1)
            var kertOrak = Enumerable.Range(kezdoOra, oraszam).ToList();

            // Kapacitás ellenőrzése (max. 20 fő / órasáv), a saját korábbi foglalásait nem számítva
            foreach (int ora in kertOrak)
            {
                int foglaltFo = foglalasok.Count(f => f.Datum.Date == datum.Date && f.OraKezdet == ora
                                                       && f.VersenyzoAzonosito != kivalasztott.Azonosito);
                if (foglaltFo >= MaxFoPalyan)
                {
                    Console.WriteLine($"A(z) {ora:00}:00-{ora + 1:00}:00 sáv betelt (max. {MaxFoPalyan} fő)!");
                    return;
                }
            }

            // A versenyző esetleges korábbi foglalásainak törlése (átállítás)
            foglalasok.RemoveAll(f => f.VersenyzoAzonosito == kivalasztott.Azonosito);

            // Új foglalás(ok) rögzítése
            foreach (int ora in kertOrak)
            {
                foglalasok.Add(new Foglalas
                {
                    VersenyzoAzonosito = kivalasztott.Azonosito,
                    Datum = datum.Date,
                    OraKezdet = ora
                });
            }

            Console.WriteLine($"\nSikeres foglalás: {kivalasztott.Azonosito} -> {datum:yyyy.MM.dd.} " +
                               $"{kezdoOra:00}:00 - {kezdoOra + oraszam:00}:00");
        }
    }
}