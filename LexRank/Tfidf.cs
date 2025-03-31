using QuikGraph;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;


class Tfidf
{
    static public string Ozetle(string metin, int cumleSayisi)
    {

        Dictionary<int, Dictionary<string, double>> cumleNo_kelimeler = new Dictionary<int, Dictionary<string, double>>(); // cümlelerin numaraları ile kelime TF ikilisini tutar

        List<string> cumleler = new List<string>();

        Cumlelestir(metin, cumleler);

        List<string> orijinalCumleler = new List<string>(cumleler);

        cumleDuzenle(cumleler);

        Dictionary<string, int> kelimeCount = new Dictionary<string, int>();

        kelimeCountInitializer(cumleler, kelimeCount);

        tfDegeri(cumleler, cumleNo_kelimeler);

        Dictionary<string, int> kelimelerinBulunduguCumlelerSayisi = new Dictionary<string, int>();

        sentenceContainsWordIncreaser(cumleler, kelimelerinBulunduguCumlelerSayisi);

        Dictionary<string, double> idfDegerleri = new Dictionary<string, double>();

        idfDegerleriInitializer(idfDegerleri, kelimelerinBulunduguCumlelerSayisi);

        idfDegeri(numberOfSentences(cumleler), kelimelerinBulunduguCumlelerSayisi, idfDegerleri);

        Dictionary<int, Dictionary<string, double>> tfidfDegerleri = new Dictionary<int, Dictionary<string, double>>();

        tfidfDegeri(tfidfDegerleri, cumleNo_kelimeler, idfDegerleri);

        List<double> cumlelerinDegerleri = new List<double>();

        for (int i = 0; i < cumleler.Count; i++)                        // cümlelerin skorlarını initialize eder
        {
            cumlelerinDegerleri.Add(0d);
        }

        cosineSimilarity(tfidfDegerleri, cumlelerinDegerleri);

        string ozet = sonuc(cumleSayisi, orijinalCumleler, cumlelerinDegerleri);

        return ozet;

    }

    static void Cumlelestir(string metin, List<string> cumleler)
    {

        string mevcut = metin + " ";
        bool devam = true;
        int i = 0;

        while (devam)
        {
            if ((i = mevcut.IndexOf('.', 0)) != -1)                         // ondalıklı sayılar için çalışmıyor
            {
                string m = mevcut.Substring(0, i + 1);
                cumleler.Add(m);
                mevcut = mevcut.Remove(0, i + 2);

            }
            else { devam = false; }
        }
    }

    static void cumleDuzenle(List<string> cumleler)                         // işaret temizleme ve lower
    {
        for (int i = 0; i < cumleler.Count; i++)
        {
            if ((cumleler[i])[0] == ' ') { cumleler[i] = cumleler[i].Substring(1, cumleler[i].Length - 1); }
            cumleler[i] = cumleler[i].Replace("(", string.Empty).Replace(")", string.Empty).Replace(",", string.Empty).Replace(".", string.Empty).Replace("\n", string.Empty).Replace("\"", string.Empty).Replace("-", string.Empty).Replace("[", string.Empty).Replace("]", string.Empty).Replace("?", ".").Replace("/", string.Empty).Replace("'", string.Empty).Replace("...", ".").ToLower();
        }
    }

    static void tfDegeri(List<string> cumleler, Dictionary<int, Dictionary<string, double>> cumleNo_kelimeler) 
    {

        HashSet<string> set = new HashSet<string>();

        int cumledekiKelimeSayısı;

        int i = 0;

        foreach (string cumle in cumleler) {

            kelimeleriAl(cumle, set);

            cumleNo_kelimeler.Add(i, new Dictionary<string, double>());

            cumledekiKelimeSayısı = numberOfAllTheWords(cumle);                         // NB

            foreach (string kelime in set)
            {
                if (kelime == "")                                                       // formatsız metinlerdeki sessizleşmeyi engellemek için
                {
                    continue;
                }

                cumleNo_kelimeler[i].Add(kelime, (double)countOfTheWord(kelime, cumle) / cumledekiKelimeSayısı);     // TF = NA / NB

            }

            ++i;
            set.Clear();
        }
    }

    static int countOfTheWord(string kelime, string cumle)                         // NA
    {
        int i = 0;
        int count = 0;
        cumle = " " + cumle + " ";

        while ((i = cumle.IndexOf(" " + kelime + " ", i, StringComparison.OrdinalIgnoreCase)) != -1)
        {
            ++count;
            i += kelime.Length;
        }

        return count;
    }

    static void idfDegeri(int cumleSayisi, Dictionary<string, int> kelimelerinBulunduguCumlelerSayisi, Dictionary<string, double> idfDegerleri)
    {
        foreach(string kelime in idfDegerleri.Keys)
        {
            idfDegerleri[kelime] = Math.Log10((double)cumleSayisi / kelimelerinBulunduguCumlelerSayisi[kelime]);
        }

    }

    static void tfidfDegeri(Dictionary<int, Dictionary<string, double>> tfidfDegerleri, Dictionary<int, Dictionary<string, double>> tfDegerleri, Dictionary<string, double> idfDegerleri)
    {
        foreach(int cumleNo in tfDegerleri.Keys) 
        {
            tfidfDegerleri.Add(cumleNo, new Dictionary<string, double>());

            foreach (string kelime in tfDegerleri[cumleNo].Keys)
            {
                tfidfDegerleri[cumleNo].Add(kelime, tfDegerleri[cumleNo][kelime] * idfDegerleri[kelime]);
            }
        }
    }

    // ND number of the sentences in the text that contains the word
    static void sentenceContainsWordIncreaser(List<string> cumleler, Dictionary<string, int> kelimelerinBulunduguCumlelerSayisi)
    {
        HashSet<string> set = new HashSet<string>();

        foreach (string cumle in cumleler)
        {
            kelimeleriAl(cumle, set);

            foreach (string kelime in set)
            {
                if (kelimelerinBulunduguCumlelerSayisi.ContainsKey(kelime))
                {
                    ++kelimelerinBulunduguCumlelerSayisi[kelime];
                }
                else
                {
                    kelimelerinBulunduguCumlelerSayisi.Add(kelime, 1);
                }
            }

            set.Clear();

        }
    }

    static int numberOfSentences(List<string> cumleler )                                    // NC
    {
        return cumleler.Count;
    }

    static int numberOfAllTheWords(string cumle)                                            // NB
    {

        int i = 0;
        int count = 0;

        while ((i = cumle.IndexOf(' ', i)) != -1)
        {
            ++count;
            ++i;
        }

        return ++count;

    }

    static HashSet<string> kelimeleriAl(string cumle, HashSet<string> set)
    {
        int i = 0;
        int n = 0;

        while ((i = cumle.IndexOf(' ', i)) != -1)
        {
            set.Add(cumle.Substring(n, i - n));
            ++i;
            n = i;
        }

        set.Add(cumle.Substring(cumle.LastIndexOf(' ') + 1));

        return set;

    }

    static void kelimeCountInitializer(List<string> cumleler, Dictionary<string, int> kelimeCount)  // metindeki kelime sayılarını tutan dictionary'i initialize eder
    {
        HashSet<string> set = new HashSet<string>();

        string duzenliMetin = "";

        for (int i = 0; i < cumleler.Count; i++)
        {
            if (i == cumleler.Count - 1)
            {
                duzenliMetin += cumleler[i];
            }
            else {
                duzenliMetin += cumleler[i] + " ";
            }
        }

        kelimeleriAl(duzenliMetin, set);

        foreach (string kelime in set)
        {
            if (kelime == "")                                                       // sessizleşmeyi engellemek için
            {
                continue;
            }
            kelimeCount.Add(kelime, countOfTheWord(kelime, duzenliMetin));
        }

        
    }

    static void idfDegerleriInitializer(Dictionary<string, double> idfDegerleri, Dictionary<string, int> kelimeleriTutanDictionary)
    {
        foreach(string kelime in kelimeleriTutanDictionary.Keys)
        {
            idfDegerleri.Add(kelime, 0);
        }
    }

    static List<double> cosineSimilarity(Dictionary<int, Dictionary<string, double>> tfidfDegerleri, List<double> cumlelerinDegerleri)
    {
        List<double> magnitudes = new List<double>();

        int diziUzunlugu = (tfidfDegerleri.Count * (tfidfDegerleri.Count - 1)) / 2;

        foreach (Dictionary<string, double> vector in tfidfDegerleri.Values)
        {
            magnitudes.Add(magnitudeValue(vector));
        }

        List<double> cosineValues = new List<double>();

        for (int i = 0; i < tfidfDegerleri.Count; i++)
        {
            double cosineSkor = 0d;

            for (int j = i + 1; j < tfidfDegerleri.Count; j++)
            {
                cosineSkor = dotProduct(tfidfDegerleri[i], tfidfDegerleri[j]) / (magnitudes[i] * magnitudes[j]);
                cosineValues.Add(cosineSkor);
                cumlelerinDegerleri[i] += cosineSkor;                                           // ilk vertex'e edge skorunu ekle
                cumlelerinDegerleri[j] += cosineSkor;                                           // ikinci vertex'e edge skorunu ekle
            }
        }

        return cosineValues;

    }

    static double dotProduct(Dictionary<string, double> ilkVector, Dictionary<string, double> ikinciVektor)
    {
        double sonuc = 0d;
        foreach(string kelime in ilkVector.Keys)
        {
            if(ikinciVektor.ContainsKey(kelime))
            {
                sonuc = ilkVector[kelime] * ikinciVektor[kelime];
            }
        }

        return sonuc;
    }

    static double magnitudeValue(Dictionary<string, double> vector)
    {
        double sonuc = 0d;

        foreach(double deger in vector.Values)
        {
            sonuc += (deger * deger);
        }

        return Math.Sqrt(sonuc);
    }

    static string sonuc(int cumleSayisi, List<string> cumleler, List<double> cumlelerinDegerleri)
    {
        string sonuc = "";

        if ( cumleSayisi > cumleler.Count)
        {
            cumleSayisi = cumleler.Count;
        }

        List<double> skorlar = new List<double>(cumleSayisi);
        List<int> cumleNumaralari = new List<int>(cumleSayisi);

        for (int i = 0; i < cumleSayisi; i++)
        {
            skorlar.Add(0d);
            cumleNumaralari.Add(0);
        }

        double enKucuk = 0d;                                    // enkücük değer
        int enKucugunIndexi = 0;                                // skorlar ve cumleno'lardaki en küçük değeri tutan bilgilerin indexi

        for (int i = 0;  i < cumleler.Count; i++)
        {
            if (enKucuk < cumlelerinDegerleri[i])
            {
                skorlar[enKucugunIndexi] = cumlelerinDegerleri[i];
                cumleNumaralari[enKucugunIndexi] = i;
                enKucuk = cumlelerinDegerleri[i];

                for (int j = 0; j < cumleSayisi; j++)
                {
                    if (enKucuk > skorlar[j] )
                    {
                        enKucuk = skorlar[j];
                        enKucugunIndexi = j;
                    }
                }
            }
        }

        foreach(int i in cumleNumaralari)
        {
            if (i == cumleNumaralari.Last())
            {
                sonuc += cumleler[i];

            }
            else
            {
                sonuc += cumleler[i] + " ";
            }
        }

        return sonuc;

    }
}
