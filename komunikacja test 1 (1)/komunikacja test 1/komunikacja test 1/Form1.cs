using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;

namespace komunikacja_test_1
{
    public partial class Form1 : Form
    {
        List<Przystanek> przystanki = new List<Przystanek>();   //lista przystankow
        List<Linia> linie = new List<Linia>();                  //lista linii
        List<Wynik> droga = new List<Wynik>();                  //obecna droga którą idzie algorytm
        List<List<Wynik>> wyniki = new List<List<Wynik>>();     //nieobrobione wyniki
        List<Rozwiazanie> rozwiazania = new List<Rozwiazanie>();    
        public Form1()
        {
            InitializeComponent();
            parse();
        }
        private void parse() 
        {
            DirectoryInfo di = new DirectoryInfo(@"E:\Moje bzdury\_STUDIA\8 Semestr\psi 3\program\rozklady_xml\zdik");

            foreach (FileInfo fi in di.GetFiles())
            {
                if (fi.Name != "01282.xml") continue;
                if (fi.Name == "0119.xml") continue;
                parseFile2(fi.FullName);
            }

            usunWielokrotnePrzystanki();

            foreach (Przystanek p in przystanki)
            {
                comboBox1.Items.Add(p.nazwa);
                comboBox2.Items.Add(p.nazwa);
            }
            comboBox1.Sorted = true;
            comboBox2.Sorted = true;
        }
        private void usunWielokrotnePrzystanki()
        {
            List<Przystanek> przystankiPom = new List<Przystanek>();
            foreach (Przystanek p in przystanki) 
            {
                Przystanek przystanekWPom = przystankiPom.Find(
                        delegate(Przystanek p2) 
                        {
                            return p2.nazwa == p.nazwa;
                        });

                if (przystanekWPom == null)
                {
                    przystankiPom.Add(p);
                }
                else
                {
                    foreach (Linia l in p.linie) 
                    {
                        przystanekWPom.dodajLinie(l);
                    }
                }   

            }
            przystanki = przystankiPom;        
        }
        private void search(string _from, string to, int nodes, int initialNodes, string previousLine, DateTime godzina) 
        {
            if (_from == to)//trafilem
            {
                List<Wynik> pom = new List<Wynik>();
                foreach (Wynik wyn in droga)
                {
                    pom.Add(new Wynik(wyn.linia, wyn.czas,wyn.czekaj));
                }
                wyniki.Add(pom);

                droga.RemoveAt(droga.Count - 1);
                return;
            }
            else//nie trafilem
            {
                //wsiadz i jedz jeden przystanek kazda linia i sprawdzaj dalej
                foreach (Linia l in przystanki.Find(
                        delegate(Przystanek p)
                        {
                            return p.nazwa == _from;
                        }
                        ).linie
                    )
                {
                    DateTime wyjazdLinii = new DateTime(100,11,11,0,0,0);

                    //sprawdz, czy wybrana linia ma taka godzine wyjazdu, zeby dojechac na obecny przystanek po obecnej godzinie
                    //i wybierz pierwsza taka godizne wyjazdu
                    //wybierajac sposrod wyjazdow dla odpowiedniego dnia (robocze/sobota/niedziela)
                    
                                            
                    //dojscie tu oznacza ze sprawdzamy czas dla dobrego dnia
                    List<DateTime> list = wyjazdyLiniiWDzien(l,godzina);
                    if (list == null) continue;
                    foreach (DateTime d in list)
                    {
                        if (godzina > d.Add(new TimeSpan(0, l.czasDo(_from), 0)))
                        {
                            continue;
                        }
                        else
                        {
                            wyjazdLinii = d;
                            break;
                        }
                    }
                    
                   
                    // jesli nie ma juz godziny odjazdu ktora by spelniala powyzsze wymaganie, pomin linie;
                    if (wyjazdLinii.Year == 100) continue;

                    //jesli juz nie ma przesiadek i wybrano inna linie niz poprzednia - pomin te linie
                    if (nodes == 0 && l.numerLinii != previousLine && previousLine != "z buta") continue;

                    //dowiedz sie jaki jest nasteepny przystanek wybranej linii
                    NastPrzyst nastPrzyst = jedzLinia(l, _from);

                    //jesli nie ma dalszych przystankow jakiejs linii pomin ja
                    if (nastPrzyst == null) continue;

                    //jesli ta linia juz odjechala - pomin ja
                    DateTime odjazdLiniiZPrzystanku = wyjazdLinii.AddMinutes(l.czasDo(_from));
                    if (godzina > odjazdLiniiZPrzystanku) continue;

                    //oblicz za ile odjezdza ta linia z tego przystanku
                    TimeSpan zaIleOdjezdza = odjazdLiniiZPrzystanku - godzina;
                    
                    //dodaj info do dorogi algorytmu jaka linia, na jaki przystanek i ile musiales czekac na autobus
                    droga.Add(new Wynik(l.numerLinii, nastPrzyst.czas,(int)zaIleOdjezdza.TotalMinutes));

                    //oblicz ktora bedzie godzina jak dojedzie na natepny przystanek (czekanie na autobus + jazda)
                    int h, m;
                    h = godzina.Hour + zaIleOdjezdza.Hours;
                    m = godzina.Minute + zaIleOdjezdza.Minutes + l.czasZDo(_from, nastPrzyst.nastPrzyst);
                     

                    //rekurencja - podzial na kontunuacje linii i zmiane linii            
                    if ((previousLine == l.numerLinii) || previousLine == "z buta")
                    {
                        search(nastPrzyst.nastPrzyst, to, nodes, initialNodes, l.numerLinii, new DateTime(godzina.Year, godzina.Month, godzina.Day, h + m / 60, m % 60, 0));
                    }
                    else 
                    {
                        search(nastPrzyst.nastPrzyst, to, nodes - 1, initialNodes, l.numerLinii, new DateTime(godzina.Year, godzina.Month, godzina.Day, h + m / 60, m % 60, 0));
                    }
                    
                }
            }
            if(droga.Count > 0)
            droga.RemoveAt(droga.Count - 1);
        }
        private List<DateTime> wyjazdyLiniiWDzien(Linia l, DateTime g)
        {
            string dzien="";

            if (g.DayOfWeek == DayOfWeek.Saturday)
            {
                dzien = "Sobota";
            }
            if (g.DayOfWeek == DayOfWeek.Sunday)
            {
                dzien = "Niedziela";
            }
            if (g.DayOfWeek != DayOfWeek.Sunday &&
                g.DayOfWeek != DayOfWeek.Saturday)
            {
                dzien = "w dni robocze";
            }

            foreach (Wyjazd w in l.wyjazd) 
            {
                if (w.typDnia == dzien) return w.wyjazd;
            }
            //nie ma odpowiednich wyjazdow
            return null;
        }
        private int czasJazdyZPetliNaPrzystanek(string _linia, string to) 
        {
            Linia linia = linie.Find(
                delegate(Linia l)
                {
                    return l.numerLinii == _linia;
                }
                );

            for (int i = 0; i < linia.przystanki.Count; i++)
            {
                if (linia.przystanki[i].nazwa == to)
                {
                    return linia.czasJazdy[i];
                }
   
            }
            return -1;
        }
        private NastPrzyst jedzLinia(Linia l, string _from) 
        {
            int count = 0;

            foreach (Przystanek p in l.przystanki) 
            {
                if (p.nazwa == _from)
                {
                    break;
                }
                else
                {
                    count++;
                }
            }
            if (count + 1 >= l.przystanki.Count) return null;

            return new NastPrzyst(l.przystanki[count + 1].nazwa, l.czasJazdy[count + 1] - l.czasJazdy[count]);

        
        }
        private Wynik findPrzystanekFromPrzystanek(string _from, string to) 
        {
            if (_from == to) return null;
            List<Wynik> wyniki = new List<Wynik>();
            foreach (Linia l in (przystanki.Find(
                delegate(Przystanek prz) 
                {
                    return prz.nazwa == _from;
                }
                )).linie)
            {
                Przystanek toZnaleziony = l.przystanki.Find(
                                    delegate(Przystanek prz)
                                    {
                                        return prz.nazwa == to;
                                    }
                    );

                if (toZnaleziony == null) continue;

                int t = l.czasZDo(_from, to);
             
                //TODO
                if (wyniki.Count == 0 || t < wyniki[0].czas)
                    wyniki.Insert(0, new Wynik(l.numerLinii, t,0));
            }

            if (wyniki.Count == 0 ||wyniki[0].czas<0) return null;
            else
                return wyniki[0];
        }
        private void button1_Click(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
           
            search(
                (string)comboBox1.SelectedItem,
                (string)comboBox2.SelectedItem,
                (int)numericUpDown1.Value, 
                (int)numericUpDown1.Value,
                "z buta",
                new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, Int32.Parse(textBox1.Text), Int32.Parse(textBox2.Text), Int32.Parse(textBox3.Text))
            );
            normalizeResults();
            showResults();
            wyniki.Clear();
            droga.Clear();
            rozwiazania.Clear();
            
            

        }
   /*     private void parseFile(string strPath)
        {
            //zle dodaje przystanki - ciagle pierwszy...
            //zle liczy czas jazdy
            //
            XmlTextReader xmlReader = new XmlTextReader(strPath);
            bool pierwszyPrzystanek = true;
            int aktualnaGodzina=0,aktualnaMinuta=0;
            string dzien = "";
            Przystanek aktualnieObrabianyPrzystanek = null;
            string nazwaLinii = "", typLinii = "", idPrzystanku = "", nazwaPrzystanku = "", ulicaPrzystanku = "";
            // Read the line of the xml file
            while (xmlReader.Read())
            {
                switch (xmlReader.NodeType)
                {
                    case XmlNodeType.Element:
                        
                        //zapisz dane obrabianej linii
                        if (xmlReader.Name == "linia")
                        {
                            xmlReader.MoveToAttribute(0);
                            nazwaLinii = xmlReader.Value;

                            xmlReader.MoveToAttribute(1);
                            typLinii = xmlReader.Value;
                            break;

                        }
                        //dodaj wariant linii do bazy
                        if (xmlReader.Name == "wariant")
                        { 
                            linie.Add(new Linia(nazwaLinii,typLinii));
                            pierwszyPrzystanek = true;
                            break;
                        }
                        //utworz aktualnie obrabiany przystanek
                        if (xmlReader.Name == "przystanek") 
                        {
                            xmlReader.MoveToAttribute(0);
                            idPrzystanku = xmlReader.Value;

                            xmlReader.MoveToAttribute(1);
                            nazwaPrzystanku = xmlReader.Value;

                            xmlReader.MoveToAttribute(2);
                            ulicaPrzystanku = xmlReader.Value;

                            //sprawdz czy taki przystanek juz jest
                            aktualnieObrabianyPrzystanek = przystanki.Find(
                                    delegate(Przystanek p)
                                    {
                                        return p.id == idPrzystanku;
                                    }
                                    );

                            //jak go nie ma to go dodaj
                            if (aktualnieObrabianyPrzystanek == null)
                            {
                                aktualnieObrabianyPrzystanek = new Przystanek(idPrzystanku, nazwaPrzystanku, ulicaPrzystanku);
                                przystanki.Add(aktualnieObrabianyPrzystanek);
                            }
                            //dodaj do tego przystanku linie
                            aktualnieObrabianyPrzystanek.dodajLinie(linie[linie.Count-1]);

                            //jesli jest to pierwszy przystanek
                            if (linie[linie.Count - 1].przystanki.Count == 0)
                            {   
                                //dodaj go do linii z czasem 0
                                linie[linie.Count - 1].dodajPrzystanek(aktualnieObrabianyPrzystanek, 0);
                                pierwszyPrzystanek = true;
                            }
                            else 
                            {
                                pierwszyPrzystanek = false;
                            }
                            break;

                        }
                        //zapisz dzien (roboczy/sobota...)
                        if(xmlReader.Name == "dzien")
                        {
                            xmlReader.MoveToAttribute(0);
                            dzien = xmlReader.Value;
                            break;
                        }
                        //zapisz godzine
                        if (xmlReader.Name == "godz")
                        {
                            xmlReader.MoveToAttribute(0);
                            aktualnaGodzina = Int32.Parse( xmlReader.Value);
                            break;
                        }
                        //przerabianie minut;
                        if (xmlReader.Name == "min")
                        {
                            xmlReader.MoveToAttribute(0);
                            aktualnaMinuta = Int32.Parse(xmlReader.Value);

                            //jesli przerabiamy pierwszy przystanek linii to kazda godzina jest godzina odjazdu z petli - trzeba je pododawac do linii
                            if (pierwszyPrzystanek)
                            {
                                //dodaj nowy wyjazd dla linii - typ dnia (roboczy, sobota...) i pierwsza godizne
                                linie[linie.Count - 1].wyjazd.Add(new Wyjazd(dzien, new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, aktualnaGodzina%24, aktualnaMinuta, 0)));
                                
                                //znacznik dzien oznacza, ze juz nie ma wiecej godzin
                                while (xmlReader.Read() && xmlReader.Name != "dzien") 
                                {
                                    if (xmlReader.Name == "godz" && xmlReader.HasAttributes) 
                                    {
                                        xmlReader.MoveToAttribute(0);
                                        aktualnaGodzina = Int32.Parse(xmlReader.Value);
                                    }
                                    if (xmlReader.Name == "min" && xmlReader.HasAttributes)
                                    {
                                        xmlReader.MoveToAttribute(0);
                                        aktualnaMinuta = Int32.Parse(xmlReader.Value);

                                        //do obecnie przerabianego dnia (roboczy, sobota...) dodaj kolejne godziny
                                        linie[linie.Count - 1].wyjazd.Find(
                                            delegate(Wyjazd w)
                                            {
                                                return w.typDnia == dzien;
                                            }
                                            ).wyjazd.Add(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, aktualnaGodzina % 24, aktualnaMinuta, 0));
                                    }
                                }
                            }
                            //jesli jest to kolejny przystanek linii, to czas dojazdu na ten przystanek to pierwszy czas wyjazdu z petli i pierwszy autobus na tym przystanku
                            else 
                            {
                                //znajdzi pierwsza godzine wyjazd z petli
                                DateTime wyjazd = linie[linie.Count - 1].wyjazd.Find(
                                    delegate(Wyjazd w)
                                    {
                                        return w.typDnia == dzien;
                                    }
                                    ).wyjazd[0];

                                //stworz godzine przyjazdu na podstawie odczytanych wczesniej danych
                                DateTime przyjazd = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,aktualnaGodzina%24,aktualnaMinuta,0);

                                TimeSpan czas = przyjazd - wyjazd;

                                //dodaj ten przystanek do linii z obliczonym czasem dojazdu
                                linie[linie.Count - 1].dodajPrzystanek(aktualnieObrabianyPrzystanek, (int)czas.TotalMinutes);
                                
                                //dopoki nie ma znacznika przystanek to sa tam godziny ktore nas juz nie interesuja. ta petla je pomija
                                while (xmlReader.Name != "przystanek") xmlReader.Read();

                                //dopoki nie ma znacznika wariant to sa tam przystanki, dla ktorych nalezy obliczyc czas dojazdu
                                while (xmlReader.Name != "wariant") 
                                {
                                    //tak jak poprzednio stworz dane o przystanku i jesli go nie ma to go dodaj
                                    if (xmlReader.Name == "przystanek" && xmlReader.HasAttributes)
                                    {
                                        xmlReader.MoveToAttribute(0);
                                        idPrzystanku = xmlReader.Value;

                                        xmlReader.MoveToAttribute(1);
                                        nazwaPrzystanku = xmlReader.Value;

                                        xmlReader.MoveToAttribute(2);
                                        ulicaPrzystanku = xmlReader.Value;

                                        aktualnieObrabianyPrzystanek = przystanki.Find(
                                                delegate(Przystanek p)
                                                {
                                                    return p.id == idPrzystanku;
                                                }
                                                );
                                        if (aktualnieObrabianyPrzystanek == null)
                                        {
                                            aktualnieObrabianyPrzystanek = new Przystanek(idPrzystanku, nazwaPrzystanku, ulicaPrzystanku);
                                            przystanki.Add(aktualnieObrabianyPrzystanek);
                                        }
                                        aktualnieObrabianyPrzystanek.dodajLinie(linie[linie.Count - 1]);

                                        //poniewaz ostatni przystanek nie ma godzin odjazdow, to nie ma tam znacznika <godz> ktory jest wylapywany przez ten algorytm
                                        //2 przyeczytania nie szkodza, gdyz albo trafimy na znacnzik tabliczka (nie ostatni przystanek), ktory jest ignorowany przez algorytm
                                        //albo trafimy na znacznik </przystanek> co oznacza, ze jest to ostatni przystanek linii i czas jazdy wynosi tyle samo co na przedostatni
                                        xmlReader.Read();
                                        xmlReader.Read();
                                        if (xmlReader.NodeType == XmlNodeType.EndElement) 
                                        {
                                            //dzien ma 1440 minut, dlatego jesli autobus wyjezdza przed polnoca a dojezdza po, to nalezy poprawic wynik
                                            if (czas.TotalMinutes < 0)
                                            {
                                                linie[linie.Count - 1].dodajPrzystanek(aktualnieObrabianyPrzystanek, (int)czas.TotalMinutes + 1440);
                                            }
                                            else
                                            {
                                                linie[linie.Count - 1].dodajPrzystanek(aktualnieObrabianyPrzystanek, (int)czas.TotalMinutes);
                                            }
                                        
                                        }
                                    }

                                    if (xmlReader.Name == "godz" && xmlReader.HasAttributes)
                                    {
                                        xmlReader.MoveToAttribute(0);
                                        aktualnaGodzina = Int32.Parse( xmlReader.Value);
                                        continue;
                                    }
                                    if (xmlReader.Name == "min" && xmlReader.HasAttributes)
                                    {
                                        xmlReader.MoveToAttribute(0);
                                        aktualnaMinuta = Int32.Parse(xmlReader.Value);
                                        przyjazd = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, aktualnaGodzina % 24, aktualnaMinuta, 0);

                                        czas = przyjazd - wyjazd;
                                        if (czas.TotalMinutes < 0)
                                        {
                                            linie[linie.Count - 1].dodajPrzystanek(aktualnieObrabianyPrzystanek, (int)czas.TotalMinutes + 1440);
                                        }
                                        else
                                        {
                                            linie[linie.Count - 1].dodajPrzystanek(aktualnieObrabianyPrzystanek, (int)czas.TotalMinutes);
                                        }
                                        while (xmlReader.Name != "tabliczka") xmlReader.Read();
                                    }
                                    xmlReader.Read();
                                
                                }
                            }
                        
                        }

                            
                        
                        break;
                    case XmlNodeType.EndElement:
                        
                        break;
                    case XmlNodeType.Text:

                        richTextBox1.Text += xmlReader.Value + "\n";
                        break;
                    default:
                        break;
                }
            }
        }*/
        private void parseFile2(string strPath) 
        {
            List<DateTime> czasy = new List<DateTime>();
            int godzina=0,minuta=0;
            Przystanek aktualnyPrzystanek = null;
            string dzien = "";
            string nazwaLinii = "", typLinii = "", nazwaWariantu="", idPrzystanku = "", nazwaPrzystanku = "", ulicaPrzystanku = "";
            bool pierwszyPrzystanek = true;
            XmlTextReader xmlReader = new XmlTextReader(strPath);
            while(xmlReader.Read())
            {
                switch (xmlReader.NodeType) 
                {
                    case XmlNodeType.Element:
                        {
                            switch (xmlReader.Name) 
                            {
                                case "linia":
                                    {
                                        xmlReader.MoveToAttribute(0);
                                        nazwaLinii = xmlReader.Value;

                                        xmlReader.MoveToAttribute(1);
                                        typLinii = xmlReader.Value;
                                        break;
                                    }
                                case "wariant":
                                    {
                                        xmlReader.MoveToAttribute(1);
                                        nazwaWariantu = xmlReader.Value;

                                        linie.Add(new Linia(nazwaLinii,typLinii,nazwaWariantu));
                                        pierwszyPrzystanek = true;

                                        while (xmlReader.Read() && xmlReader.Name != "wariant") 
                                        {
                                            switch (xmlReader.NodeType) 
                                            {
                                                case XmlNodeType.Element: 
                                                    {
                                                        switch (xmlReader.Name) 
                                                        {
                                                            case "przystanek":
                                                                {
                                                                    xmlReader.MoveToAttribute(0);
                                                                    idPrzystanku = xmlReader.Value;

                                                                    xmlReader.MoveToAttribute(1);
                                                                    nazwaPrzystanku = xmlReader.Value;

                                                                    xmlReader.MoveToAttribute(2);
                                                                    ulicaPrzystanku = xmlReader.Value;

                                                                    //sprawdz czy taki przystanek juz jest
                                                                    aktualnyPrzystanek = przystanki.Find(
                                                                            delegate(Przystanek p)
                                                                            {
                                                                                return p.nazwa == nazwaPrzystanku;
                                                                            }
                                                                            );

                                                                    //jak go nie ma to go dodaj
                                                                    if (aktualnyPrzystanek == null)
                                                                    {
                                                                        aktualnyPrzystanek = new Przystanek(idPrzystanku, nazwaPrzystanku, ulicaPrzystanku);
                                                                        przystanki.Add(aktualnyPrzystanek);
                                                                    }
                                                                    //dodaj do tego przystanku linie
                                                                    aktualnyPrzystanek.dodajLinie(linie[linie.Count - 1]);
                                                                    
                                                                    if (linie[linie.Count - 1].przystanki.Count == 0)
                                                                    {
                                                                        //dodaj go do linii z czasem 0
                                                                        linie[linie.Count - 1].dodajPrzystanek(aktualnyPrzystanek, 0);
                                                                        pierwszyPrzystanek = true;
                                                                    }
                                                                    else
                                                                    {
                                                                        pierwszyPrzystanek = false;
                                                                    }
                                                                    break;
                                                                }
                                                            case "dzien":
                                                                {
                                                                    xmlReader.MoveToAttribute(0);
                                                                    dzien = xmlReader.Value;
                                                                    break;
                                                                }
                                                            case "godz":
                                                                {
                                                                    xmlReader.MoveToAttribute(0);
                                                                    godzina = Int32.Parse(xmlReader.Value);
                                                                    break;
                                                                }
                                                            case "min":
                                                                {
                                                                    xmlReader.MoveToAttribute(0);
                                                                    minuta = Int32.Parse(xmlReader.Value);
                                                                    break;
                                                                }
                                                            default: break;
                                                        }
                                                        break;
                                                    }
                                                case XmlNodeType.EndElement: 
                                                    {
                                                        if (pierwszyPrzystanek)
                                                        {
                                                            if (xmlReader.Name == "min")
                                                            {
                                                                czasy.Add(new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, godzina, minuta, 0));
                                                            }
                                                            if (xmlReader.Name == "dzien")
                                                            {
                                                                linie[linie.Count - 1].wyjazd.Add(new Wyjazd(dzien, czasy));
                                                            }
                                                        }
                                                        else 
                                                        {
                                                            if (xmlReader.Name == "min")
                                                            {
                                                                DateTime wyjazd = linie[linie.Count - 1].wyjazd.Find(
                                                                    delegate(Wyjazd w)
                                                                    {
                                                                        return w.typDnia == dzien;
                                                                    }
                                                                    ).wyjazd[0];

                                                                DateTime przyjazd = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, godzina, minuta, 0);

                                                                TimeSpan czasDojazdu = przyjazd - wyjazd;

                                                                linie[linie.Count - 1].dodajPrzystanek(aktualnyPrzystanek, (int)czasDojazdu.TotalMinutes);

                                                                while (xmlReader.Name != "dzien" && xmlReader.Read());
                                                            }
                                                        }
                                                        break;
                                                    }
                                                default: break;
                                                
                                            }
                                        
                                        }

                                        break;
                                    }
                                //default dla Element
                                default: break;
                            }
                            break;
                        }
                    //default glownego switcha
                    default: break;
                }
            }
        
        }
        private void showResults() 
        {
            foreach (Rozwiazanie r in rozwiazania) 
            {
                richTextBox1.Text += "--== kolejna możliwość ==--\n";

                for (int i = 0; i < r.linie.Count; i++)
                {
                    richTextBox1.Text += "Czekaj "+r.czekaj[i]+ " min na linie " + r.linie[i] + " jedz " +r.liczbaPrzystankow[i] +" przystanek/ów przez " + r.czasWLinii[i] + " min\n";
                }
                richTextBox1.Text += "-----\nliczba przesiadek: " + r.liczbaPrzesiadek + "\n";
                richTextBox1.Text += "czas calkowity: " + r.czasCalkowity;
                richTextBox1.Text += "\n\n";
            }
        
        }
        private void normalizeResults() 
        {
            foreach (List<Wynik> w in wyniki)
            {
                for (int i = 0; i < w.Count-1; i++) 
                {
                    if (w[i].linia == w[i + 1].linia)
                    {
                        w[i].liczbaPrzystankow++;
                        w[i].czas += w[i + 1].czas;
                        w.RemoveAt(i + 1);
                        
                    }
                }
                rozwiazania.Add(new Rozwiazanie(w));
            }
            


        }
        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            comboBox1.Items.Clear();

            foreach (Przystanek p in przystanki)
            {
                if (p.nazwa.ToLower().Contains(textBox4.Text.ToLower())) comboBox1.Items.Add(p.nazwa);
            }
            comboBox1.Sorted = true;
        }
        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            comboBox2.Items.Clear();

            foreach (Przystanek p in przystanki)
            {
                if (p.nazwa.ToLower().Contains(textBox5.Text.ToLower())) comboBox2.Items.Add(p.nazwa);
            }
            comboBox2.Sorted = true;
        }

    }
    
    #region klasy
    public class Rozwiazanie 
    {
        public List<int> czekaj = new List<int>();
        public List<string> linie = new List<string>();
        public List<int> liczbaPrzystankow = new List<int>();
        public List<int> czasWLinii = new List<int>();
        public int liczbaPrzesiadek;
        public int czasCalkowity = 0;
        public Rozwiazanie(List<Wynik> w)
        {
            for (int i = 0; i < w.Count; i++) 
            {
                czekaj.Add(w[i].czekaj);
                linie.Add(w[i].linia);
                liczbaPrzystankow.Add(w[i].liczbaPrzystankow);
                czasWLinii.Add(w[i].czas);
                czasCalkowity += w[i].czas + w[i].czekaj;
            
            }

            liczbaPrzesiadek = this.linie.Count - 1;


        }
    }
    public class NastPrzyst
    {
        public string nastPrzyst;
        public int czas;
        public NastPrzyst(string nazwa, int t) 
        {
            nastPrzyst = nazwa;
            czas = t;
        }
    }
    public class Wynik 
    {
        public int czekaj = 0;
        public string linia;
        public int czas;
        public int liczbaPrzystankow = 1;
        public Wynik(string l, int t, int czekajNaLinie) 
        {
            czekaj = czekajNaLinie;
            linia = l;
            czas = t;
        }
    }
    public class Przystanek 
    {
        public string id;
        public string nazwa;
        public string ulica;
        public List<Linia> linie = new List<Linia>();

        public Przystanek(string _id,string _nazwa,string _ulica) 
        {
            id = _id;
            nazwa = _nazwa;
            ulica = _ulica;
        }
        public void dodajLinie(Linia linia)
        {
            linie.Add(linia);
        }
    
    }
    public class Wyjazd 
    {
        public List<DateTime> wyjazd = new List<DateTime>();
        public string typDnia;
        public Wyjazd(string _typDnia, List<DateTime> w) 
        {
            typDnia = _typDnia;
            foreach (DateTime d in w) 
            {
                wyjazd.Add(new DateTime(d.Year, d.Month, d.Day, d.Hour, d.Minute, 0));
            }
        }
    }
    public class Linia 
    {
        public string numerLinii;
        public string typLinii;
        public string nazwaWariantu;
        public List<Wyjazd> wyjazd = new List<Wyjazd>();
        public List<Przystanek> przystanki = new List<Przystanek>();
        public List<int> czasJazdy = new List<int>();

        public Linia(string _numerLinii,string typ,string wariant) 
        {
            numerLinii = _numerLinii;
            typLinii = typ;
            nazwaWariantu = wariant;
        }
        public void dodajPrzystanek(Przystanek przystanek,int _czasJazdy)
        {
            przystanki.Add(przystanek);
            czasJazdy.Add(_czasJazdy);
        }
        public int czasZDo(string _from,string to)
        {
            int fromTimeIndex = 0;
            int toTimeIndex = 0;
            foreach (Przystanek p in przystanki) 
            {
                if (p.nazwa == _from) 
                {
                    break;
                }
                fromTimeIndex++;
            }
            foreach (Przystanek p in przystanki) 
            {
                if (p.nazwa == to) 
                {
                    break;
                }
                toTimeIndex++;
            }
            return czasJazdy[toTimeIndex] - czasJazdy[fromTimeIndex];
        }
        public int czasDo(string to) 
        {
            int toTimeIndex = 0;
            foreach (Przystanek p in przystanki)
            {
                if (p.nazwa == to)
                {
                    break;
                }
                toTimeIndex++;
            }
            return czasJazdy[toTimeIndex];
        }

    }
    #endregion
}
