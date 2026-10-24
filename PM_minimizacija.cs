using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PM_Projeck
{
    public static class Minimizator
    {

        //Klasa kojom opisujemo AND clanove 
        //npr. za A & !B su to A i !B
        public class AndMember
        {
            public string logickiIzrazClana { get; }
            public int brojNepoznanica { get; }

            //covers nam cuva broj kombinacije za koju je taj AND clan jednak 1 npr. za primjer gore je to 2 i 3 za A
            public List<int>covers { get; }


            //konstruktor
            public AndMember(string logickiIzrazClana, int brojNepoznanica, List<int> covers)
            {
                this.logickiIzrazClana = logickiIzrazClana;
                this.brojNepoznanica = brojNepoznanica;
                this.covers = covers;
            }
        }




        //Funkcija za pronalazenje svih varijabli i logickom izrazu
        public static string PronadiVarijable(string logickiIzraz)
        {
            List<char>varijable=new List<char>();

            foreach (char c in logickiIzraz)
            {
                if (c >= 'A' && c <= 'Z' && !varijable.Contains(c))
                {
                    varijable.Add(c);
                }
            }
            varijable.Sort();
            return new string(varijable.ToArray());
        }

        //Funkcija koja pretvara broj kombinacije u vrijednosti varijabli. npr. 5 ce biti 101 sto odgovara kao da je A=1,B=0,C=1
        public static bool Bit(int kombinacija, int index, int brojVarijabli)
        {
            string binary = Convert.ToString(kombinacija, 2).PadLeft(brojVarijabli, '0');
            return binary[index] == '1';
        }


        //Za jednu kombinaciju prode kroz cijeli logicki izraz te izracuna izlaznu vrijednost. Za to prvo mijenja format da bi se mogla koristiti vec postojeca funkcija iz .NET-a.
        public static bool RacunanjeJedneKombinacije(string logickiIzraz, string varijable, int kombinacija)
        {
            var izmjenjenLogickiIzraz = new StringBuilder();
            foreach (char c in logickiIzraz)
            {
                if (c >= 'A' && c <= 'Z')
                {
                    int index = varijable.IndexOf(c);
                    if (index < 0)
                    {
                        throw new FormatException("Nepoznata varijabla.");
                    }
                    izmjenjenLogickiIzraz.Append(Bit(kombinacija, index, varijable.Length) ? " true " : " false ");
                } else if (c == '0')
                {
                    izmjenjenLogickiIzraz.Append(" false ");
                }
                else if (c == '1')
                {
                    izmjenjenLogickiIzraz.Append(" true ");
                }
                else if (c == '!')
                {
                    izmjenjenLogickiIzraz.Append(" NOT ");
                }
                else if (c == '&')
                {
                    izmjenjenLogickiIzraz.Append(" AND ");
                }
                else if (c == '|')
                {
                    izmjenjenLogickiIzraz.Append(" OR ");
                }
                else if (c == '(' || c == ')' || char.IsWhiteSpace(c))
                {
                    izmjenjenLogickiIzraz.Append(c);
                }
                else
                {
                    throw new FormatException($"Nedopusteni znak '{c}'.");
                }
            }

            using (var racunanje = new DataTable())
            {
                try
                {
                    object rezultat = racunanje.Compute(izmjenjenLogickiIzraz.ToString(), "");
                    if (rezultat is bool odgovor)
                    {
                        return odgovor;
                    }
                    throw new FormatException("Izraz je prazan ili nema logicku vrijednost.");
                }
                catch (Exception e) when (e is not FormatException)
                {
                    throw new FormatException("Raspored negdje nije dobar.");
                }
            }

        }


        //Izraduje tablicu istinitosti tako sto prvo odredi koliko kombinacija ima pa za svaku racuna izlaz i sprema ga u array bool tipa
        public static bool[] NapraviTablicuIstinitosti(string logickiIzraz, string varijable)
        {
            int brojKombinacija = (int)Math.Pow(2, varijable.Length);
            var izlazi = new bool[brojKombinacija];

            for(int i = 0; i < izlazi.Length; i++)
            {
                izlazi[i] = RacunanjeJedneKombinacije(logickiIzraz, varijable, i);
            }
            return izlazi;
        }


        public static List<AndMember> PronadiDopusteneAndClanove(string varijable, bool[] izlazi)
        {
            var andMembers = new List<AndMember>();
            var pattern = new int[varijable.Length]; // 0 znaci !A, 1 znaci A, 2 znaci izostavi A

            void Generiraj(int index)
            {
                if(index < varijable.Length)
                {
                    for(int state = 0; state < 3; state++)
                    {
                        pattern[index] = state;
                        Generiraj(index + 1);
                    }
                    return;
                }

                var covers = new List<int>();

                for(int i = 0; i < izlazi.Length; i++)
                {
                    bool podudaranje = true;

                    for(int j = 0; j < varijable.Length; j++)
                    {
                        if (pattern[j] == 2)
                        {
                            continue;
                        }
                        bool potrebno = pattern[j] == 1;

                        if (Bit(i, j, varijable.Length) != potrebno)
                        {
                            podudaranje = false;
                            break;
                        }
                    }

                    if (!podudaranje)
                    {
                        continue;
                    }

                    if (!izlazi[i])
                    {
                        return; //Clan ukljucuje nulu pa ga se odbacuje
                    }

                    covers.Add(i);
                }

                var nepoznanice = new List<string>();

                for(int i = 0; i < varijable.Length; i++)
                {
                    if (pattern[i] == 0)
                    {
                        nepoznanice.Add("!" + varijable[i]);
                    }
                    if (pattern[i] == 1)
                    {
                        nepoznanice.Add(varijable[i].ToString());
                    }
                }

                string andMemberLogickiIzraz = nepoznanice.Count == 0 ? "1" : string.Join(" & ", nepoznanice);
                andMembers.Add(new AndMember(andMemberLogickiIzraz, nepoznanice.Count,covers));

            }

            Generiraj(0);

            andMembers.Sort((a, b) =>
            {
                int byCost = a.brojNepoznanica.CompareTo(b.brojNepoznanica);
                return byCost != 0 ? byCost : string.CompareOrdinal(a.logickiIzrazClana, b.logickiIzrazClana);
            });
            return andMembers;
        }


        public static List<AndMember> PronadiNajboljeAndMembere(List<AndMember> andMembers, bool[] izlazi)
        {
            var odabran = new List<AndMember>();
            var najbolji = new List<AndMember>();
            int brojNajboljihNepoznanica = int.MaxValue;

            void Search(bool[] covered, int brojNepoznanica)
            {
                int missing = -1;

                for (int i = 0; i < izlazi.Length; i++)
                {
                    if (izlazi[i] && !covered[i])
                    {
                        missing = i;
                        break;
                    }
                }
                    if (missing == -1)
                    {
                        if(brojNepoznanica < brojNajboljihNepoznanica || (brojNepoznanica == brojNajboljihNepoznanica && odabran.Count < najbolji.Count))
                        {
                            najbolji = new List<AndMember>(odabran);
                            brojNajboljihNepoznanica = brojNepoznanica;
                        }
                        return;
                    }

                    //Daljnji clanovi ne mogu smanjiti vec potrosen broj nepoznanica

                    if(brojNepoznanica >= brojNajboljihNepoznanica)
                    {
                        return;
                    }

                    foreach (AndMember andMember in andMembers)
                    {
                        if (!andMember.covers.Contains(missing))
                        {
                            continue;
                        }
                        bool[] nextCovered = (bool[])covered.Clone();

                        foreach(int j in andMember.covers)
                        {
                            nextCovered[j] = true;
                        }
                        odabran.Add(andMember);
                        Search(nextCovered, brojNepoznanica + andMember.brojNepoznanica);
                        odabran.RemoveAt(odabran.Count - 1);
                    }
                }
            Search(new bool[izlazi.Length], 0);
            return najbolji;
            }

            
        
        //Funkcija koja na jednom mjestu povezuje sve ono gore
        public static string Minimizacija(string logickiIzraz)
        {
            string varijable = PronadiVarijable(logickiIzraz);
            int maxVarijabli = 6;

            if(varijable.Length > maxVarijabli)
            {
                throw new ArgumentException($"Podrzano je najvise {maxVarijabli} varijabli.");
            }
            bool[] izlazi = NapraviTablicuIstinitosti(logickiIzraz, varijable);

            int jedinice = 0;

            foreach(bool vrijednost in izlazi)
            {
                if (vrijednost)
                {
                    jedinice++;
                }
            }
            if (jedinice == 0)
            {
                return "0";
            }
            if(jedinice == izlazi.Length)
            {
                return "1";
            }

            List<AndMember> dopusteni = PronadiDopusteneAndClanove(varijable, izlazi);
            List<AndMember> najbolji = PronadiNajboljeAndMembere(dopusteni, izlazi);

            var dijelovi = new List<string>();

            foreach(AndMember andMember in najbolji)
            {
                dijelovi.Add(andMember.brojNepoznanica > 1 ? "(" + andMember.logickiIzrazClana + ")" : andMember.logickiIzrazClana);
            }
            dijelovi.Sort(StringComparer.Ordinal);
            return string.Join(" | ", dijelovi);
        }



        //Ovu funkciju poziva JavaScript (script.js) preko DotNet.invokeMethodAsync
        [JSInvokable]
        public static object MinimizirajJS(string izraz)
        {
            izraz = izraz?.Trim() ?? "";
            if (izraz.Length == 0 || izraz.Length > 200)
            {
                throw new ArgumentException("Upisi izraz (najvise 200 znakova).");
            }
            string rezultat = Minimizacija(izraz);
            string varijable = PronadiVarijable(izraz);
            bool[] izlazi = NapraviTablicuIstinitosti(izraz, varijable);
            return new { rezultat, varijable, izlazi };
        }
    }
}
