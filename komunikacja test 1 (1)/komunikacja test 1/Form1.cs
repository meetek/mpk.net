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
           // addData();
        }
        private void addData() 
        {

            przystanki.Add(new Przystanek("a", "a", "a"));
            przystanki.Add(new Przystanek("b", "b", "b"));
            przystanki.Add(new Przystanek("c", "c", "c"));
            przystanki.Add(new Przystanek("d", "d", "d"));
            przystanki.Add(new Przystanek("e", "e", "e"));
            przystanki.Add(new Przystanek("f", "f", "f"));

            linie.Add(new Linia("1", "Linia normalna"));
            linie.Add(new Linia("2", "Linia normalna"));
            linie.Add(new Linia("3", "Linia normalna"));
            linie.Add(new Linia("4", "Linia normalna"));
            linie.Add(new Linia("5", "Linia normalna"));
            linie.Add(new Linia("6", "Linia normalna"));

            linie[0].wyjazd.Add(new Wyjazd("Niedziela", new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 5, 0, 0)));
            linie[0].wyjazd.Add(new Wyjazd("Niedziela", new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 5, 6, 0)));
            
            linie[0].dodajPrzystanek(przystanki[0], 0);
            linie[0].dodajPrzystanek(przystanki[1], 10);
            linie[0].dodajPrzystanek(przystanki[2], 110);


            linie[1].wyjazd.Add(new Wyjazd("Niedziela", new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 5, 10, 0)));
            linie[1].dodajPrzystanek(przystanki[5], 0);
            linie[1].dodajPrzystanek(przystanki[1], 5);
            linie[1].dodajPrzystanek(przystanki[3], 13);

            linie[2].wyjazd.Add(new Wyjazd("Niedziela", new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 5, 22, 0)));
            linie[2].wyjazd.Add(new Wyjazd("Niedziela", new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 6, 27, 0)));
            linie[2].wyjazd.Add(new Wyjazd("Niedziela", new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 6, 30, 0)));
            linie[2].dodajPrzystanek(przystanki[3], 0);
            linie[2].dodajPrzystanek(przystanki[4], 9);

            linie[3].wyjazd.Add(new Wyjazd("Niedziela", new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 5, 35, 0)));
            linie[3].wyjazd.Add(new Wyjazd("Niedziela", new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 6, 50, 0)));
            linie[3].dodajPrzystanek(przystanki[4], 0);
            linie[3].dodajPrzystanek(przystanki[2], 3);

            linie[4].wyjazd.Add(new Wyjazd("Niedziela", new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 5, 0, 0)));
            linie[4].dodajPrzystanek(przystanki[0], 0);
            linie[4].dodajPrzystanek(przystanki[5], 10);

            linie[5].wyjazd.Add(new Wyjazd("Niedziela", new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 5, 20, 0)));
            linie[5].dodajPrzystanek(przystanki[5], 0);
            linie[5].dodajPrzystanek(przystanki[2], 50);

            przystanki[0].dodajLinie(linie[0]);
            przystanki[0].dodajLinie(linie[4]);

            przystanki[1].dodajLinie(linie[0]);
            przystanki[1].dodajLinie(linie[1]);

            przystanki[2].dodajLinie(linie[0]);
            przystanki[2].dodajLinie(linie[3]);
            przystanki[2].dodajLinie(linie[5]);

            przystanki[3].dodajLinie(linie[1]);
            przystanki[3].dodajLinie(linie[2]);

            przystanki[4].dodajLinie(linie[2]);
            przystanki[4].dodajLinie(linie[3]);

            przystanki[5].dodajLinie(linie[1]);
            przystanki[5].dodajLinie(linie[4]);
            przystanki[5].dodajLinie(linie[5]);

            foreach(Przystanek p in przystanki)
            {
                comboBox1.Items.Add(p.nazwa);
                comboBox2.Items.Add(p.nazwa);
            }
        }
        private void search(string _from, string to,int nodes,int initialNodes,string previousLine,DateTime godzina) 
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
                    
                    foreach (Wyjazd w in l.wyjazd) 
                    {
                        if (godzina.DayOfWeek == DayOfWeek.Saturday && w.typDnia != "Sobota") 
                        {
                            continue;
                        }
                        if (godzina.DayOfWeek == DayOfWeek.Sunday && w.typDnia != "Niedziela")
                        {
                            continue;
                        }
                        if (godzina.DayOfWeek != DayOfWeek.Sunday && 
                            godzina.DayOfWeek != DayOfWeek.Saturday &&
                            (w.typDnia == "Niedziela" ||
                            w.typDnia == "Sobota"))
                        {
                            continue;
                        }
                        //dojscie tu oznacza ze sprawdzamy czas dla dobrego dnia
                        foreach (DateTime d in w.wyjazd)
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
           
            /*search(
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
            */
            ParseFile(@"E:\Moje bzdury\_STUDIA\8 Semestr\psi 3\program\rozklady_xml\zdik\0240.xml");

        }
        public void ParseFile(string strPath)
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
        public Wyjazd(string _typDnia, DateTime _wyjazd) 
        {
            typDnia = _typDnia;
            wyjazd.Add(_wyjazd);
        }
    }
    public class Linia 
    {
        public string numerLinii;
        public string typLinii;
        public List<Wyjazd> wyjazd = new List<Wyjazd>();
        public List<Przystanek> przystanki = new List<Przystanek>();
        public List<int> czasJazdy = new List<int>();

        public Linia(string _numerLinii,string typ) 
        {
            numerLinii = _numerLinii;
            typLinii = typ;
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
