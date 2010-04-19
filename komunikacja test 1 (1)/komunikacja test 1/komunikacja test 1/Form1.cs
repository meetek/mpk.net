using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

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
            addData();
        }
        private void addData() 
        {
            
            przystanki.Add(new Przystanek("a"));
            przystanki.Add(new Przystanek("b"));
            przystanki.Add(new Przystanek("c"));
            przystanki.Add(new Przystanek("d"));
            przystanki.Add(new Przystanek("e"));
            przystanki.Add(new Przystanek("f"));

            linie.Add(new Linia("1"));
            linie.Add(new Linia("2"));
            linie.Add(new Linia("3"));
            linie.Add(new Linia("4"));
            linie.Add(new Linia("5"));
            linie.Add(new Linia("6"));

            linie[0].wyjazd = new DateTime(2000,11,11,5,0,0);
            
            linie[0].dodajPrzystanek(przystanki[0], 0);
            linie[0].dodajPrzystanek(przystanki[1], 10);
            linie[0].dodajPrzystanek(przystanki[2], 110);


            linie[1].wyjazd = new DateTime(2000, 11, 11, 5, 10, 0);
            linie[1].dodajPrzystanek(przystanki[5], 0);
            linie[1].dodajPrzystanek(przystanki[1], 5);
            linie[1].dodajPrzystanek(przystanki[3], 13);

            linie[2].wyjazd = new DateTime(2000, 11, 11, 5, 24, 0);
            linie[2].dodajPrzystanek(przystanki[3], 0);
            linie[2].dodajPrzystanek(przystanki[4], 9);

            linie[3].wyjazd = new DateTime(2000, 11, 11, 5, 35, 0);
            linie[3].dodajPrzystanek(przystanki[4], 0);
            linie[3].dodajPrzystanek(przystanki[2], 3);

            linie[4].wyjazd = new DateTime(2000, 11, 11, 5, 0, 0);
            linie[4].dodajPrzystanek(przystanki[0], 0);
            linie[4].dodajPrzystanek(przystanki[5], 10);

            linie[5].wyjazd = new DateTime(2000, 11, 11, 5, 20, 0);
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
                comboBox1.Items.Add(p.id);
                comboBox2.Items.Add(p.id);
            }
        }
        private void search(string _from, string to,int nodes,int initialNodes,string previousLine,DateTime godzina) 
        {
            //DateTime godzina = new DateTime(2000, 11, 11, g.Hour, g.Minute, g.Second);
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
                            return p.id == _from;
                        }
                        ).linie
                    )
                {

                    if (nodes == 0 && l.numerLinii != previousLine && previousLine != "z buta") continue;
                    NastPrzyst nastPrzyst = jedzLinia(l, _from);

                    //jesli nie ma dalszych przystankow jakiejs linii pomin ja
                    
                    if (nastPrzyst == null) continue;

                    //jesli ta linia juz odjechala - pomin ja
                    DateTime odjazdLiniiZPrzystanku = l.wyjazd.AddMinutes(l.czasDo(_from));
                    if (godzina > odjazdLiniiZPrzystanku) continue;

                    TimeSpan zaIleOdjezdza = odjazdLiniiZPrzystanku - godzina;
                    
                    droga.Add(new Wynik(l.numerLinii, nastPrzyst.czas,(int)zaIleOdjezdza.TotalMinutes));

                    //oblicz ktora bedzie godzina jak dojedzie na natepny przystanek (czekan ie na autobus + jazda)
                    int h, m;
                    h = godzina.Hour + zaIleOdjezdza.Hours;
                    m = godzina.Minute + zaIleOdjezdza.Minutes + l.czasZDo(_from, nastPrzyst.nastPrzyst);
                     

                                       
                    if ((previousLine == l.numerLinii) || previousLine == "z buta")
                    {
                        search(nastPrzyst.nastPrzyst, to, nodes, initialNodes, l.numerLinii,new DateTime(2000, 11, 11, h+m/60,m%60,0));
                    }
                    else 
                    {
                        search(nastPrzyst.nastPrzyst, to, nodes - 1, initialNodes, l.numerLinii,new DateTime(2000, 11, 11, h+m/60,m%60,0));
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
                if (linia.przystanki[i].id == to)
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
                if (p.id == _from)
                {
                    break;
                }
                else
                {
                    count++;
                }
            }
            if (count + 1 >= l.przystanki.Count) return null;

            return new NastPrzyst(l.przystanki[count + 1].id, l.czasJazdy[count + 1] - l.czasJazdy[count]);

        
        }
        private Wynik findPrzystanekFromPrzystanek(string _from, string to) 
        {
            if (_from == to) return null;
            List<Wynik> wyniki = new List<Wynik>();
            foreach (Linia l in (przystanki.Find(
                delegate(Przystanek prz) 
                {
                    return prz.id == _from;
                }
                )).linie)
            {
                Przystanek toZnaleziony = l.przystanki.Find(
                                    delegate(Przystanek prz)
                                    {
                                        return prz.id == to;
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
           // godzina = new DateTime(2000, 11, 11, Int32.Parse(textBox1.Text), Int32.Parse(textBox2.Text), Int32.Parse(textBox3.Text));
            search(
                (string)comboBox1.SelectedItem,
                (string)comboBox2.SelectedItem,
                (int)numericUpDown1.Value, 
                (int)numericUpDown1.Value,
                "z buta",
                new DateTime(2000, 11, 11, Int32.Parse(textBox1.Text), Int32.Parse(textBox2.Text), Int32.Parse(textBox3.Text))
            );
            normalizeResults();
            showResults();
            wyniki.Clear();
            droga.Clear();
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
                czasCalkowity += w[i].czas;
            
            }

            liczbaPrzesiadek = this.linie.Count - 1;


        }
    }
    public class NastPrzyst
    {
        public string nastPrzyst;
        public int czas;
        public NastPrzyst(string id, int t) 
        {
            nastPrzyst = id;
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
        public List<Linia> linie = new List<Linia>();

        public Przystanek(string _id) 
        {
            id = _id;
        }
        public void dodajLinie(Linia linia)
        {
            linie.Add(linia);
        }
    
    }
    public class Linia 
    {
        public string numerLinii;
        public DateTime wyjazd;
        public List<Przystanek> przystanki = new List<Przystanek>();
        public List<int> czasJazdy = new List<int>();

        public Linia(string _numerLinii) 
        {
            numerLinii = _numerLinii;
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
                if (p.id == _from) 
                {
                    break;
                }
                fromTimeIndex++;
            }
            foreach (Przystanek p in przystanki) 
            {
                if (p.id == to) 
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
                if (p.id == to)
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
