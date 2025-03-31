# LexRank Ve TextRank İle Çıkarımsal Metin Özetleme

Bu repo, TextRank ve LexRank olmak üzere iki metin özetleme algoritmasını içermektedir. Bu algoritmalar, başlangıçta bir ASP.NET projesinin bir parçası olarak geliştirilmiştir. Projenin ana amacı, TextRank ve LexRank algoritmalarının performanslarını çeşitli ölçütlere göre incelemektir. Her iki algoritma da formatlanmış metinler üzerinde daha başarılı sonuçlar vermektedir.


## Kullanılan Algoritmalar Hakkında:
### TextRank
PageRank algoritmasına dayalı, grafik tabanlı bir metin özetleme algoritmasıdır. TextRank, metin içerisindeki cümlelerin birbirleriyle olan ilişkilerini analiz ederek, en önemli cümleleri seçer.

### LexRank
Kosinüs benzerliği ve kümeleme tekniklerini kullanarak, metinlerdeki en anlamlı cümleleri seçen grafik temelli bir algoritmadır. LexRank, metin içindeki cümlelerin benzerliklerini analiz ederek özet oluşturur.

## Bağımlılıklar
Bu proje, TextRank algoritmasında grafik tabanlı işlemler için QuikGraph kütüphanesini kullanmaktadır.




