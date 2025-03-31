using Microsoft.AspNetCore.Components.Forms;
using QuikGraph;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using WebApplication1;
using static System.Runtime.InteropServices.JavaScript.JSType;


class Textrank
{
    const double dampingFactor_g = 0.85;

    public static string Ozetle(string metin, int cumleSayisi)
    {

        var graph = new AdjacencyGraph<int, TaggedEdge<int, double>>();
        List<string> duzenliCumleler = new List<string>();


        Cumlelestir(graph, metin, duzenliCumleler);
        List<string> duzensizCumleler = new List<string>(duzenliCumleler);          // işlenecek cümleyi ayır

        cumleDuzenle(duzensizCumleler);                                             // işlenecek cümleyi temizle

        Bagla(graph, duzensizCumleler);
        Double[] Skor = new double[graph.VertexCount];
        Hesapla(graph, Skor);
        return Sonuc(duzenliCumleler, Skor, cumleSayisi);
    }
    static void Cumlelestir(AdjacencyGraph<int, TaggedEdge<int, double>> graph, string metin, List<string> cumleler)
    {
        string mevcut = metin;
        bool devam = true;
        int i = 0;
        int vertex = 0;
        while (devam)
        {
            if ((i = mevcut.IndexOf('.', 0)) != -1)
            {
                string m = mevcut.Substring(0, i + 1);
                cumleler.Add(m);
                graph.AddVertex(vertex);
                ++vertex;
                mevcut = mevcut.Remove(0, i + 1);                               // ondalıklı sayılar için çalışmıyor

            }
            else { devam = false; }
        }
    }
    static void Bagla(AdjacencyGraph<int, TaggedEdge<int, double>> graph, List<string> cumleler)
    {
        int i = 0;                                                              // cümle indexOf indexi
        string ilkCumle;
        string ikinciCumle;
        int topAgırlık = 0;
        HashSet<string> kelimelerSet = new HashSet<string>();
        for (int a = 0; a < (cumleler.Count() - 1); a++)
        {
            ilkCumle = cumleler[a].ToLower();
            string[] kelimeler = ilkCumle.Split(' ');                           // kelime ayırıcı
            kelimelerSet.UnionWith(kelimeler);                                  // ilk cümle seti

            for (int b = a + 1; b < cumleler.Count(); b++)
            {
                ikinciCumle = cumleler[b].ToLower();
                string[] kelimelerIkinci = ilkCumle.Split(' ', ',');
                foreach (string s in kelimelerSet)
                {
                    if (s == " ") { continue; }                                 // boşluğu geç (ihtiyaç olmayabilir)

                    else if (s == "") { continue; }

                    else
                    {
                        while ((i = ikinciCumle.IndexOf(" " + s + " ", i)) != -1)
                        {
                            ++topAgırlık;
                            i += s.Length;
                        }
                        i = 0;
                    }
                }

                // ilk cümle ve ikinci cümle indexleri vertex, aralarındaki edge değeri ise formül sonucu olarak ekleniyor
                graph.AddEdge(new TaggedEdge<int, double>(a, b, (topAgırlık / (Math.Log10(kelimeler.Length) + Math.Log10(kelimelerIkinci.Length)))));
                kelimelerSet.Clear();
            }
        }
    }
    static void Hesapla(AdjacencyGraph<int, TaggedEdge<int, double>> graph, Double[] Skor)
    {
        for (int i = 0; i < Skor.Length; i++)                                       // skorları 1'le
        {
            Skor[i] = 1;
        }
        Double[] edgeSkor = new double[Skor.Length];
        edgesSkor(graph, edgeSkor);                                                 // vertex'lerin kenarlarının toplamı

        double threshold = 0.0001;
        double maks = 0;
        bool devam = true;
        while (devam)
        {
            maks = 0;
            foreach (var i in graph.Vertices)                                       // her i vertex için
            {
                double tumKenarlarınToplamı = 0;
                foreach (var seti in graph.OutEdges(i))                             // her i nin kenarı için
                {
                    int j = seti.Target;                                            // kenarın diğer vertexi j 
                    double wji = seti.Tag;                                          // W_ji
                    double toplamWjk = edgeSkor[j];                                 // j'nin her kenarının toplam ağırlığı
                    double skorJ = Skor[j];                                         // j'nin skoru WS(j)

                    tumKenarlarınToplamı += ((wji / toplamWjk) * skorJ);
                }
                double yeniSkor = (1 - dampingFactor_g) + (dampingFactor_g * tumKenarlarınToplamı);
                maks = Math.Max(Math.Abs(Skor[i] - yeniSkor), maks);               // yeni skor ile eski skorun farkı ile bu graph iterasyonundaki
                                                                                   // maksimum farkın (def = 0) en büyüğü

                                                                                   // maks için overflow olabilir

                Skor[i] = yeniSkor;
            }
            if (maks < threshold) { devam = false; }                               // vertexlerin önceki değer ve şimdiki değer farkı < th ise çık

        }
    }
    static string Sonuc(List<string> cumleler, Double[] Skor, int cumleSayisi)
    {
        SortedList<double, int> bagıntı = new SortedList<double, int>();           // <skor, vertex>  (aynı skorları eklemeyecek!)
        for (int i = 0; i < Skor.Length; i++)
        {
            if (bagıntı.ContainsKey(Skor[i]))
            {
                bagıntı.Add(Skor[i] + 0.00001d, i);
            }
            else
            {
                bagıntı.Add(Skor[i], i);
            }
        }
        if (Skor.Length < cumleSayisi)
        {
            cumleSayisi = Skor.Length;
        }
        var indeks = bagıntı.Count - 1;                                             // sona al

        var ind = bagıntı.Values[indeks];
        var cumle = cumleler[ind];
        if (cumle[0] == ' ') { cumle = cumle.Substring(1, cumle.Length - 1); }
        string ozet = cumle;
        --cumleSayisi;
        //Console.Write(cumle);

        for (int i = 0; i < cumleSayisi; i++)
        {
            indeks--;
            ind = bagıntı.Values[indeks];
            cumle = cumleler[ind];
            if ((cumle[0]) != ' ') { cumle = ' ' + cumle; }
            ozet = ozet + cumle;
            //Console.Write(cumle);
        }
        return ozet;
    }
    static void edgesSkor(AdjacencyGraph<int, TaggedEdge<int, double>> graph, Double[] edgeSkor)
    {
        foreach (var i in graph.Vertices)
        {
            foreach (var seti in graph.OutEdges(i))
            {
                edgeSkor[i] += seti.Tag;
                int j = seti.Target;
                edgeSkor[j] += seti.Tag;
            }
        }
    }
    static void cumleDuzenle(List<string> cumleler)
    {
        for(int i = 0; i < cumleler.Count; i++) 
        {
            if ((cumleler[i])[0] == ' ') { cumleler[i] = cumleler[i].Substring(1, cumleler[i].Length - 1); }
            cumleler[i] = cumleler[i].Replace("(", string.Empty).Replace(")", string.Empty).Replace(",", string.Empty).Replace(".", string.Empty).Replace("\"", string.Empty).Replace("-", string.Empty).Replace("[", string.Empty).Replace("]", string.Empty).Replace("?", ".").Replace("\n", string.Empty).Replace("/", string.Empty).Replace("'", string.Empty).Replace("...", ".");
        }
    }
}
